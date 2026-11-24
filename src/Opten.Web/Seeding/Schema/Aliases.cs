namespace Opten.Web.Seeding.Schema;

/// <summary>Aliases of all document types created by the seeder. Views and services reference these – never strings.</summary>
public static class DocTypes
{
    public const string HomePage = "homePage";
    public const string ContentPage = "contentPage";
    public const string NewsOverview = "newsOverview";
    public const string NewsArticle = "newsArticle";
    public const string SearchPage = "searchPage";

    // Compositions
    public const string SeoComposition = "seoComposition";
    public const string GeoComposition = "geoComposition";
}

/// <summary>Element types: Block Grid modules, nested Block List items and the block settings.</summary>
public static class Elements
{
    public const string HeroStage = "heroStage";
    public const string RichText = "richText";
    public const string ImageText = "imageText";
    public const string TeaserSection = "teaserSection";
    public const string FaqAccordion = "faqAccordion";
    public const string CtaBanner = "ctaBanner";
    public const string NewsList = "newsList";

    public const string TeaserCard = "teaserCard";
    public const string FaqItem = "faqItem";

    public const string SectionSettings = "sectionSettings";
}

public static class Props
{
    // Content
    public const string MainContent = "mainContent";
    public const string Headline = "headline";
    public const string Text = "text";
    public const string Image = "image";
    public const string Link = "link";
    public const string Items = "items";
    public const string Question = "question";
    public const string Answer = "answer";
    public const string ImagePosition = "imagePosition";
    public const string NewsRoot = "newsRoot";
    public const string Count = "count";
    public const string TeaserText = "teaserText";
    public const string PublishDate = "publishDate";
    public const string Author = "author";
    public const string HideFromNavigation = "umbracoNaviHide";

    // Site settings (start page)
    public const string MainNavigation = "mainNavigation";
    public const string FooterLinks = "footerLinks";
    public const string SocialLinks = "socialLinks";
    public const string OrganizationName = "organizationName";
    public const string OrganizationLogo = "organizationLogo";
    public const string OrganizationEmail = "organizationEmail";
    public const string OrganizationPhone = "organizationPhone";
    public const string OrganizationAddress = "organizationAddress";

    // SEO (seoComposition)
    public const string MetaTitle = "metaTitle";
    public const string MetaDescription = "metaDescription";
    public const string FocusKeyword = "focusKeyword";
    public const string CanonicalUrl = "canonicalUrl";
    public const string NoIndex = "robotsNoIndex";
    public const string NoFollow = "robotsNoFollow";
    public const string SitemapExclude = "sitemapExclude";
    public const string SitemapChangeFrequency = "sitemapChangeFrequency";
    public const string SitemapPriority = "sitemapPriority";
    public const string OgTitle = "ogTitle";
    public const string OgDescription = "ogDescription";
    public const string OgImage = "ogImage";

    // GEO (geoComposition + start page)
    public const string SchemaType = "schemaType";
    public const string LlmSummary = "llmSummary";
    public const string LlmKeyFacts = "llmKeyFacts";
    public const string LlmsExclude = "llmsExclude";
    public const string LlmsOptional = "llmsOptional";
    public const string LlmsPreview = "llmsPreview";
    public const string LlmsIntro = "llmsIntro";
    public const string BlockAiCrawlers = "blockAiCrawlers";

    // Block settings
    public const string Background = "background";
    public const string SpacingTop = "spacingTop";
    public const string SpacingBottom = "spacingBottom";
}

/// <summary>Option values of the dropdown data types. Views validate against these lists.</summary>
public static class Options
{
    public static readonly string[] Backgrounds = ["white", "muted", "brand"];
    public static readonly string[] ImagePositions = ["left", "right"];
    public static readonly string[] SitemapChangeFrequencies =
        ["always", "hourly", "daily", "weekly", "monthly", "yearly", "never"];
    public static readonly string[] SitemapPriorities =
        ["1.0", "0.9", "0.8", "0.7", "0.6", "0.5", "0.4", "0.3", "0.2", "0.1"];

    /// <summary>schema.org type of a page (GEO tab). Empty = derived from the document type.</summary>
    public static readonly string[] SchemaTypes =
        ["WebPage", "AboutPage", "ContactPage", "FAQPage", "CollectionPage", "ItemPage", "ProfilePage"];
}
