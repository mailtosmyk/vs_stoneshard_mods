#!/usr/bin/env python3
"""Bump Alchemy version (if needed), sync *Version.cs, prepend change.log entries."""
from __future__ import annotations

import argparse
import json
import re
import sys
from pathlib import Path

MODS_JSON = Path(__file__).resolve().parents[1] / "mods.json"
ROOT = Path(__file__).resolve().parents[4]


def load_cfg() -> dict:
    return json.loads(MODS_JSON.read_text(encoding="utf-8"))


def read_version(folder: Path) -> str:
    return (folder / "build.txt").read_text(encoding="utf-8").strip()


def write_version(folder: Path, version: str) -> None:
    (folder / "build.txt").write_text(version + "\n", encoding="utf-8")


def bump_patch(version: str) -> str:
    parts = [int(x) for x in version.split(".")]
    parts[-1] += 1
    return ".".join(str(x) for x in parts)


def sync_version_cs(path: Path, version: str) -> None:
    if not path.is_file():
        return
    text = path.read_text(encoding="utf-8")
    updated = re.sub(r'Value = "[^"]+"', f'Value = "{version}"', text)
    path.write_text(updated, encoding="utf-8")


def prepend_changelog(path: Path, version: str, notes: list[str]) -> None:
    block_lines = [version] + [f"- {n}" for n in notes] + [""]
    block = "\n".join(block_lines) + "\n"
    existing = path.read_text(encoding="utf-8") if path.is_file() else ""
    if existing.startswith(version + "\n"):
        print(f"changelog already has {version}: {path}")
        return
    path.write_text(block + existing, encoding="utf-8")
    print(f"changelog -> {version}: {path}")


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument(
        "mods",
        nargs="*",
        default=["necromancy", "summoner", "alchemy"],
        help="Mod keys from mods.json (default: all)",
    )
    ap.add_argument(
        "--note",
        action="append",
        default=[],
        help="Changelog bullet (repeatable). Default: rebuild note.",
    )
    ap.add_argument(
        "--bump-alchemy",
        action="store_true",
        help="Bump alchemy/build.txt patch before sync (Alchemy has no auto-bump).",
    )
    args = ap.parse_args()
    cfg = load_cfg()
    notes = args.note or [
        "Rebuild package for ModShardLauncher Enhanced / Nexus .sml release."
    ]

    for key in args.mods:
        mod = cfg["mods"][key]
        folder = ROOT / mod["folder"]
        version = read_version(folder)
        if key == "alchemy" and args.bump_alchemy:
            version = bump_patch(version)
            write_version(folder, version)
            print(f"alchemy build.txt -> {version}")
        sync_version_cs(ROOT / mod["version_cs"], version)
        prepend_changelog(folder / "change.log", version, notes)
        print(f"{key} version={version}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
