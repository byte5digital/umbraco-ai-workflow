using Opten.Web.Seeding.Schema;

namespace Opten.Web.Seeding.Ai;

/// <param name="PropertyAliases">Fields the action is offered on; empty = scoped by <paramref name="EditorUiAliases"/> only.</param>
/// <param name="OptionCount">1 = single value, 2+ = editor picks one of several suggestions.</param>
public sealed record AiPromptDef(
    string Alias,
    string Name,
    string Description,
    string Instructions,
    IReadOnlyList<string> PropertyAliases,
    int OptionCount = 1,
    bool IsTiptapTool = false,
    IReadOnlyList<string>? EditorUiAliases = null);

/// <summary>
/// Editorial prompt actions: SEO, GEO, copywriting and translation.
/// Placeholders: {{currentValue}}, {{name}} (page name), {{culture}}, {{selection}} (RTE selection), {{focusKeyword}}.
/// </summary>
public static class AiPromptDefinitions
{
    private const string LanguageRule =
        "Antworte in der Sprache der bearbeiteten Sprachvariante ({{culture}}; de = Deutsch, en = Englisch). " +
        "Gib nur den fertigen Text zurück – ohne Anführungszeichen, Erklärungen oder Markdown.";

    private const string RichTextEditorUi = "Umb.PropertyEditorUi.Tiptap";

    public static readonly IReadOnlyList<AiPromptDef> All =
    [
        new("starter-seo-meta-title", "SEO: Meta-Titel vorschlagen",
            "Drei Meta-Titel (30–60 Zeichen) auf Basis von Seiteninhalt und Fokus-Keyword.",
            "Schreibe einen Meta-Titel für die Seite „{{name}}“. Länge 30–60 Zeichen inklusive Leerzeichen. " +
            "Fokus-Keyword möglichst weit vorne: „{{focusKeyword}}“. Jede Variante mit eigenem Schwerpunkt. " + LanguageRule,
            [Props.MetaTitle, Props.OgTitle], OptionCount: 3),

        new("starter-seo-meta-description", "SEO: Meta-Beschreibung vorschlagen",
            "Drei Meta-Beschreibungen (120–155 Zeichen) mit Handlungsaufforderung.",
            "Schreibe eine Meta-Beschreibung für die Seite „{{name}}“. Länge 120–155 Zeichen inklusive Leerzeichen. " +
            "Fasse den Nutzen zusammen, nenne das Fokus-Keyword „{{focusKeyword}}“ natürlich und ende mit einer kurzen " +
            "Handlungsaufforderung. " + LanguageRule,
            [Props.MetaDescription, Props.OgDescription], OptionCount: 3),

        new("starter-geo-summary", "GEO: KI-Zusammenfassung erstellen",
            "1–3 sachliche Sätze für llms.txt und KI-Assistenten (ChatGPT, Perplexity & Co.).",
            "Fasse die Seite „{{name}}“ in 1–3 sachlichen, eigenständig verständlichen Sätzen zusammen, so dass ein " +
            "KI-Assistent sie zitieren kann. Nenne die Organisation explizit, keine Werbesprache, keine Pronomen ohne Bezug. " +
            LanguageRule,
            [Props.LlmSummary]),

        new("starter-geo-key-facts", "GEO: Kernfakten extrahieren",
            "Zitierfähige Fakten (Zahlen, Orte, Zeiten) aus dem Seiteninhalt – eine Aussage pro Zeile.",
            "Extrahiere aus dem Inhalt der Seite „{{name}}“ 3–6 zitierfähige Kernfakten: Zahlen, Orte, Öffnungszeiten, " +
            "Preise, Fristen. Eine vollständige Aussage pro Zeile, ohne Aufzählungszeichen. Nur Fakten, die im Inhalt " +
            "stehen – nichts erfinden. " + LanguageRule,
            [Props.LlmKeyFacts]),

        new("starter-text-headline", "Text: Überschrift-Varianten",
            "Drei prägnante Alternativen zur aktuellen Überschrift.",
            "Formuliere die Überschrift „{{currentValue}}“ neu: prägnant, max. 60 Zeichen, nutzenorientiert. " +
            "Ist das Feld leer, leite die Überschrift aus dem übrigen Inhalt ab. " + LanguageRule,
            [Props.Headline], OptionCount: 3),

        new("starter-rte-improve", "Text verbessern (Brand Voice)",
            "Markierten Text stilistisch an die Brand Voice anpassen.",
            "Überarbeite den folgenden Text gemäß der Brand Voice: klarer, kürzer, aktiv formuliert, Inhalt und Fakten " +
            "unverändert. Behalte vorhandene HTML-Formatierung bei.\n\n{{selection}}\n\n" + LanguageRule,
            [], IsTiptapTool: true, EditorUiAliases: [RichTextEditorUi]),

        new("starter-rte-translate", "Übersetzen (DE ⇄ EN)",
            "Markierten Text ins Englische bzw. Deutsche übersetzen.",
            "Übersetze den folgenden Text: Deutsch → Englisch bzw. Englisch → Deutsch. Behalte HTML-Formatierung bei. " +
            "Gib nur die Übersetzung zurück.\n\n{{selection}}",
            [], IsTiptapTool: true, EditorUiAliases: [RichTextEditorUi]),
    ];
}
