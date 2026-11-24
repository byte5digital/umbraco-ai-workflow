namespace UmbracoAI.Web.Site;

/// <summary>Languages of the site and their URL prefixes. The first entry is the default culture.</summary>
public static class Cultures
{
    public const string German = "de-DE";
    public const string English = "en-US";

    public static readonly IReadOnlyList<SiteCulture> All =
    [
        new(German, "Deutsch (Deutschland)", "/de", "DE"),
        new(English, "English (United States)", "/en", "EN"),
    ];

    public static SiteCulture Default => All[0];

    public static SiteCulture Get(string? isoCode) =>
        All.FirstOrDefault(c => c.IsoCode.Equals(isoCode, StringComparison.OrdinalIgnoreCase)) ?? Default;
}

/// <param name="DomainPath">Relative Umbraco domain of the start page in this culture ("/de").</param>
public sealed record SiteCulture(string IsoCode, string Name, string DomainPath, string ShortLabel)
{
    public bool IsEnglish => IsoCode == Cultures.English;

    /// <summary>Picks the text for this culture (seed data and fixed UI labels).</summary>
    public string T(string german, string english) => IsEnglish ? english : german;
}
