#!/usr/bin/env python3
"""Render a Necromancy skill-tree preview from necromancy.json.

Reads skills + tree.tiers + tree.edges, places icons from Sprites/, draws
prerequisite connections, and can show filled (color) or empty (grayscale)
icons based on an unlocked set.

Layout modes:
  auto  — place children under parents from tree.edges (default)
  game  — use in-game s_nmancy_reworked slot coordinates
"""

from __future__ import annotations

import argparse
import json
import sys
from collections import defaultdict
from dataclasses import dataclass, field
from pathlib import Path
from typing import Iterable

from PIL import Image, ImageDraw, ImageEnhance, ImageFont

# Game-layout centers on s_nmancy_reworked (162×257).
# Keep slug → X mapping aligned with NecromancySchool.WriteCategoryEvents.
TIER_Y = {1: 55, 2: 118, 3: 172, 4: 220}
GAME_SLUG_X: dict[str, int] = {
    "desecration": 33,
    "bone_ward": 81,
    "summon_skeleton": 130,
    "essence_leech": 24,
    "deaths_blessing": 62,
    "grave_blight": 100,
    "resurrection": 138,
    "death_touch": 24,
    "corpse_burst": 81,
    "black_sepulchre": 138,
    "mastery_of_binding": 24,
    "immortality": 62,
    "blooming_madness": 100,
    "wraith_summoning": 138,
}
GAME_FALLBACK_X = {
    1: (33, 81, 130),
    2: (24, 62, 100, 138),
    3: (24, 81, 138),
    4: (24, 62, 100, 138),
}

BG = (24, 20, 18, 255)
LINE = (168, 148, 118, 255)
LINE_DIM = (96, 84, 68, 255)
LABEL = (220, 210, 195, 255)
TITLE = (235, 225, 210, 255)


@dataclass(frozen=True)
class Skill:
    id: str
    name: str
    slug: str
    tier: int
    image: str
    type: str


@dataclass(frozen=True)
class Edge:
    src: str
    dst: str


@dataclass
class Node:
    skill: Skill
    cx: float = 0.0
    cy: float = 0.0
    filled: bool = True
    icon: Image.Image | None = field(default=None, repr=False)


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


def parse_tree(data: dict, skills: dict[str, Skill]) -> tuple[list[list[str]], list[Edge]]:
    tree = data.get("tree") or {}
    tiers_raw = tree.get("tiers") or []
    tiers: list[list[str]] = []
    if tiers_raw:
        for row in sorted(tiers_raw, key=lambda t: int(t.get("tier") or 0)):
            ids = [sid for sid in (row.get("skills") or []) if sid in skills]
            if ids:
                tiers.append(ids)
    else:
        by_tier: dict[int, list[str]] = {}
        for skill in skills.values():
            by_tier.setdefault(skill.tier, []).append(skill.id)
        for tier in sorted(by_tier):
            tiers.append(by_tier[tier])

    edges: list[Edge] = []
    for raw in tree.get("edges") or []:
        src, dst = raw.get("from"), raw.get("to")
        if src in skills and dst in skills:
            edges.append(Edge(str(src), str(dst)))

    # Fallback: derive edges from skill requirements if tree.edges is missing.
    if not edges:
        for skill in skills.values():
            for parent in skill_requirements(data, skill.id):
                if parent in skills:
                    edges.append(Edge(parent, skill.id))
    return tiers, edges


def skill_requirements(data: dict, skill_id: str) -> list[str]:
    for raw in data.get("skills") or []:
        if raw.get("id") == skill_id:
            req = raw.get("requirements") or {}
            return list(req.get("skills") or [])
    return []


def resolve_sprite(sprites_dir: Path, image_stem: str) -> Path | None:
    if not image_stem:
        return None
    candidates = [
        sprites_dir / f"{image_stem}_0.png",
        sprites_dir / f"{image_stem}.png",
    ]
    for path in candidates:
        if path.is_file():
            return path
    matches = sorted(sprites_dir.glob(f"{image_stem}_*.png"))
    return matches[0] if matches else None


def load_icon(path: Path | None, size: int, filled: bool) -> Image.Image:
    if path is None:
        icon = Image.new("RGBA", (size, size), (40, 36, 34, 255))
        draw = ImageDraw.Draw(icon)
        draw.rectangle((1, 1, size - 2, size - 2), outline=(90, 80, 70, 255))
        draw.line((6, 6, size - 7, size - 7), fill=(90, 80, 70, 255), width=2)
        draw.line((size - 7, 6, 6, size - 7), fill=(90, 80, 70, 255), width=2)
        return icon

    icon = Image.open(path).convert("RGBA")
    if icon.width != size or icon.height != size:
        icon = icon.resize((size, size), Image.Resampling.NEAREST)

    if not filled:
        gray = icon.convert("LA").convert("RGBA")
        gray.putalpha(icon.split()[-1])
        gray = ImageEnhance.Brightness(gray).enhance(0.72)
        gray = ImageEnhance.Contrast(gray).enhance(0.9)
        icon = gray
    return icon


def layout_game(
    skills: dict[str, Skill],
    tiers: list[list[str]],
    unlocked: set[str] | None,
    scale: float,
) -> dict[str, Node]:
    nodes: dict[str, Node] = {}
    for row in tiers:
        for index, skill_id in enumerate(row):
            skill = skills[skill_id]
            xs = GAME_FALLBACK_X.get(skill.tier, GAME_FALLBACK_X[2])
            game_x = GAME_SLUG_X.get(skill.slug)
            if game_x is None:
                game_x = xs[index] if index < len(xs) else xs[-1] + (index - len(xs) + 1) * 38
            game_y = TIER_Y.get(skill.tier, 55 + max(skill.tier - 1, 0) * 55)
            nodes[skill_id] = Node(
                skill=skill,
                cx=game_x * scale,
                cy=game_y * scale,
                filled=_is_filled(skill, unlocked),
            )
    return nodes


def _branch_width(skill_id: str, children: dict[str, list[str]], memo: dict[str, int]) -> int:
    """Leaf-column width of a subtree (used to space roots)."""
    if skill_id in memo:
        return memo[skill_id]
    kids = children.get(skill_id) or []
    if not kids:
        memo[skill_id] = 1
        return 1
    memo[skill_id] = max(1, sum(_branch_width(kid, children, memo) for kid in kids))
    return memo[skill_id]


def layout_auto(
    skills: dict[str, Skill],
    tiers: list[list[str]],
    edges: list[Edge],
    unlocked: set[str] | None,
    col_gap: float,
    row_gap: float,
) -> dict[str, Node]:
    """Place each tier so children sit under parents from tree.edges."""
    parents: dict[str, list[str]] = defaultdict(list)
    children: dict[str, list[str]] = defaultdict(list)
    for edge in edges:
        parents[edge.dst].append(edge.src)
        children[edge.src].append(edge.dst)

    tree_order = {sid: index for index, sid in enumerate(sid for row in tiers for sid in row)}
    width_memo: dict[str, int] = {}

    xs: dict[str, float] = {}
    nodes: dict[str, Node] = {}

    for row_index, row in enumerate(tiers):
        cy = row_index * row_gap
        row_xs: dict[str, float] = {}

        roots = [sid for sid in row if not any(pid in xs for pid in parents.get(sid, []))]
        if roots:
            roots = sorted(roots, key=lambda s: tree_order[s])
            # Pack root centers by subtree width so sibling fans don't collide.
            cursor = 0.0
            centers: list[float] = []
            for sid in roots:
                width = _branch_width(sid, children, width_memo)
                span = max(1, width) * col_gap
                centers.append(cursor + span / 2)
                cursor += span
            mid = cursor / 2
            for sid, center in zip(roots, centers):
                row_xs[sid] = center - mid

        non_roots = [sid for sid in row if sid not in row_xs]
        for sid in non_roots:
            parent_ids = [pid for pid in parents.get(sid, []) if pid in xs]
            row_xs[sid] = sum(xs[pid] for pid in parent_ids) / len(parent_ids)

        # Fan siblings under a shared parent using each child's subtree width.
        fanned: set[str] = set()
        for parent_id, kids in children.items():
            if parent_id not in xs:
                continue
            tier_kids = [kid for kid in kids if kid in row_xs]
            if len(tier_kids) < 2:
                continue
            tier_kids.sort(key=lambda kid: tree_order[kid])
            widths = [_branch_width(kid, children, width_memo) for kid in tier_kids]
            total = sum(widths) * col_gap
            cursor = xs[parent_id] - total / 2
            for kid, width in zip(tier_kids, widths):
                span = width * col_gap
                row_xs[kid] = cursor + span / 2
                cursor += span
                fanned.add(kid)

        for sid in non_roots:
            if sid in fanned:
                continue
            parent_ids = [pid for pid in parents.get(sid, []) if pid in xs]
            if len(parent_ids) == 1:
                row_xs[sid] = xs[parent_ids[0]]

        for _ in range(32):
            ordered = sorted(row, key=lambda sid: (row_xs[sid], tree_order[sid]))
            moved = False
            for left_id, right_id in zip(ordered, ordered[1:]):
                need = col_gap - (row_xs[right_id] - row_xs[left_id])
                if need > 1e-6:
                    row_xs[left_id] -= need / 2
                    row_xs[right_id] += need / 2
                    moved = True
            if not moved:
                break

        for sid in row:
            xs[sid] = row_xs[sid]
            skill = skills[sid]
            nodes[sid] = Node(
                skill=skill,
                cx=row_xs[sid],
                cy=cy,
                filled=_is_filled(skill, unlocked),
            )

    if nodes:
        mid = sum(n.cx for n in nodes.values()) / len(nodes)
        for node in nodes.values():
            node.cx -= mid
    return nodes


def _is_filled(skill: Skill, unlocked: set[str] | None) -> bool:
    if unlocked is None:
        return True
    return skill.id in unlocked or skill.slug in unlocked


def connector_points(
    x0: float,
    y0: float,
    x1: float,
    y1: float,
    icon_size: int,
) -> list[tuple[float, float]]:
    """Elbow from bottom-center of parent to top-center of child.

    Horizontal segment is placed nearer the child so sibling fans stay clean
    and mid-tier nodes are not falsely joined by a shared bus.
    """
    half = icon_size / 2
    start = (x0, y0 + half - 1)
    end = (x1, y1 - half + 1)
    if abs(x0 - x1) < 0.5:
        return [start, end]
    # Drop most of the way, then sideways, then into the child.
    elbow_y = end[1] - max(10.0, (end[1] - start[1]) * 0.22)
    if elbow_y <= start[1]:
        elbow_y = (start[1] + end[1]) / 2
    return [start, (x0, elbow_y), (x1, elbow_y), end]


def draw_connections(
    canvas: Image.Image,
    nodes: dict[str, Node],
    edges: list[Edge],
    icon_size: int,
    line_width: int,
) -> None:
    draw = ImageDraw.Draw(canvas)
    for edge in edges:
        a = nodes.get(edge.src)
        b = nodes.get(edge.dst)
        if a is None or b is None:
            continue
        color = LINE if (a.filled and b.filled) else LINE_DIM
        points = connector_points(a.cx, a.cy, b.cx, b.cy, icon_size)
        draw.line(points, fill=color, width=line_width, joint="curve")


def try_font(size: int) -> ImageFont.ImageFont:
    candidates = [
        "/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf",
        "/usr/share/fonts/truetype/liberation/LiberationSans-Regular.ttf",
        "/usr/share/fonts/truetype/freefont/FreeSans.ttf",
    ]
    for path in candidates:
        if Path(path).is_file():
            return ImageFont.truetype(path, size=size)
    return ImageFont.load_default()


def parse_unlocked(values: Iterable[str] | None, skills: dict[str, Skill]) -> set[str] | None:
    if values is None:
        return None  # all filled
    unlocked: set[str] = set()
    by_slug = {s.slug: s.id for s in skills.values()}
    by_name = {s.name.lower(): s.id for s in skills.values()}
    for raw in values:
        for token in raw.split(","):
            key = token.strip()
            if not key:
                continue
            if key in skills:
                unlocked.add(key)
            elif key in by_slug:
                unlocked.add(by_slug[key])
            elif key.lower() in by_name:
                unlocked.add(by_name[key.lower()])
            else:
                print(f"warning: unknown skill for --filled: {key}", file=sys.stderr)
    return unlocked


def render(
    data: dict,
    sprites_dir: Path,
    out_path: Path,
    unlocked: set[str] | None,
    scale: float,
    icon_size: int,
    show_labels: bool,
    title: str | None,
    layout: str,
) -> Path:
    skills = parse_skills(data)
    if not skills:
        raise SystemExit("no skills found in JSON")

    tiers, edges = parse_tree(data, skills)
    if layout == "game":
        nodes = layout_game(skills, tiers, unlocked, scale=scale)
    else:
        col_gap = icon_size * 2.35
        row_gap = icon_size * 2.8
        nodes = layout_auto(
            skills,
            tiers,
            edges,
            unlocked,
            col_gap=col_gap,
            row_gap=row_gap,
        )

    for node in nodes.values():
        sprite = resolve_sprite(sprites_dir, node.skill.image)
        node.icon = load_icon(sprite, icon_size, node.filled)

    pad_x = max(36, icon_size)
    pad_y = max(28, icon_size // 2)
    label_space = max(22, icon_size // 2) if show_labels else 8
    title_space = 42 if title else 12

    xs = [n.cx for n in nodes.values()]
    ys = [n.cy for n in nodes.values()]
    min_x, max_x = min(xs), max(xs)
    min_y, max_y = min(ys), max(ys)

    width = int(max_x - min_x + icon_size + pad_x * 2)
    height = int(max_y - min_y + icon_size + pad_y * 2 + label_space + title_space)

    ox = pad_x + icon_size / 2 - min_x
    oy = title_space + pad_y + icon_size / 2 - min_y
    for node in nodes.values():
        node.cx += ox
        node.cy += oy

    canvas = Image.new("RGBA", (width, height), BG)
    line_width = max(2, icon_size // 24)
    draw_connections(canvas, nodes, edges, icon_size=icon_size, line_width=line_width)

    draw = ImageDraw.Draw(canvas)
    label_font = try_font(max(11, icon_size // 6))
    title_font = try_font(max(14, icon_size // 4))

    if title:
        school = str(data.get("name") or "Skill Tree")
        text = title if title != "auto" else f"{school} Skill Tree"
        bbox = draw.textbbox((0, 0), text, font=title_font)
        tw = bbox[2] - bbox[0]
        draw.text(((width - tw) / 2, 12), text, font=title_font, fill=TITLE)

    for node in nodes.values():
        assert node.icon is not None
        x = int(round(node.cx - icon_size / 2))
        y = int(round(node.cy - icon_size / 2))
        canvas.alpha_composite(node.icon, (x, y))
        if show_labels:
            name = node.skill.name
            bbox = draw.textbbox((0, 0), name, font=label_font)
            tw = bbox[2] - bbox[0]
            tx = node.cx - tw / 2
            ty = node.cy + icon_size / 2 + 4
            draw.text((tx, ty), name, font=label_font, fill=LABEL)

    out_path.parent.mkdir(parents=True, exist_ok=True)
    canvas.save(out_path)
    return out_path


def build_argparser() -> argparse.ArgumentParser:
    root = Path(__file__).resolve().parents[1]
    p = argparse.ArgumentParser(description=__doc__)
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
        "--out",
        type=Path,
        default=root / "_debug" / "necromancy-skill-tree.png",
        help="Output PNG path",
    )
    p.add_argument(
        "--filled",
        action="append",
        default=None,
        metavar="SKILL",
        help=(
            "Unlocked/filled skill id, slug, or name. Repeatable or comma-separated. "
            "Omit to fill all skills; pass --filled '' for all empty."
        ),
    )
    p.add_argument(
        "--layout",
        choices=("auto", "game"),
        default="auto",
        help="Node placement: auto (under parents) or game (in-game slots)",
    )
    p.add_argument(
        "--scale",
        type=float,
        default=4.0,
        help="Scale for --layout game relative to branch coords (default: 4)",
    )
    p.add_argument(
        "--icon-size",
        type=int,
        default=96,
        help="Icon pixel size (default: 96)",
    )
    p.add_argument("--no-labels", action="store_true", help="Hide skill name labels")
    p.add_argument(
        "--title",
        default="auto",
        help="Title text, 'auto' for school name, or empty to hide",
    )
    return p


def main(argv: list[str] | None = None) -> int:
    args = build_argparser().parse_args(argv)
    data = load_json(args.json)
    skills = parse_skills(data)

    filled_args = args.filled
    if filled_args is not None and len(filled_args) == 1 and filled_args[0].strip() == "":
        unlocked: set[str] | None = set()
    else:
        unlocked = parse_unlocked(filled_args, skills)

    icon_size = max(16, int(args.icon_size))
    title = None if args.title == "" else args.title
    out = render(
        data=data,
        sprites_dir=args.sprites,
        out_path=args.out,
        unlocked=unlocked,
        scale=max(1.0, float(args.scale)),
        icon_size=icon_size,
        show_labels=not args.no_labels,
        title=title,
        layout=args.layout,
    )
    print(f"wrote {out}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
