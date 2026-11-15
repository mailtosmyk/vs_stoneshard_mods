# Stoneshard Mods (Vitali Smyk)

C# mods for [Stoneshard](https://store.steampowered.com/app/625960/Stoneshard/), built for [ModShardLauncher](https://github.com/ModShardTeam/ModShardLauncher) (MSL).

| Mod | Folder | What it does |
|-----|--------|--------------|
| **Necromancy** | `necromancy/` | Occult skill school — raise skeletons/wraiths, curses, bone magic (14 skills). |
| **Summoner** | `summonner/` | Beastbinder skill school — living companions (Wolf, Crawler, Bear, Bison, Gulon) with per-species XP (13 skills). |
| **Alchemy** | `alchemy/` | Extra craft recipes / custom elixirs, throwables, coatings, smelting, scrolls — **no new skill school**. Schematics at herbalists / Brynn alchemist; study by putting 3 of the same product + any gem in a craft station. |

Target game version: **Stoneshard 0.9.4.x** (`TargetVersion` 0.9.4.25) on the Steam **modbranch** beta (in-game version ends with `-vm`).

---

## Requirements

### To play

- Stoneshard on Steam → Properties → Betas → **modbranch**
- [ModShardLauncher](https://github.com/ModShardTeam/ModShardLauncher/releases) (or [Nexus](https://www.nexusmods.com/stoneshard/mods/29))
- [.NET Desktop Runtime 6.0](https://dotnet.microsoft.com/download/dotnet/6.0) if MSL does not start
- Official MSL play guide: https://modshardteam.github.io/ModShardLauncher/guides/how-to-play-mod.html

### To build from source

- **.NET SDK 6.0+** (`dotnet --version`)
- **Python 3** (embed JSON / pack `.sml`; Summoner also generates icons)
- **ModShardLauncher** unzipped so its DLLs sit next to this repo:

```text
<parent>/
  msl/                    # ModShardLauncher.dll, UndertaleModLib.dll, Mods/, …
  vs_stoneshard_mods/     # this repository
    alchemy/
    necromancy/
    summonner/
    scripts/
```

Each `.csproj` references `../../msl/ModShardLauncher.dll` and `UndertaleModLib.dll`.  
You can override the MSL path with `MSL_DIR=/path/to/msl`.

Optional for CI-style packaging: `curl`, `unzip`, `zip` (see `scripts/ci-build-sml.sh`).

---

## Install into the game

1. Switch Stoneshard to **modbranch** (version should end with `-vm`).
2. Install ModShardLauncher and follow the [how-to-play-mod](https://modshardteam.github.io/ModShardLauncher/guides/how-to-play-mod.html) guide.
3. In the Stoneshard folder, rename `data.win` → `vanilla.win` (keep a backup).
4. Put each mod’s **`.sml`** into MSL’s **`Mods`** folder  
   (local builds already copy there when `OUT_DIR` defaults to `$MSL_DIR/Mods`).
5. Open MSL → **Anvil (Mods)** → load `vanilla.win` → enable the mods you want → **Save as** e.g. `modded.win`.
6. Launch Stoneshard and select `modded.win`.

Tips:

- Back up saves before modding.
- If MSL looks frozen while loading a `.win`, wait — that is normal.
- Multiple MSL mods can be enabled together; load order / conflicts are not guaranteed.

Prebuilt zips (`.sml` + `.dll`) for Necromancy and Alchemy are published on GitHub Releases from the `develop` branch workflow. Summoner can be built locally the same way (see below).

---

## Build

### One mod

From a mod folder (example: Necromancy):

```bash
# Optional: skip auto bump of build.txt (Necromancy / Summoner)
export SKIP_VERSION_BUMP=1

# Optional overrides
# export MSL_DIR=/path/to/msl
# export OUT_DIR=/path/to/output   # default: $MSL_DIR/Mods
# export CONFIG=Release

./necromancy/build-sml.sh
# ./alchemy/build-sml.sh
# ./summonner/build-sml.sh
```

Each script:

1. Embeds JSON / prepares assets (Python)
2. Runs `dotnet build` (net6.0)
3. Packs `ModName.sml` and copies `ModName.dll` into `OUT_DIR`

**Note:** Necromancy and Summoner bump the patch version in `build.txt` on every local build unless `SKIP_VERSION_BUMP=1`. Alchemy does not auto-bump.

### CI / both Necromancy + Alchemy

```bash
export MSL_VERSION=v0.13.2.0   # MSL release tag if DLLs are missing
export ARTIFACTS_DIR="$(pwd)/artifacts"
./scripts/ci-build-sml.sh
```

This downloads MSL if needed, builds Necromancy and Alchemy with `SKIP_VERSION_BUMP=1`, and writes versioned zips under `artifacts/`.

### IDE

Open `Necromancy.csproj`, `Alchemy.csproj`, or `Summoner.csproj` in your IDE after `msl/` is in place. You can also symlink/copy a mod under MSL `ModSources` and compile from the MSL UI if you prefer that workflow.

---

## Repo layout

```text
alchemy/       Alchemy crafting / items mod
necromancy/    Necromancy skill school
summonner/     Summoner (beastbinder) skill school
scripts/       CI helper (ci-build-sml.sh)
.github/       Release workflow (Necromancy + Alchemy on develop)
```

Per-mod: `src/` (C# + JSON), `Codes/` (GML patches), `Sprites/`, `python/` (build helpers), `build.txt` (version), `build-sml.sh`.

---

## Support

- Patreon: https://www.patreon.com/c/vitali_smyk
- PayPal: https://www.paypal.com/donate/?hosted_button_id=FV3P3RCGG9MD8
