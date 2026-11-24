using static Opten.Web.Seeding.Schema.SeoGeoDefinitions;

namespace Opten.Web.Seeding.Schema;

/// <summary>
/// All page document types. Every page gets the SEO and GEO compositions plus a template named like its alias
/// (<c>Views/{alias}.cshtml</c>). Order matters: children must be defined before their parents.
/// </summary>
public static class PageTypeDefinitions
{
    private static readonly PropertyDef MainContent =
        new(Props.MainContent, "Inhaltsmodule", DataTypeNames.ContentGrid, VariesByCulture: true);

    private static readonly PropertyDef HideFromNavigation =
        new(Props.HideFromNavigation, "In Navigation ausblenden", DataTypeNames.TrueFalse);

    private static GroupDef Content(params PropertyDef[] properties) => new(ContentTab, "main", "Inhalt", 0, properties);

    public static readonly IReadOnlyList<PageTypeDef> All =
    [
        new(DocTypes.SearchPage, "Suchseite", "icon-search",
            [Content(MainContent, HideFromNavigation)],
            AllowedChildren: []),

        new(DocTypes.NewsArticle, "News-Artikel", "icon-newspaper-alt",
        [
            Content(
                new(Props.TeaserText, "Teaser-Text", DataTypeNames.Textarea, Mandatory: true, VariesByCulture: true),
                new(Props.Image, "Bild", DataTypeNames.ImagePicker),
                new(Props.PublishDate, "Veröffentlichungsdatum", DataTypeNames.DatePicker),
                new(Props.Author, "Autor/in", DataTypeNames.Textstring),
                MainContent),
        ], AllowedChildren: []),

        new(DocTypes.NewsOverview, "News-Übersicht", "icon-newspaper",
            [Content(MainContent, HideFromNavigation)],
            AllowedChildren: [DocTypes.NewsArticle]),

        new(DocTypes.ContentPage, "Inhaltsseite", "icon-document",
            [Content(MainContent, HideFromNavigation)],
            AllowedChildren: [], AllowSelfAsChild: true),

        new(DocTypes.HomePage, "Startseite", "icon-home",
        [
            Content(MainContent),
            new(SiteTab, "navigation", "Navigation & Footer", 0,
            [
                new(Props.MainNavigation, "Hauptnavigation (optional)", DataTypeNames.MultiLinks, VariesByCulture: true,
                    Description: "Leer = alle sichtbaren Seiten der ersten Ebene."),
                new(Props.FooterLinks, "Footer-Links", DataTypeNames.MultiLinks, VariesByCulture: true),
                new(Props.SocialLinks, "Social-Media-Profile", DataTypeNames.MultiLinks,
                    Description: "Absolute URLs – werden als sameAs im schema.org-Organisationsprofil ausgegeben."),
            ]),
            new(SiteTab, "organization", "Organisation (schema.org)", 1,
            [
                new(Props.OrganizationName, "Name der Organisation", DataTypeNames.Textstring, Mandatory: true),
                new(Props.OrganizationLogo, "Logo", DataTypeNames.ImagePicker),
                new(Props.OrganizationEmail, "E-Mail", DataTypeNames.Textstring),
                new(Props.OrganizationPhone, "Telefon", DataTypeNames.Textstring),
                new(Props.OrganizationAddress, "Adresse", DataTypeNames.Textarea,
                    Description: "Zeile 1: Straße, Zeile 2: PLZ Ort, Zeile 3: Land (ISO, z. B. CH)."),
            ]),
            SiteGeoGroup,
        ],
        AllowedChildren: [DocTypes.ContentPage, DocTypes.NewsOverview, DocTypes.SearchPage],
        IsRoot: true),
    ];
}
