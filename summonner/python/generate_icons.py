#!/usr/bin/env python3
"""Extract frameless 25×24 skill icons from Summoner's Arcane Bestiary Icons.png."""

from __future__ import annotations

import colorsys
from pathlib import Path

from PIL import Image

ROOT = Path(__file__).resolve().parent.parent
OUT = ROOT / "Sprites"
ICON_SIZE = (25, 24)

# Post-extract hue shifts (sheet art is purple-tinted).
# Hunt Command: keep grey wolf, push aura to red.
# Kindred Spirit: full icon to green (passive bond).
RECOLOR: dict[str, tuple[float, bool, float]] = {
    # name -> (target_hue, preserve_neutral, sat_floor)
    "s_hunt_command": (0.0, True, 0.30),
    "s_kindred_spirit": (0.33, False, 0.22),
}

# Sheet is laid out 3 / 3 / 4 / 3 (tree shape). Cells mapped by art, not tree column order.
LAYOUT: list[list[str]] = [
    ["s_summon_wolf", "s_alphas_howl", "s_unsummon"],
    ["s_pack_instincts", "s_hunt_command", "s_mend_pack"],
    ["s_summon_crawler", "s_summon_bear", "s_summon_bison", "s_savage_bond"],
    ["s_summon_gulon", "s_kindred_spirit", "s_primal_apex"],
]


def find_sheet() -> Path:
    matches = sorted(ROOT.glob("*Arcane Bestiary Icons*.png"))
    if not matches:
        raise FileNotFoundError(f"No Arcane Bestiary Icons sheet under {ROOT}")
    return matches[0]


def is_content(px, x: int, y: int) -> bool:
    r, g, b, a = px[x, y]
    return a > 0 and (r > 12 or g > 12 or b > 12)


def is_frame_stone(px, x: int, y: int) -> bool:
    """Grey/purple stone border (not the dark inner art plate)."""
    r, g, b, a = px[x, y]
    if a <= 0:
        return False
    luma = 0.299 * r + 0.587 * g + 0.114 * b
    chroma = abs(r - g) + abs(g - b) + abs(b - r)
    return luma > 55 and chroma < 90 and abs(r - b) < 50


def contiguous_bands(density: list[int], threshold: int) -> list[tuple[int, int]]:
    bands: list[tuple[int, int]] = []
    in_band = False
    start = 0
    for i, value in enumerate(density):
        if value > threshold and not in_band:
            start = i
            in_band = True
        elif value <= threshold and in_band:
            bands.append((start, i - 1))
            in_band = False
    if in_band:
        bands.append((start, len(density) - 1))
    return bands


def detect_cells(img: Image.Image) -> list[tuple[int, int, int, int]]:
    w, h = img.size
    px = img.load()
    row_dens = [sum(1 for x in range(w) if is_content(px, x, y)) for y in range(h)]
    rows = contiguous_bands(row_dens, 50)
    if len(rows) != 4:
        raise RuntimeError(f"Expected 4 icon rows, found {len(rows)}: {rows}")

    cells: list[tuple[int, int, int, int]] = []
    expected_cols = [3, 3, 4, 3]
    for row_i, (y0, y1) in enumerate(rows):
        col_dens = [
            sum(1 for y in range(y0, y1 + 1) if is_content(px, x, y)) for x in range(w)
        ]
        cols = contiguous_bands(col_dens, 20)
        if len(cols) != expected_cols[row_i]:
            raise RuntimeError(
                f"Row {row_i}: expected {expected_cols[row_i]} icons, found {len(cols)}: {cols}"
            )
        for x0, x1 in cols:
            cells.append((x0, y0, x1, y1))
    return cells


def frame_inset(px, x0: int, y0: int, x1: int, y1: int) -> tuple[int, int, int, int]:
    """Shrink cell bbox past the stone frame on all sides."""

    def edge_inset(vertical: bool, from_start: bool) -> int:
        if vertical:
            length = y1 - y0 + 1
            span = range(x0 + 8, x1 - 7)
        else:
            length = x1 - x0 + 1
            span = range(y0 + 8, y1 - 7)
        span_list = list(span)
        if not span_list:
            return 14

        seen_frame = False
        best = 14
        for offset in range(length // 3):
            if vertical:
                y = y0 + offset if from_start else y1 - offset
                frac = sum(1 for x in span_list if is_frame_stone(px, x, y)) / len(span_list)
            else:
                x = x0 + offset if from_start else x1 - offset
                frac = sum(1 for y in span_list if is_frame_stone(px, x, y)) / len(span_list)

            if frac >= 0.5:
                seen_frame = True
                best = offset
                continue
            if seen_frame and frac < 0.25:
                # clear stone + thin inner bevel before the art plate
                return min(length // 3 - 1, offset + 6)
            best = offset
        return max(16, best)

    left = edge_inset(False, True)
    right = edge_inset(False, False)
    top = edge_inset(True, True)
    bottom = edge_inset(True, False)

    ix0 = x0 + left
    iy0 = y0 + top
    ix1 = x1 - right
    iy1 = y1 - bottom
    if ix1 - ix0 < 32 or iy1 - iy0 < 32:
        # Fallback: ~12% inset
        pad_x = max(12, int((x1 - x0 + 1) * 0.12))
        pad_y = max(12, int((y1 - y0 + 1) * 0.12))
        return x0 + pad_x, y0 + pad_y, x1 - pad_x, y1 - pad_y
    return ix0, iy0, ix1, iy1


def to_game_icon(src: Image.Image) -> Image.Image:
    """Downscale frameless art to Stoneshard skill icon size (25×24)."""
    tw, th = ICON_SIZE
    sw, sh = src.size
    # Cover-fit: scale so the shorter side fills, then center-crop.
    scale = max(tw / sw, th / sh)
    rw, rh = max(tw, int(round(sw * scale))), max(th, int(round(sh * scale)))
    scaled = src.resize((rw, rh), Image.Resampling.LANCZOS)
    left = (rw - tw) // 2
    top = (rh - th) // 2
    return scaled.crop((left, top, left + tw, top + th))


def recolor_purple(
    img: Image.Image,
    target_h: float,
    *,
    preserve_neutral: bool,
    sat_floor: float = 0.0,
) -> Image.Image:
    """Shift purple/violet glow pixels toward ``target_h`` (HSV hue 0–1)."""
    out = img.convert("RGBA").copy()
    px = out.load()
    w, h = out.size
    for y in range(h):
        for x in range(w):
            r, g, b, a = px[x, y]
            if a < 10:
                continue
            hf, s, v = colorsys.rgb_to_hsv(r / 255, g / 255, b / 255)
            chroma = abs(r - g) + abs(g - b) + abs(b - r)
            if preserve_neutral and chroma < 45 and s < 0.28:
                continue
            purple = (s >= 0.10 and (hf >= 0.68 or hf <= 0.04)) or (
                b > g + 6 and (r + b) > 28 and s >= 0.08
            )
            if not purple:
                continue
            ns = min(1.0, max(s, sat_floor) * 1.08)
            nr, ng, nb = colorsys.hsv_to_rgb(target_h, ns, v)
            if v < 0.18:
                if target_h < 0.1:
                    nr, ng, nb = min(1.0, v * 2.4), v * 0.12, v * 0.15
                else:
                    nr, ng, nb = v * 0.18, min(1.0, v * 2.2), v * 0.3
            px[x, y] = (int(nr * 255), int(ng * 255), int(nb * 255), a)
    return out


def write_tree_placeholder() -> None:
    branch = Image.new("RGBA", (162, 257), (18, 16, 14, 255))
    branch.save(OUT / "s_summoner_tree_0.png")


def main() -> None:
    sheet_path = find_sheet()
    sheet = Image.open(sheet_path).convert("RGBA")
    cells = detect_cells(sheet)
    px = sheet.load()

    names = [name for row in LAYOUT for name in row]
    if len(names) != len(cells):
        raise RuntimeError(f"Layout has {len(names)} skills but sheet has {len(cells)} cells")

    OUT.mkdir(parents=True, exist_ok=True)
    for name, (x0, y0, x1, y1) in zip(names, cells):
        ix0, iy0, ix1, iy1 = frame_inset(px, x0, y0, x1, y1)
        art = sheet.crop((ix0, iy0, ix1 + 1, iy1 + 1))
        icon = to_game_icon(art)
        if name in RECOLOR:
            target_h, preserve_neutral, sat_floor = RECOLOR[name]
            icon = recolor_purple(
                icon, target_h, preserve_neutral=preserve_neutral, sat_floor=sat_floor
            )
        out_path = OUT / f"{name}_0.png"
        icon.save(out_path)
        print(f"wrote {out_path.name} from cell ({x0},{y0})-({x1},{y1}) inset ({ix0},{iy0})-({ix1},{iy1})")

    write_tree_placeholder()
    print("wrote s_summoner_tree_0.png")


if __name__ == "__main__":
    main()
