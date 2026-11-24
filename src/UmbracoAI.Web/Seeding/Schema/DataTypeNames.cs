using UmbracoAI.Web.Site;

namespace UmbracoAI.Web.Seeding.Schema;

/// <summary>Names of the data types used by the schema. Built-in names must match a fresh Umbraco install.</summary>
public static class DataTypeNames
{
    // Built-in (created by the Umbraco installer)
    public const string Textstring = "Textstring";
    public const string Textarea = "Textarea";
    public const string RichText = "Richtext editor";
    public const string TrueFalse = "True/false";
    public const string Numeric = "Numeric";
    public const string ContentPicker = "Content Picker";
    public const string ImagePicker = "Image Media Picker";
    public const string DatePicker = "Date Picker";

    // Custom (created by the seeder)
    private const string P = SiteOptions.DataTypePrefix;
    public const string SingleLink = P + "Link";
    public const string MultiLinks = P + "Links";
    public const string Background = P + "Hintergrund";
    public const string ImagePosition = P + "Bildposition";
    public const string SitemapChangeFrequency = P + "Sitemap-Änderungshäufigkeit";
    public const string SitemapPriority = P + "Sitemap-Priorität";
    public const string SchemaType = P + "schema.org-Seitentyp";
    public const string LlmsPreview = P + "llms.txt- & Markdown-Vorschau";
    public const string TeaserCardList = P + "Teaser-Karten";
    public const string FaqItemList = P + "FAQ-Einträge";
    public const string ContentGrid = P + "Inhaltsmodule (Block Grid)";
}
