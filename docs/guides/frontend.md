# Frontend & Tailwind

- **Tailwind CSS v4** (CLI, kein Bundler). Quelle: `src/Opten.Web/Styles/site.css`, Ausgabe: `wwwroot/css/site.css`
  (generiert, in `.gitignore`).
- `npm run watch:css` während der Entwicklung. `dotnet build` baut das CSS automatisch, sobald `node_modules`
  existiert (Target `BuildTailwind`). In CI und Docker wird es explizit gebaut.
- Gescannt werden nur `Views/` und `Site/` (`@source`). Dynamische Klassen deshalb vollständig ausschreiben,
  z. B. in `Site/ViewExtensions.SectionClasses`.
- Markenfarben unter `@theme` (`--color-brand-*`). Rich Text nutzt `@tailwindcss/typography` (`prose`).

## Views

| Datei | Zweck |
| --- | --- |
| `Views/master.cshtml` | Layout: SEO-Head, Header, Footer |
| `Views/{docType}.cshtml` | Template je Seitentyp |
| `Views/Partials/site/` | Header (Navigation, Sprachwechsel, Suche), Footer, Breadcrumb, SEO-Head, News-Karte |
| `Views/Partials/blockgrid/Components/{alias}.cshtml` | Block-Grid-Module |
| `Views/Partials/blocklist/Components/{alias}.cshtml` | Block-List-Einträge |

UI-Texte kommen aus dem Umbraco-Dictionary (`@Umbraco.Ui(UiTexts.X)`, Fallback auf die geseedeten Texte).

Barrierefreiheit: Skip-Link, `aria-current`, `lang`/`hreflang` am Sprachwechsel, native `<details>` beim FAQ.
