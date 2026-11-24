namespace Opten.Web.Seeding.Schema;

/// <summary>Tabs of all page types and the SEO / Social / GEO groups of the compositions.</summary>
public static class SeoGeoDefinitions
{
    public static readonly TabDef ContentTab = new("content", "Inhalt", 0);
    public static readonly TabDef SeoTab = new("seo", "SEO", 10);
    public static readonly TabDef SocialTab = new("social", "Social Media", 20);
    public static readonly TabDef GeoTab = new("geo", "GEO & KI", 30);
    public static readonly TabDef SiteTab = new("site", "Website-Einstellungen", 40);

    /// <summary>Groups of the "SEO (Komposition)" document type (SEO + Social Media tabs).</summary>
    public static readonly IReadOnlyList<GroupDef> SeoGroups =
    [
        new(SeoTab, "serp", "Suchergebnis (Google)", 0,
        [
            new(Props.MetaTitle, "Meta-Titel", DataTypeNames.Textstring, VariesByCulture: true,
                Description: "Ideal 30–60 Zeichen. Leer = Seitenname | Website-Name."),
            new(Props.MetaDescription, "Meta-Beschreibung", DataTypeNames.Textarea, VariesByCulture: true,
                Description: "Ideal 120–155 Zeichen. Wird auch für Social Media und llms.txt genutzt."),
            new(Props.FocusKeyword, "Fokus-Keyword", DataTypeNames.Textstring, VariesByCulture: true,
                Description: "Wird von den KI-Prompts für Titel und Beschreibung verwendet."),
        ]),
        new(SeoTab, "indexing", "Indexierung & Sitemap", 1,
        [
            new(Props.CanonicalUrl, "Canonical-URL", DataTypeNames.Textstring, VariesByCulture: true,
                Description: "Nur bei Duplicate Content setzen (absolute URL). Leer = eigene URL."),
            new(Props.NoIndex, "Nicht indexieren (noindex)", DataTypeNames.TrueFalse, VariesByCulture: true,
                Description: "Seite erscheint nicht in Suchmaschinen, Sitemap, llms.txt und der Website-Suche."),
            new(Props.NoFollow, "Links nicht folgen (nofollow)", DataTypeNames.TrueFalse, VariesByCulture: true),
            new(Props.SitemapExclude, "Aus XML-Sitemap ausschließen", DataTypeNames.TrueFalse),
            new(Props.SitemapChangeFrequency, "Änderungshäufigkeit", DataTypeNames.SitemapChangeFrequency,
                Description: "Standard: weekly"),
            new(Props.SitemapPriority, "Priorität", DataTypeNames.SitemapPriority,
                Description: "Standard: 1.0 Startseite, 0.8 Hauptseiten, 0.6 Unterseiten"),
        ]),
        new(SocialTab, "openGraph", "Open Graph / X (Twitter)", 0,
        [
            new(Props.OgTitle, "Social-Titel", DataTypeNames.Textstring, VariesByCulture: true, Description: "Leer = Meta-Titel"),
            new(Props.OgDescription, "Social-Beschreibung", DataTypeNames.Textarea, VariesByCulture: true,
                Description: "Leer = Meta-Beschreibung"),
            new(Props.OgImage, "Social-Bild", DataTypeNames.ImagePicker,
                Description: "Empfohlen 1200 × 630 px. Leer = Seitenbild bzw. erstes Bild der Inhaltsmodule."),
        ]),
    ];

    /// <summary>Groups of the "GEO & KI (Komposition)" document type – Generative Engine Optimization per page.</summary>
    public static readonly IReadOnlyList<GroupDef> GeoGroups =
    [
        new(GeoTab, "summary", "Inhalte für KI-Assistenten & Agenten", 0,
        [
            new(Props.LlmSummary, "KI-Zusammenfassung", DataTypeNames.Textarea, VariesByCulture: true,
                Description: "1–3 sachliche Sätze: Worum geht es auf der Seite? Erscheint in llms.txt und als Einleitung " +
                             "der Markdown-Version. Leer = Meta-Beschreibung."),
            new(Props.LlmKeyFacts, "Kernfakten", DataTypeNames.Textarea, VariesByCulture: true,
                Description: "Eine zitierfähige Aussage pro Zeile (Zahlen, Öffnungszeiten, Preise, Fristen …)."),
            new(Props.SchemaType, "schema.org-Seitentyp", DataTypeNames.SchemaType,
                Description: "Leer = automatisch (WebPage, Article, SearchResultsPage …)."),
        ]),
        new(GeoTab, "control", "Steuerung", 1,
        [
            new(Props.LlmsExclude, "Aus llms.txt ausschließen", DataTypeNames.TrueFalse),
            new(Props.LlmsOptional, "In llms.txt als „Optional“ führen", DataTypeNames.TrueFalse,
                Description: "Für Agenten mit wenig Kontext verzichtbare Seiten."),
        ]),
        new(GeoTab, "preview", "Vorschau", 2,
        [
            new(Props.LlmsPreview, "llms.txt & Markdown", DataTypeNames.LlmsPreview, VariesByCulture: true,
                Description: "So sehen KI-Agenten diese Seite (Markdown: zuletzt gespeicherter Stand)."),
        ]),
    ];

    /// <summary>Site-wide GEO settings – only on the start page.</summary>
    public static readonly GroupDef SiteGeoGroup = new(GeoTab, "site", "Website-weit (llms.txt & robots.txt)", 3,
    [
        new(Props.LlmsIntro, "llms.txt-Einleitung", DataTypeNames.Textarea, VariesByCulture: true,
            Description: "Kurzbeschreibung der gesamten Website als Blockzitat oben in /llms.txt."),
        new(Props.BlockAiCrawlers, "KI-Trainings-Crawler blockieren", DataTypeNames.TrueFalse,
            Description: "Sperrt GPTBot, ClaudeBot, Google-Extended, CCBot & Co. in robots.txt. " +
                         "KI-Suche (OAI-SearchBot, PerplexityBot, Claude-SearchBot) bleibt erlaubt."),
    ]);
}
