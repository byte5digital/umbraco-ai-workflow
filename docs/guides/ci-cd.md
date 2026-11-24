# CI/CD & Git-Hooks

## GitHub Actions (`.github/workflows/`)

| Workflow | Auslöser | Inhalt |
| --- | --- | --- |
| `ci.yml` | Push auf `main`, Pull Requests | Tailwind bauen, `dotnet format --verify-no-changes`, Build mit Warnungen als Fehler, Tests, Prüfung auf verwundbare NuGet-/npm-Pakete, Publish-Artefakt |
| `security.yml` | Pull Requests, wöchentlich | **AI Security Review** (Claude) auf dem PR-Diff → PR-Kommentare + Issues mit Severity, Secret-Scan (gitleaks), Dependency Review |
| `release.yml` | Tag `v*` | Publish-Artefakt (ZIP) + GitHub Release (ohne Hosting-Annahme) |

### AI Security Review

Zwei Stufen mit denselben Projektregeln ([`.github/security-review-instructions.md`](../../.github/security-review-instructions.md):
Angriffsfläche, feste Kategorien, Severity-Definitionen, Pflichtfelder, Out of scope) und demselben Fund-Format
(`file`, `line`, `severity` HIGH|MEDIUM|LOW, `category`, `description`, `exploit_scenario`, `recommendation`, `confidence`):

| Stufe | Wann | Werkzeug | Wirkung |
| --- | --- | --- | --- |
| **Lokal** | jeder Commit (`pre-commit`) | Claude Code CLI auf dem **gestagten Diff** (`scripts/ai-security-check.mjs`) | TL;DR-Tabelle im Terminal bzw. in der Claude-Antwort, **HIGH blockiert**, MEDIUM/LOW warnen |
| **CI** | jeder PR (`security.yml`) | `anthropics/claude-code-security-review` | PR-Kommentare + **ein GitHub-Issue pro Fund** + Tabelle in der Job-Summary |

#### Issues aus dem CI-Review

`.github/scripts/create-security-issues.mjs` legt pro Fund ein Issue an:

- Titel `[Security][HIGH] authz_missing in src/…/File.cs:42`
- Labels `security`, `ai-review`, `severity:high|medium|low` (werden bei Bedarf angelegt)
- Body: Tabelle (Severity, Kategorie, Permalink auf die Zeile, Confidence, PR), Beschreibung, Exploit-Szenario,
  Empfehlung, Definition of Done
- **Keine Duplikate:** Ein Fingerprint (Datei + Kategorie + Beschreibung, ohne Zeilennummer) steht im Body. Ist ein
  passendes Issue noch offen, kommt nur ein Kommentar „Found again in PR #…“ dazu.
- Das Skript läuft aus dem **Base-Branch** (`_trusted/`), nie aus dem PR, weil es `issues: write` hat.

Repository-Variablen (optional): `SECURITY_MIN_CONFIDENCE` (Standard `0.8`), `SECURITY_ISSUE_MIN_SEVERITY`
(Standard `LOW`, z. B. `MEDIUM` für weniger Issues).

Einrichtung: Repository → Settings → Secrets and variables → Actions → `ANTHROPIC_API_KEY` anlegen. Ohne das Secret
überspringt der Job den Review mit einem Hinweis, statt fehlzuschlagen. PRs aus Forks werden nicht geprüft (keine Secrets).

#### Lokaler Check

- Voraussetzung: Claude Code CLI (`claude`) ist installiert und angemeldet. Fehlt die CLI, überspringt der Hook den
  Check mit einem Hinweis.
- Geprüft wird nur der gestagte Diff ohne Doku, Bilder, Lockfiles, `.env.example` und SeedData (max. 120.000 Zeichen).
- Report: `.git/ai-security-report.md` (+ `.json`)
- Fehler oder Timeouts blockieren nie, denn die CI ist das eigentliche Gate.
- Optionen: `SKIP_AI_SECURITY=1 git commit …`, dauerhaft `git config opten.aiSecurity off`,
  `AI_SECURITY_ALLOW_HIGH=1` (HIGH melden, aber nicht blockieren), `AI_SECURITY_MODEL`, `AI_SECURITY_TIMEOUT_S`.
- **Commits über Claude Code:** Der Hook gibt den Report als markierten Block aus, und `.claude/hooks/security-tldr.mjs`
  reicht ihn an Claude weiter. Claudes Antwort beginnt dann mit „TL;DR Security“ und der Findings-Tabelle (Regel in `CLAUDE.md`).

### Deployment

Bewusst ohne Cloud-Anbieter: `release.yml` erzeugt ein framework-abhängiges Publish-Paket. Den Deploy-Schritt pro
Projekt ergänzen (IIS/Web Deploy, Linux + systemd, Container-Registry über `docker/Dockerfile` …) und dafür eine ADR schreiben.

## Git-Hooks (`.githooks/`)

`npm install` setzt `core.hooksPath` automatisch (manuell: `npm run hooks:install`).

| Hook | Prüft / macht |
| --- | --- |
| `pre-commit` | blockt `.env`-Dateien und offensichtliche Secrets, `dotnet format` auf geänderten C#-Dateien (verify), JSON-Syntax, **AI Security Check des Diffs** (HIGH blockiert) |
| `commit-msg` | Conventional-Commits-Format |
| `post-commit` | Hinweis, wenn Schema-Dateien geändert wurden (Seed-Step-Version erhöhen? ADR nötig?) |

Notfalls überspringen mit `git commit --no-verify`. CI prüft dasselbe trotzdem.
