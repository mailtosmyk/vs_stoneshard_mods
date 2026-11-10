#!/usr/bin/env bash
# Build Necromancy.sml (dotnet build + MSLM pack).
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
if [[ -z "${MSL_DIR:-}" ]]; then
  MSL_DIR="$(cd "$ROOT/../../msl" && pwd)"
fi
OUT_DIR="${OUT_DIR:-$MSL_DIR/Mods}"
MOD_NAME="${MOD_NAME:-Necromancy}"
CONFIG="${CONFIG:-Release}"
SKIP_VERSION_BUMP="${SKIP_VERSION_BUMP:-0}"

# Prefer a user-local or system dotnet install.
if [[ -z "${DOTNET_ROOT:-}" ]]; then
  if [[ -x "${HOME}/.dotnet/dotnet" ]]; then
    export DOTNET_ROOT="${HOME}/.dotnet"
  elif [[ -x /usr/share/dotnet/dotnet ]]; then
    export DOTNET_ROOT=/usr/share/dotnet
  fi
fi
export PATH="${DOTNET_ROOT:-}:${HOME}/.dotnet/tools:${PATH:-}"

if ! command -v dotnet >/dev/null 2>&1; then
  echo "error: dotnet SDK not found (need .NET 6+)." >&2
  exit 1
fi

if ! command -v python3 >/dev/null 2>&1; then
  echo "error: python3 not found." >&2
  exit 1
fi

if [[ ! -f "$MSL_DIR/ModShardLauncher.dll" ]]; then
  echo "error: ModShardLauncher.dll not found in $MSL_DIR" >&2
  exit 1
fi

if [[ ! -f "$ROOT/src/necromancy.json" ]]; then
  echo "error: necromancy.json missing in $ROOT/src" >&2
  exit 1
fi

mkdir -p "$OUT_DIR"

# Bump first so the packed DLL and build.txt share the same version.
# embed_json.py (also run by MSBuild BeforeCompile) refreshes ContentBase64 + version consts.
# CI sets SKIP_VERSION_BUMP=1 so release packaging does not mutate build.txt.
if [[ "$SKIP_VERSION_BUMP" != "1" ]]; then
  python3 "$ROOT/python/bump_build_version.py" "$ROOT/build.txt"
fi
python3 "$ROOT/python/embed_json.py" "$ROOT"

echo "Building $MOD_NAME ($CONFIG)..."
dotnet build "$ROOT/Necromancy.csproj" -c "$CONFIG" --nologo

DLL="$ROOT/bin/$CONFIG/net6.0/$MOD_NAME.dll"
if [[ ! -f "$DLL" ]]; then
  echo "error: build output missing: $DLL" >&2
  exit 1
fi

SML="$OUT_DIR/$MOD_NAME.sml"
echo "Packing $SML..."
python3 "$ROOT/python/pack_sml.py" \
  --root "$ROOT" \
  --dll "$DLL" \
  --out "$SML" \
  --name "$MOD_NAME" \
  --msl "$MSL_DIR"

# Also drop the raw DLL next to Mods for quick inspection / MSL refresh helpers.
cp -f "$DLL" "$OUT_DIR/$MOD_NAME.dll"

echo "Done. Mod version=$(tr -d '[:space:]' <"$ROOT/build.txt")"
