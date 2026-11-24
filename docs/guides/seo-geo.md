# SEO & GEO

Alle Seitentypen erben zwei Kompositionen. Damit hat jede Seite dieselben Tabs.

| Tab | Felder | Wirkung |
| --- | --- | --- |
| **SEO** | Meta-Titel, Meta-Beschreibung, Fokus-Keyword, Canonical, noindex/nofollow, Sitemap (Ausschluss, Häufigkeit, Priorität) | `<head>`, `sitemap.xml` |
| **Social Media** | OG-Titel, -Beschreibung, -Bild | Open Graph / X |
| **GEO & KI** | KI-Zusammenfassung, Kernfakten, schema.org-Seitentyp, llms.txt-Ausschluss/Optional, **Vorschau „llms.txt & Markdown“** | `llms.txt`, Markdown-Zwilling, JSON-LD |
| **Website-Einstellungen** (nur Startseite) | Navigation, Footer, Social-Profile, Organisation, llms.txt-Einleitung, KI-Crawler blockieren | JSON-LD Organization, `robots.txt` |

## Vorschau im Backoffice (GEO & KI › Vorschau)

Zeigt der Redaktion, wie KI-Agenten die Seite sehen:

- Status (in llms.txt enthalten, optional, ausgeschlossen wegen noindex bzw. Ausschluss, nicht veröffentlicht) und
  eine grobe Token-Schätzung
- **Eintrag in llms.txt (live):** reagiert sofort auf ungespeicherte Änderungen an Zusammenfassung und Meta-Beschreibung
- **Markdown-Version:** zuletzt gespeicherter Stand, wird nach jedem Speichern automatisch aktualisiert, mit Kopier-Button
- Links auf Markdown-Zwilling, `llms.txt` und `llms-full.txt`

Technik: Property-Editor-UI `Opten.PropertyEditorUi.LlmsPreview` (`wwwroot/App_Plugins/OptenGeo/`) auf einem
Label-Datentyp, Daten aus `GET /umbraco/management/api/v1/opten/geo/page/{key}?culture=…` (`Geo/GeoPreviewController.cs`,
nur mit Backoffice-Token und Zugriff auf die Content-Section).

## Fallback-Ketten

- Titel: Meta-Titel → `Seitenname | Website-Name` (Website-Name = Organisation der Startseite → `Site:Name`)
- Beschreibung: Meta-Beschreibung → Teaser-Text
- KI-Zusammenfassung: Feld → Meta-Beschreibung → Teaser-Text
- Social-Bild: OG-Bild → Seitenbild → erstes Bild im Block Grid

## schema.org pro Seitentyp (`Seo/StructuredDataBuilder.cs`)

| Seitentyp | WebPage-Typ (Standard) | Zusätzlich |
| --- | --- | --- |
| Startseite | `WebPage` | `Organization`, `WebSite` mit `SearchAction` |
| Inhaltsseite | `WebPage` (oder Feld „schema.org-Seitentyp“, z. B. `AboutPage`, `ContactPage`) | `BreadcrumbList` |
| News-Übersicht | `CollectionPage` | `BreadcrumbList` |
| News-Artikel | `WebPage` | `Article` mit Autor und Datum |
| Suchseite | `SearchResultsPage` (noindex) | |
| jede Seite mit FAQ-Modul | zusätzlich `FAQPage` mit `mainEntity` | |

## Agenten-Endpunkte

- `/llms.txt` (Standardsprache) und `/{de|en}/llms.txt`: Index aller Seiten mit KI-Zusammenfassung, gruppiert
- `/llms-full.txt`: gesamter Inhalt als Markdown
- `/{seite}.md`: Markdown-Zwilling jeder Seite (`noindex`, `Link: rel=canonical` auf die HTML-Seite)
- `<link rel="alternate" type="text/markdown">` im `<head>` jeder Seite

Seiten mit `noindex` oder „Aus llms.txt ausschließen“ erscheinen weder in llms.txt noch als Markdown-Zwilling noch in
der Website-Suche.

## robots.txt

KI-**Suche** (OAI-SearchBot, ChatGPT-User, Claude-SearchBot, PerplexityBot …) bleibt immer erlaubt, damit die
Seite in KI-Antworten zitiert werden kann. KI-**Training** (GPTBot, ClaudeBot, Google-Extended, CCBot …) lässt sich
auf der Startseite (Tab „GEO & KI“) sperren.
