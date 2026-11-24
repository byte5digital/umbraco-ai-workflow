#!/usr/bin/env node
// Claude Code PostToolUse hook (Bash): after a `git commit` run by Claude, hands the report of the local AI security
// check (.git/ai-security-report.md, written by scripts/ai-security-check.mjs) back to Claude as additional context,
// so the answer starts with a TL;DR table of the findings. Never blocks.
import { existsSync, readFileSync, renameSync } from "node:fs";
import { execFileSync } from "node:child_process";
import { join } from "node:path";

const MAX_REPORT_AGE_MS = 15 * 60 * 1000;

try {
  const input = JSON.parse(readFileSync(0, "utf8"));
  const command = String(input?.tool_input?.command ?? "");
  if (!/\bgit\b[^|;&]*\bcommit\b/.test(command)) process.exit(0);

  const gitDir = execFileSync("git", ["rev-parse", "--absolute-git-dir"], { cwd: input.cwd, encoding: "utf8" }).trim();
  const jsonPath = join(gitDir, "ai-security-report.json");
  const mdPath = join(gitDir, "ai-security-report.md");
  if (!existsSync(jsonPath) || !existsSync(mdPath)) process.exit(0);

  const { createdAt } = JSON.parse(readFileSync(jsonPath, "utf8"));
  if (Date.now() - createdAt > MAX_REPORT_AGE_MS) process.exit(0);

  const report = readFileSync(mdPath, "utf8");
  renameSync(jsonPath, `${jsonPath}.shown`); // show each report only once

  process.stdout.write(JSON.stringify({
    hookSpecificOutput: {
      hookEventName: "PostToolUse",
      additionalContext:
        "AI security check of the commit (pre-commit hook). Start your reply with a short 'TL;DR Security' section " +
        "containing exactly the findings table below (or 'keine Findings'), then continue.\n\n" + report,
    },
  }));
} catch {
  // Hooks must never break the session.
}
process.exit(0);
