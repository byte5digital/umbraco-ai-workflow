using System.Text.Json;
using Microsoft.Extensions.Options;
using Umbraco.AI.Agent.Core.Agents;
using Umbraco.AI.Core.Connections;
using Umbraco.AI.Core.Contexts;
using Umbraco.AI.Core.Contexts.ResourceTypes.BuiltIn;
using Umbraco.AI.Core.Models;
using Umbraco.AI.Core.Profiles;
using Umbraco.AI.Prompt.Core.Prompts;

namespace UmbracoAI.Web.Seeding.Ai;

/// <summary>
/// Creates the Umbraco AI setup: OpenAI connection (key referenced from configuration), editorial chat profile
/// with the brand voice, prompt actions and the Copilot agent. Idempotent by alias – existing items
/// (possibly edited by editors) are left untouched.
/// </summary>
public sealed class AiSetupSeeder(
    IAIConnectionService connectionService,
    IAIProfileService profileService,
    IAIContextService contextService,
    IAIPromptService promptService,
    IAIAgentService agentService,
    IOptions<AiSetupOptions> options,
    ILogger<AiSetupSeeder> logger)
{
    /// <summary>Same shape the backoffice sends (camelCase JSON), so settings round-trip through the AI editors.</summary>
    private static readonly JsonSerializerOptions SettingsJson = new(JsonSerializerDefaults.Web);

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        AIConnection connection = await EnsureConnectionAsync(cancellationToken);
        AIContext brandVoice = await EnsureBrandVoiceAsync(cancellationToken);
        AIProfile profile = await EnsureProfileAsync(connection.Id, brandVoice.Id, cancellationToken);

        foreach (AiPromptDef definition in AiPromptDefinitions.All)
        {
            await EnsurePromptAsync(definition, profile.Id, cancellationToken);
        }

        await EnsureCopilotAgentAsync(profile.Id, cancellationToken);
    }

    private async Task<AIConnection> EnsureConnectionAsync(CancellationToken cancellationToken)
    {
        AIConnection? existing = await connectionService.GetConnectionByAliasAsync(AiAliases.Connection, cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        var connection = new AIConnection
        {
            Alias = AiAliases.Connection,
            Name = "OpenAI",
            ProviderId = AiAliases.ProviderId,
            Settings = ToSettings(new { apiKey = AiAliases.ApiKeyReference }),
        };

        AIConnection saved = await connectionService.SaveConnectionAsync(connection, cancellationToken);
        logger.LogInformation("AI setup: created connection {Alias}", saved.Alias);
        return saved;
    }

    private async Task<AIContext> EnsureBrandVoiceAsync(CancellationToken cancellationToken)
    {
        AIContext? existing = await contextService.GetContextByAliasAsync(AiAliases.BrandVoiceContext, cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        var context = new AIContext
        {
            Alias = AiAliases.BrandVoiceContext,
            Name = AiBrandVoice.ContextName,
            Resources =
            [
                new AIContextResource
                {
                    ResourceTypeId = "brand-voice",
                    Name = AiBrandVoice.ContextName,
                    InjectionMode = AIContextResourceInjectionMode.Always,
                    Settings = ToSettings(new BrandVoiceResourceSettings
                    {
                        ToneDescription = AiBrandVoice.Tone,
                        TargetAudience = AiBrandVoice.Audience,
                        StyleGuidelines = AiBrandVoice.StyleGuidelines,
                        AvoidPatterns = AiBrandVoice.AvoidPatterns,
                    }),
                },
            ],
        };

        AIContext saved = await contextService.SaveContextAsync(context, cancellationToken);
        logger.LogInformation("AI setup: created context {Alias}", saved.Alias);
        return saved;
    }

    private async Task<AIProfile> EnsureProfileAsync(Guid connectionId, Guid brandVoiceId, CancellationToken cancellationToken)
    {
        AIProfile? existing = await profileService.GetProfileByAliasAsync(AiAliases.ChatProfile, cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        var profile = new AIProfile
        {
            Alias = AiAliases.ChatProfile,
            Name = "Redaktion",
            Capability = AICapability.Chat,
            Model = new AIModelRef(AiAliases.ProviderId, options.Value.ChatModel),
            ConnectionId = connectionId,
            // Brand voice applies to every prompt and agent that runs on this profile
            Settings = new AIChatProfileSettings { ContextIds = [brandVoiceId] },
            Tags = ["editorial"],
        };

        AIProfile saved = await profileService.SaveProfileAsync(profile, cancellationToken);
        logger.LogInformation("AI setup: created profile {Alias} ({Model})", saved.Alias, options.Value.ChatModel);
        return saved;
    }

    private async Task EnsurePromptAsync(AiPromptDef definition, Guid profileId, CancellationToken cancellationToken)
    {
        if (await promptService.GetPromptByAliasAsync(definition.Alias, cancellationToken) is not null)
        {
            return;
        }

        var prompt = new AIPrompt
        {
            Alias = definition.Alias,
            Name = definition.Name,
            Description = definition.Description,
            Instructions = definition.Instructions,
            ProfileId = profileId,
            OptionCount = definition.OptionCount,
            IncludeEntityContext = true,
            DisplayMode = definition.IsTiptapTool ? AIPromptDisplayMode.TipTapTool : AIPromptDisplayMode.PropertyAction,
            Scope = new AIPromptScope { AllowRules = [BuildScopeRule(definition)] },
            Tags = ["starter"],
        };

        await promptService.SavePromptAsync(prompt, cancellationToken);
        logger.LogInformation("AI setup: created prompt {Alias}", definition.Alias);
    }

    private static AIPromptScopeRule BuildScopeRule(AiPromptDef definition) => new()
    {
        PropertyAliases = definition.PropertyAliases.Count > 0 ? definition.PropertyAliases : null,
        PropertyEditorUiAliases = definition.EditorUiAliases,
    };

    private async Task EnsureCopilotAgentAsync(Guid profileId, CancellationToken cancellationToken)
    {
        if (await agentService.GetAgentByAliasAsync(AiAliases.CopilotAgent, cancellationToken) is not null)
        {
            return;
        }

        var agent = new AIAgent
        {
            Alias = AiAliases.CopilotAgent,
            Name = AiCopilotDefinition.Name,
            Description = AiCopilotDefinition.Description,
            AgentType = AIAgentType.Standard,
            ProfileId = profileId,
            SurfaceIds = [AiAliases.CopilotSurface],
            Scope = new AIAgentScope { AllowRules = [new AIAgentScopeRule { Sections = AiCopilotDefinition.Sections }] },
            Config = new AIStandardAgentConfig
            {
                Instructions = AiCopilotDefinition.Instructions,
                AllowedToolScopeIds = AiCopilotDefinition.ToolScopes,
            },
            StarterPrompts = AiCopilotDefinition.StarterPrompts.Select(text => new AIStarterPrompt { Prompt = text }).ToArray(),
        };

        await agentService.SaveAgentAsync(agent, cancellationToken);
        logger.LogInformation("AI setup: created Copilot agent {Alias}", AiAliases.CopilotAgent);
    }

    private static JsonElement ToSettings<T>(T value) => JsonSerializer.SerializeToElement(value, SettingsJson);
}
