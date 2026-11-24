import { test } from "node:test";
import assert from "node:assert/strict";
import { fingerprint, issueBody, issueTitle, normalizeFindings, summaryTable } from "./security-findings.mjs";

const sample = {
  findings: [
    { file: "./src/A.cs", line: 10, severity: "medium", category: "XSS", description: "Raw output", confidence: 0.9 },
    { file: "src/B.cs", line: 3, severity: "CRITICAL", category: "authz_missing", description: "No policy", confidence: 0.95,
      exploit_scenario: "GET /x", recommendation: "Add [Authorize]" },
    { file: "src/C.cs", severity: "LOW", category: "x", description: "speculative", confidence: 0.5 },
    { severity: "HIGH", description: "no file" },
  ],
};

test("normalizeFindings validates, maps CRITICAL to HIGH, filters by confidence and sorts by severity", () => {
  const result = normalizeFindings(sample);
  assert.deepEqual(result.map((f) => [f.severity, f.file, f.category]), [
    ["HIGH", "src/B.cs", "authz_missing"],
    ["MEDIUM", "src/A.cs", "xss"],
  ]);
});

test("normalizeFindings respects the minimum severity", () => {
  assert.equal(normalizeFindings(sample, { minSeverity: "HIGH" }).length, 1);
});

test("fingerprint ignores line numbers and whitespace", () => {
  const [high] = normalizeFindings(sample);
  assert.equal(fingerprint(high), fingerprint({ ...high, line: 99, description: " No   policy " .trim() }));
});

test("issue title and body contain severity, location and fingerprint", () => {
  const [high] = normalizeFindings(sample);
  assert.equal(issueTitle(high), "[Security][HIGH] authz_missing in src/B.cs:3");
  const body = issueBody(high, { repoUrl: "https://github.com/o/r", sha: "abcdef1234", prNumber: 7, prUrl: "https://github.com/o/r/pull/7" });
  assert.match(body, /blob\/abcdef1234\/src\/B\.cs#L3/);
  assert.match(body, /\[PR #7\]/);
  assert.match(body, /security-fingerprint: [a-f0-9]{16}/);
});

test("summaryTable renders an empty state", () => {
  assert.match(summaryTable([]), /No security findings/);
});
