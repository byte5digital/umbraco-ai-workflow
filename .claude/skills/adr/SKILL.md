---
name: adr
description: Architecture Decision Record anlegen oder aktualisieren. Verwenden, wenn eine Architektur- oder Technologieentscheidung getroffen, geändert oder abgelöst wird (neues Paket, Hosting, Datenbank, Rendering, Integrationen, Sicherheitskonzept) oder wenn der Nutzer "/adr", "ADR", "Entscheidung dokumentieren" sagt.
---

# ADR-Skill

Dokumentiert Architekturentscheidungen als nummerierte Markdown-Dateien in `docs/adr/` (Format: MADR, deutsch).

## Ablauf

1. **Kontext klären.** Worum geht es, welche Optionen gab es, was wurde entschieden und warum? Fehlt eine Angabe
   (z. B. verworfene Alternativen), kurz nachfragen statt erfinden.
2. **Datei anlegen** mit dem Skript (vergibt die nächste Nummer und den Slug, trägt die ADR im Index ein):
   ```bash
   bash .claude/skills/adr/new-adr.sh "Titel der Entscheidung"
   ```
   Das Skript gibt den Pfad der neuen Datei aus.
3. **Ausfüllen** nach `docs/adr/0000-template.md`: Status, Datum, Kontext, Entscheidung, Optionen (mit Vor- und
   Nachteilen), Konsequenzen (positiv **und** negativ), Links zu Code und Doku.
4. **Ablösen statt löschen.** Ersetzt eine neue ADR eine alte, wird die alte auf `Status: Abgelöst durch [NNNN](…)`
   gesetzt. Die neue verweist mit `Löst ab: [NNNN](…)` zurück. Committete ADRs werden nicht inhaltlich umgeschrieben,
   auch offene nicht. Eine Entscheidung zu einer offenen ADR ist ebenfalls eine neue, ablösende ADR.
   **Einzige Ausnahme – Initialphase:** Solange das Repo noch keinen Commit auf `main` hat
   (`git rev-parse --verify main` schlägt fehl), dürfen ADRs direkt überarbeitet, zusammengelegt und neu nummeriert werden.
5. **Querverweise:** Ist die Entscheidung für Agenten relevant, gehört eine Zeile in die „Harten Regeln“ von `AGENTS.md`.

## Qualitätskriterien

- Ein Thema pro ADR, Titel als Entscheidung formuliert („SQLite für lokale Entwicklung“, nicht „Datenbank“).
- Kontext ist auch in zwei Jahren verständlich: keine Chat-Referenzen, konkrete Versionen und Zahlen.
- Konsequenzen nennen auch Kosten und Risiken.
