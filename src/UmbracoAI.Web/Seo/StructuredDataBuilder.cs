using System.Text.Json;
using System.Text.Json.Serialization;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;
using UmbracoAI.Web.Geo;
using UmbracoAI.Web.Seeding.Schema;

namespace UmbracoAI.Web.Seo;

/// <summary>
/// schema.org JSON-LD graph per page type (GEO): Organization + WebSite (with SearchAction) on every page, the page
/// itself typed by document type or the "schema.org-Seitentyp" field, BreadcrumbList, Article for news and FAQPage
/// data generated from the FAQ modules. The default encoder escapes "&lt;", so the output is safe inside a script tag.
/// </summary>
public sealed class StructuredDataBuilder
{
    private const string Context = "https://schema.org";

    /// <summary>Default schema.org type per document type – editors can override it in the GEO tab.</summary>
    private static readonly IReadOnlyDictionary<string, string> DefaultPageTypes = new Dictionary<string, string>
    {
        [DocTypes.NewsOverview] = "CollectionPage",
        [DocTypes.SearchPage] = "SearchResultsPage",
    };

    private static readonly JsonSerializerOptions JsonOptions = new() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };

    public string Build(IPublishedContent page, SiteContext site, string url, string? description, string? imageUrl)
    {
        string homeUrl = site.PageUrl(site.Home);
        string organizationId = $"{homeUrl}#organization";
        string websiteId = $"{homeUrl}#website";

        var graph = new List<object>
        {
            Organization(site, organizationId),
            WebSite(site, websiteId, organizationId),
            WebPage(page, site, url, description, imageUrl, websiteId),
        };

        if (!page.IsHome())
        {
            graph.Add(Breadcrumb(page, site, url));
        }

        if (page.ContentType.Alias == DocTypes.NewsArticle)
        {
            graph.Add(Article(page, site.Culture, url, description, imageUrl, organizationId));
        }

        return JsonSerializer.Serialize(new Dictionary<string, object> { ["@context"] = Context, ["@graph"] = graph }, JsonOptions);
    }

    /// <summary>schema.org type of the page: GEO field → document type default → WebPage.</summary>
    public static string PageType(IPublishedContent page) =>
        page.Text(Props.SchemaType) is { } chosen && Options.SchemaTypes.Contains(chosen) ? chosen
        : DefaultPageTypes.GetValueOrDefault(page.ContentType.Alias, "WebPage");

    private static Dictionary<string, object?> WebPage(
        IPublishedContent page, SiteContext site, string url, string? description, string? imageUrl, string websiteId)
    {
        object[]? questions = FaqQuestions(page, site.Culture);
        string type = PageType(page);
        return new Dictionary<string, object?>
        {
            // FAQ modules turn any page into an FAQPage as well, so the answers can be cited by AI search.
            ["@type"] = questions is null || type == "FAQPage" ? type : new[] { type, "FAQPage" },
            ["@id"] = $"{url}#webpage",
            ["url"] = url,
            ["name"] = page.Title(site),
            ["description"] = description,
            ["abstract"] = page.Text(Props.LlmSummary, site.Culture),
            ["inLanguage"] = site.Culture,
            ["isPartOf"] = Ref(websiteId),
            ["dateModified"] = page.UpdateDate.ToString("O"),
            ["primaryImageOfPage"] = imageUrl,
            ["mainEntity"] = questions,
        };
    }

    private static Dictionary<string, object?> Organization(SiteContext site, string id)
    {
        IPublishedContent home = site.Home;
        string[] address = (home.Text(Props.OrganizationAddress) ?? string.Empty)
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        string? logo = home.HasProperty(Props.OrganizationLogo) ? home.Value<MediaWithCrops>(Props.OrganizationLogo)?.Url() : null;

        return new Dictionary<string, object?>
        {
            ["@type"] = "Organization",
            ["@id"] = id,
            ["name"] = site.SiteName,
            ["url"] = site.PageUrl(home),
            ["logo"] = logo is null ? null : site.Absolute(logo),
            ["email"] = home.Text(Props.OrganizationEmail),
            ["telephone"] = home.Text(Props.OrganizationPhone),
            ["address"] = address.Length == 0 ? null : PostalAddress(address),
            ["sameAs"] = SocialLinks(home),
        };
    }

    /// <summary>Address lines: street, "postal code locality", country.</summary>
    private static Dictionary<string, object?> PostalAddress(string[] lines)
    {
        string? cityLine = lines.ElementAtOrDefault(1);
        int space = cityLine?.IndexOf(' ') ?? -1;
        return new Dictionary<string, object?>
        {
            ["@type"] = "PostalAddress",
            ["streetAddress"] = lines[0],
            ["postalCode"] = space > 0 ? cityLine![..space] : null,
            ["addressLocality"] = space > 0 ? cityLine![(space + 1)..] : cityLine,
            ["addressCountry"] = lines.ElementAtOrDefault(2),
        };
    }

    private static Dictionary<string, object?> WebSite(SiteContext site, string id, string organizationId)
    {
        IPublishedContent? searchPage = site.Home.Children(site.Culture)?.FirstOrDefault(c => c.ContentType.Alias == DocTypes.SearchPage);
        return new Dictionary<string, object?>
        {
            ["@type"] = "WebSite",
            ["@id"] = id,
            ["url"] = site.PageUrl(site.Home),
            ["name"] = site.SiteName,
            ["inLanguage"] = site.Culture,
            ["publisher"] = Ref(organizationId),
            ["potentialAction"] = searchPage is null ? null : new Dictionary<string, object?>
            {
                ["@type"] = "SearchAction",
                ["target"] = site.PageUrl(searchPage) + "?q={search_term_string}",
                ["query-input"] = "required name=search_term_string",
            },
        };
    }

    private static Dictionary<string, object?> Breadcrumb(IPublishedContent page, SiteContext site, string url)
    {
        var trail = page.Ancestors().Reverse().ToList();
        object[] items = trail
            .Select((ancestor, index) => ListItem(index + 1, ancestor.Name(site.Culture) ?? string.Empty, site.PageUrl(ancestor)))
            .Append(ListItem(trail.Count + 1, page.Name(site.Culture) ?? string.Empty, url))
            .ToArray();
        return new Dictionary<string, object?> { ["@type"] = "BreadcrumbList", ["itemListElement"] = items };
    }

    private static Dictionary<string, object?> Article(
        IPublishedContent page, string culture, string url, string? description, string? imageUrl, string organizationId)
    {
        DateTime published = page.Value<DateTime>(Props.PublishDate);
        string? author = page.Text(Props.Author);
        return new Dictionary<string, object?>
        {
            ["@type"] = "Article",
            ["@id"] = $"{url}#article",
            ["mainEntityOfPage"] = Ref($"{url}#webpage"),
            ["headline"] = page.Name(culture),
            ["description"] = description,
            ["image"] = imageUrl,
            ["datePublished"] = (published > DateTime.MinValue ? published : page.CreateDate).ToString("O"),
            ["dateModified"] = page.UpdateDate.ToString("O"),
            ["inLanguage"] = culture,
            ["author"] = author is null ? Ref(organizationId) : new Dictionary<string, object?> { ["@type"] = "Person", ["name"] = author },
            ["publisher"] = Ref(organizationId),
        };
    }

    private static object[]? FaqQuestions(IPublishedContent page, string culture)
    {
        object[] questions = page.Grid(culture).AllElements()
            .Where(element => element.ContentType.Alias == Elements.FaqItem)
            .Select(item => (object)new Dictionary<string, object?>
            {
                ["@type"] = "Question",
                ["name"] = item.Value<string>(Props.Question),
                ["acceptedAnswer"] = new Dictionary<string, object?>
                {
                    ["@type"] = "Answer",
                    ["text"] = HtmlToMarkdown.ToPlainText(item.Value<IHtmlEncodedString>(Props.Answer)?.ToHtmlString()),
                },
            })
            .ToArray();
        return questions.Length == 0 ? null : questions;
    }

    private static string[]? SocialLinks(IPublishedContent home)
    {
        string[] urls = (home.HasProperty(Props.SocialLinks) ? home.Value<IEnumerable<Link>>(Props.SocialLinks) ?? [] : [])
            .Select(link => link.Url)
            .Where(u => Uri.TryCreate(u, UriKind.Absolute, out _))
            .OfType<string>()
            .ToArray();
        return urls.Length == 0 ? null : urls;
    }

    private static Dictionary<string, object?> ListItem(int position, string name, string url) => new()
    {
        ["@type"] = "ListItem",
        ["position"] = position,
        ["name"] = name,
        ["item"] = url,
    };

    private static Dictionary<string, object?> Ref(string id) => new() { ["@id"] = id };
}
