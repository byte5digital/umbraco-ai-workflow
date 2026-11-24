using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace UmbracoAI.Web.Geo;

/// <summary>Minimal HTML → Markdown conversion for rich text (headings, paragraphs, lists, links, emphasis).</summary>
public static partial class HtmlToMarkdown
{
    public static string Convert(string? html, Func<string?, string> absoluteUrl)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return string.Empty;
        }

        var document = new HtmlDocument();
        document.LoadHtml(html);
        var builder = new StringBuilder();
        WriteChildren(document.DocumentNode, builder, absoluteUrl, listDepth: 0);
        return MultipleBlankLines().Replace(builder.ToString(), "\n\n").Trim();
    }

    /// <summary>Plain text without markup (e.g. FAQ answers in JSON-LD).</summary>
    public static string ToPlainText(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return string.Empty;
        }

        var document = new HtmlDocument();
        document.LoadHtml(html);
        return Whitespace().Replace(WebUtility.HtmlDecode(document.DocumentNode.InnerText), " ").Trim();
    }

    private static void WriteChildren(HtmlNode node, StringBuilder builder, Func<string?, string> absoluteUrl, int listDepth)
    {
        foreach (HtmlNode child in node.ChildNodes)
        {
            WriteNode(child, builder, absoluteUrl, listDepth);
        }
    }

    private static void WriteNode(HtmlNode node, StringBuilder builder, Func<string?, string> absoluteUrl, int listDepth)
    {
        if (node.NodeType == HtmlNodeType.Text)
        {
            builder.Append(Whitespace().Replace(WebUtility.HtmlDecode(node.InnerText), " "));
            return;
        }

        if (node.NodeType != HtmlNodeType.Element)
        {
            return;
        }

        string name = node.Name.ToLowerInvariant();
        switch (name)
        {
            case "h1" or "h2" or "h3" or "h4" or "h5" or "h6":
                // Rich text headings sit below the module heading (##) → start at ###.
                int level = Math.Min(6, name[1] - '0' + 1);
                builder.Append("\n\n").Append(new string('#', level)).Append(' ')
                    .Append(Inline(node, absoluteUrl)).Append("\n\n");
                break;
            case "p" or "div":
                builder.Append("\n\n");
                WriteChildren(node, builder, absoluteUrl, listDepth);
                builder.Append("\n\n");
                break;
            case "br":
                builder.Append("  \n");
                break;
            case "ul" or "ol":
                WriteList(node, builder, absoluteUrl, listDepth, ordered: name == "ol");
                break;
            case "strong" or "b":
                builder.Append("**").Append(Inline(node, absoluteUrl)).Append("**");
                break;
            case "em" or "i":
                builder.Append('_').Append(Inline(node, absoluteUrl)).Append('_');
                break;
            case "a":
                string text = Inline(node, absoluteUrl);
                string href = absoluteUrl(node.GetAttributeValue("href", string.Empty));
                builder.Append(string.IsNullOrEmpty(href) ? text : $"[{text}]({href})");
                break;
            case "script" or "style" or "img" or "figure":
                break;
            default:
                WriteChildren(node, builder, absoluteUrl, listDepth);
                break;
        }
    }

    private static void WriteList(HtmlNode list, StringBuilder builder, Func<string?, string> absoluteUrl, int depth, bool ordered)
    {
        builder.Append(depth == 0 ? "\n\n" : "\n");
        int index = 1;
        foreach (HtmlNode item in list.ChildNodes.Where(n => n.Name.Equals("li", StringComparison.OrdinalIgnoreCase)))
        {
            var itemBuilder = new StringBuilder();
            WriteChildren(item, itemBuilder, absoluteUrl, depth + 1);
            string marker = ordered ? $"{index++}." : "-";
            builder.Append(new string(' ', depth * 2)).Append(marker).Append(' ')
                .Append(MultipleBlankLines().Replace(itemBuilder.ToString().Trim(), "\n")).Append('\n');
        }

        builder.Append(depth == 0 ? "\n" : string.Empty);
    }

    private static string Inline(HtmlNode node, Func<string?, string> absoluteUrl)
    {
        var builder = new StringBuilder();
        WriteChildren(node, builder, absoluteUrl, listDepth: 0);
        return Whitespace().Replace(builder.ToString(), " ").Trim();
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();

    [GeneratedRegex(@"\n\s*\n(\s*\n)+")]
    private static partial Regex MultipleBlankLines();
}
