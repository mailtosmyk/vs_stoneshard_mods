#!/usr/bin/env bash
# Build Alchemy.sml (dotnet build + MSLM pack).
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
if [[ -z "${MSL_DIR:-}" ]]; then
  MSL_DIR="$(cd "$ROOT/../../msl" && pwd)"
fi
OUT_DIR="${OUT_DIR:-$MSL_DIR/Mods}"
MOD_NAME="${MOD_NAME:-Alchemy}"
CONFIG="${CONFIG:-Release}"

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

if [[ ! -f "$MSL_DIR/ModShardLauncher.dll" ]]; then
  echo "error: ModShardLauncher.dll not found in $MSL_DIR" >&2
  exit 1
fi

mkdir -p "$OUT_DIR"

echo "Building $MOD_NAME ($CONFIG)..."
dotnet build "$ROOT/Alchemy.csproj" -c "$CONFIG" --nologo

DLL="$ROOT/bin/$CONFIG/net6.0/$MOD_NAME.dll"
if [[ ! -f "$DLL" ]]; then
  echo "error: build output missing: $DLL" >&2
  exit 1
fi

if [[ -f "$ROOT/python/pack_sml.py" ]]; then
  SML="$OUT_DIR/$MOD_NAME.sml"
  echo "Packing $SML..."
  python3 "$ROOT/python/pack_sml.py" \
    --root "$ROOT" \
    --dll "$DLL" \
    --out "$SML" \
    --name "$MOD_NAME" \
    --msl "$MSL_DIR"
fi

cp -f "$DLL" "$OUT_DIR/$MOD_NAME.dll"
echo "Done. Mod version=$(tr -d '[:space:]' <"$ROOT/build.txt")"
