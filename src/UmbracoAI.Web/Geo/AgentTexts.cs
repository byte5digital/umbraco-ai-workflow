using UmbracoAI.Web.Site;

namespace UmbracoAI.Web.Geo;

/// <summary>Fixed labels of the agent-facing Markdown output (llms.txt, llms-full.txt, page .md).</summary>
public sealed record AgentTexts(
    string MainPages,
    string MorePages,
    string News,
    string Optional,
    string KeyFacts,
    string SubPages,
    string Source,
    string Updated,
    string Published,
    string OtherLanguage,
    string FullContent,
    string MarkdownHint)
{
    public static readonly AgentTexts German = new(
        "Hauptseiten", "Weitere Seiten", "News", "Optional", "Kernfakten", "Unterseiten", "Quelle",
        "Zuletzt aktualisiert", "Veröffentlicht", "English version", "Vollständiger Inhalt aller Seiten",
        "Jede Seite ist als Markdown abrufbar: URL + „.md“.");

    public static readonly AgentTexts English = new(
        "Main pages", "More pages", "News", "Optional", "Key facts", "Sub pages", "Source",
        "Last updated", "Published", "Deutsche Version", "Full content of all pages",
        "Every page is available as Markdown: URL + “.md”.");

    public static AgentTexts For(string culture) => culture == Cultures.English ? English : German;
}
