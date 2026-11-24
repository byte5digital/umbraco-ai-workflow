#!/usr/bin/env bash
# Starts the official Umbraco Developer MCP server (stdio) against the local site – used by Claude Code via .mcp.json.
# Credentials come from .env (the API user is created by Seeding/Mcp/McpApiUserSetup.cs on Umbraco start).
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
ENV_FILE="$ROOT/.env"
[[ -f "$ENV_FILE" ]] || { echo "Missing $ENV_FILE – run: cp .env.example .env" >&2; exit 1; }
read_env() { grep -E "^$1=" "$ENV_FILE" | tail -n1 | cut -d= -f2- || true; }

SECRET="$(read_env McpApiUser__ClientSecret)"
[[ -n "$SECRET" ]] || { echo "McpApiUser__ClientSecret missing in $ENV_FILE (openssl rand -hex 32)" >&2; exit 1; }

export UMBRACO_CLIENT_ID="umbraco-back-office-mcp"
export UMBRACO_CLIENT_SECRET="$SECRET"
export UMBRACO_BASE_URL="$(read_env UMBRACO_MCP_BASE_URL)"
export UMBRACO_BASE_URL="${UMBRACO_BASE_URL:-https://localhost:44380}"
# Local dev certificate of "dotnet run" (https) is self-signed
[[ "$UMBRACO_BASE_URL" == https://localhost* ]] && export NODE_TLS_REJECT_UNAUTHORIZED=0

exec npx -y @umbraco-cms/mcp-dev@18 "$@"
