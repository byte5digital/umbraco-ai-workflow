#!/usr/bin/env bash
# Creates the next numbered ADR from docs/adr/0000-template.md and adds it to docs/adr/README.md.
# Usage: new-adr.sh "Title of the decision"
set -euo pipefail

title="${1:?Usage: new-adr.sh \"Title of the decision\"}"
root="$(git rev-parse --show-toplevel 2>/dev/null || pwd)"
dir="$root/docs/adr"

last="$(ls "$dir" | grep -E '^[0-9]{4}-' | sed -E 's/^([0-9]{4}).*/\1/' | sort -n | tail -1)"
next="$(printf '%04d' $((10#${last:-0} + 1)))"
slug="$(echo "$title" | iconv -f utf-8 -t ascii//TRANSLIT 2>/dev/null | tr '[:upper:]' '[:lower:]' | sed -E 's/[^a-z0-9]+/-/g; s/^-|-$//g')"
file="$dir/$next-$slug.md"

sed -e "s/^# NNNN – Titel/# $next – $title/" -e "s/^- \*\*Datum:\*\* .*/- **Datum:** $(date +%Y-%m-%d)/" \
    "$dir/0000-template.md" > "$file"
echo "| [$next]($next-$slug.md) | $title | Vorgeschlagen |" >> "$dir/README.md"
echo "$file"
