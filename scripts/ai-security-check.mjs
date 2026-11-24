#!/usr/bin/env node
// Local AI security review of the staged diff (called by .githooks/pre-commit).
// Uses the Claude Code CLI with the same project rules as CI (.github/security-review-instructions.md) and the same
// finding schema as anthropics/claude-code-security-review.
//
// Result: TL;DR table on stderr + report in .git/ai-security-report.{md,json}.
// HIGH findings block the commit; MEDIUM/LOW only warn. Errors/timeouts never block – CI is the gate.
//
// Opt-out:  SKIP_AI_SECURITY=1 git commit …   or permanently: git config opten.aiSecurity off
// Tuning:   AI_SECURITY_MODEL (default claude-sonnet-5-5), AI_SECURITY_TIMEOUT_S (default 240),
//           AI_SECURITY_ALLOW_HIGH=1 (report HIGH but do not block)
import { execFileSync, spawnSync } from "node:child_process";
import { mkdirSync, readFileSync, writeFileSync } from "node:fs";
import { tmpdir } from "node:os";
import { join } from "node:path";
import { normalizeFindings, summaryTable } from "../.github/scripts/security-findings.mjs";

const MAX_DIFF_CHARS = 120_000;
const MAX_BUDGET_USD = "1.00";
const EXCLUDED_PATHS = [
  ":(exclude)package-lock.json", ":(exclude)docs/**", ":(exclude).env.example", ":(exclude)**/SeedData/**",
  ":(exclude)**/*.png", ":(exclude)**/*.jpg", ":(exclude)**/*.ico", ":(exclude)**/*.md",
];

const FINDINGS_SCHEMA = {
  type: "object",
  properties: {
    findings: {
      type: "array",
      items: {
        type: "object",
        properties: {
          file: { type: "string" },
          line: { type: "integer" },
          severity: { type: "string", enum: ["HIGH", "MEDIUM", "LOW"] },
          category: { type: "string" },
          description: { type: "string" },
          exploit_scenario: { type: "string" },
          recommendation: { type: "string" },
          confidence: { type: "number" },
        },
        required: ["file", "severity", "category", "description", "exploit_scenario", "recommendation", "confidence"],
      },
    },
  },
  required: ["findings"],
};

const git = (...args) => execFileSync("git", args, { encoding: "utf8", maxBuffer: 64 * 1024 * 1024 }).trim();
const log = (message) => process.stderr.write(`${message}\n`);
const isClaudeSession = process.env.CLAUDECODE === "1";

function isDisabled() {
  if (process.env.SKIP_AI_SECURITY === "1") return true;
  try {
    return git("config", "--get", "opten.aiSecurity") === "off";
  } catch {
    return false; // config key not set
  }
}

function buildPrompt(root) {
  const projectRules = readFileSync(join(root, ".github/security-review-instructions.md"), "utf8");
  return [
    "You are a senior application security engineer. Review ONLY the staged git diff provided on stdin",
    "(lines starting with '+' are new code; use '-' and context lines only to understand it).",
    "Report real, exploitable vulnerabilities introduced or touched by this diff – no style issues, no speculation.",
    "Return JSON matching the provided schema. Use an empty findings array when there is nothing to report.",
    "",
    "Severity: HIGH = directly exploitable (data breach, auth bypass, RCE, leaked production secrets);",
    "MEDIUM = exploitable under specific conditions with significant impact; LOW = defense in depth.",
    "Only report findings with confidence >= 0.8.",
    "",
    "# Project rules",
    "",
    projectRules,
  ].join("\n");
}

function runClaude(prompt, diff) {
  const timeoutMs = Number(process.env.AI_SECURITY_TIMEOUT_S ?? 240) * 1000;
  const result = spawnSync("claude", [
    "-p", prompt,
    "--output-format", "json",
    "--json-schema", JSON.stringify(FINDINGS_SCHEMA),
    "--model", process.env.AI_SECURITY_MODEL ?? "claude-sonnet-5-5",
    "--tools", "",
    "--no-session-persistence",
    "--max-budget-usd", MAX_BUDGET_USD,
  ], {
    input: diff,
    encoding: "utf8",
    timeout: timeoutMs,
    maxBuffer: 32 * 1024 * 1024,
    cwd: tmpdir(), // no project CLAUDE.md/hooks – the prompt carries all rules explicitly
  });

  if (result.error) throw result.error;
  if (result.status !== 0) throw new Error(`claude exited with ${result.status}: ${result.stderr.slice(0, 300)}`);

  const output = JSON.parse(result.stdout);
  if (output.is_error) throw new Error(`claude reported an error: ${output.result ?? output.subtype}`);
  return output.structured_output ?? JSON.parse(output.result);
}

function writeReport(root, findings, files) {
  const gitDir = git("rev-parse", "--absolute-git-dir");
  mkdirSync(gitDir, { recursive: true });
  const details = findings.map((f, i) => [
    `### ${i + 1}. [${f.severity}] ${f.category} – \`${f.file}${f.line ? `:${f.line}` : ""}\``,
    "", f.description, "", `**Exploit:** ${f.exploitScenario || "–"}`, "", `**Fix:** ${f.recommendation || "–"}`, "",
  ].join("\n"));
  const markdown = [
    "## AI security check (staged diff)", "",
    `Files: ${files.length} · Findings: ${findings.length} · ${new Date().toISOString()}`, "",
    summaryTable(findings), ...details,
  ].join("\n");
  writeFileSync(join(gitDir, "ai-security-report.md"), markdown);
  writeFileSync(join(gitDir, "ai-security-report.json"), JSON.stringify({ createdAt: Date.now(), findings }, null, 2));
  return markdown;
}

function main() {
  if (isDisabled()) return 0;

  const root = git("rev-parse", "--show-toplevel");
  const files = git("diff", "--cached", "--name-only", "--diff-filter=ACMR", "--", ".", ...EXCLUDED_PATHS)
    .split("\n").filter(Boolean);
  if (files.length === 0) return 0;

  const hasClaude = spawnSync("claude", ["--version"], { encoding: "utf8" }).status === 0;
  if (!hasClaude) {
    log("ℹ AI security check skipped: Claude Code CLI not found (https://docs.claude.com/claude-code). CI still reviews the PR.");
    return 0;
  }

  let diff = git("diff", "--cached", "-U5", "--diff-filter=ACMR", "--", ".", ...EXCLUDED_PATHS);
  if (diff.length > MAX_DIFF_CHARS) {
    log(`ℹ Staged diff is large (${diff.length} chars) – reviewing the first ${MAX_DIFF_CHARS} characters only.`);
    diff = diff.slice(0, MAX_DIFF_CHARS);
  }

  log(`🔒 AI security check: reviewing ${files.length} staged file(s)…`);
  let findings;
  try {
    findings = normalizeFindings(runClaude(buildPrompt(root), diff), { minConfidence: 0.8 });
  } catch (error) {
    log(`⚠ AI security check failed (commit not blocked): ${error instanceof Error ? error.message : error}`);
    return 0;
  }

  const report = writeReport(root, findings, files);
  const highCount = findings.filter((f) => f.severity === "HIGH").length;
  const isBlocking = highCount > 0 && process.env.AI_SECURITY_ALLOW_HIGH !== "1";

  if (isClaudeSession) {
    // Picked up by the agent (and by .claude/hooks/security-tldr.mjs): show this table as TL;DR in the reply.
    log("<<<AI-SECURITY-TLDR>>>");
    log(report);
    log("<<<END-AI-SECURITY-TLDR>>>");
  } else {
    log(summaryTable(findings));
    if (findings.length > 0) log(`Details: ${join(git("rev-parse", "--absolute-git-dir"), "ai-security-report.md")}`);
  }

  if (isBlocking) {
    log(`✖ ${highCount} HIGH severity finding(s) – commit blocked. Fix them, or override consciously with AI_SECURITY_ALLOW_HIGH=1.`);
    return 1;
  }
  return 0;
}

process.exit(main());
