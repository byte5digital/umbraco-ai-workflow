using System.Text;
using Opten.Web.Seeding.Schema;
using Opten.Web.Seo;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;
using EditorAliases = Umbraco.Cms.Core.Constants.PropertyEditors.Aliases;

namespace Opten.Web.Geo;

/// <summary>
/// Renders Block Grid content as Markdown – generically by property type, so new modules work without extra code:
/// headline → heading, text/textarea → paragraph, rich text → Markdown, links → Markdown links, block lists → bullets.
/// Layout options (dropdowns, toggles, numbers) and images are skipped.
/// </summary>
public sealed class BlockMarkdownWriter(SiteContext site)
{
    private static readonly HashSet<string> PlainTextEditors = [EditorAliases.TextBox, EditorAliases.TextArea];
    private static readonly string[] ListItemTitleAliases = [Props.Headline, Props.Question];

    private readonly StringBuilder _builder = new();

    public string Write(BlockGridModel? grid)
    {
        foreach (BlockGridItem item in grid ?? Enumerable.Empty<BlockGridItem>())
        {
            WriteGridItem(item, headingLevel: 2);
        }

        return _builder.ToString().Trim();
    }

    private void WriteGridItem(BlockGridItem item, int headingLevel)
    {
        WriteModule(item.Content, headingLevel);
        foreach (BlockGridItem child in item.Areas.SelectMany(area => area))
        {
            WriteGridItem(child, headingLevel + 1);
        }
    }

    private void WriteModule(IPublishedElement element, int headingLevel)
    {
        if (element.Text(Props.Headline) is { } headline)
        {
            Paragraph($"{new string('#', headingLevel)} {headline}");
        }

        foreach (IPublishedProperty property in element.Properties.Where(p => p.Alias != Props.Headline))
        {
            switch (property.GetValue())
            {
                case string text when IsPlainText(property) && !string.IsNullOrWhiteSpace(text):
                    Paragraph(text.Trim());
                    break;
                case IHtmlEncodedString html:
                    Paragraph(HtmlToMarkdown.Convert(html.ToHtmlString(), site.Absolute));
                    break;
                case Link link:
                    Paragraph(LinkMarkdown(link));
                    break;
                case IEnumerable<Link> links:
                    Paragraph(string.Join('\n', links.Select(l => $"- {LinkMarkdown(l)}")));
                    break;
                case BlockListModel list:
                    WriteList(list.Select(item => item.Content), headingLevel + 1);
                    break;
                case IPublishedContent { ItemType: PublishedItemType.Content } page:
                    Paragraph($"→ [{page.Name(site.Culture)}]({site.PageUrl(page)})");
                    break;
            }
        }
    }

    private void WriteList(IEnumerable<IPublishedElement> items, int headingLevel)
    {
        var bullets = new List<string>();
        foreach (IPublishedElement item in items)
        {
            if (item.ContentType.Alias == Elements.FaqItem)
            {
                Paragraph($"{new string('#', headingLevel)} {item.Text(Props.Question)}");
                Paragraph(HtmlToMarkdown.Convert(item.Value<IHtmlEncodedString>(Props.Answer)?.ToHtmlString(), site.Absolute));
                continue;
            }

            bullets.Add(ListItem(item));
        }

        Paragraph(string.Join('\n', bullets.Where(b => b.Length > 2)));
    }

    /// <summary>One bullet: **title** – texts (link). A link named like the title becomes the title link itself.</summary>
    private string ListItem(IPublishedElement item)
    {
        string? title = ListItemTitleAliases.Select(alias => item.Text(alias)).FirstOrDefault(t => t is not null);
        var texts = new List<string>();
        var links = new List<Link>();
        foreach (IPublishedProperty property in item.Properties.Where(p => !ListItemTitleAliases.Contains(p.Alias)))
        {
            switch (property.GetValue())
            {
                case string text when IsPlainText(property) && !string.IsNullOrWhiteSpace(text):
                    texts.Add(text.Trim());
                    break;
                case Link link when !string.IsNullOrEmpty(site.Absolute(link.Url)):
                    links.Add(link);
                    break;
            }
        }

        Link? titleLink = links.FirstOrDefault();
        var parts = new List<string>();
        if (title is not null)
        {
            parts.Add(titleLink is null ? $"**{title}**" : $"**[{title}]({site.Absolute(titleLink.Url)})**");
        }

        parts.AddRange(texts);
        return "- " + string.Join(" – ", parts);
    }

    private string LinkMarkdown(Link link)
    {
        string url = site.Absolute(link.Url);
        string name = string.IsNullOrWhiteSpace(link.Name) ? url : link.Name;
        return string.IsNullOrEmpty(url) ? name : $"[{name}]({url})";
    }

    private void Paragraph(string? text)
    {
        if (!string.IsNullOrWhiteSpace(text))
        {
            _builder.Append(text.Trim()).Append("\n\n");
        }
    }

    private static bool IsPlainText(IPublishedProperty property) => PlainTextEditors.Contains(property.PropertyType.EditorAlias);
}
