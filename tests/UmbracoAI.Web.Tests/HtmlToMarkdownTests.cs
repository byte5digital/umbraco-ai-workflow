using UmbracoAI.Web.Geo;

namespace UmbracoAI.Web.Tests;

public sealed class HtmlToMarkdownTests
{
    private static string Absolute(string? url) => url is null ? string.Empty : new Uri(new Uri("https://example.com/"), url).ToString();

    [Fact]
    public void Convert_RendersHeadingsListsAndAbsoluteLinks()
    {
        const string html = "<h2>Titel</h2><p>Text mit <strong>fett</strong> und <a href=\"/kontakt/\">Link</a>.</p><ul><li>Eins</li><li>Zwei</li></ul>";

        string markdown = HtmlToMarkdown.Convert(html, Absolute);

        Assert.Contains("### Titel", markdown);
        Assert.Contains("**fett**", markdown);
        Assert.Contains("[Link](https://example.com/kontakt/)", markdown);
        Assert.Contains("- Eins\n- Zwei", markdown);
    }

    [Fact]
    public void ToPlainText_RemovesMarkupAndDecodesEntities() =>
        Assert.Equal("A & B ist gut", HtmlToMarkdown.ToPlainText("<p>A &amp; B</p> <p>ist <em>gut</em></p>"));
}
