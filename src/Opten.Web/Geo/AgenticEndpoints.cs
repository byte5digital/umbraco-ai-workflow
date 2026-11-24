using Opten.Web.Seeding.Schema;
using Opten.Web.Seo;
using Opten.Web.Site;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;

namespace Opten.Web.Geo;

/// <summary>
/// Crawler and agent endpoints: /robots.txt, /sitemap.xml, /llms.txt, /llms-full.txt (default culture and per
/// language prefix, e.g. /en/llms.txt), the Markdown twin of every page (/de/ueber-uns.md) and the "/" redirect
/// to the default culture.
/// </summary>
public static class AgenticEndpoints
{
    private const string MarkdownContentType = "text/markdown; charset=utf-8";
    private const string TextContentType = "text/plain; charset=utf-8";
    private const string MarkdownSuffix = ".md";

    public static void MapAgenticEndpoints(this IEndpointRouteBuilder endpoints)
    {
        // Pages live below the culture domains (/de, /en) – the bare host redirects to the default language.
        endpoints.MapGet("/", () => Results.Redirect(Cultures.Default.DomainPath + "/"));

        endpoints.MapGet("/robots.txt", (HttpContext context, SiteResolver resolver) =>
            resolver.Resolve(context.Request, Cultures.Default.IsoCode) is { } site
                ? Results.Text(CrawlerFiles.Robots(site.SiteName, site.Home.Flag(Props.BlockAiCrawlers),
                    site.Absolute("/sitemap.xml"), site.LlmsUrl(full: false)), TextContentType)
                : Results.NotFound());

        endpoints.MapGet("/sitemap.xml", (HttpContext context, SiteResolver resolver) =>
        {
            SiteContext[] sites = Cultures.All
                .Select(culture => resolver.Resolve(context.Request, culture.IsoCode))
                .OfType<SiteContext>()
                .ToArray();
            return sites.Length == 0 ? Results.NotFound() : Results.Text(CrawlerFiles.Sitemap(sites), "application/xml; charset=utf-8");
        });

        MapLlms(endpoints, "/llms.txt", full: false);
        MapLlms(endpoints, "/llms-full.txt", full: true);
        MapLlms(endpoints, "/{culturePath}/llms.txt", full: false);
        MapLlms(endpoints, "/{culturePath}/llms-full.txt", full: true);

        endpoints.MapGet("/{**path:regex(\\.md$)}", PageMarkdown);
    }

    private static void MapLlms(IEndpointRouteBuilder endpoints, string pattern, bool full) =>
        endpoints.MapGet(pattern, (HttpContext context, SiteResolver resolver, AgentMarkdownService markdown, string? culturePath) =>
        {
            string? culture = SiteResolver.CultureFromPath(culturePath);
            if (culture is null || resolver.Resolve(context.Request, culture) is not { } site)
            {
                return Results.NotFound();
            }

            return Results.Text(full ? markdown.BuildFull(site) : markdown.BuildIndex(site), TextContentType);
        });

    private static IResult PageMarkdown(
        HttpContext context, string path, SiteResolver resolver, AgentMarkdownService markdown, IDocumentUrlService documentUrlService)
    {
        string pagePath = "/" + path[..^MarkdownSuffix.Length].Trim('/');
        string? culture = CultureOf(pagePath);
        var pageUri = new Uri($"{context.Request.Scheme}://{context.Request.Host}{pagePath}/");
        Guid? key = documentUrlService.GetDocumentKeyByUri(pageUri, false);
        if (key is null || culture is null || resolver.Resolve(context.Request, culture) is not { } site)
        {
            return Results.NotFound();
        }

        IPublishedContent? page = site.Pages().FirstOrDefault(p => p.Key == key);
        if (page is null || page.IsHiddenFromAgents(culture))
        {
            return Results.NotFound();
        }

        // The HTML page stays the canonical, indexable version.
        context.Response.Headers.Append("Link", $"<{site.PageUrl(page)}>; rel=\"canonical\"");
        context.Response.Headers.Append("X-Robots-Tag", "noindex");
        return Results.Text(markdown.BuildPage(page, site), MarkdownContentType);
    }

    /// <summary>Culture by the first path segment (/de/…, /en/…).</summary>
    private static string? CultureOf(string pagePath) =>
        Cultures.All.FirstOrDefault(c =>
            pagePath.Equals(c.DomainPath, StringComparison.OrdinalIgnoreCase)
            || pagePath.StartsWith(c.DomainPath + "/", StringComparison.OrdinalIgnoreCase))?.IsoCode;
}
