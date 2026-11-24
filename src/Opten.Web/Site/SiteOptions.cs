namespace Opten.Web.Site;

/// <summary>Project-wide settings (section "Site"). Change these first when starting a new project.</summary>
public sealed class SiteOptions
{
    public const string SectionName = "Site";

    /// <summary>Brand name used in titles ("Page | Name"), Open Graph and JSON-LD.</summary>
    public string Name { get; set; } = "Opten Starter";

    /// <summary>Prefix of all data types created by the seeder (keeps them apart from Umbraco's built-ins).</summary>
    public const string DataTypePrefix = "Starter – ";
}
