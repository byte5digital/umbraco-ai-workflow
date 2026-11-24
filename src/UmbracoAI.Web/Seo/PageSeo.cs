using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;
using UmbracoAI.Web.Seeding.Schema;
using UmbracoAI.Web.Site;

namespace UmbracoAI.Web.Seo;

public sealed record HreflangLink(string Hreflang, string Url);

/// <summary>Everything the &lt;head&gt; needs for SEO, social sharing, structured data and agent discovery.</summary>
public sealed record PageSeo(
    string SiteName,
    string Title,
    string? Description,
    string CanonicalUrl,
    string Robots,
    string OgType,
    string OgTitle,
    string? OgDescription,
    string? OgImageUrl,
    string OgLocale,
    IReadOnlyList<string> OgAlternateLocales,
    IReadOnlyList<HreflangLink> Alternates,
    string? MarkdownUrl,
    string LlmsUrl,
    string JsonLd);

/// <summary>Builds <see cref="PageSeo"/> for the current page (see Views/Partials/site/seoHead.cshtml).</summary>
public sealed class PageSeoService(StructuredDataBuilder structuredData)
{
    public const int ShareImageWidth = 1200;
    public const int ShareImageHeight = 630;
    private const string IndexRobots = "index, follow, max-image-preview:large, max-snippet:-1";

    public PageSeo Build(IPublishedContent page, SiteContext site)
    {
        string culture = site.Culture;
        string url = site.PageUrl(page);
        string title = page.Title(site);
        string? description = page.Description(culture);
        string canonical = ValidCanonical(page.Text(Props.CanonicalUrl, culture)) ?? url;
        MediaWithCrops? image = page.ShareImage(culture);
        string? imageUrl = image is null
            ? null
            : site.Absolute(image.GetCropUrl(width: ShareImageWidth, height: ShareImageHeight, imageCropMode: ImageCropMode.Crop));

        IReadOnlyList<HreflangLink> alternates = Alternates(page, site);
        string[] ogAlternateLocales = alternates
            .Where(a => a.Hreflang != culture && a.Hreflang != "x-default")
            .Select(a => OgLocale(a.Hreflang))
            .ToArray();

        return new PageSeo(
            site.SiteName,
            title,
            description,
            canonical,
            Robots(page, culture),
            page.ContentType.Alias == DocTypes.NewsArticle ? "article" : "website",
            page.Text(Props.OgTitle, culture) ?? title,
            page.Text(Props.OgDescription, culture) ?? description,
            imageUrl,
            OgLocale(culture),
            ogAlternateLocales,
            alternates,
            page.IsHiddenFromAgents(culture) ? null : SiteContext.MarkdownUrlFor(url),
            site.LlmsUrl(full: false),
            structuredData.Build(page, site, canonical, description, imageUrl));
    }

    private static string Robots(IPublishedContent page, string culture)
    {
        bool noFollow = page.Flag(Props.NoFollow, culture);
        return page.IsNoIndex(culture)
            ? $"noindex, {(noFollow ? "nofollow" : "follow")}"
            : noFollow ? "index, nofollow" : IndexRobots;
    }

    private static IReadOnlyList<HreflangLink> Alternates(IPublishedContent page, SiteContext site)
    {
        var links = Cultures.All
            .Where(c => page.IsPublished(c.IsoCode))
            .Select(c => new HreflangLink(c.IsoCode, site.Absolute(page.Url(c.IsoCode))))
            .ToList();
        if (links.FirstOrDefault(l => l.Hreflang == Cultures.Default.IsoCode) is { } fallback)
        {
            links.Add(new HreflangLink("x-default", fallback.Url));
        }

        return links;
    }

    private static string? ValidCanonical(string? value) =>
        Uri.TryCreate(value, UriKind.Absolute, out Uri? uri) && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp)
            ? uri.ToString()
            : null;

    private static string OgLocale(string culture) => culture.Replace('-', '_');
}
