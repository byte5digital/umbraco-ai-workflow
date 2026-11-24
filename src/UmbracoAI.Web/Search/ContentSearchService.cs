using System.Text;
using System.Text.RegularExpressions;
using Examine;
using Examine.Search;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Umbraco.AI.Search.Core;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Cms.Search.Core.Services;
using CmsSearcher = Umbraco.Cms.Search.Core.Services.ISearcher;
using CmsSearchResult = Umbraco.Cms.Search.Core.Models.Searching.SearchResult;

namespace UmbracoAI.Web.Search;

/// <summary>
/// Hybrid content search: semantic vector search (Umbraco.AI.Search, index "UmbAI_Search") finds pages by meaning,
/// Examine full-text search (Umbraco's "ExternalIndex") finds exact terms such as names and codes. Both rankings are
/// merged with Reciprocal Rank Fusion, so a page that ranks well in both lists wins.
/// Returns ranked document keys; turning them into pages is up to the caller (published cache, culture, noindex).
/// </summary>
public sealed partial class ContentSearchService(
    ISearcherResolver searcherResolver,
    IExamineManager examineManager,
    IMemoryCache memoryCache,
    IOptions<SiteSearchOptions> options,
    IConfiguration configuration,
    ILogger<ContentSearchService> logger)
{
    /// <summary>Standard RRF constant – dampens the influence of the very first ranks.</summary>
    private const int RrfK = 60;

    /// <summary>Candidates per source before merging (more than shown, so the fusion has something to work with).</summary>
    private const int CandidatesPerSource = 30;

    private const string CachePrefix = "umbracoai-search:content";

    /// <summary>Culture-specific Examine fields end with "_de-de" / "_en-us".</summary>
    [GeneratedRegex("_[a-z]{2}-[a-z]{2}$", RegexOptions.CultureInvariant)]
    private static partial Regex CultureSuffix();

    public async Task<IReadOnlyList<RankedContent>> SearchAsync(string query, string culture)
    {
        string cacheKey = $"{CachePrefix}:{culture}:{query.ToLowerInvariant()}";
        if (memoryCache.TryGetValue(cacheKey, out IReadOnlyList<RankedContent>? cached) && cached is not null)
        {
            return cached;
        }

        Task<IReadOnlyList<Guid>> semantic = SemanticAsync(query, culture);
        (IReadOnlyList<Guid> fullText, bool isFullTextReady) = FullText(query, culture);
        IReadOnlyList<RankedContent> merged = Fuse(await semantic, fullText);

        // The Examine indexes start empty after a (container) start and are rebuilt by Umbraco shortly after –
        // a ranking from that window must not stick for the whole cache duration.
        if (isFullTextReady)
        {
            memoryCache.Set(cacheKey, merged, TimeSpan.FromSeconds(options.Value.CacheSeconds));
        }

        return merged;
    }

    private async Task<IReadOnlyList<Guid>> SemanticAsync(string query, string culture)
    {
        // Without an API key there is no embedding profile – full-text search only (no error per query in the log).
        if (string.IsNullOrWhiteSpace(configuration[Seeding.Ai.AiAliases.ApiKeyConfigKey]))
        {
            return [];
        }

        CmsSearcher? searcher = searcherResolver.GetSearcher(AISearchConstants.IndexAliases.Search);
        if (searcher is null)
        {
            logger.LogWarning("Site search: no searcher registered for {Index}", AISearchConstants.IndexAliases.Search);
            return [];
        }

        // AIVectorSearcher logs and returns an empty result on provider errors – the full-text part still works.
        CmsSearchResult result = await searcher.SearchAsync(
            AISearchConstants.IndexAliases.Search, query, culture: culture, take: CandidatesPerSource);
        return result.Documents
            .Where(document => document.ObjectType == UmbracoObjectTypes.Document)
            .Select(document => document.Id)
            .ToArray();
    }

    /// <returns>Ranked keys, and whether the index was populated (false = result is not trustworthy, do not cache).</returns>
    private (IReadOnlyList<Guid> Keys, bool IsReady) FullText(string query, string culture)
    {
        if (!examineManager.TryGetIndex(Constants.UmbracoIndexes.ExternalIndexName, out IIndex? index))
        {
            logger.LogWarning("Site search: Examine index {Index} not found", Constants.UmbracoIndexes.ExternalIndexName);
            return ([], false);
        }

        if (index is IIndexStats stats && stats.GetDocumentCount() == 0)
        {
            return ([], false);
        }

        string terms = ToSearchTerms(query);
        if (terms.Length == 0)
        {
            return ([], true);
        }

        try
        {
            ISearchResults results = index.Searcher.CreateQuery(IndexTypes.Content)
                .ManagedQuery(terms, FieldsFor(index, culture))
                .Execute(QueryOptions.SkipTake(0, CandidatesPerSource));

            Guid[] keys = results
                .Select(result => result.Values.TryGetValue(UmbracoExamineFieldNames.NodeKeyFieldName, out string? key)
                    && Guid.TryParse(key, out Guid id) ? id : Guid.Empty)
                .Where(id => id != Guid.Empty)
                .ToArray();
            return (keys, true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Site search: full-text query failed for {Query}", query);
            return ([], false);
        }
    }

    /// <summary>Invariant fields plus the fields of the requested culture – no German hits for English searches.</summary>
    private static string[] FieldsFor(IIndex index, string culture)
    {
        string suffix = "_" + culture.ToLowerInvariant();
        IEnumerable<string> names = index is IIndexStats stats ? stats.GetFieldNames() : [];
        return names
            .Where(name => !name.StartsWith("__", StringComparison.Ordinal))
            .Where(name => name.EndsWith(suffix, StringComparison.Ordinal) || !CultureSuffix().IsMatch(name))
            .ToArray();
    }

    /// <summary>Letters, digits and spaces only – keeps Lucene query syntax (quotes, wildcards, AND/OR) out.</summary>
    internal static string ToSearchTerms(string query)
    {
        var builder = new StringBuilder(query.Length);
        foreach (char c in query)
        {
            builder.Append(char.IsLetterOrDigit(c) ? c : ' ');
        }

        return string.Join(' ', builder.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(word => word is not ("AND" or "OR" or "NOT" or "TO")));
    }

    internal static IReadOnlyList<RankedContent> Fuse(IReadOnlyList<Guid> semantic, IReadOnlyList<Guid> fullText)
    {
        var scores = new Dictionary<Guid, (double Score, bool Semantic, bool FullText)>();
        void Add(IReadOnlyList<Guid> ranking, bool isSemantic)
        {
            for (int rank = 0; rank < ranking.Count; rank++)
            {
                (double score, bool s, bool f) = scores.GetValueOrDefault(ranking[rank]);
                scores[ranking[rank]] = (score + 1.0 / (RrfK + rank + 1), s || isSemantic, f || !isSemantic);
            }
        }

        Add(semantic, isSemantic: true);
        Add(fullText, isSemantic: false);

        return scores
            .OrderByDescending(entry => entry.Value.Score)
            .Select(entry => new RankedContent(entry.Key, entry.Value.Semantic, entry.Value.FullText))
            .ToArray();
    }
}

/// <summary>A content hit before it is resolved to a page; the flags tell which search found it.</summary>
public sealed record RankedContent(Guid Key, bool IsSemanticMatch, bool IsFullTextMatch);
