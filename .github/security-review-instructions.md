# Project-specific security review instructions

These instructions are appended to the security audit prompt. Follow the output schema of the main prompt exactly. Each
finding automatically becomes a GitHub issue, so it must make sense on its own without the PR.

## 1. System under review

- **Stack:** Umbraco CMS 18 on ASP.NET Core / .NET 10, server-side Razor (MVC). No headless frontend; the Content
  Delivery API is disabled.
- **Data:** SQLite (local) or SQL Server, accessed only through Umbraco services, with no hand-written SQL.
- **AI:** Umbraco.AI with OpenAI (prompts, Copilot agent, embeddings for the semantic search).
- **Trust boundaries:** anonymous visitors → public endpoints; editors → backoffice (`/umbraco`, Management API);
  CI/automation → API users (client credentials).

## 2. Attack surface – check these first

| Entry point | Code | Must hold |
| --- | --- | --- |
| `GET /api/search?q=&culture=` | `src/UmbracoAI.Web/Search/` | rate limited; query length-limited; Lucene syntax stripped before Examine; only published, indexable pages returned |
| `/robots.txt`, `/sitemap.xml`, `/llms.txt`, `/llms-full.txt`, `/{page}.md` | `src/UmbracoAI.Web/Geo/`, `src/UmbracoAI.Web/Seo/` | expose only published content that is not `noindex`/`llmsExclude`; no drafts; no path traversal through `{**path}` |
| `GET /umbraco/management/api/v1/starterkit/geo/page/{key}` | `src/UmbracoAI.Web/Geo/GeoPreviewController.cs` | requires backoffice auth with content-section access; returns drafts only to authorised editors |
| Razor views and partials | `src/UmbracoAI.Web/Views/` | output encoded; `Html.Raw` only for server-generated JSON-LD |
| Startup, seeding, configuration | `src/UmbracoAI.Web/Program.cs`, `Configuration/`, `Seeding/` | secrets only from configuration; seeding never consumes request data; MCP admin API user only in Development |
| Backoffice extensions | `src/UmbracoAI.Web/wwwroot/App_Plugins/` | no token leakage; no injection of unescaped HTML |
| CI/CD, Docker, hooks | `.github/`, `docker/`, `.githooks/` | no secrets in logs or images; least-privilege `permissions:`; no untrusted input in `run:` scripts |

## 3. Review checklist and category slugs

Use exactly these `category` values. They become the issue title and are used for deduplication.

| `category` | What to look for |
| --- | --- |
| `authz_missing` | endpoint or controller without the required `[Authorize]` policy; drafts/unpublished content reachable anonymously |
| `injection` | user input reaching Examine/Lucene query syntax, SQL, shell commands, file paths or prompts (prompt injection into Umbraco.AI with tool access) |
| `xss` | unencoded output, `Html.Raw` on editor or request data, unsafe `innerHTML`/`unsafeHTML` in backoffice JS |
| `secret_exposure` | API keys, passwords, client secrets or connection strings committed, logged, stored in the DB or baked into images. **Exception:** placeholder/dev values in `.env.example` (`ChangeMe!…`, `local-dev-mcp-secret-…`) |
| `insecure_config` | TLS/HTTPS disabled outside Development (`LocalDev:AllowHttpBackoffice`), debug settings in production config, overly broad CORS |
| `information_disclosure` | stack traces, internal paths or exception details returned to anonymous users |
| `ssrf_open_redirect` | server-side requests or redirects to user-controlled URLs |
| `dos_cost` | missing rate limits or unbounded loops on endpoints that trigger paid AI calls (embeddings, chat) |
| `ci_supply_chain` | workflow script injection (`${{ github.event.* }}` in `run:`), unpinned third-party actions with write permissions, excessive `permissions:` |
| `privilege_escalation` | API users, seed steps or AI agents that gain more rights than needed (e.g. Copilot tool scopes that can publish or delete) |

## 4. Severity – apply strictly

| Severity | Use when | Examples in this project |
| --- | --- | --- |
| `HIGH` | directly exploitable by an anonymous visitor or a low-privileged editor; leads to data breach, account or backoffice takeover, RCE or leaked production secrets | draft content served anonymously; real API key committed; MCP admin user created in production |
| `MEDIUM` | exploitable under specific conditions (authenticated, special configuration, chained) with significant impact | stored XSS by an editor against other editors; missing rate limit on a paid AI endpoint |
| `LOW` | defense in depth; low impact; hard to exploit | missing security header; overly verbose error for authenticated users |

Report only `confidence >= 0.8`. Do not inflate severity: when in doubt, choose the lower level and explain the
condition in `exploit_scenario`.

## 5. Field requirements (each finding becomes an issue)

- `file` / `line`: repository-relative path and the line where the fix belongs.
- `description`: one or two sentences: **what** is wrong and **why** it is a vulnerability. No PR references
  ("this PR", "the change").
- `exploit_scenario`: concrete steps an attacker takes, including the request, input or precondition.
- `recommendation`: a concrete fix in this codebase's terms (e.g. "add `[Authorize(Policy = AuthorizationPolicies.SectionAccessContent)]`",
  "use `SiteSearchOptions.Normalize`"). Name the file or type to change.

## 6. Out of scope – do not report

- Values in `.env.example`, `docs/`, test fixtures and `src/UmbracoAI.Web/SeedData/`.
- The intentionally public endpoints in section 2 as such; report only if they leak more than allowed.
- Generic advice without a concrete code location (e.g. "consider adding a WAF").
- Findings in Umbraco/NuGet/npm packages themselves; dependency scanning covers them.
- Theoretical issues requiring an already compromised server or an administrator account.
