#!/usr/bin/env bash
# Build Summoner.sml (dotnet build + MSLM pack).
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
if [[ -z "${MSL_DIR:-}" ]]; then
  MSL_DIR="$(cd "$ROOT/../../msl" && pwd)"
fi
OUT_DIR="${OUT_DIR:-$MSL_DIR/Mods}"
MOD_NAME="${MOD_NAME:-Summoner}"
CONFIG="${CONFIG:-Release}"
SKIP_VERSION_BUMP="${SKIP_VERSION_BUMP:-0}"

if [[ -z "${DOTNET_ROOT:-}" ]]; then
  if [[ -x "${HOME}/.dotnet/dotnet" ]]; then
    export DOTNET_ROOT="${HOME}/.dotnet"
  elif [[ -x /usr/share/dotnet/dotnet ]]; then
    export DOTNET_ROOT=/usr/share/dotnet
  fi
fi
export PATH="${DOTNET_ROOT:-}:${HOME}/.dotnet/tools:${PATH:-}"

command -v dotnet >/dev/null || { echo "error: dotnet SDK not found" >&2; exit 1; }
command -v python3 >/dev/null || { echo "error: python3 not found" >&2; exit 1; }
[[ -f "$MSL_DIR/ModShardLauncher.dll" ]] || { echo "error: ModShardLauncher.dll missing in $MSL_DIR" >&2; exit 1; }
[[ -f "$ROOT/src/summonner.json" ]] || { echo "error: summonner.json missing" >&2; exit 1; }

mkdir -p "$OUT_DIR"

python3 "$ROOT/python/generate_icons.py"
if [[ "$SKIP_VERSION_BUMP" != "1" ]]; then
  python3 "$ROOT/python/bump_build_version.py" "$ROOT/build.txt"
fi
python3 "$ROOT/python/embed_json.py" "$ROOT"

echo "Building $MOD_NAME ($CONFIG)..."
dotnet build "$ROOT/Summoner.csproj" -c "$CONFIG" --nologo

DLL="$ROOT/bin/$CONFIG/net6.0/$MOD_NAME.dll"
[[ -f "$DLL" ]] || { echo "error: build output missing: $DLL" >&2; exit 1; }

SML="$OUT_DIR/$MOD_NAME.sml"
echo "Packing $SML..."
python3 "$ROOT/python/pack_sml.py" \
  --root "$ROOT" \
  --dll "$DLL" \
  --out "$SML" \
  --name "$MOD_NAME" \
  --msl "$MSL_DIR"

cp -f "$DLL" "$OUT_DIR/$MOD_NAME.dll"
echo "Done. Mod version=$(tr -d '[:space:]' <"$ROOT/build.txt")"
