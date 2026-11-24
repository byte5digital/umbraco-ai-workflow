namespace Opten.Web.Seeding.Ai;

/// <summary>
/// Umbraco AI setup: connection, brand voice, editorial profile, prompts, Copilot and the AI button in the rich text
/// toolbar. Skipped (and retried on every start) until an OpenAI API key is configured.
/// </summary>
public sealed class AiSetupStep(
    IConfiguration configuration,
    AiRichTextToolbarUpgrader toolbarUpgrader,
    AiSetupSeeder seeder,
    ILogger<AiSetupStep> logger) : ISeedStep
{
    public string Id => "umbraco-ai";
    public int Version => 1;
    public int Order => 500;

    public async Task<SeedStepResult> ExecuteAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(configuration[AiAliases.ApiKeyConfigKey]))
        {
            logger.LogWarning("AI setup skipped: no OpenAI API key configured (Umbraco__AI__Secrets__OpenAIApiKey in .env)");
            return SeedStepResult.Skipped;
        }

        await toolbarUpgrader.UpgradeAsync();
        await seeder.SeedAsync(cancellationToken);
        return SeedStepResult.Completed;
    }
}
