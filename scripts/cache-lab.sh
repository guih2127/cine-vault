#!/usr/bin/env bash
set -euo pipefail

BASE_URL="${BASE_URL:-http://localhost:8080}"
NEW_TITLE="${1:-Renamed at $(date +%H:%M:%S)}"

TOKEN=$(curl -s -X POST "$BASE_URL/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email":"demo@cinevault.com","password":"Demo123!"}' | sed -E 's/.*"token":"([^"]+)".*/\1/')
AUTH="Authorization: Bearer $TOKEN"

MOVIE_ID=$(curl -s "$BASE_URL/api/movies" -H "$AUTH" | sed -E 's/^\[\{"id":"([^"]+)".*/\1/')

upstream() {
  grep -i "^x-upstream" | tr -d '\r' | sed -E 's/^[^:]+: */  /' | tr -d '\n'
}

read_movie() {
  for _ in 1 2 3 4 5 6; do
    curl -s -D - -o /tmp/movie.json "$BASE_URL/api/movies/$MOVIE_ID" -H "$AUTH" | upstream
    sed -E 's/.*"title":"([^"]+)".*/  -> \1/' /tmp/movie.json
    echo
  done
}

echo "Movie: $MOVIE_ID"
echo
echo "1) Reading (warms the cache on every replica)"
read_movie
echo
echo "2) Renaming to \"$NEW_TITLE\""
curl -s -D - -o /dev/null -X PUT "$BASE_URL/api/movies/$MOVIE_ID/title" \
  -H "$AUTH" -H "Content-Type: application/json" -d "{\"title\":\"$NEW_TITLE\"}" | upstream
echo "  -> PUT handled here, only this replica invalidated its cache"
echo
echo "3) Reading again"
read_movie
