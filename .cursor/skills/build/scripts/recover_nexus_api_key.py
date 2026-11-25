#!/usr/bin/env python3
"""Recover Nexus Mods API key into /tmp/.nexus_api_key (never print the key)."""
from __future__ import annotations

import json
import os
import re
import sys
import urllib.request
from pathlib import Path

ROOT = Path(__file__).resolve().parents[4]
MODS_JSON = Path(__file__).resolve().parents[1] / "mods.json"


def load_cfg() -> dict:
    return json.loads(MODS_JSON.read_text(encoding="utf-8"))


def extract_key(transcript: Path) -> str:
    best = ""
    for line in transcript.open(encoding="utf-8", errors="replace"):
        try:
            obj = json.loads(line)
        except json.JSONDecodeError:
            continue
        content = obj.get("message", {}).get("content", [])
        text = json.dumps(content)
        for m in re.finditer(r"NEXUSMODS_API_KEY='([^']+)'", text):
            cand = m.group(1)
            if len(cand) > len(best) and re.fullmatch(r"[A-Za-z0-9+/=_-]+", cand):
                best = cand
    if not best:
        raise SystemExit(f"key not found in {transcript}")
    return best


def validate(key: str) -> str:
    req = urllib.request.Request(
        "https://api.nexusmods.com/v1/users/validate.json",
        headers={
            "apikey": key,
            "Accept": "application/json",
            "Application-Name": "ModsUploader",
            "Application-Version": "1.0",
            "User-Agent": "ModsUploader/1.0",
        },
    )
    with urllib.request.urlopen(req, timeout=30) as resp:
        data = json.loads(resp.read())
    return data.get("name") or "ok"


def main() -> int:
    cfg = load_cfg()
    key_file = Path(cfg["nexus_key_file"])
    transcript = Path(cfg["nexus_key_transcript"])

    key = None
    if key_file.is_file() and key_file.stat().st_size >= 40:
        key = key_file.read_text(encoding="utf-8").strip()
        try:
            name = validate(key)
            print(f"key_ok cached user={name} len={len(key)}")
            return 0
        except Exception:
            key = None

    if not transcript.is_file():
        raise SystemExit(
            f"missing API key file {key_file} and transcript {transcript}; "
            "write a valid key to the key file first"
        )

    key = extract_key(transcript)
    key_file.write_text(key, encoding="utf-8")
    os.chmod(key_file, 0o600)
    name = validate(key)
    print(f"key_ok recovered user={name} len={len(key)}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
