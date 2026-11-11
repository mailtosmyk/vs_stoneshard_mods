#!/usr/bin/env python3
"""Render a Necromancy skill-tree image from necromancy.json.

Modes
-----
locked tree (default)
    Blank panel from ``python/skill_tree/`` (background + top line). Every
    skill is a lock frame; all ``tree.edges`` are drawn as orthogonal links.
    No skill icons.

--icons
    Composite skill icons onto ``s_nmancy_reworked`` (sockets + link art already
    in the branch sprite). Icon centers match in-game layout.

--skills A,B,...
    Blank panel + lock frames for the listed skills only. Direct ``tree.edges``
    between them are drawn. Omit ``--filled`` to show icons in those sockets;
    ``--filled ''`` / ``--locked`` keeps locks only.

--empty-back
    Blank panel only (fill + top ornament; no skills or links).

Skill tokens (``--skills`` / ``--filled``) accept id, slug, or display name;
repeat the flag or use commas. ``--filled ''`` shows locks only.
"""

from __future__ import annotations

import argparse
import json
import sys
from dataclasses import dataclass
from pathlib import Path
from typing import Iterable

from PIL import Image, ImageDraw, ImageEnhance, ImageFont

# Centers on s_nmancy_reworked (162×257). Origin of skill icons is (12, 12).
BRANCH_SIZE = (162, 257)
ICON_ORIGIN = (12, 12)
ICON_NATIVE = (25, 24)
# Locked socket frame from skill_tree/locked_skill.png (31×31), centered on slot.
LOCK_ORIGIN = (15, 15)
LOCK_NATIVE = (31, 31)

# Dual-tone orthogonal links (sampled from s_nmancy_reworked).
LINK_DARK = (17, 16, 26, 255)
LINK_LIGHT = (44, 45, 57, 255)
# Stop links at the socket edge (half of 31px frame).
LINK_STOP = 16

TIER_Y = {1: 55, 2: 118, 3: 172, 4: 220}
SLUG_X: dict[str, int] = {
    # Left: Dark Bolt → Essence Leech → Death Touch → Corpse Burst
    # Center: Bone Ward → Blessing/Blight → Black Sepulchre → Immortality/Blooming
    # Right: Summon Skeleton → Resurrection → Mastery → Wraith
    # Tier 1 — 24 / 81 / 138
    "desecration": 24,
    "bone_ward": 81,
    "summon_skeleton": 138,
    # Tier 2 — 24 / 62 / 100 / 138
    "essence_leech": 24,
    "deaths_blessing": 62,
    "grave_blight": 100,
    "resurrection": 138,
    # Tier 3 — 24 / 81 / 138
    "death_touch": 24,
    "black_sepulchre": 81,
    "mastery_of_binding": 138,
    # Tier 4 — 24 / 62 / 100 / 138
    "corpse_burst": 24,
    "immortality": 62,
    "blooming_madness": 100,
    "wraith_summoning": 138,
}
FALLBACK_X = {
    1: (24, 81, 138),
    2: (24, 62, 100, 138),
    3: (24, 81, 138),
    4: (24, 62, 100, 138),
}

LABEL = (210, 205, 198, 255)
TITLE = (230, 225, 215, 255)

COMPONENTS_DIR = Path(__file__).resolve().parent / "skill_tree"


@dataclass(frozen=True)
class Skill:
    id: str
    name: str
    slug: str
    tier: int
    image: str
    type: str


@dataclass(frozen=True)
class Slot:
    skill: Skill
    cx: int
    cy: int
    filled: bool


@dataclass(frozen=True)
class Edge:
    src: str
    dst: str


def load_json(path: Path) -> dict:
    with path.open(encoding="utf-8") as f:
        return json.load(f)


def parse_skills(data: dict) -> dict[str, Skill]:
    skills: dict[str, Skill] = {}
    for raw in data.get("skills") or []:
        skill = Skill(
            id=str(raw["id"]),
            name=str(raw.get("name") or raw.get("slug") or raw["id"]),
            slug=str(raw.get("slug") or ""),
            tier=int(raw.get("tier") or 0),
            image=str(raw.get("image") or ""),
            type=str(raw.get("type") or "active"),
        )
        skills[skill.id] = skill
    return skills


def parse_edges(data: dict) -> list[Edge]:
    edges: list[Edge] = []
    for raw in (data.get("tree") or {}).get("edges") or []:
        src = str(raw.get("from") or raw.get("src") or "")
        dst = str(raw.get("to") or raw.get("dst") or "")
        if src and dst:
            edges.append(Edge(src=src, dst=dst))
    return edges


def tree_rows(data: dict, skills: dict[str, Skill]) -> list[list[str]]:
    tree = data.get("tree") or {}
    tiers_raw = tree.get("tiers") or []
    if tiers_raw:
        rows: list[list[str]] = []
        for row in sorted(tiers_raw, key=lambda t: int(t.get("tier") or 0)):
            ids = [sid for sid in (row.get("skills") or []) if sid in skills]
            if ids:
                rows.append(ids)
        return rows

    by_tier: dict[int, list[str]] = {}
    for skill in skills.values():
        by_tier.setdefault(skill.tier, []).append(skill.id)
    return [by_tier[t] for t in sorted(by_tier)]


def slot_position(skill: Skill, index_in_tier: int) -> tuple[int, int]:
    xs = FALLBACK_X.get(skill.tier, FALLBACK_X[2])
    cx = SLUG_X.get(skill.slug)
    if cx is None:
        cx = xs[index_in_tier] if index_in_tier < len(xs) else xs[-1] + (index_in_tier - len(xs) + 1) * 38
    cy = TIER_Y.get(skill.tier, 55 + max(skill.tier - 1, 0) * 55)
    return int(cx), int(cy)


def build_slots(
    data: dict,
    skills: dict[str, Skill],
    unlocked: set[str] | None,
    only: set[str] | None = None,
) -> list[Slot]:
    slots: list[Slot] = []
    for row in tree_rows(data, skills):
        for index, skill_id in enumerate(row):
            if only is not None and skill_id not in only:
                continue
            skill = skills[skill_id]
            cx, cy = slot_position(skill, index)
            filled = True if unlocked is None else (skill.id in unlocked or skill.slug in unlocked)
            slots.append(Slot(skill=skill, cx=cx, cy=cy, filled=filled))
    return slots


def resolve_skill_key(key: str, skills: dict[str, Skill]) -> str | None:
    by_slug = {s.slug: s.id for s in skills.values()}
    by_name = {s.name.lower(): s.id for s in skills.values()}
    if key in skills:
        return key
    if key in by_slug:
        return by_slug[key]
    if key.lower() in by_name:
        return by_name[key.lower()]
    return None


def parse_skill_list(
    values: Iterable[str] | None,
    skills: dict[str, Skill],
    *,
    flag: str,
) -> list[str]:
    """Parse repeatable/comma-separated skill tokens into ordered unique ids."""
    if values is None:
        return []
    ordered: list[str] = []
    seen: set[str] = set()
    for raw in values:
        for token in raw.split(","):
            key = token.strip()
            if not key:
                continue
            skill_id = resolve_skill_key(key, skills)
            if skill_id is None:
                print(f"warning: unknown skill for {flag}: {key}", file=sys.stderr)
                continue
            if skill_id not in seen:
                seen.add(skill_id)
                ordered.append(skill_id)
    return ordered


def resolve_sprite(sprites_dir: Path, image_stem: str) -> Path | None:
    if not image_stem:
        return None
    for path in (
        sprites_dir / f"{image_stem}_0.png",
        sprites_dir / f"{image_stem}.png",
    ):
        if path.is_file():
            return path
    matches = sorted(sprites_dir.glob(f"{image_stem}_*.png"))
    return matches[0] if matches else None


def load_icon(path: Path | None, filled: bool) -> Image.Image | None:
    if path is None:
        return None
    icon = Image.open(path).convert("RGBA")
    if icon.size != ICON_NATIVE:
        icon = icon.resize(ICON_NATIVE, Image.Resampling.NEAREST)
    if not filled:
        gray = icon.convert("LA").convert("RGBA")
        gray.putalpha(icon.split()[-1])
        gray = ImageEnhance.Brightness(gray).enhance(0.75)
        gray = ImageEnhance.Contrast(gray).enhance(0.85)
        icon = gray
    return icon


def find_branch_sprite(sprites_dir: Path, explicit: Path | None) -> Path:
    if explicit is not None:
        if not explicit.is_file():
            raise SystemExit(f"branch sprite not found: {explicit}")
        return explicit
    for name in ("s_nmancy_reworked_0.png", "s_nmancy_reworked.png"):
        path = sprites_dir / name
        if path.is_file():
            return path
    raise SystemExit(f"branch sprite s_nmancy_reworked not found in {sprites_dir}")


# Top decorative divider lives above the first socket row (~y=39).
ORNAMENT_BOTTOM = 28
TOP_LINE_Y = 17


def make_empty_back(branch: Image.Image | None = None) -> Image.Image:
    """Solid panel + top ornament only — no sockets, locks, or link lines."""
    bg_path = COMPONENTS_DIR / "background.png"
    top_path = COMPONENTS_DIR / "top_line.png"
    if bg_path.is_file() and top_path.is_file():
        out = Image.open(bg_path).convert("RGBA")
        top = Image.open(top_path).convert("RGBA")
        out.alpha_composite(top, (0, TOP_LINE_Y))
        return out

    if branch is None:
        raise SystemExit(
            f"skill_tree components missing in {COMPONENTS_DIR} and no branch sprite given"
        )
    branch = branch.convert("RGBA")
    # Sample panel fill from the left margin below the ornament (avoids sockets).
    sample_y = min(branch.height - 1, max(ORNAMENT_BOTTOM + 8, 100))
    fill = branch.getpixel((2, sample_y))
    out = Image.new("RGBA", branch.size, fill)
    # Keep the header filigree from the branch art.
    top = branch.crop((0, 0, branch.width, min(ORNAMENT_BOTTOM, branch.height)))
    out.paste(top, (0, 0))
    return out


def load_lock_frame() -> Image.Image | None:
    path = COMPONENTS_DIR / "locked_skill.png"
    if not path.is_file():
        return None
    frame = Image.open(path).convert("RGBA")
    if frame.size != LOCK_NATIVE:
        frame = frame.resize(LOCK_NATIVE, Image.Resampling.NEAREST)
    return frame


def scale_nearest(image: Image.Image, scale: int) -> Image.Image:
    scale = max(1, int(scale))
    if scale == 1:
        return image
    return image.resize(
        (image.width * scale, image.height * scale),
        Image.Resampling.NEAREST,
    )


def try_font(size: int) -> ImageFont.ImageFont:
    for path in (
        "/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf",
        "/usr/share/fonts/truetype/liberation/LiberationSans-Regular.ttf",
        "/usr/share/fonts/truetype/freefont/FreeSans.ttf",
    ):
        if Path(path).is_file():
            return ImageFont.truetype(path, size=size)
    return ImageFont.load_default()


def parse_unlocked(values: Iterable[str] | None, skills: dict[str, Skill]) -> set[str] | None:
    if values is None:
        return None
    return set(parse_skill_list(values, skills, flag="--filled"))


def _put(canvas: Image.Image, x: int, y: int, color: tuple[int, int, int, int]) -> None:
    if 0 <= x < canvas.width and 0 <= y < canvas.height:
        canvas.putpixel((x, y), color)


def draw_v_segment(canvas: Image.Image, x: int, y0: int, y1: int) -> None:
    """3px vertical link: dark | light | dark."""
    lo, hi = (y0, y1) if y0 <= y1 else (y1, y0)
    for y in range(lo, hi + 1):
        _put(canvas, x - 1, y, LINK_DARK)
        _put(canvas, x, y, LINK_LIGHT)
        _put(canvas, x + 1, y, LINK_DARK)


def draw_h_segment(canvas: Image.Image, y: int, x0: int, x1: int) -> None:
    """3px horizontal link: dark / light / dark."""
    lo, hi = (x0, x1) if x0 <= x1 else (x1, x0)
    for x in range(lo, hi + 1):
        _put(canvas, x, y - 1, LINK_DARK)
        _put(canvas, x, y, LINK_LIGHT)
        _put(canvas, x, y + 1, LINK_DARK)


def draw_junction(canvas: Image.Image, x: int, y: int) -> None:
    """Small cross accent at elbow / tee points."""
    for dx, dy in ((0, 0), (-1, 0), (1, 0), (0, -1), (0, 1)):
        _put(canvas, x + dx, y + dy, LINK_LIGHT)
    for dx, dy in ((-1, -1), (1, -1), (-1, 1), (1, 1)):
        _put(canvas, x + dx, y + dy, LINK_DARK)


def draw_orthogonal_link(canvas: Image.Image, x0: int, y0: int, x1: int, y1: int) -> None:
    """Elbow link between skill centers, stopping at socket edges."""
    if x0 == x1 and y0 == y1:
        return

    # Prefer parent→child downward flow (game tree is top-down).
    if y0 <= y1:
        sx, sy, dx, dy = x0, y0, x1, y1
    else:
        sx, sy, dx, dy = x1, y1, x0, y0

    start_y = sy + LINK_STOP
    end_y = dy - LINK_STOP
    if end_y < start_y:
        # Overlapping / same-tier: short stubs + horizontal at mid.
        mid_y = (sy + dy) // 2
        draw_v_segment(canvas, sx, sy + max(1, LINK_STOP // 2), mid_y)
        draw_h_segment(canvas, mid_y, sx, dx)
        draw_v_segment(canvas, dx, mid_y, dy - max(1, LINK_STOP // 2))
        if sx != dx:
            draw_junction(canvas, sx, mid_y)
            draw_junction(canvas, dx, mid_y)
        return

    mid_y = (start_y + end_y) // 2
    draw_v_segment(canvas, sx, start_y, mid_y)
    if sx != dx:
        draw_h_segment(canvas, mid_y, sx, dx)
        draw_junction(canvas, sx, mid_y)
        draw_junction(canvas, dx, mid_y)
    draw_v_segment(canvas, dx, mid_y, end_y)


def paint_slot(
    canvas: Image.Image,
    slot: Slot,
    sprites_dir: Path,
    *,
    lock_frame: Image.Image | None,
    empty_style: str,
    place_lock: bool,
    allow_icons: bool = True,
) -> None:
    if place_lock and lock_frame is not None:
        canvas.alpha_composite(
            lock_frame,
            (slot.cx - LOCK_ORIGIN[0], slot.cy - LOCK_ORIGIN[1]),
        )

    if not allow_icons:
        return

    if not slot.filled and empty_style == "lock":
        # Keep padlock art (from branch or placed lock frame).
        return

    sprite = resolve_sprite(sprites_dir, slot.skill.image)
    icon = load_icon(sprite, filled=slot.filled)
    if icon is None:
        return
    canvas.alpha_composite(
        icon,
        (slot.cx - ICON_ORIGIN[0], slot.cy - ICON_ORIGIN[1]),
    )


def draw_edges_for_slots(
    canvas: Image.Image,
    data: dict,
    slots: list[Slot],
) -> None:
    """Draw orthogonal links for every tree.edge whose ends are both shown."""
    by_id = {slot.skill.id: slot for slot in slots}
    selected = set(by_id)
    for edge in parse_edges(data):
        if edge.src not in selected or edge.dst not in selected:
            continue
        a, b = by_id[edge.src], by_id[edge.dst]
        draw_orthogonal_link(canvas, a.cx, a.cy, b.cx, b.cy)


def apply_chrome(
    canvas: Image.Image,
    data: dict,
    slots: list[Slot],
    fill_sample: tuple[int, int, int, int],
    scale: int,
    show_labels: bool,
    title: str | None,
) -> Image.Image:
    canvas = scale_nearest(canvas, scale)

    label_h = 0
    title_h = 0
    if title:
        title_h = max(28, 10 * scale)
    if show_labels:
        label_h = max(18, 7 * scale) * 4 + max(12, 4 * scale)

    if not title_h and not label_h:
        return canvas

    pad = max(8, 4 * scale)
    wide = canvas.width + pad * 2
    high = canvas.height + title_h + label_h + pad * 2
    framed = Image.new("RGBA", (wide, high), fill_sample)
    framed.alpha_composite(canvas, (pad, title_h + pad))
    draw = ImageDraw.Draw(framed)
    if title:
        school = str(data.get("name") or "Skill Tree")
        text = title if title != "auto" else school
        font = try_font(max(12, 5 * scale))
        bbox = draw.textbbox((0, 0), text, font=font)
        tw = bbox[2] - bbox[0]
        draw.text(((wide - tw) / 2, max(4, title_h // 4)), text, font=font, fill=TITLE)
    if show_labels:
        font = try_font(max(10, 4 * scale))
        by_tier: dict[int, list[Slot]] = {}
        for slot in slots:
            by_tier.setdefault(slot.skill.tier, []).append(slot)
        y = title_h + pad + canvas.height + max(6, 2 * scale)
        for tier in sorted(by_tier):
            names = "  ·  ".join(s.skill.name for s in by_tier[tier])
            line = f"T{tier}: {names}"
            bbox = draw.textbbox((0, 0), line, font=font)
            tw = bbox[2] - bbox[0]
            draw.text(((wide - tw) / 2, y), line, font=font, fill=LABEL)
            y += max(16, 6 * scale)
    return framed


def render_branch(
    data: dict,
    sprites_dir: Path,
    branch_path: Path,
    out_path: Path,
    unlocked: set[str] | None,
    scale: int,
    empty_style: str,
    show_labels: bool,
    title: str | None,
    only: set[str] | None = None,
    *,
    locked: bool = False,
    use_icons: bool = False,
) -> Path:
    skills = parse_skills(data)
    if not skills:
        raise SystemExit("no skills found in JSON")

    # Locked tree never fills icons, even if --filled listed skills.
    if locked:
        unlocked = set()

    slots = build_slots(data, skills, unlocked, only=only)
    if only is not None and not slots:
        raise SystemExit("no matching skills for --skills")

    branch = Image.open(branch_path).convert("RGBA")
    if branch.size != BRANCH_SIZE:
        # Still usable, but positions assume native 162×257 art.
        print(
            f"warning: branch size {branch.size} != {BRANCH_SIZE}; "
            "socket coordinates may be wrong",
            file=sys.stderr,
        )

    # Component panel: selected skills, explicit --locked, or default (no --icons).
    use_components = only is not None or locked or not use_icons
    allow_icons = use_icons or (only is not None and not locked)

    if use_components:
        canvas = make_empty_back(branch)
        lock_frame = load_lock_frame()
        if lock_frame is None:
            print(
                f"warning: {COMPONENTS_DIR / 'locked_skill.png'} missing; "
                "sockets will have no frame",
                file=sys.stderr,
            )
        draw_edges_for_slots(canvas, data, slots)
        for slot in slots:
            paint_slot(
                canvas,
                slot,
                sprites_dir,
                lock_frame=lock_frame,
                empty_style=empty_style,
                place_lock=True,
                allow_icons=allow_icons,
            )
    else:
        canvas = branch.copy()
        for slot in slots:
            paint_slot(
                canvas,
                slot,
                sprites_dir,
                lock_frame=None,
                empty_style=empty_style,
                place_lock=False,
                allow_icons=True,
            )

    canvas = apply_chrome(
        canvas,
        data,
        slots,
        fill_sample=branch.getpixel((1, 1)),
        scale=scale,
        show_labels=show_labels,
        title=title,
    )

    out_path.parent.mkdir(parents=True, exist_ok=True)
    canvas.save(out_path)
    return out_path


HELP_EPILOG = """
examples:
  # Full locked tree (default): panel + edges + lock frames, no skill icons
  python3 python/generate_skill_tree.py

  # Same, explicit
  python3 python/generate_skill_tree.py --locked

  # Full tree with skill icons on branch sprite
  python3 python/generate_skill_tree.py --icons

  # Icons tree, only some skills filled (rest stay as locks)
  python3 python/generate_skill_tree.py --icons \\
    --filled desecration,bone_ward,summon_skeleton

  # Selected skills on blank panel; draw direct tree.edges between them
  python3 python/generate_skill_tree.py \\
    --skills desecration,essence_leech,death_touch \\
    --out _debug/necromancy-skill-tree-selected.png

  # Selected skills as locks only
  python3 python/generate_skill_tree.py \\
    --skills black_sepulchre,blooming_madness,wraith_summoning --locked

  # Blank panel asset (no skills)
  python3 python/generate_skill_tree.py --empty-back

  # Labels + title + upscale
  python3 python/generate_skill_tree.py --title auto --labels --scale 4

components (used by locked tree / --skills / --empty-back):
  python/skill_tree/background.png
  python/skill_tree/top_line.png
  python/skill_tree/locked_skill.png

skill tokens:
  id (uuid), slug (desecration), or name ("Desecration")
  comma-separated and/or repeated flags
""".strip()


def build_argparser() -> argparse.ArgumentParser:
    root = Path(__file__).resolve().parents[1]
    p = argparse.ArgumentParser(
        prog="generate_skill_tree.py",
        description=__doc__,
        epilog=HELP_EPILOG,
        formatter_class=argparse.RawDescriptionHelpFormatter,
    )
    p.add_argument(
        "--json",
        type=Path,
        default=root / "src" / "necromancy.json",
        help="Path to skills JSON (default: src/necromancy.json)",
    )
    p.add_argument(
        "--sprites",
        type=Path,
        default=root / "Sprites",
        help="Directory with skill icon PNGs (default: Sprites/)",
    )
    p.add_argument(
        "--branch",
        type=Path,
        default=None,
        help="Branch background sprite (default: Sprites/s_nmancy_reworked_0.png)",
    )
    p.add_argument(
        "--out",
        type=Path,
        default=root / "_debug" / "necromancy-skill-tree.png",
        help="Output PNG path (default: _debug/necromancy-skill-tree.png)",
    )
    p.add_argument(
        "--empty-back",
        action="store_true",
        help=(
            "Write blank panel only (fill + top ornament; no sockets/skills/links). "
            "Ignores --skills/--filled/--empty-style/--labels."
        ),
    )
    p.add_argument(
        "--locked",
        action="store_true",
        help=(
            "Locked tree: blank panel + lock frames + tree.edges, no skill icons. "
            "Default when neither --icons nor --skills is set."
        ),
    )
    p.add_argument(
        "--icons",
        action="store_true",
        help=(
            "Composite skill icons onto the branch sprite (full tree). "
            "With --skills, fill selected sockets with icons instead of locks."
        ),
    )
    p.add_argument(
        "--skills",
        action="append",
        default=None,
        metavar="SKILL",
        help=(
            "Only these skills (id/slug/name; comma or repeat). Blank panel + lock "
            "frames; draws direct tree.edges between the selection."
        ),
    )
    p.add_argument(
        "--filled",
        action="append",
        default=None,
        metavar="SKILL",
        help=(
            "Which shown skills are filled (id/slug/name; comma or repeat). "
            "Omit = fill all shown when icons are enabled; --filled '' = locks only."
        ),
    )
    p.add_argument(
        "--empty-style",
        choices=("lock", "gray"),
        default="lock",
        help="Unfilled sockets: keep padlock art (default), or grayscale icons",
    )
    p.add_argument(
        "--scale",
        type=int,
        default=4,
        help="Nearest-neighbor upscale factor (default: 4)",
    )
    p.add_argument(
        "--labels",
        action="store_true",
        help="Append tier skill-name legend under the tree",
    )
    p.add_argument(
        "--title",
        default="",
        help="Optional title above the tree ('auto' = school name; empty = none)",
    )
    return p


def main(argv: list[str] | None = None) -> int:
    args = build_argparser().parse_args(argv)
    branch_path = find_branch_sprite(args.sprites, args.branch)
    out_path: Path = args.out
    root = Path(__file__).resolve().parents[1]
    default_out = root / "_debug" / "necromancy-skill-tree.png"

    if args.empty_back:
        if out_path == default_out:
            out_path = out_path.with_name("necromancy-skill-tree-back.png")
        branch = Image.open(branch_path).convert("RGBA")
        canvas = scale_nearest(make_empty_back(branch), args.scale)
        out_path.parent.mkdir(parents=True, exist_ok=True)
        canvas.save(out_path)
        print(f"wrote {out_path}")
        return 0

    data = load_json(args.json)
    skills = parse_skills(data)

    only: set[str] | None = None
    if args.skills is not None:
        selected = parse_skill_list(args.skills, skills, flag="--skills")
        if not selected:
            raise SystemExit("no valid skills in --skills")
        only = set(selected)

    # Default (no --icons / --skills): locked full tree from components.
    locked = bool(args.locked) or (only is None and not args.icons)

    filled_args = args.filled
    if locked or (
        filled_args is not None and len(filled_args) == 1 and filled_args[0].strip() == ""
    ):
        unlocked: set[str] | None = set()
    else:
        unlocked = parse_unlocked(filled_args, skills)

    # Icon compositing: --icons for full tree, or --skills without --locked.
    use_icons = (bool(args.icons) or only is not None) and not locked

    if out_path == default_out and locked and only is None:
        out_path = out_path.with_name("necromancy-skill-tree-locked.png")

    title = None if args.title == "" else args.title
    out = render_branch(
        data=data,
        sprites_dir=args.sprites,
        branch_path=branch_path,
        out_path=out_path,
        unlocked=unlocked,
        scale=args.scale,
        empty_style=args.empty_style,
        show_labels=args.labels,
        title=title,
        only=only,
        locked=locked,
        use_icons=use_icons,
    )
    print(f"wrote {out}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
