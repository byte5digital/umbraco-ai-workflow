using System.Text;
using Opten.Web.Seeding.Schema;
using Opten.Web.Seo;
using Opten.Web.Site;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace Opten.Web.Geo;

/// <summary>
/// Agent-facing views of the site following the llms.txt proposal (https://llmstxt.org):
/// /llms.txt (index), /llms-full.txt (all content) and a Markdown twin of every page (URL + ".md").
/// </summary>
public sealed class AgentMarkdownService
{
    private const string DateFormat = "yyyy-MM-dd";

    /// <summary>The llms.txt index: title, summary, key facts and one linked entry per page grouped by section.</summary>
    public string BuildIndex(SiteContext site)
    {
        AgentTexts texts = AgentTexts.For(site.Culture);
        string[] sectionOrder = [texts.MainPages, texts.MorePages, texts.News, texts.Optional];
        var builder = new StringBuilder();
        WriteSiteHeader(builder, site);

        foreach (IGrouping<string, IPublishedContent> section in AgentPages(site)
                     .Where(page => !page.IsHome())
                     .GroupBy(page => SectionOf(page, site, texts))
                     .OrderBy(group => Array.IndexOf(sectionOrder, group.Key)))
        {
            builder.Append("## ").Append(section.Key).Append("\n\n");
            foreach (IPublishedContent page in section)
            {
                builder.Append(IndexEntry(page, site)).Append('\n');
            }

            builder.Append('\n');
        }

        return builder.ToString().TrimEnd() + "\n";
    }

    /// <summary>llms-full.txt: the site header followed by the Markdown of every page.</summary>
    public string BuildFull(SiteContext site)
    {
        var builder = new StringBuilder();
        WriteSiteHeader(builder, site);
        foreach (IPublishedContent page in AgentPages(site))
        {
            builder.Append("\n---\n\n").Append(BuildPage(page, site));
        }

        return builder.ToString().TrimEnd() + "\n";
    }

    /// <summary>Markdown twin of a single page – also the source text of the AI answers and the Copilot.</summary>
    public string BuildPage(IPublishedContent page, SiteContext site)
    {
        AgentTexts texts = AgentTexts.For(site.Culture);
        var builder = new StringBuilder();
        builder.Append("# ").Append(page.Name(site.Culture)).Append("\n\n");

        if (page.AgentSummary(site.Culture) is { } summary)
        {
            builder.Append("> ").Append(summary).Append("\n\n");
        }

        if (page.HasProperty(Props.PublishDate) && page.Value<DateTime>(Props.PublishDate) is var published && published > DateTime.MinValue)
        {
            builder.Append(texts.Published).Append(": ").Append(published.ToString(DateFormat)).Append("\n\n");
        }

        IReadOnlyList<string> facts = page.KeyFacts(site.Culture);
        if (facts.Count > 0)
        {
            builder.Append("## ").Append(texts.KeyFacts).Append("\n\n");
            builder.AppendJoin('\n', facts.Select(fact => $"- {fact}")).Append("\n\n");
        }

        string body = new BlockMarkdownWriter(site).Write(page.Grid(site.Culture));
        if (body.Length > 0)
        {
            builder.Append(body).Append("\n\n");
        }

        WriteChildren(builder, page, site, texts);

        builder.Append("---\n\n")
            .Append(texts.Source).Append(": ").Append(site.PageUrl(page)).Append("  \n")
            .Append(texts.Updated).Append(": ").Append(page.UpdateDate.ToString(DateFormat)).Append('\n');
        return builder.ToString();
    }

    /// <summary>One llms.txt line: "- [Name](url.md): summary".</summary>
    public static string IndexEntry(IPublishedContent page, SiteContext site)
    {
        string entry = $"- [{page.Name(site.Culture)}]({site.MarkdownUrl(page)})";
        return page.AgentSummary(site.Culture) is { } summary ? $"{entry}: {summary}" : entry;
    }

    /// <summary>Pages visible to agents: published in the culture, not noindex, not excluded.</summary>
    public static IEnumerable<IPublishedContent> AgentPages(SiteContext site) =>
        site.Pages().Where(page => !page.IsHiddenFromAgents(site.Culture));

    private static void WriteSiteHeader(StringBuilder builder, SiteContext site)
    {
        AgentTexts texts = AgentTexts.For(site.Culture);
        IPublishedContent home = site.Home;
        builder.Append("# ").Append(site.SiteName).Append("\n\n");

        if ((home.Text(Props.LlmsIntro, site.Culture) ?? home.AgentSummary(site.Culture)) is { } intro)
        {
            builder.Append("> ").Append(intro).Append("\n\n");
        }

        IReadOnlyList<string> facts = home.KeyFacts(site.Culture);
        if (facts.Count > 0)
        {
            builder.AppendJoin('\n', facts.Select(fact => $"- {fact}")).Append("\n\n");
        }

        builder.Append(texts.MarkdownHint).Append(' ').Append($"[{texts.FullContent}]({site.LlmsUrl(full: true)})");
        foreach (SiteCulture other in Cultures.All.Where(c => c.IsoCode != site.Culture && home.IsPublished(c.IsoCode)))
        {
            builder.Append(" · ").Append($"[{texts.OtherLanguage}]({site.Absolute($"{other.DomainPath}/llms.txt")})");
        }

        builder.Append(" · ").Append($"[{home.Name(site.Culture)}]({site.MarkdownUrl(home)})").Append("\n\n");
    }

    private static void WriteChildren(StringBuilder builder, IPublishedContent page, SiteContext site, AgentTexts texts)
    {
        IPublishedContent[] children = page.Children(site.Culture)?
            .Where(child => child.IsPublished(site.Culture) && !child.IsHiddenFromAgents(site.Culture))
            .ToArray() ?? [];
        if (children.Length == 0 || page.IsHome())
        {
            return;
        }

        builder.Append("## ").Append(texts.SubPages).Append("\n\n");
        builder.AppendJoin('\n', children.Select(child => IndexEntry(child, site))).Append("\n\n");
    }

    private static string SectionOf(IPublishedContent page, SiteContext site, AgentTexts texts) =>
        page.Flag(Props.LlmsOptional, site.Culture) ? texts.Optional
        : page.ContentType.Alias == DocTypes.NewsArticle ? texts.News
        : page.Level <= 2 ? texts.MainPages
        : texts.MorePages;
}
