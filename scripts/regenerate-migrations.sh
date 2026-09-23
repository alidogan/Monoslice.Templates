#!/usr/bin/env bash
# Regenerates the InitialCreate migrations of the sample Catalog module for every database provider.
# EF Core reuses an existing model snapshot file with the same name, so each provider's folder is
# moved out of the project while the other provider's migration is generated.
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
SOLUTION="$ROOT/templates/solution"
MODULE="src/Modules/Catalog/Catalog"
MIGRATIONS="$SOLUTION/$MODULE/Data/Migrations"
STASH="$(mktemp -d)"

cd "$SOLUTION"
dotnet tool restore >/dev/null

rm -rf "$MIGRATIONS"
mkdir -p "$MIGRATIONS"

for provider in PostgreSql SqlServer; do
  database="$provider"
  [[ "$provider" == "PostgreSql" ]] && database="PostgreSQL"

  echo "Generating $provider migration..."
  TemplateDatabase="$database" dotnet ef migrations add InitialCreate \
    --project "$MODULE" \
    --startup-project src/Bootstrapper/Api \
    --output-dir "Data/Migrations/$provider"

  mv "$MIGRATIONS/$provider" "$STASH/$provider"
done

mv "$STASH"/* "$MIGRATIONS/"
rmdir "$STASH"
echo "Done: $(find "$MIGRATIONS" -name '*.cs' | wc -l | tr -d ' ') files in $MIGRATIONS"
