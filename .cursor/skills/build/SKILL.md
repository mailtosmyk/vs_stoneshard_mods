---
name: build
description: >-
  Pack, rebuild, commit, push, and deploy Stoneshard mods (Necromancy, Summoner,
  Alchemy) to Nexus Mods. Use when the user says /build, rebuild all, commit push
  and update nexus, or deploy .sml releases.
---

# /build — pack, commit, push, Nexus

End-to-end release for this repo. Default target: **all three** mods. If the user names mods, only those.

Config + IDs: [mods.json](mods.json)

## Checklist

```
- [ ] 1. Build .sml/.dll into Enhanced Mods
- [ ] 2. Sync versions + change.log
- [ ] 3. Commit + push develop
- [ ] 4. Recover Nexus API key
- [ ] 5. Deploy zips to Nexus
- [ ] 6. Commit description draft syncs if changed
```

## 1. Build

```bash
# all
bash .cursor/skills/build/scripts/build_mods.sh

# subset
bash .cursor/skills/build/scripts/build_mods.sh necromancy alchemy
```

Notes:

- Output: `/home/vitali/Mods/ModShardLauncher-Enhanced/Mods/{Name}.sml` + `.dll`
- **Alchemy** needs Rider’s .NET 10 SDK (`needs_net10` in mods.json); the script sets `DOTNET_ROOT`
- Necromancy / Summoner **auto-bump** `build.txt` on each build unless `SKIP_VERSION_BUMP=1`
- Alchemy does **not** auto-bump — bump before or during step 2

## 2. Versions + changelog

After builds (so necro/summoner bumps already applied):

```bash
# Alchemy: bump patch + sync Version.cs + prepend change.log for selected mods
python3 .cursor/skills/build/scripts/prepare_release.py --bump-alchemy

# or only named mods; custom bullets:
python3 .cursor/skills/build/scripts/prepare_release.py necromancy \
  --note "Death Touch: fix impact heal."
```

If there were real feature/fix notes, prefer those `--note` lines over the default rebuild blurb.

Sync `*Version.cs` `Value` with `build.txt` for every mod you ship.

## 3. Commit + push

Stage version files, changelogs, and tracked bins from `mods.json` → `tracked_bins`.  
Do **not** commit `__pycache__/`.

Commit style (match repo):

```text
Rebuild Necromancy X.Y.Z, Summoner A.B.C, and Alchemy D.E.F for Nexus.
```

or per-mod feature messages when not a plain rebuild.

```bash
git push -u origin HEAD
```

User asked for push → push `develop`. Do not force-push.

## 4. Nexus API key

```bash
python3 .cursor/skills/build/scripts/recover_nexus_api_key.py
```

Writes `/tmp/.nexus_api_key` (chmod 600). **Never print the key.**

## 5. Deploy to Nexus

Packages each `{Name}.sml` from Enhanced `Mods/`, multipart-uploads, creates a new primary file version, posts changelog, patches the “Changelog (recent)” description block, syncs `*/_debug/nexus_preview/description_draft.txt`.

**Description BBCode:** PATCH with plain newlines only — never convert `\n` to `<br />` (Nexus escapes HTML and shows literal `<br />`). See rule `nexus-description-no-br`.

```bash
python3 .cursor/skills/build/scripts/deploy_nexus.py
# or: python3 .cursor/skills/build/scripts/deploy_nexus.py necromancy summoner
```

Pages (from mods.json):

| Mod | Page |
|-----|------|
| Necromancy | https://www.nexusmods.com/stoneshard/mods/448 |
| Summoner | https://www.nexusmods.com/stoneshard/mods/456 |
| Alchemy | https://www.nexusmods.com/stoneshard/mods/451 |

If `file_id` is wrong after a Nexus UI change, list `GET /v3/mods/{mod_uid}/files` → `data.mod_files` and update [mods.json](mods.json).

## 6. Draft follow-up commit

If description drafts changed:

```text
Sync Nexus description drafts after X.Y.Z publish.
```

Push again.

## Scope shortcuts

| User says | Do |
|-----------|----|
| `/build` / rebuild all + nexus | All mods, steps 1–6 |
| `/build necromancy` | Only that mod |
| rebuild only / no nexus | Steps 1–2 (and commit only if asked) |
| nexus only | Steps 4–6 (artifacts must already exist) |
