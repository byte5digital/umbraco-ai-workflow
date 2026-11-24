namespace Opten.Web.Seeding.Schema;

/// <summary>
/// All element types. To add a module: define it here, add it to <see cref="Modules"/>, create
/// <c>Views/Partials/blockgrid/Components/{alias}.cshtml</c> and (optionally) seed it in the starter content.
/// </summary>
public static class ElementDefinitions
{
    private static readonly PropertyDef Headline = new(Props.Headline, "Überschrift", DataTypeNames.Textstring);
    private static readonly PropertyDef Image = new(Props.Image, "Bild", DataTypeNames.ImagePicker);
    private static readonly PropertyDef Link = new(Props.Link, "Link / Button", DataTypeNames.SingleLink,
        Description: "Der Linktitel wird als Button-Text verwendet.");

    /// <summary>Items used inside Block Lists – must exist before the list data types.</summary>
    public static readonly IReadOnlyList<ElementDef> ListItems =
    [
        new(Elements.TeaserCard, "Teaser-Karte", "icon-picture",
        [
            Image,
            Headline with { Mandatory = true },
            new(Props.Text, "Text", DataTypeNames.Textarea),
            Link,
        ]),
        new(Elements.FaqItem, "FAQ-Eintrag", "icon-help-alt",
        [
            new(Props.Question, "Frage", DataTypeNames.Textstring, Mandatory: true),
            new(Props.Answer, "Antwort", DataTypeNames.RichText, Mandatory: true),
        ]),
    ];

    public static readonly ElementDef SectionSettings = new(Elements.SectionSettings, "Abschnitts-Einstellungen", "icon-settings",
    [
        new(Props.Background, "Hintergrund", DataTypeNames.Background),
        new(Props.SpacingTop, "Abstand oben", DataTypeNames.TrueFalse),
        new(Props.SpacingBottom, "Abstand unten", DataTypeNames.TrueFalse),
    ]);

    /// <summary>Block Grid modules (top-level content blocks of every page).</summary>
    public static readonly IReadOnlyList<ElementDef> Modules =
    [
        new(Elements.HeroStage, "Bühne", "icon-display",
        [
            Headline with { Mandatory = true },
            new(Props.Text, "Text", DataTypeNames.Textarea),
            Image,
            Link,
        ], Description: "Große Einstiegsbühne mit Bild, Überschrift und Button."),
        new(Elements.RichText, "Fließtext", "icon-script",
        [
            new(Props.Text, "Text", DataTypeNames.RichText, Mandatory: true),
        ]),
        new(Elements.ImageText, "Bild & Text", "icon-picture",
        [
            Headline,
            new(Props.Text, "Text", DataTypeNames.RichText),
            Image with { Mandatory = true },
            new(Props.ImagePosition, "Bildposition", DataTypeNames.ImagePosition),
            Link,
        ]),
        new(Elements.TeaserSection, "Teaser-Bereich", "icon-thumbnails-small",
        [
            Headline,
            new(Props.Items, "Teaser", DataTypeNames.TeaserCardList, Mandatory: true),
        ]),
        new(Elements.FaqAccordion, "FAQ", "icon-help-alt",
        [
            Headline,
            new(Props.Items, "Fragen", DataTypeNames.FaqItemList, Mandatory: true,
                Description: "Wird zusätzlich als schema.org FAQPage ausgegeben (GEO)."),
        ]),
        new(Elements.CtaBanner, "Call-to-Action", "icon-flag",
        [
            Headline with { Mandatory = true },
            new(Props.Text, "Text", DataTypeNames.Textarea),
            Link with { Mandatory = true },
        ]),
        new(Elements.NewsList, "News-Liste", "icon-newspaper",
        [
            Headline,
            new(Props.NewsRoot, "News-Übersicht", DataTypeNames.ContentPicker, Mandatory: true),
            new(Props.Count, "Anzahl", DataTypeNames.Numeric, Description: "Standard: 3"),
        ]),
    ];
}
