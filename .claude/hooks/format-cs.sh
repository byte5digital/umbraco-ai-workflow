#!/usr/bin/env bash
# Claude Code PostToolUse hook: formats the edited C# file (whitespace only, fast). Never blocks the agent.
set -uo pipefail
file="$(jq -r '.tool_input.file_path // empty' 2>/dev/null)"
[[ "$file" == *.cs && -f "$file" ]] || exit 0
cd "${CLAUDE_PROJECT_DIR:-.}" || exit 0
dotnet format whitespace Opten.Umbraco.slnx --include "$file" >/dev/null 2>&1 || true
exit 0
