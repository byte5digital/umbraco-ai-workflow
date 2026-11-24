using Microsoft.Extensions.Options;
using Opten.Web.Seeding.Schema;
using Opten.Web.Seo;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Extensions;

namespace Opten.Web.Search;

/// <param name="MatchedBy">"semantic" and/or "text" – which search found the page.</param>
public sealed record SiteSearchHit(Guid Id, string Title, string? Description, string Url, string Type, IReadOnlyList<string> MatchedBy);

public sealed record SiteSearchResponse(string Query, string Culture, IReadOnlyList<SiteSearchHit> Results);

/// <summary>
/// Turns the ranked keys of <see cref="ContentSearchService"/> into visible pages: published in the culture,
/// part of the site, not noindex/excluded and not the search page itself. Pages are always read from the
/// published cache, so unpublished pages disappear immediately even while the ranking is cached.
/// </summary>
public sealed class SiteSearchService(
    ContentSearchService contentSearch,
    IPublishedContentCache contentCache,
    IOptions<SiteSearchOptions> options)
{
    public async Task<SiteSearchResponse> SearchAsync(string query, SiteContext site)
    {
        IReadOnlyList<RankedContent> ranked = await contentSearch.SearchAsync(query, site.Culture);
        SiteSearchHit[] hits = ranked
            .Select(hit => (Hit: hit, Page: contentCache.GetById(hit.Key)))
            .Where(x => x.Page is not null && IsVisible(x.Page, site))
            .Take(options.Value.MaxContentResults)
            .Select(x => ToHit(x.Page!, x.Hit, site))
            .ToArray();
        return new SiteSearchResponse(query, site.Culture, hits);
    }

    private static bool IsVisible(IPublishedContent page, SiteContext site) =>
        page.IsPublished(site.Culture)
        && page.Path.Split(',').Contains(site.Home.Id.ToString(System.Globalization.CultureInfo.InvariantCulture))
        && page.ContentType.Alias != DocTypes.SearchPage
        && !page.IsHiddenFromAgents(site.Culture);

    private static SiteSearchHit ToHit(IPublishedContent page, RankedContent hit, SiteContext site)
    {
        var matchedBy = new List<string>(2);
        if (hit.IsSemanticMatch)
        {
            matchedBy.Add("semantic");
        }

        if (hit.IsFullTextMatch)
        {
            matchedBy.Add("text");
        }

        return new SiteSearchHit(
            page.Key,
            page.Name(site.Culture) ?? string.Empty,
            page.AgentSummary(site.Culture),
            site.PageUrl(page),
            page.ContentType.Alias == DocTypes.NewsArticle ? "news" : "page",
            matchedBy);
    }
}
