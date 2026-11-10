#!/usr/bin/env bash
# CI helper: fetch ModShardLauncher refs, build Alchemy + Necromancy .sml packs.
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
MSL_VERSION="${MSL_VERSION:-v0.13.2.0}"
ARTIFACTS_DIR="${ARTIFACTS_DIR:-$ROOT/artifacts}"
# Default matches csproj HintPath: <mod>/../../msl
MSL_DIR="${MSL_DIR:-$(cd "$ROOT/.." && pwd)/msl}"
CONFIG="${CONFIG:-Release}"

mkdir -p "$ARTIFACTS_DIR" "$MSL_DIR"

need_msl=0
if [[ ! -f "$MSL_DIR/ModShardLauncher.dll" || ! -f "$MSL_DIR/UndertaleModLib.dll" ]]; then
  need_msl=1
fi

if [[ "$need_msl" -eq 1 ]]; then
  echo "Downloading ModShardLauncher $MSL_VERSION..."
  tmp="$(mktemp -d)"
  zip="$tmp/msl.zip"
  url="https://github.com/ModShardTeam/ModShardLauncher/releases/download/${MSL_VERSION}/msl.zip"
  curl -fsSL "$url" -o "$zip"
  # Archive roots at msl/...; extract parent so $MSL_DIR gets the DLLs.
  parent="$(dirname "$MSL_DIR")"
  mkdir -p "$parent"
  unzip -qo "$zip" -d "$parent"
  rm -rf "$tmp"
fi

if [[ ! -f "$MSL_DIR/ModShardLauncher.dll" ]]; then
  echo "error: ModShardLauncher.dll missing under $MSL_DIR" >&2
  exit 1
fi

export MSL_DIR
export OUT_DIR="$ARTIFACTS_DIR"
export SKIP_VERSION_BUMP=1
export CONFIG

echo "Building Necromancy..."
bash "$ROOT/necromancy/build-sml.sh"

echo "Building Alchemy..."
bash "$ROOT/alchemy/build-sml.sh"

NEC_VER="$(tr -d '[:space:]' <"$ROOT/necromancy/build.txt")"
ALC_VER="$(tr -d '[:space:]' <"$ROOT/alchemy/build.txt")"

mv -f "$ARTIFACTS_DIR/Necromancy.sml" "$ARTIFACTS_DIR/Necromancy-${NEC_VER}.sml"
mv -f "$ARTIFACTS_DIR/Alchemy.sml" "$ARTIFACTS_DIR/Alchemy-${ALC_VER}.sml"
rm -f "$ARTIFACTS_DIR/Necromancy.dll" "$ARTIFACTS_DIR/Alchemy.dll"

{
  echo "necromancy_version=$NEC_VER"
  echo "alchemy_version=$ALC_VER"
} >"$ARTIFACTS_DIR/versions.env"

echo "Artifacts:"
ls -la "$ARTIFACTS_DIR"/*.sml
cat "$ARTIFACTS_DIR/versions.env"
