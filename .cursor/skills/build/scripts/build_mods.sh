#!/usr/bin/env bash
# Build one or more mods into ModShardLauncher-Enhanced/Mods.
set -euo pipefail

SKILL_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
ROOT="$(cd "$SKILL_DIR/../../.." && pwd)"
CFG="$SKILL_DIR/mods.json"

MSL_DIR="$(python3 -c "import json; print(json.load(open('$CFG'))['msl_dir'])")"
OUT_DIR="$(python3 -c "import json; print(json.load(open('$CFG'))['out_dir'])")"
NET10_ROOT="$(python3 -c "import json; print(json.load(open('$CFG'))['dotnet_root_net10'])")"

export MSL_DIR OUT_DIR
mkdir -p "$OUT_DIR"

if [[ $# -eq 0 ]]; then
  mods=(necromancy summoner alchemy)
else
  mods=("$@")
fi

for key in "${mods[@]}"; do
  script="$(python3 -c "import json; print(json.load(open('$CFG'))['mods']['$key']['build_script'])")"
  needs_net10="$(python3 -c "import json; print(json.dumps(json.load(open('$CFG'))['mods']['$key']['needs_net10']))")"
  echo "===== BUILD $key ====="
  if [[ "$needs_net10" == "true" ]]; then
    export DOTNET_ROOT="$NET10_ROOT"
    export PATH="$DOTNET_ROOT:$PATH"
  else
    unset DOTNET_ROOT || true
    export PATH="${HOME}/.dotnet:${PATH}"
  fi
  ( cd "$ROOT" && ./"$script" )
done
