#!/usr/bin/env python3
"""Zip .sml from Enhanced Mods and publish a new Nexus primary-file version."""
from __future__ import annotations

import argparse
import html
import json
import os
import re
import subprocess
import sys
import time
import urllib.error
import urllib.request
import zipfile
from pathlib import Path

MODS_JSON = Path(__file__).resolve().parents[1] / "mods.json"
ROOT = Path(__file__).resolve().parents[4]
API_BASE = "https://api.nexusmods.com/v3"


def load_cfg() -> dict:
    return json.loads(MODS_JSON.read_text(encoding="utf-8"))


def api(key: str, method: str, path: str, body=None):
    headers = {
        "apikey": key,
        "User-Agent": "ModsUploader/1.0",
        "Accept": "application/json",
    }
    data = None
    if body is not None:
        data = json.dumps(body).encode()
        headers["Content-Type"] = "application/json"
    req = urllib.request.Request(
        f"{API_BASE}{path}", data=data, headers=headers, method=method
    )
    try:
        with urllib.request.urlopen(req, timeout=180) as resp:
            raw = resp.read()
            return resp.status, (json.loads(raw) if raw else None)
    except urllib.error.HTTPError as e:
        raise RuntimeError(
            f"{method} {path} -> {e.code}: {e.read().decode(errors='replace')}"
        ) from None


def recent_changelog_bb(changelog_text: str, n: int = 4) -> str:
    lines = changelog_text.splitlines()
    recent: list[str] = []
    ver_count = 0
    for line in lines:
        if line and line[0].isdigit() and "." in line.split()[0]:
            ver_count += 1
            if ver_count > n:
                break
            recent.append(line)
        elif ver_count and ver_count <= n:
            if line.startswith("- "):
                recent.append("• " + line[2:])
            elif line.strip() == "":
                recent.append("")
            else:
                recent.append(line)
    return "\n".join(recent).strip()


def normalize_description(raw: str) -> str:
    """BBCode with real newlines. Nexus escapes HTML, so never send <br> tags."""
    text = html.unescape(raw)
    text = text.replace("\r\n", "\n").replace("\r", "\n")
    text = re.sub(r"<br\s*/?>", "\n", text, flags=re.I)
    text = re.sub(r"&lt;br\s*/?&gt;", "\n", text, flags=re.I)
    text = re.sub(r"\[b\]\s*([^\n\[]+?)\s*\n\s*\[/b\]", r"[b]\1[/b]", text, flags=re.I)

    def compact_table(m: re.Match) -> str:
        return re.sub(r"\n{2,}", "\n", m.group(0))

    text = re.sub(r"\[table\].*?\[/table\]", compact_table, text, flags=re.I | re.S)
    text = re.sub(r"\n{3,}", "\n\n", text)
    return text.strip() + "\n"


def patch_description(key: str, mod_uid: str, changelog_text: str, draft: Path) -> None:
    _, mod = api(key, "GET", f"/mods/{mod_uid}")
    desc = normalize_description(mod["data"]["description"])
    markers = [
        "[b]\xa0 Changelog (recent)[/b]",
        "[b]  Changelog (recent)[/b]",
        "[b] Changelog (recent)[/b]",
    ]
    support_markers = [
        "[b]\xa0 Support[/b]",
        "[b]  Support[/b]",
        "[b] Support[/b]",
    ]
    marker = next((m for m in markers if desc.find(m) >= 0), None)
    if marker is None:
        print("  WARN: changelog marker missing; skip description patch")
        return
    idx = desc.find(marker)
    support = -1
    for m in support_markers:
        j = desc.find(m, idx)
        if j >= 0:
            support = j
            break
    if support < 0:
        print("  WARN: support marker missing; skip description patch")
        return
    line_end = desc.find("\n", idx)
    if line_end < 0:
        line_end = idx + len(marker)
    recent = recent_changelog_bb(changelog_text)
    new_desc = desc[: line_end + 1] + "\n" + recent + "\n\n" + desc[support:]
    new_desc = normalize_description(new_desc)
    # Plain BBCode newlines only — sending <br /> gets HTML-escaped and shows literally.
    api(key, "PATCH", f"/mods/{mod_uid}", {"description": new_desc})
    draft.parent.mkdir(parents=True, exist_ok=True)
    draft.write_text(new_desc, encoding="utf-8")
    print("  description patched; draft synced")


def zip_sml(sml_path: Path, zip_path: Path) -> None:
    zip_path.unlink(missing_ok=True)
    with zipfile.ZipFile(zip_path, "w", compression=zipfile.ZIP_DEFLATED, compresslevel=9) as zf:
        zf.write(sml_path, arcname=sml_path.name)


def upload_one(key: str, cfg: dict, mod_key: str) -> None:
    mod = cfg["mods"][mod_key]
    out_dir = Path(cfg["out_dir"])
    sml = out_dir / f"{mod['sml_name']}.sml"
    if not sml.is_file():
        raise SystemExit(f"missing {sml}; build first")
    version = (ROOT / mod["folder"] / "build.txt").read_text(encoding="utf-8").strip()
    changelog = (ROOT / mod["folder"] / "change.log").read_text(encoding="utf-8").strip()
    zip_path = Path(f"/tmp/{mod['sml_name']}_{version}.zip")
    zip_sml(sml, zip_path)
    size = zip_path.stat().st_size
    print(f"\n=== {mod['display']} {version} ({size} bytes) ===")

    _, created = api(
        key,
        "POST",
        "/uploads/multipart",
        {"filename": zip_path.name, "size_bytes": str(size)},
    )
    data = created["data"]
    upload_id = data["id"]
    part_urls = data["part_presigned_urls"]
    part_size = int(data["part_size_bytes"])
    complete_url = data["complete_presigned_url"]
    parts = []
    with zip_path.open("rb") as f:
        for i, url in enumerate(part_urls, start=1):
            chunk = f.read(part_size)
            req = urllib.request.Request(
                url,
                data=chunk,
                method="PUT",
                headers={
                    "Content-Type": "application/octet-stream",
                    "Content-Length": str(len(chunk)),
                },
            )
            with urllib.request.urlopen(req, timeout=180) as resp:
                etag = (resp.headers.get("ETag") or "").strip('"')
                parts.append((i, etag))
                print(f"  part {i}/{len(part_urls)} ok")
    xml = (
        "<CompleteMultipartUpload>\n"
        + "\n".join(
            f"  <Part>\n    <PartNumber>{n}</PartNumber>\n    <ETag>{e}</ETag>\n  </Part>"
            for n, e in parts
        )
        + "\n</CompleteMultipartUpload>"
    )
    req = urllib.request.Request(
        complete_url,
        data=xml.encode(),
        method="POST",
        headers={"Content-Type": "application/xml"},
    )
    with urllib.request.urlopen(req, timeout=180) as resp:
        print("  complete multipart:", resp.status)
    api(key, "POST", f"/uploads/{upload_id}/finalise")
    for attempt in range(60):
        _, up = api(key, "GET", f"/uploads/{upload_id}")
        state = up["data"]["state"]
        print(f"  poll {attempt}: {state}")
        if state == "available":
            break
        if state in ("failed", "cancelled"):
            raise RuntimeError(up)
        time.sleep(2)
    else:
        raise RuntimeError("upload never became available")

    _, ver = api(
        key,
        "POST",
        f"/mod-files/{mod['file_id']}/versions",
        {
            "upload_id": upload_id,
            "name": mod["display"],
            "version": version,
            "file_category": "main",
            "archive_existing_file": True,
            "update_mod_version": True,
            "primary_mod_manager_download": True,
            "allow_mod_manager_download": True,
            "description": mod["description"],
        },
    )
    print("  version created:", json.dumps(ver)[:300])
    api(
        key,
        "POST",
        f"/mods/{mod['mod_uid']}/changelogs",
        {"version": version, "changelog": changelog},
    )
    print("  changelog posted")
    patch_description(key, mod["mod_uid"], changelog, ROOT / mod["draft"])
    _, live = api(key, "GET", f"/mods/{mod['mod_uid']}")
    page = mod["nexus_page"]
    print(
        f"  LIVE version={live['data'].get('version')} "
        f"https://www.nexusmods.com/stoneshard/mods/{page}"
    )


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument(
        "mods",
        nargs="*",
        default=["necromancy", "summoner", "alchemy"],
        help="Mod keys from mods.json",
    )
    args = ap.parse_args()
    cfg = load_cfg()
    key_file = Path(cfg["nexus_key_file"])
    if not key_file.is_file():
        subprocess.check_call(
            [sys.executable, str(Path(__file__).with_name("recover_nexus_api_key.py"))]
        )
    key = key_file.read_text(encoding="utf-8").strip()
    os.environ["NEXUSMODS_API_KEY"] = key
    for mod_key in args.mods:
        if mod_key not in cfg["mods"]:
            raise SystemExit(f"unknown mod key: {mod_key}")
        upload_one(key, cfg, mod_key)
    print("\nALL UPLOADS DONE")
    return 0


if __name__ == "__main__":
    sys.exit(main())
