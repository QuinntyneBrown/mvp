#!/usr/bin/env bash
#
# Verifies that every relative href/src in the site resolves to a file that exists.
#
# The site is hand-written static HTML, so a renamed page or a mistyped asset path
# produces a 404 that no build step would otherwise catch. Run from anywhere:
#
#   ./website/check-links.sh
#
set -euo pipefail

root="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
failures=0
checked=0

while IFS= read -r page; do
  dir="$(dirname "$page")"
  references="$(
    grep -oE '(href|src)="[^"]*"' "$page" |
      sed -E 's/^(href|src)="//; s/"$//' |
      sed -E 's/[#?].*$//' |
      sort -u ||
      true
  )"

  while IFS= read -r target; do
    case "$target" in
      '' | http://* | https://* | mailto:* | data:* | //*) continue ;;
    esac

    checked=$((checked + 1))
    if [ ! -e "$dir/$target" ]; then
      echo "broken reference in ${page#"$root"/}: $target" >&2
      failures=$((failures + 1))
    fi
  done <<<"$references"
done < <(find "$root" -name '*.html' | sort)

if [ "$failures" -ne 0 ]; then
  echo "$failures broken reference(s) found." >&2
  exit 1
fi

echo "All $checked internal references resolve."
