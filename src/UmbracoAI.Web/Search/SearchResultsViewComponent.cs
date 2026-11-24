using Microsoft.AspNetCore.Mvc;
using UmbracoAI.Web.Seo;

namespace UmbracoAI.Web.Search;

public sealed record SearchResultsModel(string Query, bool IsTooShort, bool IsUnavailable, SiteSearchResponse? Response);

/// <summary>Renders the results on the search page (Views/Shared/Components/SearchResults/Default.cshtml).</summary>
public sealed class SearchResultsViewComponent(SiteSearchService search, ILogger<SearchResultsViewComponent> logger) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync(SiteContext site, string? query)
    {
        string normalized = SiteSearchOptions.Normalize(query);
        if (normalized.Length == 0)
        {
            return View(new SearchResultsModel(normalized, false, false, null));
        }

        if (normalized.Length < SiteSearchOptions.MinQueryLength)
        {
            return View(new SearchResultsModel(normalized, true, false, null));
        }

        try
        {
            return View(new SearchResultsModel(normalized, false, false, await search.SearchAsync(normalized, site)));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Site search failed for {Query}", normalized);
            return View(new SearchResultsModel(normalized, false, true, null));
        }
    }
}
