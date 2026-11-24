using Opten.Web.Seeding.Schema;
using Opten.Web.Site;
using Umbraco.Cms.Core.Models;
using static Opten.Web.Seeding.Content.PropertyValues;

namespace Opten.Web.Seeding.Content;

/// <summary>A text in both site languages.</summary>
public sealed record Bilingual(string German, string English)
{
    public string For(SiteCulture culture) => culture.T(German, English);
}

/// <summary>Shared state while seeding one culture plus factory methods for the Block Grid modules.</summary>
public sealed class SeedContext(
    IReadOnlyDictionary<string, IMedia> media,
    IReadOnlyDictionary<string, Guid> pages,
    BlockValueBuilder blocks,
    SiteCulture culture)
{
    public SiteCulture Culture => culture;

    public string T(string german, string english) => culture.T(german, english);

    /// <summary>Seed image by file name without extension – null when it is not shipped in SeedData/images.</summary>
    public IMedia? Media(string name) => media.GetValueOrDefault(name);

    public LinkDef To(string pageId, string name) =>
        new(name, ContentKey: pages.TryGetValue(pageId, out Guid key) ? key : throw new KeyNotFoundException($"Seed page '{pageId}' not found."));

    public Guid PageKey(string pageId) => pages[pageId];

    // ---- Module factories (one per element type in ElementDefinitions.Modules) ---------------------

    public static Dictionary<string, object?> Settings(string background = "white", bool top = true, bool bottom = true) => new()
    {
        [Props.Background] = Dropdown(background),
        [Props.SpacingTop] = top,
        [Props.SpacingBottom] = bottom,
    };

    public Block Hero(string headline, string? text, string? image, LinkDef? link) =>
        new(Elements.HeroStage, new Dictionary<string, object?>
        {
            [Props.Headline] = headline,
            [Props.Text] = text,
            [Props.Image] = image is null ? null : Image(Media(image)),
            [Props.Link] = link is null ? null : Link(link),
        })
        { Settings = Settings("brand", top: false, bottom: false) };

    public static Block RichTextBlock(string html, string background = "white") =>
        new(Elements.RichText, new Dictionary<string, object?> { [Props.Text] = RichText(html) }) { Settings = Settings(background) };

    /// <summary>Image & text – falls back to plain rich text when the seed image is missing (the image is mandatory).</summary>
    public Block ImageText(string headline, string html, string image, LinkDef? link = null, string position = "left") =>
        Media(image) is { } picture
            ? new(Elements.ImageText, new Dictionary<string, object?>
            {
                [Props.Headline] = headline,
                [Props.Text] = RichText(html),
                [Props.Image] = Image(picture),
                [Props.ImagePosition] = Dropdown(position),
                [Props.Link] = link is null ? null : Link(link),
            })
            { Settings = Settings() }
            : RichTextBlock($"<h2>{headline}</h2>{html}");

    public Block Teasers(string? headline, params (string Headline, string Text, string? Image, LinkDef Link)[] teasers) =>
        new(Elements.TeaserSection, new Dictionary<string, object?>
        {
            [Props.Headline] = headline,
            [Props.Items] = blocks.List(teasers.Select(teaser => new Block(Elements.TeaserCard, new Dictionary<string, object?>
            {
                [Props.Headline] = teaser.Headline,
                [Props.Text] = teaser.Text,
                [Props.Image] = teaser.Image is null ? null : Image(Media(teaser.Image)),
                [Props.Link] = Link(teaser.Link),
            }))),
        })
        { Settings = Settings("muted") };

    public Block Faq(string? headline, params (string Question, string AnswerHtml)[] items) =>
        new(Elements.FaqAccordion, new Dictionary<string, object?>
        {
            [Props.Headline] = headline,
            [Props.Items] = blocks.List(items.Select(item => new Block(Elements.FaqItem, new Dictionary<string, object?>
            {
                [Props.Question] = item.Question,
                [Props.Answer] = RichText(item.AnswerHtml),
            }))),
        })
        { Settings = Settings() };

    public static Block Cta(string headline, string? text, LinkDef link) =>
        new(Elements.CtaBanner, new Dictionary<string, object?>
        {
            [Props.Headline] = headline,
            [Props.Text] = text,
            [Props.Link] = Link(link),
        })
        { Settings = Settings("brand") };

    public Block NewsList(string headline, string newsPageId, int count = 3) =>
        new(Elements.NewsList, new Dictionary<string, object?>
        {
            [Props.Headline] = headline,
            [Props.NewsRoot] = DocumentUdi(PageKey(newsPageId)),
            [Props.Count] = count,
        })
        { Settings = Settings("muted") };
}
