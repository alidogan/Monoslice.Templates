#!/usr/bin/env bash
# Packs the templates, installs them into an isolated template hive and, for each variant:
# generates a solution, adds a module and two features, then checks formatting, builds and runs all tests.
#
#   ./scripts/test-templates.sh                 # the default set of variants
#   ./scripts/test-templates.sh --all           # every variant
#   ./scripts/test-templates.sh sqlserver       # only the named variant(s)
#
# Environment: WORK_DIR (default: a temp dir), DOCKER_BUILD=1 to also build the API container image,
#              SKIP_TESTS=1 to only format and build, KEEP_OUTPUT=1 to keep each generated solution.
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
WORK_DIR="${WORK_DIR:-$(mktemp -d)}"
HIVE="$WORK_DIR/hive"
VERSION="0.0.0-local.$(date +%s)"

# Works with the bash 3.2 that ships with macOS (no associative arrays).
ALL_VARIANTS=(default sqlserver rabbitmq servicebus minimal sqlserver-rabbitmq-noauth)
DEFAULT_SET=(default sqlserver rabbitmq minimal)

variant_args() {
  case "$1" in
    default) echo "" ;;
    sqlserver) echo "--database sqlserver" ;;
    rabbitmq) echo "--broker rabbitmq" ;;
    servicebus) echo "--broker azureservicebus" ;;
    minimal) echo "--auth none --include-sample false --docker false --ci none" ;;
    sqlserver-rabbitmq-noauth) echo "--database sqlserver --broker rabbitmq --auth none --ci azure" ;;
    *) echo "Unknown variant: $1" >&2; return 1 ;;
  esac
}

if [[ "${1:-}" == "--all" ]]; then
  SELECTED=("${ALL_VARIANTS[@]}")
elif [[ $# -gt 0 ]]; then
  SELECTED=("$@")
else
  SELECTED=("${DEFAULT_SET[@]}")
fi

step() { printf '\n\033[1;34m==> %s\033[0m\n' "$*"; }

step "Packing Monoslice.Templates $VERSION"
dotnet pack "$ROOT/Monoslice.Templates.csproj" --output "$WORK_DIR/packages" -p:PackageVersion="$VERSION" --nologo -v quiet
dotnet new install "$WORK_DIR/packages/Monoslice.Templates.$VERSION.nupkg" --debug:custom-hive "$HIVE" >/dev/null

for variant in "${SELECTED[@]}"; do
  args="$(variant_args "$variant")"
  output="$WORK_DIR/$variant"

  step "[$variant] dotnet new ms-sln -n Acme.Shop $args"
  # shellcheck disable=SC2086
  dotnet new ms-sln -n Acme.Shop --output "$output" $args --debug:custom-hive "$HIVE"
  cd "$output"

  step "[$variant] ms-module Ordering + ms-feature CreateOrder (command) and GetOrderById (query)"
  dotnet new ms-module -n Ordering --debug:custom-hive "$HIVE"
  # The manual step printed by ms-module: register the module.
  sed -i.bak 's|^    \[$|    [\n        new Ordering.OrderingModule(),|' src/Bootstrapper/Api/ModuleRegistry.cs && rm src/Bootstrapper/Api/ModuleRegistry.cs.bak
  (
    cd src/Modules/Ordering/Ordering
    dotnet new ms-feature -n CreateOrder --aggregate Orders --kind command --debug:custom-hive "$HIVE"
    dotnet new ms-feature -n GetOrderById --aggregate Orders --kind query --debug:custom-hive "$HIVE"
  )
  dotnet tool restore >/dev/null
  dotnet restore >/dev/null
  dotnet ef migrations add InitialCreate -p src/Modules/Ordering/Ordering -s src/Bootstrapper/Api -o Data/Migrations

  step "[$variant] format, build, test"
  dotnet format --verify-no-changes
  dotnet build --configuration Release -warnaserror
  if [[ "${SKIP_TESTS:-0}" != "1" ]]; then
    dotnet test --configuration Release --no-build
  fi

  if [[ "${DOCKER_BUILD:-0}" == "1" && -f src/Bootstrapper/Api/Dockerfile ]]; then
    step "[$variant] docker build"
    docker build --file src/Bootstrapper/Api/Dockerfile --tag "monoslice-$variant:local" .
  fi

  cd "$ROOT"
  if [[ "${KEEP_OUTPUT:-0}" != "1" ]]; then
    rm -rf "$output"
  fi
done

step "All variants passed: ${SELECTED[*]} (output in $WORK_DIR)"
