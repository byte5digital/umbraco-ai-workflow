# Semantische Suche (Template)

Hybride Suche in `Search/ContentSearchService.cs`:

1. **Semantisch:** Umbraco.AI.Search (Index `UmbAI_Search`, Vektoren in der Umbraco-Datenbank, SQLite und SQL Server)
2. **Volltext:** Examine `ExternalIndex`, nur Felder der angefragten Kultur und invariante Felder
3. **Fusion:** Reciprocal Rank Fusion (k = 60). Seiten, die in beiden Listen auftauchen, landen vorne.

Ohne OpenAI-Key läuft nur die Volltextsuche. Es gibt dann weder einen Fehler noch Embedding-Kosten.

## Nutzung

- Suchseite (Seitentyp `searchPage`, Starter: `/de/suche/?q=…`), gerendert über `SearchResultsViewComponent`
- JSON: `GET /api/search?q=team&culture=de-DE` → `{ query, culture, results: [{ id, title, description, url, type, matchedBy }] }`
- Rate Limit: `SiteSearch:RequestsPerMinute` pro IP (jede ungecachte Anfrage kostet ein Embedding)

## Konfiguration (`appsettings.json`)

| Schlüssel | Bedeutung |
| --- | --- |
| `Umbraco:AI:DefaultEmbeddingProfileAlias` | Embedding-Profil (vom Seeder angelegt: `starter-embedding`) |
| `Umbraco:AI:Search:ChunkSize/ChunkOverlap/MinScore` | Chunking und Relevanzschwelle der Vektorsuche |
| `SiteSearch:MaxContentResults/CacheSeconds/RequestsPerMinute` | Trefferzahl, Cache, Limit |

## Verhalten, das man kennen sollte

- Examine-Indizes sind direkt nach einem Kaltstart leer und werden im Hintergrund aufgebaut. Leere Rankings werden
  deshalb nicht gecacht.
- Ausgeblendet werden Seiten mit `noindex` oder „Aus llms.txt ausschließen“ sowie die Suchseite selbst.
- Nach einem Wechsel des Embedding-Modells den Index im Backoffice neu aufbauen (Settings → Search).
