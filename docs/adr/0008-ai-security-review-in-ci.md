# 0008 – Zweistufiger AI Security Review: lokal im Pre-Commit-Hook und in CI mit Issues

- **Status:** Akzeptiert
- **Datum:** 2026-11-24

## Kontext

Ein großer Teil des Codes entsteht mit KI-Agenten. Klassische Linter finden logische Sicherheitslücken (fehlende
Autorisierung, Drafts für anonyme Besucher, Injection über Umwege) oft nicht. Funde, die nur als PR-Kommentar
existieren, gehen nach dem Merge verloren. Je später eine Lücke auffällt, desto teurer ist sie.

## Entscheidung

Wir prüfen Änderungen **zweistufig mit Claude**. Beide Stufen nutzen dieselben Projektregeln
(`.github/security-review-instructions.md`) und dasselbe Fund-Format:

1. **Lokal, Pre-Commit-Hook** (`scripts/ai-security-check.mjs`): Die Claude Code CLI prüft den gestagten Diff.
   **HIGH blockiert den Commit**, MEDIUM und LOW warnen. Fehler oder eine fehlende CLI blockieren nie. Wird über
   Claude Code committet, beginnt Claudes Antwort mit einer **TL;DR-Tabelle** der Findings
   (`.claude/hooks/security-tldr.mjs`, Regel in `CLAUDE.md`).
2. **CI, jeder PR** (`.github/workflows/security.yml`): `anthropics/claude-code-security-review` kommentiert den PR.
   `.github/scripts/create-security-issues.mjs` legt **pro Fund ein Issue** an, mit Labels `security`, `ai-review`
   und `severity:high|medium|low`. Duplikate erkennt das Skript per Fingerprint. Ergänzend laufen gitleaks
   (Secrets) und GitHub Dependency Review.

## Betrachtete Optionen

### Option A – nur CI-Review mit PR-Kommentaren

- ✅ Keine lokale Abhängigkeit, ein zentraler Ort.
- ❌ Feedback erst nach dem Push. Funde verschwinden nach dem Merge aus dem Blick.

### Option B – nur lokaler Check

- ✅ Schnellstes Feedback.
- ❌ Lässt sich umgehen (`--no-verify`, keine CLI) und ist deshalb kein verlässliches Gate.

### Option C – beide Stufen + Issues (gewählt)

- ✅ Frühes Feedback beim Commit, verlässliches Gate im PR, nachverfolgbare Issues mit Severity.
- ❌ Doppelte API-Kosten und mögliche False Positives auf zwei Ebenen.

## Konsequenzen

- Positiv: Lücken fallen beim Commit auf. Was trotzdem durchrutscht, wird im PR kommentiert und als Issue mit
  Severity nachverfolgbar.
- Negativ / Risiken:
  - API-Kosten pro Commit (lokal, mit `--max-budget-usd` begrenzt) und pro PR.
  - Commits dauern etwa 10–30 Sekunden länger.
  - False Positives erzeugen Issues, die das Team prüfen und schließen muss.
  - Der Review ist **beratend**. Die menschliche Code-Review bleibt Pflicht, und Issues sollten vor dem Fix
    verifiziert werden.
- Sicherheitsvorkehrung: Das Issue-Skript läuft mit `issues: write` und wird deshalb aus dem Base-Branch ausgeführt,
  nie aus dem PR-Code.
- Voraussetzungen:
  - Repository-Secret `ANTHROPIC_API_KEY`. Fehlt es, wird der CI-Review übersprungen.
  - Lokal eine angemeldete Claude Code CLI. Fehlt sie, wird der lokale Check übersprungen.
- Stellschrauben:
  - Repo-Variablen `SECURITY_MIN_CONFIDENCE` und `SECURITY_ISSUE_MIN_SEVERITY`.
  - Lokal `SKIP_AI_SECURITY`, `git config starterkit.aiSecurity off` und `AI_SECURITY_ALLOW_HIGH`.

## Links

- Prompt/Regeln: `.github/security-review-instructions.md`
- Code: `scripts/ai-security-check.mjs`, `.github/scripts/`, `.claude/hooks/security-tldr.mjs`, `.githooks/pre-commit`
- Doku: [docs/guides/ci-cd.md](../guides/ci-cd.md)
