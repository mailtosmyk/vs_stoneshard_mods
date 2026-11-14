#!/usr/bin/env python3
"""Render game-like Summoner skill panels (per tier) for Nexus description preview."""

from __future__ import annotations

import argparse
import html
import json
import re
from collections import defaultdict
from pathlib import Path

from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parents[1]
SPRITES = ROOT / "Sprites"
JSON_PATH = ROOT / "src" / "summonner.json"

# Stoneshard-ish tooltip palette
BG = (18, 16, 24, 255)
PANEL = (28, 26, 36, 255)
PANEL_EDGE = (58, 54, 72, 255)
TITLE = (232, 226, 214, 255)
MUTED = (150, 144, 160, 255)
BODY = (198, 192, 184, 255)
GREEN = (120, 198, 110, 255)
UNHOLY = (176, 112, 214, 255)
WHITE = (236, 232, 224, 255)
RED = (214, 96, 96, 255)
ORANGE = (220, 160, 90, 255)
ACCENT = (104, 148, 88, 255)

YELLOW = (230, 210, 120, 255)
COLOR_MAP = {
    "lg": GREEN,
    "ur": UNHOLY,
    "w": WHITE,
    "r": RED,
    "o": ORANGE,
    "ly": YELLOW,
}

# Layout order matching generate_skill_tree.py / in-game branch
TIER_ORDER = {
    1: ["hunt_command", "summon_wolf", "unsummon"],
    2: ["alphas_howl", "pack_instincts", "mend_pack"],
    3: ["summon_crawler", "summon_bear", "summon_bison", "savage_bond"],
    4: ["summon_gulon", "kindred_spirit", "primal_apex"],
}

TAG_RE = re.compile(r"~(lg|ur|w|r|o|ly)~(.*?)~/~", re.DOTALL)


def load_font(size: int, bold: bool = False) -> ImageFont.FreeTypeFont | ImageFont.ImageFont:
    candidates = [
        "/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf" if bold else "/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf",
        "/usr/share/fonts/truetype/liberation/LiberationSans-Bold.ttf" if bold else "/usr/share/fonts/truetype/liberation/LiberationSans-Regular.ttf",
    ]
    for path in candidates:
        if Path(path).is_file():
            return ImageFont.truetype(path, size)
    return ImageFont.load_default()


def strip_tags(text: str) -> str:
    return TAG_RE.sub(r"\2", text).replace("##", "\n").replace("#", "\n")


def parse_rich(text: str) -> list[tuple[str, tuple[int, int, int, int]]]:
    """Convert Stoneshard color tags into (segment, rgba) runs. # / ## -> newlines."""
    text = text.replace("##", "\n").replace("#", "\n")
    runs: list[tuple[str, tuple[int, int, int, int]]] = []
    pos = 0
    for m in TAG_RE.finditer(text):
        if m.start() > pos:
            runs.append((text[pos : m.start()], BODY))
        runs.append((m.group(2), COLOR_MAP.get(m.group(1), BODY)))
        pos = m.end()
    if pos < len(text):
        runs.append((text[pos:], BODY))
    return runs or [("", BODY)]


def wrap_runs(
    runs: list[tuple[str, tuple[int, int, int, int]]],
    font: ImageFont.ImageFont,
    max_width: int,
    draw: ImageDraw.ImageDraw,
) -> list[list[tuple[str, tuple[int, int, int, int]]]]:
    lines: list[list[tuple[str, tuple[int, int, int, int]]]] = [[]]
    for text, color in runs:
        parts = text.split("\n")
        for i, part in enumerate(parts):
            if i > 0:
                lines.append([])
            words = part.split(" ") if part else [""]
            for wi, word in enumerate(words):
                token = word if wi == 0 else (" " + word)
                trial = "".join(t for t, _ in lines[-1]) + token
                if lines[-1] and draw.textlength(trial, font=font) > max_width:
                    lines.append([(word, color)])
                else:
                    if token or lines[-1]:
                        lines[-1].append((token, color))
    return lines


def draw_rich_lines(
    draw: ImageDraw.ImageDraw,
    xy: tuple[int, int],
    lines: list[list[tuple[str, tuple[int, int, int, int]]]],
    font: ImageFont.ImageFont,
    line_height: int,
) -> int:
    x0, y = xy
    for line in lines:
        x = x0
        for text, color in line:
            draw.text((x, y), text, font=font, fill=color)
            x += int(draw.textlength(text, font=font))
        y += line_height
    return y


def icon_path(image_stem: str) -> Path:
    p = SPRITES / f"{image_stem}_0.png"
    if p.is_file():
        return p
    matches = sorted(SPRITES.glob(f"{image_stem}*.png"))
    if matches:
        return matches[0]
    raise FileNotFoundError(image_stem)


def skill_meta(skill: dict) -> str:
    combat = skill.get("combat") or {}
    kind = (skill.get("type") or "active").capitalize()
    if kind.lower() == "passive":
        return "Passive"
    bits = [kind]
    cd = combat.get("cooldown")
    en = combat.get("energy")
    rng = combat.get("range")
    if cd not in (None, "", 0):
        bits.append(f"CD {cd}")
    if en not in (None, "", 0):
        bits.append(f"Energy {en}")
    if rng not in (None, "", "0", 0):
        bits.append(f"Range {rng}")
    return "  ·  ".join(bits)


def measure_card(skill: dict, width: int, fonts: dict) -> int:
    # Approximate height by wrapping description.
    img = Image.new("RGBA", (width, 40), BG)
    draw = ImageDraw.Draw(img)
    runs = parse_rich(skill["description"])
    lines = wrap_runs(runs, fonts["body"], width - 24 - 64, draw)
    return 16 + 56 + max(len(lines), 1) * 18 + 16


def render_card(skill: dict, width: int, fonts: dict) -> Image.Image:
    height = measure_card(skill, width, fonts)
    card = Image.new("RGBA", (width, height), PANEL)
    draw = ImageDraw.Draw(card)
    draw.rectangle([0, 0, width - 1, height - 1], outline=PANEL_EDGE, width=2)

    icon = Image.open(icon_path(skill["image"])).convert("RGBA")
    icon = icon.resize((48, 48), Image.Resampling.NEAREST)
    # frame
    draw.rectangle([12, 12, 12 + 52, 12 + 52], outline=ACCENT, width=2)
    card.paste(icon, (14, 14), icon)

    title_x = 76
    draw.text((title_x, 14), skill["name"], font=fonts["title"], fill=TITLE)
    draw.text((title_x, 38), skill_meta(skill), font=fonts["meta"], fill=MUTED)

    runs = parse_rich(skill["description"])
    lines = wrap_runs(runs, fonts["body"], width - 24 - 64, draw)
    draw_rich_lines(draw, (title_x, 70), lines, fonts["body"], 18)
    return card


def ordered_skills(data: dict) -> dict[int, list[dict]]:
    by_slug = {s["slug"]: s for s in data["skills"]}
    by_tier: dict[int, list[dict]] = {}
    for tier, slugs in TIER_ORDER.items():
        by_tier[tier] = [by_slug[s] for s in slugs if s in by_slug]
    # Include any skills missing from explicit order
    seen = {s["slug"] for skills in by_tier.values() for s in skills}
    extras: dict[int, list[dict]] = defaultdict(list)
    for s in data["skills"]:
        if s["slug"] not in seen:
            extras[int(s["tier"])].append(s)
    for tier, skills in extras.items():
        by_tier.setdefault(tier, []).extend(skills)
    return dict(sorted(by_tier.items()))


def render_tier(tier: int, skills: list[dict], width: int, fonts: dict) -> Image.Image:
    cards = [render_card(s, width - 32, fonts) for s in skills]
    gap = 12
    header_h = 54
    total_h = header_h + sum(c.height for c in cards) + gap * (len(cards) + 1)
    canvas = Image.new("RGBA", (width, total_h), BG)
    draw = ImageDraw.Draw(canvas)
    draw.rectangle([0, 0, width - 1, total_h - 1], outline=ACCENT, width=2)
    label = f"TIER {tier}"
    draw.text((16, 14), label, font=fonts["header"], fill=ACCENT)
    subtitle = {
        1: "Foundation — hunt mark, wolf, and safe dismiss",
        2: "Pack — howl, instincts, and mend",
        3: "Beasts — crawler, bear, bison, and savage bond",
        4: "Apex — gulon, kindred spirit, and primal power",
    }.get(tier, "")
    draw.text((16, 34), subtitle, font=fonts["meta"], fill=MUTED)

    y = header_h
    for card in cards:
        canvas.paste(card, (16, y), card)
        y += card.height + gap
    return canvas


def build_nexus_bbcode(tree_url: str, tier_urls: dict[int, str], keep_prefix: str, keep_suffix: str) -> str:
    parts = [keep_prefix.rstrip(), "", "[b] Skill tree[/b]", "<br />", ""]
    if tree_url:
        parts.append(f'[img]{tree_url}[/img]')
        parts.append("<br />")
        parts.append("")
    parts.append("[b] Skills by tier[/b]")
    parts.append("<br />")
    for tier in sorted(tier_urls):
        parts.append("")
        parts.append(f"[b] Tier {tier}[/b]")
        parts.append("<br />")
        parts.append(f'[img]{tier_urls[tier]}[/img]')
        parts.append("<br />")
    parts.append("")
    parts.append(keep_suffix.lstrip())
    # Nexus stores description with <br /> line breaks commonly
    out_lines = []
    for line in parts:
        if line == "":
            out_lines.append("<br />")
        elif line.startswith("[") or line.startswith("<"):
            out_lines.append(line)
            if not line.endswith("<br />"):
                out_lines.append("<br />")
        else:
            out_lines.append(f" {line}")
            out_lines.append("<br />")
    return "\n".join(out_lines)


def write_html_preview(
    out: Path,
    tree_path: Path,
    tier_paths: dict[int, Path],
    skills_by_tier: dict[int, list[dict]],
) -> None:
    sections = []
    sections.append(f'<img class="tree" src="{html.escape(tree_path.name)}" alt="Skill tree" />')
    for tier, path in sorted(tier_paths.items()):
        sections.append(f'<h2>Tier {tier}</h2>')
        sections.append(f'<img class="tier" src="{html.escape(path.name)}" alt="Tier {tier}" />')
        sections.append('<div class="names">')
        for s in skills_by_tier[tier]:
            sections.append(
                f'<span><img src="../Sprites/{html.escape(s["image"])}_0.png" width="24" height="24" '
                f'style="image-rendering:pixelated;vertical-align:middle" /> '
                f'{html.escape(s["name"])} <em>({html.escape(s["type"])})</em></span>'
            )
        sections.append("</div>")

    doc = f"""<!DOCTYPE html>
<html lang="en">
<head>
<meta charset="utf-8" />
<title>Summoner Nexus description preview</title>
<style>
  body {{
    margin: 0; padding: 24px;
    background: #1a1620; color: #e6e0d6;
    font-family: DejaVu Sans, Liberation Sans, sans-serif;
    max-width: 920px;
  }}
  h1 {{ color: #a8d080; font-size: 1.4rem; }}
  h2 {{ color: #88b868; margin-top: 28px; border-bottom: 1px solid #405040; padding-bottom: 6px; }}
  .tree {{ width: 320px; image-rendering: pixelated; border: 1px solid #4a6040; }}
  .tier {{ width: 100%; max-width: 860px; border: 1px solid #4a6040; }}
  .names {{ display: flex; flex-wrap: wrap; gap: 12px 18px; margin: 8px 0 0; color: #b8b0c0; font-size: 0.9rem; }}
  em {{ color: #788868; font-style: normal; }}
  .note {{ color: #788868; margin-bottom: 20px; }}
</style>
</head>
<body>
  <h1>Summoner — skill overview preview</h1>
  <p class="note">Local preview only. Not published to Nexus yet.</p>
  {"".join(sections)}
</body>
</html>
"""
    out.write_text(doc, encoding="utf-8")


def split_existing_description(desc: str) -> tuple[str, str]:
    """Keep everything before Skill overview and from Requirements onward."""
    start_markers = [
        "[b]  Skill tree[/b]", "[b] Skill tree[/b]", "Skill tree",
        "[b]  Skill overview[/b]", "[b] Skill overview[/b]", "Skill overview",
    ]
    end_markers = ["[b]  Requirements[/b]", "[b] Requirements[/b]", "Requirements"]
    start = -1
    for m in start_markers:
        i = desc.find(m)
        if i >= 0:
            # back up to previous separator if present
            sep = desc.rfind("──", 0, i)
            start = sep if sep >= 0 else i
            break
    end = -1
    for m in end_markers:
        i = desc.find(m)
        if i >= 0:
            sep = desc.rfind("──", 0, i)
            end = sep if sep >= 0 else i
            break
    if start < 0 or end < 0 or end <= start:
        # fallback: replace nothing; caller can append
        return desc, ""
    return desc[:start].rstrip(), desc[end:].lstrip()


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument(
        "--out-dir",
        type=Path,
        default=ROOT / "_debug" / "nexus_preview",
        help="Output directory for preview assets",
    )
    parser.add_argument("--width", type=int, default=860)
    parser.add_argument(
        "--existing-desc",
        type=Path,
        default=None,
        help="Optional JSON/text dump of current Nexus description for BBCode draft",
    )
    args = parser.parse_args()

    out_dir: Path = args.out_dir
    out_dir.mkdir(parents=True, exist_ok=True)

    data = json.loads(JSON_PATH.read_text(encoding="utf-8"))
    skills_by_tier = ordered_skills(data)

    fonts = {
        "header": load_font(22, bold=True),
        "title": load_font(18, bold=True),
        "meta": load_font(13),
        "body": load_font(14),
    }

    # Skill tree overview (reuse existing generator)
    tree_out = out_dir / "skill_tree.png"
    import subprocess

    subprocess.check_call(
        [
            "python3",
            str(ROOT / "python" / "generate_skill_tree.py"),
            "--icons",
            "--title",
            "Summoner",
            "--labels",
            "--scale",
            "3",
            "--out",
            str(tree_out),
        ]
    )

    tier_paths: dict[int, Path] = {}
    for tier, skills in skills_by_tier.items():
        img = render_tier(tier, skills, args.width, fonts)
        path = out_dir / f"tier_{tier}.png"
        img.save(path)
        tier_paths[tier] = path
        print(f"Wrote {path} ({img.size[0]}x{img.size[1]})")

    write_html_preview(out_dir / "preview.html", tree_out, tier_paths, skills_by_tier)
    print(f"Wrote {out_dir / 'preview.html'}")

    # Draft BBCode with local file placeholders (replaced after gallery upload)
    placeholders = {t: f"PLACEHOLDER_TIER_{t}_URL" for t in tier_paths}
    prefix = suffix = ""
    if args.existing_desc and args.existing_desc.is_file():
        raw = args.existing_desc.read_text(encoding="utf-8")
        try:
            parsed = json.loads(raw)
            raw = parsed.get("description") or parsed.get("data", {}).get("description") or raw
        except json.JSONDecodeError:
            pass
        prefix, suffix = split_existing_description(raw)
    else:
        # Use live dump if present
        dump = Path("/tmp/nexus_mod456.json")
        if dump.is_file():
            parsed = json.loads(dump.read_text(encoding="utf-8"))
            raw = (
                parsed.get("description")
                or (parsed.get("data") or {}).get("description")
                or ""
            )
            if raw:
                prefix, suffix = split_existing_description(raw)

    bb = build_nexus_bbcode("PLACEHOLDER_TREE_URL", placeholders, prefix, suffix)
    (out_dir / "description_draft.txt").write_text(bb, encoding="utf-8")
    print(f"Wrote {out_dir / 'description_draft.txt'}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
