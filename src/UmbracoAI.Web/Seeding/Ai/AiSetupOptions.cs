namespace UmbracoAI.Web.Seeding.Ai;

/// <summary>Settings of the Umbraco AI setup (section "AiSetup").</summary>
public sealed class AiSetupOptions
{
    public const string SectionName = "AiSetup";

    /// <summary>OpenAI chat model of the editorial profile – changeable later in the backoffice (AI › Profiles).</summary>
    public string ChatModel { get; set; } = "gpt-6.2-sol";

    /// <summary>
    /// Embedding model of the semantic search. 1536 dimensions stay below the SQL Server 2025 vector limit (1998),
    /// so native VECTOR_DISTANCE() can be used later.
    /// </summary>
    public string EmbeddingModel { get; set; } = "text-embedding-3-small";
}

/// <summary>Aliases and config keys of everything the AI setup creates. Aliases are the idempotency keys.</summary>
public static class AiAliases
{
    public const string ProviderId = "openai";
    public const string Connection = "starter-openai";
    public const string ChatProfile = "starter-editorial";

    /// <summary>Default embedding profile – referenced by "Umbraco:AI:DefaultEmbeddingProfileAlias" in appsettings.json.</summary>
    public const string EmbeddingProfile = "starter-embedding";
    public const string BrandVoiceContext = "starter-brand-voice";
    public const string CopilotAgent = "starter-copilot";
    public const string CopilotSurface = "copilot";

    /// <summary>Config key of the API key – only resolvable because it lives under the allowed "Umbraco:AI:Secrets" prefix.</summary>
    public const string ApiKeyConfigKey = "Umbraco:AI:Secrets:OpenAIApiKey";

    /// <summary>The connection stores this reference instead of the key, so the secret never reaches the database.</summary>
    public const string ApiKeyReference = "$" + ApiKeyConfigKey;

    // Rich text editor extension contributed by Umbraco.AI.Prompt (prompts in the Tiptap toolbar)
    public const string TiptapExtension = "UmbracoAI.Tiptap.Prompts";
    public const string TiptapToolbarItem = "UmbracoAI.Tiptap.Toolbar.Prompts";
}
