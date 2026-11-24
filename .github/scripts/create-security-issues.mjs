// Creates one GitHub issue per AI security finding (labels: security, ai-review, severity:*).
// Deduplicates via a fingerprint in the issue body: an open issue for the same finding gets a comment instead.
//
// Usage: node create-security-issues.mjs <results.json>
// Env:   GITHUB_TOKEN, GITHUB_REPOSITORY, GITHUB_SHA, GITHUB_SERVER_URL, GITHUB_STEP_SUMMARY (set by Actions)
//        PR_NUMBER, PR_URL               – optional, links the finding to the PR
//        MIN_CONFIDENCE (default 0.8), MIN_SEVERITY (default LOW), DRY_RUN=true (print instead of creating)
import { appendFileSync, readFileSync } from "node:fs";
import {
  BASE_LABELS, SEVERITY_LABELS, fingerprint, issueBody, issueTitle, normalizeFindings, summaryTable,
} from "./security-findings.mjs";

const env = process.env;
const isDryRun = env.DRY_RUN === "true";
const api = env.GITHUB_API_URL || "https://api.github.com";
const repo = env.GITHUB_REPOSITORY;

async function gh(path, { method = "GET", body } = {}) {
  const response = await fetch(`${api}/repos/${repo}${path}`, {
    method,
    headers: {
      Authorization: `Bearer ${env.GITHUB_TOKEN}`,
      Accept: "application/vnd.github+json",
      "X-GitHub-Api-Version": "2022-11-28",
      ...(body ? { "Content-Type": "application/json" } : {}),
    },
    body: body ? JSON.stringify(body) : undefined,
  });
  if (!response.ok && response.status !== 422) {
    throw new Error(`GitHub API ${method} ${path} failed: ${response.status} ${await response.text()}`);
  }
  return response.status === 204 ? null : response.json();
}

async function ensureLabels() {
  for (const label of [...BASE_LABELS, ...Object.values(SEVERITY_LABELS)]) {
    await gh("/labels", { method: "POST", body: label }); // 422 = already exists
  }
}

/** Open issues created by this script, keyed by fingerprint. */
async function openIssuesByFingerprint() {
  const map = new Map();
  for (let page = 1; page <= 10; page++) {
    const issues = await gh(`/issues?state=open&labels=ai-review&per_page=100&page=${page}`);
    for (const issue of issues) {
      const match = /security-fingerprint: ([a-f0-9]+)/.exec(issue.body ?? "");
      if (match) map.set(match[1], issue);
    }
    if (issues.length < 100) break;
  }
  return map;
}

async function main() {
  const [resultsPath] = process.argv.slice(2);
  if (!resultsPath) throw new Error("Usage: create-security-issues.mjs <results.json>");

  const raw = JSON.parse(readFileSync(resultsPath, "utf8"));
  if (raw.error) throw new Error(`Security review reported an error: ${raw.error}`);

  const findings = normalizeFindings(raw, {
    minConfidence: Number(env.MIN_CONFIDENCE ?? 0.8),
    minSeverity: env.MIN_SEVERITY ?? "LOW",
  });
  const context = {
    repoUrl: `${env.GITHUB_SERVER_URL || "https://github.com"}/${repo}`,
    sha: env.GITHUB_SHA || "HEAD",
    prNumber: env.PR_NUMBER,
    prUrl: env.PR_URL,
  };

  const lines = [];
  if (isDryRun) {
    for (const finding of findings) console.log(`--- ${issueTitle(finding)}\n${issueBody(finding, context)}\n`);
  } else if (findings.length > 0) {
    if (!env.GITHUB_TOKEN || !repo) throw new Error("GITHUB_TOKEN and GITHUB_REPOSITORY are required");
    await ensureLabels();
    const existing = await openIssuesByFingerprint();
    for (const finding of findings) {
      const known = existing.get(fingerprint(finding));
      if (known) {
        const where = context.prUrl ? `PR #${context.prNumber}` : `commit ${context.sha.slice(0, 7)}`;
        await gh(`/issues/${known.number}/comments`, { method: "POST", body: { body: `Found again in ${where}.` } });
        lines.push(`- ${finding.severity}: existing issue #${known.number} (${finding.category})`);
        continue;
      }
      const issue = await gh("/issues", {
        method: "POST",
        body: {
          title: issueTitle(finding),
          body: issueBody(finding, context),
          labels: ["security", "ai-review", SEVERITY_LABELS[finding.severity].name],
        },
      });
      lines.push(`- ${finding.severity}: created #${issue.number} (${finding.category})`);
    }
  }

  const summary = `## AI security review\n\n${summaryTable(findings)}\n${lines.join("\n")}\n`;
  console.log(summary);
  if (env.GITHUB_STEP_SUMMARY) appendFileSync(env.GITHUB_STEP_SUMMARY, summary);
  if (env.GITHUB_OUTPUT) appendFileSync(env.GITHUB_OUTPUT, `high-count=${findings.filter((f) => f.severity === "HIGH").length}\n`);
}

main().catch((error) => {
  console.error(error instanceof Error ? error.message : error);
  process.exit(1);
});
