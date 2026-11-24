using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;

namespace UmbracoAI.Web.Seeding;

public sealed class SeedOptions
{
    public const string SectionName = "Seed";

    /// <summary>Master switch for all seed steps (schema, AI setup …).</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>Creates the bilingual starter pages. Turn off for an empty site that only has the content model.</summary>
    public bool StarterContent { get; set; } = true;
}

/// <summary>
/// Runs all <see cref="ISeedStep"/>s once Umbraco has started (after the unattended install). A failing step stops the
/// chain – later steps usually depend on it – and is retried on the next start; the site itself keeps running.
/// </summary>
public sealed class SeedRunner(
    IRuntimeState runtimeState,
    IKeyValueService keyValueService,
    IServiceProvider serviceProvider,
    IOptions<SeedOptions> options,
    ILogger<SeedRunner> logger)
    : INotificationAsyncHandler<UmbracoApplicationStartedNotification>
{
    public const string StateKeyPrefix = "Starterkit.Seed.";

    public async Task HandleAsync(UmbracoApplicationStartedNotification notification, CancellationToken cancellationToken)
    {
        if (!options.Value.Enabled || runtimeState.Level != RuntimeLevel.Run)
        {
            return;
        }

        using IServiceScope scope = serviceProvider.CreateScope();
        foreach (ISeedStep step in scope.ServiceProvider.GetServices<ISeedStep>().OrderBy(s => s.Order))
        {
            string stateKey = StateKeyPrefix + step.Id;
            string version = step.Version.ToString(System.Globalization.CultureInfo.InvariantCulture);
            if (keyValueService.GetValue(stateKey) == version)
            {
                continue;
            }

            try
            {
                SeedStepResult result = await step.ExecuteAsync(cancellationToken);
                if (result == SeedStepResult.Completed)
                {
                    keyValueService.SetValue(stateKey, version);
                    logger.LogInformation("Seed step {Step} v{Version} completed", step.Id, version);
                }
                else
                {
                    logger.LogInformation("Seed step {Step} skipped – will retry on next start", step.Id);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Seed step {Step} failed – remaining steps postponed to the next start", step.Id);
                return;
            }
        }
    }
}
