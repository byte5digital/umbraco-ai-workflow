using Opten.Web.Seeding.Schema;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace Opten.Web.Seo;

/// <summary>Culture-aware access to the SEO/GEO fields of a page incl. their fallback chains.</summary>
public static class PageFields
{
    public static string? Text(this IPublishedElement page, string alias, string? culture = null)
    {
        string? value = page.HasProperty(alias) ? page.Value<string>(alias, culture) : null;
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    public static bool Flag(this IPublishedElement page, string alias, string? culture = null) =>
        page.HasProperty(alias) && page.Value<bool>(alias, culture);

    public static bool IsHome(this IPublishedContent page) => page.ContentType.Alias == DocTypes.HomePage;

    public static string Title(this IPublishedContent page, SiteContext site) =>
        page.Text(Props.MetaTitle, site.Culture)
        ?? (page.IsHome() ? site.SiteName : $"{page.Name(site.Culture)} | {site.SiteName}");

    public static string? Description(this IPublishedContent page, string culture) =>
        page.Text(Props.MetaDescription, culture) ?? page.Text(Props.TeaserText, culture);

    /// <summary>Summary for AI agents: KI-Zusammenfassung → Meta-Beschreibung → Teaser-Text.</summary>
    public static string? AgentSummary(this IPublishedContent page, string culture) =>
        page.Text(Props.LlmSummary, culture) ?? page.Description(culture);

    public static IReadOnlyList<string> KeyFacts(this IPublishedContent page, string culture) =>
        (page.Text(Props.LlmKeyFacts, culture) ?? string.Empty)
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    public static bool IsNoIndex(this IPublishedContent page, string culture) => page.Flag(Props.NoIndex, culture);

    /// <summary>"noindex" or "excluded" when the page is hidden from agents, otherwise null.</summary>
    public static string? AgentExclusionReason(this IPublishedContent page, string culture) =>
        page.IsNoIndex(culture) ? "noindex" : page.Flag(Props.LlmsExclude, culture) ? "excluded" : null;

    /// <summary>Pages hidden from llms.txt, Markdown twins and the site search.</summary>
    public static bool IsHiddenFromAgents(this IPublishedContent page, string culture) =>
        page.IsNoIndex(culture) || page.Flag(Props.LlmsExclude, culture);

    public static BlockGridModel? Grid(this IPublishedContent page, string culture) =>
        page.HasProperty(Props.MainContent) ? page.Value<BlockGridModel>(Props.MainContent, culture) : null;

    /// <summary>Image for social sharing: Social-Bild → page image → first image in the Block Grid.</summary>
    public static MediaWithCrops? ShareImage(this IPublishedContent page, string culture)
    {
        static MediaWithCrops? Image(IPublishedElement element, string alias) =>
            element.HasProperty(alias) ? element.Value<MediaWithCrops>(alias) : null;

        return Image(page, Props.OgImage)
            ?? Image(page, Props.Image)
            ?? page.Grid(culture).AllElements().Select(element => Image(element, Props.Image)).FirstOrDefault(m => m is not null);
    }
}
