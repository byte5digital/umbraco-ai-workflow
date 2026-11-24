using Microsoft.Extensions.Options;
using Umbraco.AI.Core.Connections;
using Umbraco.AI.Core.Models;
using Umbraco.AI.Core.Profiles;
using Umbraco.AI.Search.Core;
using Umbraco.AI.Search.Core.Search;
using Umbraco.Cms.Search.Core.Services.ContentIndexing;

namespace Opten.Web.Seeding.Ai;

/// <summary>
/// Semantic search (Umbraco.AI.Search): creates the default embedding profile and fills the vector index
/// "UmbAI_Search" when it is empty. Learning from dus-umbraco: the indexer silently skips content while no embedding
/// profile exists, so content seeded before this step must be re-indexed explicitly.
/// </summary>
public sealed class AiSearchStep(
    IConfiguration configuration,
    IAIConnectionService connectionService,
    IAIProfileService profileService,
    AIVectorIndexer indexer,
    IDistributedContentIndexRebuilder rebuilder,
    IOptions<AiSetupOptions> options,
    ILogger<AiSearchStep> logger) : ISeedStep
{
    public string Id => "umbraco-ai-search";
    public int Version => 1;
    public int Order => 600;

    public async Task<SeedStepResult> ExecuteAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(configuration[AiAliases.ApiKeyConfigKey]))
        {
            return SeedStepResult.Skipped;
        }

        if (await profileService.GetProfileByAliasAsync(AiAliases.EmbeddingProfile, cancellationToken) is null)
        {
            AIConnection? connection = await connectionService.GetConnectionByAliasAsync(AiAliases.Connection, cancellationToken);
            if (connection is null)
            {
                logger.LogWarning("AI search setup skipped: connection {Alias} does not exist yet", AiAliases.Connection);
                return SeedStepResult.Skipped;
            }

            await profileService.SaveProfileAsync(new AIProfile
            {
                Alias = AiAliases.EmbeddingProfile,
                Name = "Suche (Embeddings)",
                Capability = AICapability.Embedding,
                Model = new AIModelRef(AiAliases.ProviderId, options.Value.EmbeddingModel),
                ConnectionId = connection.Id,
                Settings = new AIEmbeddingProfileSettings(),
                Tags = ["search"],
            }, cancellationToken);
            logger.LogInformation("AI search setup: created embedding profile {Alias}", AiAliases.EmbeddingProfile);
        }

        long documentCount = (await indexer.GetMetadataAsync(AISearchConstants.IndexAliases.Search)).DocumentCount;
        if (documentCount == 0)
        {
            bool isStarted = rebuilder.Rebuild(AISearchConstants.IndexAliases.Search);
            logger.LogInformation("AI search setup: vector index is empty – rebuild started: {IsStarted}", isStarted);
        }

        return SeedStepResult.Completed;
    }
}
