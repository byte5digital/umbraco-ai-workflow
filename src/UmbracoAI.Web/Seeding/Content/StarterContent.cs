using UmbracoAI.Web.Seeding.Schema;
using static UmbracoAI.Web.Seeding.Content.PropertyValues;

namespace UmbracoAI.Web.Seeding.Content;

/// <summary>
/// Bilingual starter site (Musterfirma AG). Replace with the project's real structure – the seeder is generic:
/// add/remove <see cref="PageSeed"/> entries, parents before children.
/// </summary>
public static class StarterContent
{
    public const string Home = "home";
    public const string About = "about";
    public const string Services = "services";
    public const string News = "news";
    public const string Contact = "contact";
    public const string Search = "search";

    public static readonly IReadOnlyList<PageSeed> Pages =
    [
        new(Home, DocTypes.HomePage, new("Musterfirma", "Musterfirma"), null, HomeModules, HomeValues),
        new(About, DocTypes.ContentPage, new("Über uns", "About us"), Home, AboutModules, c => Seo(c,
            new("Über uns – Team & Werte | Musterfirma", "About us – team & values | Musterfirma"),
            new("Lernen Sie die Musterfirma kennen: unser Team, unsere Werte und wie wir seit 2010 digitale Projekte für " +
                "Unternehmen in der Schweiz und Deutschland umsetzen.",
                "Get to know Musterfirma: our team, our values and how we have been delivering digital projects for " +
                "companies in Switzerland and Germany since 2010."),
            new("Musterfirma Team", "Musterfirma team"),
            new("Die Seite stellt Team, Werte und Geschichte der Musterfirma AG vor, einer Digitalagentur mit Sitz in Zürich.",
                "This page introduces the team, values and history of Musterfirma AG, a digital agency based in Zurich."),
            new("Gegründet 2010 in Zürich\n25 Mitarbeitende\nSchwerpunkte: Umbraco, UX und KI-Integration",
                "Founded in 2010 in Zurich\n25 employees\nFocus: Umbraco, UX and AI integration"),
            schemaType: "AboutPage")),
        new(Services, DocTypes.ContentPage, new("Leistungen", "Services"), Home, ServicesModules, c => Seo(c,
            new("Leistungen – Beratung, Umsetzung & Betrieb | Musterfirma", "Services – consulting, delivery & operations | Musterfirma"),
            new("Von der Strategie über Design und Umbraco-Entwicklung bis zum Betrieb: unsere Leistungen für Websites und " +
                "Plattformen, die messbar wirken.",
                "From strategy, design and Umbraco development to operations: our services for websites and platforms " +
                "with measurable impact."),
            new("Umbraco Agentur", "Umbraco agency"),
            new("Überblick über die Leistungen der Musterfirma: Beratung, UX/UI-Design, Umbraco-Entwicklung, KI-Integration und Hosting-Betrieb.",
                "Overview of Musterfirma services: consulting, UX/UI design, Umbraco development, AI integration and managed operations."),
            new("Umbraco-Gold-Partner\nFestpreis-Workshops ab 1 Tag\nSupport mit garantierter Reaktionszeit von 4 Stunden",
                "Umbraco Gold Partner\nFixed-price workshops from 1 day\nSupport with a guaranteed response time of 4 hours"))),
        new(News, DocTypes.NewsOverview, new("News", "News"), Home, NewsModules, c => Seo(c,
            new("News & Insights | Musterfirma", "News & insights | Musterfirma"),
            new("Aktuelles aus der Musterfirma: Projekte, Veranstaltungen und Know-how zu Umbraco, UX und künstlicher Intelligenz.",
                "Latest from Musterfirma: projects, events and know-how on Umbraco, UX and artificial intelligence."),
            new("Musterfirma News", "Musterfirma news"),
            new("Nachrichtenübersicht der Musterfirma mit Beiträgen zu Projekten, Events und Fachthemen.",
                "News overview of Musterfirma with articles on projects, events and expert topics."),
            new("", ""), schemaType: "CollectionPage")),
        Article("article-geo", new("Was ist GEO?", "What is GEO?"),
            new("Generative Engine Optimization sorgt dafür, dass Inhalte in KI-Antworten zitiert werden. So setzen wir es in Umbraco um.",
                "Generative engine optimization makes sure content is cited in AI answers. Here is how we implement it in Umbraco."),
            new DateTime(2026, 10, 1),
            new("<p>Suchmaschinen sind nicht mehr der einzige Weg zu Ihren Inhalten. KI-Assistenten beantworten Fragen direkt – " +
                "und zitieren dabei Quellen, die sie gut verstehen.</p><h2>Was wir umsetzen</h2><ul><li>llms.txt und Markdown-Versionen " +
                "jeder Seite</li><li>schema.org-Daten pro Seitentyp</li><li>KI-Zusammenfassung und Kernfakten pro Seite</li></ul>",
                "<p>Search engines are no longer the only way to your content. AI assistants answer questions directly – and cite " +
                "sources they understand well.</p><h2>What we implement</h2><ul><li>llms.txt and Markdown versions of every page</li>" +
                "<li>schema.org data per page type</li><li>AI summary and key facts per page</li></ul>")),
        Article("article-search", new("Semantische Suche mit Umbraco AI", "Semantic search with Umbraco AI"),
            new("Hybride Suche kombiniert Volltext und Vektoren – Besucher finden Inhalte auch ohne exakte Begriffe.",
                "Hybrid search combines full text and vectors – visitors find content even without the exact terms."),
            new DateTime(2026, 11, 2),
            new("<p>Klassische Volltextsuche findet nur, was wörtlich dasteht. Die semantische Suche versteht die Bedeutung " +
                "einer Anfrage.</p><p>Wir kombinieren beide Verfahren mit Reciprocal Rank Fusion.</p>",
                "<p>Classic full-text search only finds what is written literally. Semantic search understands the meaning " +
                "of a query.</p><p>We combine both approaches using reciprocal rank fusion.</p>")),
        new(Contact, DocTypes.ContentPage, new("Kontakt", "Contact"), Home, ContactModules, c => Seo(c,
            new("Kontakt – Musterfirma AG in Zürich", "Contact – Musterfirma AG in Zurich"),
            new("So erreichen Sie die Musterfirma: Telefon, E-Mail und Adresse unseres Büros in Zürich. Wir melden uns innerhalb eines Werktags.",
                "How to reach Musterfirma: phone, email and address of our Zurich office. We reply within one business day."),
            new("Musterfirma Kontakt", "Musterfirma contact"),
            new("Kontaktdaten der Musterfirma AG mit Adresse, Telefonnummer und E-Mail.",
                "Contact details of Musterfirma AG with address, phone number and email."),
            new("Adresse: Musterstrasse 1, 8000 Zürich\nTelefon: +41 44 000 00 00\nE-Mail: hallo@musterfirma.example",
                "Address: Musterstrasse 1, 8000 Zurich, Switzerland\nPhone: +41 44 000 00 00\nEmail: hello@musterfirma.example"),
            schemaType: "ContactPage")),
        new(Search, DocTypes.SearchPage, new("Suche", "Search"), Home, _ => [], c => new Dictionary<string, object?>
        {
            [Props.MetaTitle] = c.T("Suche | Musterfirma", "Search | Musterfirma"),
            [Props.NoIndex] = true,
            [Props.SitemapExclude] = true,
            [Props.LlmsExclude] = true,
            [Props.HideFromNavigation] = true,
        }),
    ];

    private static IReadOnlyList<Block> HomeModules(SeedContext c) =>
    [
        c.Hero(c.T("Digitale Erlebnisse, die wirken", "Digital experiences that work"),
            c.T("Wir planen, gestalten und betreiben Websites auf Basis von Umbraco – SEO- und KI-ready.",
                "We plan, design and run websites built on Umbraco – ready for SEO and AI."),
            "hero", c.To(Services, c.T("Unsere Leistungen", "Our services"))),
        c.Teasers(c.T("Was wir tun", "What we do"),
            (c.T("Beratung", "Consulting"), c.T("Strategie, Content-Modell und Roadmap.", "Strategy, content model and roadmap."),
                "teaser-1", c.To(Services, c.T("Mehr erfahren", "Learn more"))),
            (c.T("Umsetzung", "Delivery"), c.T("Design und Umbraco-Entwicklung aus einer Hand.", "Design and Umbraco development from one team."),
                "teaser-2", c.To(Services, c.T("Mehr erfahren", "Learn more"))),
            (c.T("KI-Integration", "AI integration"), c.T("Umbraco AI, semantische Suche und GEO.", "Umbraco AI, semantic search and GEO."),
                "teaser-3", c.To("article-geo", c.T("Mehr erfahren", "Learn more")))),
        c.NewsList(c.T("Aktuelles", "Latest news"), News),
        SeedContext.Cta(c.T("Projekt im Kopf?", "Got a project in mind?"),
            c.T("Wir freuen uns auf Ihre Nachricht.", "We look forward to hearing from you."),
            c.To(Contact, c.T("Kontakt aufnehmen", "Get in touch"))),
    ];

    private static IReadOnlyDictionary<string, object?> HomeValues(SeedContext c)
    {
        var values = Seo(c,
            new("Musterfirma – Digitalagentur für Umbraco & KI", "Musterfirma – digital agency for Umbraco & AI"),
            new("Die Musterfirma plant, gestaltet und betreibt Websites auf Basis von Umbraco – mit SEO, semantischer Suche und " +
                "KI-Funktionen für Redaktionen.",
                "Musterfirma plans, designs and runs websites built on Umbraco – with SEO, semantic search and AI features " +
                "for editorial teams."),
            new("Umbraco Agentur", "Umbraco agency"),
            new("Startseite der Musterfirma AG, einer Digitalagentur in Zürich für Umbraco-Websites, UX und KI-Integration.",
                "Home page of Musterfirma AG, a Zurich-based digital agency for Umbraco websites, UX and AI integration."),
            new("Digitalagentur mit Sitz in Zürich\nSpezialisiert auf Umbraco CMS\nGegründet 2010",
                "Digital agency based in Zurich\nSpecialised in Umbraco CMS\nFounded in 2010"));

        values[Props.OrganizationName] = "Musterfirma AG";
        values[Props.OrganizationEmail] = "hallo@musterfirma.example";
        values[Props.OrganizationPhone] = "+41 44 000 00 00";
        values[Props.OrganizationAddress] = "Musterstrasse 1\n8000 Zürich\nCH";
        values[Props.OrganizationLogo] = Image(c.Media("logo"));
        values[Props.SocialLinks] = Links(
            LinkDef.External("LinkedIn", "https://www.linkedin.com/company/example"),
            LinkDef.External("GitHub", "https://github.com/example"));
        values[Props.FooterLinks] = Links(c.To(Contact, c.T("Kontakt", "Contact")), c.To(About, c.T("Über uns", "About us")));
        values[Props.LlmsIntro] = c.T(
            "Website der Musterfirma AG, einer Digitalagentur in Zürich. Informationen zu Leistungen (Beratung, Design, " +
            "Umbraco-Entwicklung, KI-Integration), Team, News und Kontakt.",
            "Website of Musterfirma AG, a digital agency in Zurich. Information on services (consulting, design, Umbraco " +
            "development, AI integration), team, news and contact.");
        return values;
    }

    private static IReadOnlyList<Block> AboutModules(SeedContext c) =>
    [
        c.Hero(c.T("Über uns", "About us"), c.T("Menschen, die gute Websites lieben.", "People who love great websites."), "about", null),
        c.ImageText(c.T("Unser Team", "Our team"),
            c.T("<p>25 Menschen aus Beratung, Design und Entwicklung arbeiten bei uns interdisziplinär zusammen.</p>",
                "<p>25 people from consulting, design and development work together in cross-functional teams.</p>"),
            "team"),
        c.Faq(c.T("Häufige Fragen", "Frequently asked questions"),
            (c.T("Seit wann gibt es die Musterfirma?", "When was Musterfirma founded?"),
                c.T("<p>Die Musterfirma wurde 2010 in Zürich gegründet.</p>", "<p>Musterfirma was founded in Zurich in 2010.</p>")),
            (c.T("Arbeiten Sie auch remote?", "Do you work remotely?"),
                c.T("<p>Ja, wir arbeiten hybrid und betreuen Kundschaft in der Schweiz und in Deutschland.</p>",
                    "<p>Yes, we work hybrid and serve clients in Switzerland and Germany.</p>"))),
    ];

    private static IReadOnlyList<Block> ServicesModules(SeedContext c) =>
    [
        c.Hero(c.T("Leistungen", "Services"), c.T("Alles aus einer Hand.", "Everything from one team."), "services", null),
        SeedContext.RichTextBlock(c.T(
            "<h2>Beratung</h2><p>Wir klären Ziele, Zielgruppen und das Content-Modell.</p><h2>Umsetzung</h2><p>Design-System, " +
            "Tailwind-Frontend und Umbraco-Entwicklung.</p><h2>Betrieb</h2><p>Hosting, Monitoring und Weiterentwicklung.</p>",
            "<h2>Consulting</h2><p>We clarify goals, audiences and the content model.</p><h2>Delivery</h2><p>Design system, " +
            "Tailwind frontend and Umbraco development.</p><h2>Operations</h2><p>Hosting, monitoring and continuous improvement.</p>")),
        SeedContext.Cta(c.T("Workshop vereinbaren", "Book a workshop"), null, c.To(Contact, c.T("Kontakt", "Contact"))),
    ];

    private static IReadOnlyList<Block> NewsModules(SeedContext c) =>
    [
        c.Hero(c.T("News", "News"), c.T("Projekte, Events und Know-how.", "Projects, events and know-how."), null, null),
    ];

    private static IReadOnlyList<Block> ContactModules(SeedContext c) =>
    [
        SeedContext.RichTextBlock(c.T(
            "<h1>Kontakt</h1><p>Musterfirma AG<br>Musterstrasse 1<br>8000 Zürich</p><p>Telefon: +41 44 000 00 00<br>" +
            "E-Mail: <a href=\"mailto:hallo@musterfirma.example\">hallo@musterfirma.example</a></p>",
            "<h1>Contact</h1><p>Musterfirma AG<br>Musterstrasse 1<br>8000 Zurich, Switzerland</p><p>Phone: +41 44 000 00 00<br>" +
            "Email: <a href=\"mailto:hello@musterfirma.example\">hello@musterfirma.example</a></p>")),
    ];

    private static PageSeed Article(string id, Bilingual name, Bilingual teaser, DateTime published, Bilingual html) =>
        new(id, DocTypes.NewsArticle, name, News,
            c => [SeedContext.RichTextBlock(html.For(c.Culture))],
            c =>
            {
                var values = Seo(c, new($"{name.German} | Musterfirma", $"{name.English} | Musterfirma"), teaser,
                    new(name.German, name.English), teaser, new("", ""));
                values[Props.TeaserText] = teaser.For(c.Culture);
                values[Props.PublishDate] = published;
                values[Props.Author] = "Musterfirma Redaktion";
                values[Props.Image] = Image(c.Media(id));
                return values;
            });

    private static Dictionary<string, object?> Seo(
        SeedContext c, Bilingual title, Bilingual description, Bilingual keyword, Bilingual summary, Bilingual facts,
        string? schemaType = null) => new()
        {
            [Props.MetaTitle] = title.For(c.Culture),
            [Props.MetaDescription] = description.For(c.Culture),
            [Props.FocusKeyword] = keyword.For(c.Culture),
            [Props.LlmSummary] = summary.For(c.Culture),
            [Props.LlmKeyFacts] = string.IsNullOrEmpty(facts.For(c.Culture)) ? null : facts.For(c.Culture),
            [Props.SchemaType] = schemaType is null ? null : Dropdown(schemaType),
        };
}
