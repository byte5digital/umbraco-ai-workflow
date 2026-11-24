namespace Opten.Web.Seeding.Ai;

/// <summary>
/// Brand voice injected into every prompt and the Copilot via an AI context – adapt per project.
/// Editable later in the backoffice (AI › Contexts); the seeder never overwrites existing contexts.
/// </summary>
public static class AiBrandVoice
{
    public const string ContextName = "Brand Voice";

    public const string Tone =
        "Kompetent, freundlich und klar – wie eine erfahrene Beraterin, die komplexe Themen einfach erklärt. " +
        "Deutsch: Ansprache mit „Sie“. Englisch: freundliches, klares International English.";

    public const string Audience =
        "Entscheiderinnen und Entscheider in Marketing und IT mittelständischer Unternehmen sowie Redaktionen, " +
        "die Inhalte selbst pflegen.";

    public const string StyleGuidelines =
        "- Das Wichtigste zuerst, kurze Sätze, aktive Sprache.\n" +
        "- Zahlen als Ziffern, konkrete Beispiele statt Floskeln.\n" +
        "- Barrierearm schreiben: verständlich auch für Nicht-Muttersprachler.\n" +
        "- SEO: Fokus-Keyword natürlich einbauen, kein Keyword-Stuffing.";

    public const string AvoidPatterns =
        "Werbliche Superlative, Ausrufezeichen-Ketten, unnötige Anglizismen, erfundene Fakten, Preise oder Fristen ohne Quelle.";
}
