namespace Opten.Web.Site;

public sealed record UiText(string Key, string German, string English);

/// <summary>
/// Fixed UI labels, created as Umbraco dictionary items by the seeder (editable in Translation).
/// Views read them via <c>Umbraco.GetDictionaryValueOrDefault(UiTexts.X, …)</c>.
/// </summary>
public static class UiTexts
{
    public const string SkipToContent = "Ui.SkipToContent";
    public const string MainNavigation = "Ui.MainNavigation";
    public const string Breadcrumb = "Ui.Breadcrumb";
    public const string Search = "Ui.Search";
    public const string SearchPlaceholder = "Ui.SearchPlaceholder";
    public const string SearchNoResults = "Ui.SearchNoResults";
    public const string SearchResultsFor = "Ui.SearchResultsFor";
    public const string SearchTooShort = "Ui.SearchTooShort";
    public const string SearchUnavailable = "Ui.SearchUnavailable";
    public const string ReadMore = "Ui.ReadMore";
    public const string AllNews = "Ui.AllNews";
    public const string Language = "Ui.Language";

    public static readonly IReadOnlyList<UiText> All =
    [
        new(SkipToContent, "Zum Inhalt springen", "Skip to content"),
        new(MainNavigation, "Hauptnavigation", "Main navigation"),
        new(Breadcrumb, "Brotkrumen-Navigation", "Breadcrumb"),
        new(Search, "Suche", "Search"),
        new(SearchPlaceholder, "Wonach suchen Sie?", "What are you looking for?"),
        new(SearchNoResults, "Keine Treffer gefunden.", "No results found."),
        new(SearchResultsFor, "Ergebnisse für", "Results for"),
        new(SearchTooShort, "Bitte mindestens 2 Zeichen eingeben.", "Please enter at least 2 characters."),
        new(SearchUnavailable, "Die Suche ist gerade nicht verfügbar.", "Search is currently unavailable."),
        new(ReadMore, "Weiterlesen", "Read more"),
        new(AllNews, "Alle News", "All news"),
        new(Language, "Sprache", "Language"),
    ];

    /// <summary>Fallback when the dictionary item does not exist (yet).</summary>
    public static string Default(string key, string culture) =>
        All.FirstOrDefault(t => t.Key == key) is { } text ? Cultures.Get(culture).T(text.German, text.English) : key;
}
