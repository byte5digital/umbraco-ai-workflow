# CLAUDE.md

@AGENTS.md

## Claude-Code-spezifisch

- **Umbraco MCP:** `.mcp.json` startet `scripts/umbraco-mcp.sh` (offizieller `@umbraco-cms/mcp-dev`). Voraussetzung:
  `McpApiUser__ClientSecret` in `.env` gesetzt (Default aus `.env.example`), Seite läuft in Development. `UMBRACO_MCP_BASE_URL` muss zur laufenden Instanz passen.
  Mit MCP Inhalte lesen und prüfen. Schema-Änderungen trotzdem im Code (Regel 2).
- **Skill `/adr`** (`.claude/skills/adr/`): legt die nächste nummerierte ADR in `docs/adr/` an und pflegt den Index.
- **Hooks:** `.claude/settings.json` formatiert geänderte C#-Dateien nach jedem Edit (`dotnet format whitespace`) und
  gibt nach Commits den Security-Report als Kontext zurück (`security-tldr.mjs`).
  Git-Hooks in `.githooks/` laufen unabhängig davon bei jedem Commit.
- Vor Commits: Build, Tests und Format laufen lassen, die Ergebnisse zitieren und nicht nur behaupten.
- **Security-TL;DR nach jedem Commit:** Der Pre-Commit-Hook prüft den Diff mit Claude (`scripts/ai-security-check.mjs`).
  Nach jedem `git commit`, auch einem blockierten, beginnt die Antwort mit einem Abschnitt **„TL;DR Security“**, der
  die Findings-Tabelle 1:1 wiedergibt (oder „keine Findings“). Quelle: der Block `<<<AI-SECURITY-TLDR>>>` in der
  Commit-Ausgabe bzw. der Kontext des Hooks `.claude/hooks/security-tldr.mjs`. HIGH-Findings blockieren den Commit:
  beheben statt mit `AI_SECURITY_ALLOW_HIGH=1` oder `--no-verify` umgehen, außer der Nutzer entscheidet es ausdrücklich.
- Bei Umbraco-API-Fragen: Version 18 beachten. Die meisten Services sind async und liefern `Attempt`-Ergebnisse,
  die man mit `SeedGuard.Ensure` prüft. Doku über Context7 oder docs.umbraco.com holen, nicht aus dem Gedächtnis.
