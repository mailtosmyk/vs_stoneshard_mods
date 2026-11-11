#!/usr/bin/env python3
"""Pack a built Alchemy.dll + assets into an MSL .sml (MSLM) file."""

from __future__ import annotations

import argparse
import struct
import sys
from pathlib import Path


def win_rel(mod_name: str, root: Path, path: Path) -> str:
    rel = path.relative_to(root).as_posix().replace("/", "\\")
    return f"{mod_name}\\{rel}"


def collect_files(root: Path, pattern: str) -> list[Path]:
    skip = {"bin", "obj", ".git"}
    files: list[Path] = []
    for path in sorted(root.rglob(pattern)):
        if not path.is_file():
            continue
        parts = set(path.relative_to(root).parts[:-1])
        if parts & skip or any(p.startswith(".") for p in path.relative_to(root).parts):
            continue
        files.append(path)
    return files


def collect_textures(root: Path) -> list[Path]:
    """Pack only mod icon + Sprites/*.png.

    Root-level debug screenshots (e.g. Screenshot_*.png) must not be imported as
    sprites — MSL treats every packed PNG as a texture and a bad/extra sprite
    can null out TextureEntry data and break data.win saves.
    """
    textures: list[Path] = []
    icon = root / "icon.png"
    if icon.is_file():
        textures.append(icon)
    sprites_dir = root / "Sprites"
    if sprites_dir.is_dir():
        textures.extend(sorted(p for p in sprites_dir.glob("*.png") if p.is_file()))
    return textures


def _count_byte_extends_msl_version(count: int) -> bool:
    """True if LE count's first byte is still scanned as part of MSLM version.

    FileReader reads the version as ``v`` + run of ``[0-9.]``, then treats the
    next byte as the texture count. Counts whose low byte is ``.`` (46) or a
    digit (48-57) are swallowed into the version and break the .sml (e.g. 53
    textures → ``…2.05`` → cannot read 1090519040 bytes).
    """
    b = count & 0xFF
    return b == 0x2E or 0x30 <= b <= 0x39


def msl_safe_texture_count(count: int) -> int:
    """Bump count until its low byte is safe for FileReader version scanning."""
    while _count_byte_extends_msl_version(count):
        count += 1
    return count


def read_msl_version(msl_dir: Path) -> str:
    dll = msl_dir / "ModShardLauncher.dll"
    # Fallback matches the local MSL install header (MSLMv0.13.2.0).
    default = "v0.13.2.0"
    if not dll.exists():
        return default
    try:
        data = dll.read_bytes()
        # Prefer FileVersion from VERSION_INFO if present; else default.
        marker = b"FileVersion\x00"
        idx = data.find(marker)
        if idx < 0:
            return default
        # UTF-16LE string usually follows in version resources; keep it simple.
        return default
    except OSError:
        return default


def pack(
    root: Path,
    dll_path: Path,
    out_path: Path,
    mod_name: str,
    version: str,
) -> None:
    texture_paths = collect_textures(root)
    scripts = collect_files(root, "*.lua")
    codes = collect_files(root, "*.gml")
    assemblies = collect_files(root, "*.asm")

    # Non-.png pads inflate the texture-group count only. TextureLoader skips
    # anything that does not end with "png", so these never become sprites.
    pad_n = msl_safe_texture_count(len(texture_paths)) - len(texture_paths)
    texture_entries: list[tuple[str, bytes]] = [
        (win_rel(mod_name, root, path), path.read_bytes()) for path in texture_paths
    ]
    for i in range(pad_n):
        texture_entries.append((f"{mod_name}\\msl_pad_{i + 1:02d}.pad", b"\0"))

    groups: list[list[tuple[str, bytes]]] = [
        texture_entries,
        [(win_rel(mod_name, root, p), p.read_bytes()) for p in scripts],
        [(win_rel(mod_name, root, p), p.read_bytes()) for p in codes],
        [(win_rel(mod_name, root, p), p.read_bytes()) for p in assemblies],
    ]
    entries: list[tuple[str, bytes]] = [e for group in groups for e in group]

    dll_bytes = dll_path.read_bytes()
    out_path.parent.mkdir(parents=True, exist_ok=True)

    with out_path.open("wb") as fs:
        fs.write(b"MSLM")
        fs.write(version.encode("utf-8"))

        offset = 0
        # Write four group indexes (textures, scripts, codes, assemblies).
        cursor = 0
        for group in groups:
            fs.write(struct.pack("<i", len(group)))
            for name, data in group:
                name_b = name.encode("utf-8")
                cursor += 1
                fs.write(struct.pack("<i", len(name_b)))
                fs.write(name_b)
                fs.write(struct.pack("<i", offset))
                fs.write(struct.pack("<i", len(data)))
                offset += len(data)

        for _, data in entries:
            fs.write(data)

        fs.write(struct.pack("<i", len(dll_bytes)))
        fs.write(dll_bytes)

    print(f"Packed {out_path} ({out_path.stat().st_size} bytes)")
    print(
        f"  textures={len(texture_paths)}(+{pad_n} msl pads) scripts={len(scripts)} "
        f"codes={len(codes)} assemblies={len(assemblies)} dll={len(dll_bytes)}"
    )


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--root", required=True, type=Path, help="Mod project root")
    parser.add_argument("--dll", required=True, type=Path, help="Built mod DLL")
    parser.add_argument("--out", required=True, type=Path, help="Output .sml path")
    parser.add_argument("--name", default="Alchemy", help="Mod / package name")
    parser.add_argument("--msl", type=Path, help="MSL directory (for version probe)")
    parser.add_argument("--version", default="", help="MSLM version string, e.g. v0.13.2.0")
    args = parser.parse_args()

    root = args.root.resolve()
    dll = args.dll.resolve()
    out = args.out.resolve()

    if not dll.is_file():
        print(f"error: DLL not found: {dll}", file=sys.stderr)
        return 1

    version = args.version.strip() or (
        read_msl_version(args.msl.resolve()) if args.msl else "v0.13.2.0"
    )
    if not version.startswith("v"):
        version = "v" + version

    pack(root, dll, out, args.name, version)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
