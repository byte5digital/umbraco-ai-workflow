# 0005 – Umbraco.AI mit OpenAI, API-Key nur als Konfigurationsreferenz

- **Status:** Akzeptiert
- **Datum:** 2026-11-24

## Entscheidung

Wir nutzen die offiziellen **Umbraco.AI**-Pakete mit dem OpenAI-Provider. Die Connection speichert nur die Referenz
`$Umbraco:AI:Secrets:OpenAIApiKey`, nie den Key selbst. Der Copilot hat ausschließlich nicht-destruktive Tool-Scopes.

## Konsequenzen

- Positiv: Kein Secret landet in Datenbank-Backups. Key-Rotation heißt Umgebungsvariable ändern und neu starten.
- Negativ: Kosten pro Embedding und Prompt. Die Suche ist deshalb rate-limitiert und gecacht.
- Ohne Key funktioniert alles außer KI-Funktionen und der Vektorsuche (Volltext bleibt).
