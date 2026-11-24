// Shared helpers for AI security review findings (CI issue creation and the local pre-commit check).
// Finding schema (anthropics/claude-code-security-review):
//   { file, line, severity: "HIGH"|"MEDIUM"|"LOW", category, description, exploit_scenario, recommendation, confidence }
import { createHash } from "node:crypto";

export const SEVERITIES = Object.freeze(["HIGH", "MEDIUM", "LOW"]);

/** Label colours per severity (GitHub hex without #). */
export const SEVERITY_LABELS = Object.freeze({
  HIGH: { name: "severity:high", color: "b60205", description: "Security finding – high severity" },
  MEDIUM: { name: "severity:medium", color: "d93f0b", description: "Security finding – medium severity" },
  LOW: { name: "severity:low", color: "fbca04", description: "Security finding – low severity" },
});

export const BASE_LABELS = Object.freeze([
  { name: "security", color: "5319e7", description: "Security issue" },
  { name: "ai-review", color: "c5def5", description: "Found by the AI security review" },
]);

/**
 * @param {unknown} value
 * @returns {"HIGH"|"MEDIUM"|"LOW"}
 */
export function normalizeSeverity(value) {
  const upper = String(value ?? "").trim().toUpperCase();
  if (upper === "CRITICAL") return "HIGH";
  return SEVERITIES.includes(upper) ? upper : "LOW";
}

/**
 * Validates and normalises raw findings; drops entries without file/description or below the confidence threshold.
 * @param {unknown} raw parsed results JSON ({ findings: [...] }) or an array of findings
 * @param {{ minConfidence?: number, minSeverity?: string }} [options]
 */
export function normalizeFindings(raw, { minConfidence = 0.8, minSeverity = "LOW" } = {}) {
  const list = Array.isArray(raw) ? raw : Array.isArray(raw?.findings) ? raw.findings : [];
  const maxRank = SEVERITIES.indexOf(normalizeSeverity(minSeverity));
  return list
    .filter((f) => f && typeof f === "object" && typeof f.file === "string" && typeof f.description === "string")
    .map((f) => ({
      file: f.file.replace(/^\.?\//, ""),
      line: Number.isInteger(f.line) && f.line > 0 ? f.line : null,
      severity: normalizeSeverity(f.severity),
      category: String(f.category || "uncategorized").trim().toLowerCase().replace(/[^a-z0-9_-]+/g, "_"),
      description: f.description.trim(),
      exploitScenario: String(f.exploit_scenario ?? "").trim(),
      recommendation: String(f.recommendation ?? "").trim(),
      confidence: typeof f.confidence === "number" ? f.confidence : null,
    }))
    .filter((f) => f.confidence === null || f.confidence >= minConfidence)
    .filter((f) => SEVERITIES.indexOf(f.severity) <= maxRank)
    .sort((a, b) => SEVERITIES.indexOf(a.severity) - SEVERITIES.indexOf(b.severity));
}

/**
 * Stable id of a finding across commits: file + category + normalised description. Line numbers are left out on
 * purpose – they shift with unrelated edits.
 */
export function fingerprint(finding) {
  const text = `${finding.file}|${finding.category}|${finding.description.toLowerCase().replace(/\s+/g, " ").slice(0, 160)}`;
  return createHash("sha256").update(text).digest("hex").slice(0, 16);
}

export function issueTitle(finding) {
  const location = finding.line ? `${finding.file}:${finding.line}` : finding.file;
  return `[Security][${finding.severity}] ${finding.category} in ${location}`;
}

/**
 * @param {ReturnType<typeof normalizeFindings>[number]} finding
 * @param {{ repoUrl: string, sha: string, prNumber?: string|number, prUrl?: string }} context
 */
export function issueBody(finding, context) {
  const anchor = finding.line ? `#L${finding.line}` : "";
  const permalink = `${context.repoUrl}/blob/${context.sha}/${finding.file}${anchor}`;
  const source = context.prUrl ? `[PR #${context.prNumber}](${context.prUrl})` : `commit ${context.sha.slice(0, 7)}`;
  const confidence = finding.confidence === null ? "n/a" : `${Math.round(finding.confidence * 100)} %`;
  return [
    `| | |`,
    `| --- | --- |`,
    `| **Severity** | \`${finding.severity}\` |`,
    `| **Category** | \`${finding.category}\` |`,
    `| **Location** | [\`${finding.file}${finding.line ? `:${finding.line}` : ""}\`](${permalink}) |`,
    `| **Confidence** | ${confidence} |`,
    `| **Found in** | ${source} |`,
    ``,
    `## Description`,
    ``,
    finding.description,
    ``,
    `## Exploit scenario`,
    ``,
    finding.exploitScenario || "_not provided_",
    ``,
    `## Recommendation`,
    ``,
    finding.recommendation || "_not provided_",
    ``,
    `## Definition of done`,
    ``,
    `- [ ] Fix implemented (or finding documented as accepted risk / false positive)`,
    `- [ ] Test or manual verification described in the fixing PR`,
    ``,
    `---`,
    `_Created automatically by the AI security review (\`.github/workflows/security.yml\`). Verify before acting – AI findings can be false positives._`,
    ``,
    `<!-- security-fingerprint: ${fingerprint(finding)} -->`,
  ].join("\n");
}

/** Markdown table for the job summary / terminal. */
export function summaryTable(findings) {
  if (findings.length === 0) return "No security findings. ✅\n";
  const rows = findings.map((f) =>
    `| ${f.severity} | \`${f.category}\` | \`${f.file}${f.line ? `:${f.line}` : ""}\` | ${f.description.replace(/\|/g, "\\|").replace(/\n/g, " ")} |`);
  return ["| Severity | Category | Location | Description |", "| --- | --- | --- | --- |", ...rows].join("\n") + "\n";
}
