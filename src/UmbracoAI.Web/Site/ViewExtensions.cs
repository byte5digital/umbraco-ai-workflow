using System.Globalization;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common;
using Umbraco.Extensions;
using UmbracoAI.Web.Seeding.Schema;
using UmbracoAI.Web.Seo;

namespace UmbracoAI.Web.Site;

/// <summary>Helpers shared by the Razor views. Tailwind scans this file, so classes used here are generated.</summary>
public static class ViewExtensions
{
    /// <summary>Culture of the current request (set by Umbraco from the matched domain).</summary>
    public static string CurrentCulture() => Cultures.Get(CultureInfo.CurrentCulture.Name).IsoCode;

    /// <summary>Dictionary item in the current culture with the seeded text as fallback.</summary>
    public static string Ui(this UmbracoHelper umbraco, string key) =>
        umbraco.GetDictionaryValueOrDefault(key, UiTexts.Default(key, CurrentCulture()));

    /// <summary>Tailwind classes of a module section from its settings (background + spacing).</summary>
    public static string SectionClasses(this BlockGridItem item)
    {
        string background = item.Settings?.Value<string>(Props.Background) switch
        {
            "muted" => "bg-slate-100",
            "brand" => "bg-brand-800 text-white",
            _ => "bg-white",
        };
        bool top = item.Settings?.Value<bool>(Props.SpacingTop) ?? true;
        bool bottom = item.Settings?.Value<bool>(Props.SpacingBottom) ?? true;
        return $"{background} {(top ? "pt-12 md:pt-16" : string.Empty)} {(bottom ? "pb-12 md:pb-16" : string.Empty)}".Trim();
    }

    public static string? ImageUrl(this IPublishedElement element, string alias, int width, int? height = null) =>
        element.HasProperty(alias)
            ? element.Value<MediaWithCrops>(alias)?.GetCropUrl(width: width, height: height, imageCropMode: ImageCropMode.Crop)
            : null;

    /// <summary>Main navigation: editor-defined links, else all visible first-level pages.</summary>
    public static IReadOnlyList<(string Name, string Url, bool IsActive)> Navigation(this IPublishedContent home, IPublishedContent current, string culture)
    {
        IEnumerable<Link>? links = home.Value<IEnumerable<Link>>(Props.MainNavigation, culture);
        if (links?.Any() == true)
        {
            string currentUrl = current.Url(culture);
            return links
                .Select(l => (l.Name ?? l.Url ?? string.Empty, l.Url ?? "#",
                    !string.IsNullOrEmpty(l.Url) && l.Url != "/" && currentUrl.StartsWith(l.Url, StringComparison.OrdinalIgnoreCase)))
                .ToArray();
        }

        return (home.Children(culture) ?? [])
            .Where(page => page.IsPublished(culture) && !page.Value<bool>(Props.HideFromNavigation))
            .Select(page => (page.Name(culture) ?? string.Empty, page.Url(culture), current.Path.Split(',').Contains(page.Id.ToString(CultureInfo.InvariantCulture))))
            .ToArray();
    }
}
