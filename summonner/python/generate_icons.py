#!/usr/bin/env python3
"""Generate 25x24 RGBA skill icons for Summoner (pixel-art style)."""

from __future__ import annotations

from pathlib import Path

from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parent.parent
OUT = ROOT / "Sprites"

# Distinct palettes per skill (R,G,B)
SKILLS: dict[str, tuple[tuple[int, int, int], str]] = {
    "s_summon_wolf": ((160, 160, 170), "wolf"),
    "s_unsummon": ((90, 90, 110), "x"),
    "s_hunt_command": ((180, 90, 40), "hunt"),
    "s_mend_pack": ((70, 160, 90), "heal"),
    "s_pack_instincts": ((120, 140, 80), "pack"),
    "s_alphas_howl": ((200, 180, 60), "howl"),
    "s_summon_crawler": ((70, 120, 50), "crawl"),
    "s_summon_bear": ((120, 80, 50), "bear"),
    "s_summon_bison": ((150, 110, 70), "bison"),
    "s_savage_bond": ((160, 60, 60), "bond"),
    "s_summon_gulon": ((90, 50, 40), "gulon"),
    "s_kindred_spirit": ((140, 100, 180), "spirit"),
    "s_primal_apex": ((210, 120, 40), "apex"),
}


def draw_icon(kind: str, color: tuple[int, int, int]) -> Image.Image:
    im = Image.new("RGBA", (25, 24), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    r, g, b = color
    dark = (max(0, r - 50), max(0, g - 50), max(0, b - 50), 255)
    mid = (r, g, b, 255)
    light = (min(255, r + 40), min(255, g + 40), min(255, b + 40), 255)
    # frame
    d.rectangle([1, 1, 23, 22], outline=dark, fill=(20, 18, 16, 220))
    d.rectangle([2, 2, 22, 21], outline=mid)

    if kind in ("wolf", "bear", "bison", "gulon", "crawl"):
        # animal silhouette blob + head
        d.ellipse([6, 8, 18, 18], fill=mid, outline=dark)
        d.ellipse([14, 5, 21, 12], fill=light, outline=dark)
        if kind == "crawl":
            for x in (5, 9, 13, 17):
                d.line([x, 16, x - 2, 21], fill=dark, width=1)
        elif kind == "wolf":
            d.polygon([(16, 5), (18, 2), (20, 5)], fill=dark)
            d.polygon([(18, 5), (21, 3), (22, 6)], fill=dark)
        elif kind == "bear":
            d.ellipse([14, 3, 17, 6], fill=dark)
            d.ellipse([19, 3, 22, 6], fill=dark)
        elif kind == "bison":
            d.ellipse([7, 6, 16, 14], fill=dark)
        elif kind == "gulon":
            d.polygon([(15, 6), (22, 8), (15, 10)], fill=dark)
    elif kind == "x":
        d.line([7, 6, 17, 17], fill=light, width=2)
        d.line([17, 6, 7, 17], fill=light, width=2)
    elif kind == "hunt":
        d.polygon([(12, 4), (18, 12), (12, 11), (6, 12)], fill=mid, outline=dark)
        d.line([12, 11, 12, 19], fill=dark, width=2)
    elif kind == "heal":
        d.rectangle([11, 5, 14, 18], fill=light)
        d.rectangle([6, 10, 19, 13], fill=light)
    elif kind == "pack":
        for cx in (8, 13, 18):
            d.ellipse([cx - 3, 9, cx + 3, 16], fill=mid, outline=dark)
    elif kind == "howl":
        d.arc([5, 5, 20, 20], 200, 340, fill=light)
        d.arc([8, 8, 17, 17], 200, 340, fill=mid)
    elif kind == "bond":
        d.ellipse([5, 7, 13, 16], outline=light, width=2)
        d.ellipse([12, 7, 20, 16], outline=mid, width=2)
    elif kind == "spirit":
        d.ellipse([8, 5, 17, 18], fill=mid, outline=light)
        d.point([(11, 10), (14, 10)], fill=light)
    elif kind == "apex":
        d.polygon([(12, 3), (21, 19), (12, 15), (3, 19)], fill=mid, outline=light)
    else:
        d.ellipse([6, 6, 18, 18], fill=mid)
    return im


def main() -> None:
    OUT.mkdir(parents=True, exist_ok=True)
    for name, (color, kind) in SKILLS.items():
        path = OUT / f"{name}_0.png"
        draw_icon(kind, color).save(path)
        print("wrote", path.name)
    # category branch placeholder (small tree bg strip)
    branch = Image.new("RGBA", (162, 257), (18, 16, 14, 255))
    bd = ImageDraw.Draw(branch)
    bd.rectangle([0, 0, 161, 256], outline=(80, 70, 50, 255))
    for y in (55, 118, 172, 220):
        bd.line([12, y, 150, y], fill=(60, 50, 40, 255))
    branch.save(OUT / "s_summoner_tree_0.png")
    print("wrote s_summoner_tree_0.png")


if __name__ == "__main__":
    main()
