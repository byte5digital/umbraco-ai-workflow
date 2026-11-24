using Microsoft.Extensions.Options;
using Opten.Web.Seeding.Schema;
using Opten.Web.Site;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace Opten.Web.Seo;

/// <summary>The start page in one culture plus the absolute base URL of the current request.</summary>
public sealed record SiteContext(IPublishedContent Home, string Culture, Uri BaseUri, string FallbackSiteName)
{
    public SiteCulture SiteCulture => Cultures.Get(Culture);

    /// <summary>Organisation name from the start page (Website-Einstellungen), else the configured site name.</summary>
    public string SiteName => Home.Text(Props.OrganizationName) ?? FallbackSiteName;

    public string Absolute(string? url) =>
        string.IsNullOrEmpty(url) || url == "#" ? string.Empty
        // On Linux "/path" parses as an absolute file:// URI – only accept http(s) as already absolute (dus-umbraco learning).
        : Uri.TryCreate(url, UriKind.Absolute, out Uri? absolute) && absolute.Scheme.StartsWith("http", StringComparison.Ordinal)
            ? absolute.ToString()
            : new Uri(BaseUri, url).ToString();

    public string PageUrl(IPublishedContent page) => Absolute(page.Url(Culture));

    /// <summary>Markdown twin of a page (/de/ueber-uns/ → /de/ueber-uns.md) for AI agents.</summary>
    public string MarkdownUrl(IPublishedContent page) => MarkdownUrlFor(PageUrl(page));

    public static string MarkdownUrlFor(string pageUrl) =>
        string.IsNullOrEmpty(pageUrl) ? string.Empty : pageUrl.TrimEnd('/') + ".md";

    public string LlmsUrl(bool full) => Absolute($"{SiteCulture.DomainPath}/{(full ? "llms-full.txt" : "llms.txt")}");

    /// <summary>Published pages of the site in this culture (start page first, tree order).</summary>
    public IEnumerable<IPublishedContent> Pages() =>
        Home.DescendantsOrSelf(Culture).Where(page => page.IsPublished(Culture));
}

/// <summary>Resolves the published start page outside of Umbraco's page routing (endpoints, APIs).</summary>
public sealed class SiteResolver(
    IUmbracoContextFactory umbracoContextFactory,
    IDocumentNavigationQueryService navigation,
    IPublishedContentCache contentCache,
    IVariationContextAccessor variationContextAccessor,
    IOptions<SiteOptions> siteOptions)
{
    /// <summary>Culture by URL prefix ("de" / "en"); null/empty = default culture; unknown = null.</summary>
    public static string? CultureFromPath(string? culturePath) =>
        string.IsNullOrEmpty(culturePath)
            ? Cultures.Default.IsoCode
            : Cultures.All.FirstOrDefault(c => c.DomainPath.Equals($"/{culturePath}", StringComparison.OrdinalIgnoreCase))?.IsoCode;

    public SiteContext? Resolve(HttpRequest request, string culture)
    {
        // Endpoints outside the page pipeline need an Umbraco context and the culture for value converters (links).
        request.HttpContext.Response.RegisterForDispose(umbracoContextFactory.EnsureUmbracoContext());
        variationContextAccessor.VariationContext = new VariationContext(culture);

        IPublishedContent? home = navigation.TryGetRootKeys(out IEnumerable<Guid> rootKeys)
            ? rootKeys.Select(key => contentCache.GetById(key)).FirstOrDefault(c => c?.IsHome() == true)
            : null;

        return home is not null && home.IsPublished(culture) ? For(home, request, culture) : null;
    }

    /// <summary>Loads a page incl. its latest saved (unpublished) version for backoffice previews, plus its site.</summary>
    public (IPublishedContent Page, SiteContext Site)? ResolveDraft(HttpRequest request, Guid key, string culture)
    {
        request.HttpContext.Response.RegisterForDispose(umbracoContextFactory.EnsureUmbracoContext());
        variationContextAccessor.VariationContext = new VariationContext(culture);

        IPublishedContent? page = contentCache.GetById(true, key);
        IPublishedContent? home = page?.AncestorOrSelf(1);
        return page is null || home is null ? null : (page, For(home, request, culture));
    }

    /// <summary>Site of a page rendered by Umbraco (views).</summary>
    public SiteContext For(IPublishedContent home, HttpRequest request, string culture) =>
        new(home, culture, new Uri($"{request.Scheme}://{request.Host}/"), siteOptions.Value.Name);
}
