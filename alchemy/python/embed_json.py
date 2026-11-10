#!/usr/bin/env python3
"""Re-embed src/ingredients.json into AlchemyJsonResource.ContentBase64."""

from __future__ import annotations

import base64
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
JSON_PATH = ROOT / "src" / "ingredients.json"
OUT_PATH = ROOT / "src" / "AlchemyJsonResource.cs"

TEMPLATE = """namespace Alchemy;

/// <summary>
/// Fallback ingredients.json payload when filesystem/embedded resource is unavailable
/// (MSL ModSources compile may strip EmbeddedResource).
/// </summary>
internal static class AlchemyJsonResource
{{
    const string ContentBase64 = "{b64}";

    internal static bool TryOpen(out Stream stream)
    {{
        if (string.IsNullOrEmpty(ContentBase64))
        {{
            stream = null!;
            return false;
        }}

        stream = new MemoryStream(Convert.FromBase64String(ContentBase64));
        return true;
    }}

    internal static bool HasContent => !string.IsNullOrEmpty(ContentBase64);
}}
"""


def main() -> None:
    if not JSON_PATH.is_file():
        raise FileNotFoundError(f"ingredients.json not found: {JSON_PATH}")
    raw = JSON_PATH.read_bytes()
    b64 = base64.b64encode(raw).decode("ascii")
    OUT_PATH.write_text(TEMPLATE.format(b64=b64), encoding="utf-8")
    print(f"Embedded ingredients.json into {OUT_PATH.relative_to(ROOT)} ({len(raw)} bytes)")


if __name__ == "__main__":
    main()
