#!/usr/bin/env python3
"""Bump the minor version in build.txt (1.0.0 -> 1.1.0)."""

from __future__ import annotations

import sys
from pathlib import Path


def bump_minor(path: Path) -> str:
    raw = path.read_text(encoding="utf-8").strip() if path.exists() else "1.0.0"
    parts = (raw.split(".") + ["0", "0", "0"])[:3]
    try:
        major, minor, _patch = (int(p) for p in parts)
    except ValueError:
        major, minor = 1, 0
    minor += 1
    next_ver = f"{major}.{minor}.0"
    path.write_text(next_ver + "\n", encoding="utf-8")
    return next_ver


def main() -> int:
    root = Path(__file__).resolve().parent
    path = Path(sys.argv[1]) if len(sys.argv) > 1 else root / "build.txt"
    print(f"build.txt -> {bump_minor(path)}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
