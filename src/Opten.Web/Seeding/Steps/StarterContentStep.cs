using Microsoft.Extensions.Options;
using Opten.Web.Seeding.Content;
using Umbraco.Cms.Core.Services;

namespace Opten.Web.Seeding.Steps;

/// <summary>Creates the bilingual starter site – only on an empty content tree, never over existing content.</summary>
public sealed class StarterContentStep(
    IContentService contentService,
    ContentSeeder contentSeeder,
    IOptions<SeedOptions> options,
    ILogger<StarterContentStep> logger) : ISeedStep
{
    public string Id => "starter-content";
    public int Version => 1;
    public int Order => 400;

    public async Task<SeedStepResult> ExecuteAsync(CancellationToken cancellationToken)
    {
        if (!options.Value.StarterContent)
        {
            return SeedStepResult.Skipped;
        }

        if (contentService.GetRootContent().Any())
        {
            logger.LogInformation("Seed: content tree is not empty – starter content not created");
            return SeedStepResult.Completed;
        }

        await contentSeeder.CreateAsync();
        return SeedStepResult.Completed;
    }
}
