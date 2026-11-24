# Umbraco AI

Pakete: `Umbraco.AI`, `Umbraco.AI.OpenAI`, `Umbraco.AI.Prompt`, `Umbraco.AI.Agent`, `Umbraco.AI.Agent.Copilot`,
`Umbraco.AI.Search` (Versionen in `Directory.Packages.props`).

## Einrichtung

1. In `.env`: `Umbraco__AI__Secrets__OpenAIApiKey=sk-…`
2. Neu starten. Der Step `umbraco-ai` legt einmalig an:
   - Connection `starter-openai`. Der Key wird nur referenziert (`$Umbraco:AI:Secrets:OpenAIApiKey`) und nie in der DB gespeichert.
   - Kontext „Brand Voice“ (`Seeding/Ai/AiBrandVoice.cs`), wird in jedes Prompt injiziert
   - Chat-Profil `starter-editorial` (`AiSetup:ChatModel`)
   - Prompt-Aktionen (SEO-Titel und -Beschreibung, GEO-Zusammenfassung und -Kernfakten, Überschriften, RTE verbessern und übersetzen)
   - Copilot-Agent „Redaktionsassistent“: darf lesen und Felder befüllen, aber **nicht** speichern oder veröffentlichen
   - KI-Button in der Toolbar des Rich-Text-Editors
3. Der Step `umbraco-ai-search` legt das Embedding-Profil an und befüllt den Vektorindex.

Alle Einträge sind über ihren Alias idempotent. Von der Redaktion geänderte Einträge überschreibt der Seeder nicht.
Für einen neuen Projektkunden zuerst `AiBrandVoice.cs` und die Prompt-Texte anpassen.

## Umbraco Developer MCP (Claude Code)

1. `.env`: `McpApiUser__ClientSecret` ist in `.env.example` bereits mit einem Dev-Wert gesetzt (eigener Wert: `openssl rand -hex 32`).
   `UMBRACO_MCP_BASE_URL` muss zur laufenden Instanz passen (`dotnet run`: `https://localhost:44380`, Docker: `http://localhost:<WEB_PORT>`).
2. Seite starten. `McpApiUserSetup` legt bei jedem Start den API-User `mcp@starter.local` an bzw. rotiert das Secret.
3. Claude Code im Repo starten. `.mcp.json` lädt den Server `umbraco` (`scripts/umbraco-mcp.sh`).

Der API-User ist Mitglied der Administratoren-Gruppe und wird deshalb **nur in der Development-Umgebung** angelegt.
Außerhalb von Development überspringt `McpApiUserSetup` das Anlegen, auch wenn ein Secret gesetzt ist.
