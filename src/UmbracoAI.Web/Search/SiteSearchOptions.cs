namespace UmbracoAI.Web.Search;

/// <summary>Settings of the site search (section "SiteSearch").</summary>
public sealed class SiteSearchOptions
{
    public const string SectionName = "SiteSearch";

    public const int MinQueryLength = 2;
    public const int MaxQueryLength = 120;

    /// <summary>Content hits returned after merging semantic and full-text results.</summary>
    public int MaxContentResults { get; set; } = 10;

    /// <summary>How long the ranked hits of a query are cached – saves an embedding call per repeated query.</summary>
    public int CacheSeconds { get; set; } = 300;

    /// <summary>Per client IP – every uncached query costs an embedding request.</summary>
    public int RequestsPerMinute { get; set; } = 30;

    public static string Normalize(string? query)
    {
        string trimmed = (query ?? string.Empty).Trim();
        return trimmed.Length > MaxQueryLength ? trimmed[..MaxQueryLength] : trimmed;
    }
}
