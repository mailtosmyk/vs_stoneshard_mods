#!/usr/bin/env python3
"""Validate Alchemy recipes: cost band, spirit>100, mid-tier spirit+flask."""

from __future__ import annotations

import json
import math
import sys
from pathlib import Path

MIN_RATIO = 0.75
SPIRIT_COST_THRESHOLD = 100
MIDTIER_ELIXIR_COST_MIN = 80
MIDTIER_ELIXIR_COST_MAX = 150
ROOT = Path(__file__).resolve().parents[4]
INGREDIENTS = ROOT / "src" / "ingredients.json"


def recipe_cost_of(ingredients_cost: int, ings: list[str], spirit: int, flask: int) -> int:
    extra_flask = 0 if "flask" in ings else flask
    extra_spirit = 0 if "spirit" in ings else spirit
    return ingredients_cost + extra_flask + extra_spirit


def main() -> int:
    data = json.loads(INGREDIENTS.read_text(encoding="utf-8"))
    prices: dict[str, int] = {}
    for cat, items in data.items():
        if cat == "recipes" or not isinstance(items, list):
            continue
        for it in items:
            if isinstance(it, dict) and "id" in it and "price" in it:
                prices[it["id"]] = int(it["price"])

    spirit = prices.get("spirit")
    flask = prices.get("flask")
    if spirit is None or flask is None:
        print("ERROR: spirit and flask prices are required", file=sys.stderr)
        return 1

    ore_outcomes = {
        str(it["id"])
        for it in data.get("ore") or []
        if isinstance(it, dict) and it.get("id")
    }

    failures = 0
    for i, recipe in enumerate(data.get("recipes") or []):
        label = recipe.get("elixir", f"#{i}")
        ings = recipe.get("ingredients") or []
        local_fail = 0

        if not 1 <= len(ings) <= 4:
            print(f"FAIL {label}: ingredients must be 1..4, got {len(ings)}")
            failures += 1
            continue

        missing = [x for x in ings if x not in prices]
        if missing:
            print(f"FAIL {label}: unknown ingredient ids {missing}")
            failures += 1
            continue

        elixir_id = recipe.get("elixir")
        if elixir_id not in prices:
            print(f"FAIL {label}: unknown elixir id {elixir_id!r}")
            failures += 1
            continue

        ingredients_cost = sum(prices[x] for x in ings)
        elixir_cost = prices[elixir_id]
        # Ore/material smelts skip flask+spirit surcharge and spirit/midtier rules.
        is_material = elixir_id in ore_outcomes
        recipe_cost = (
            ingredients_cost
            if is_material
            else recipe_cost_of(ingredients_cost, ings, spirit, flask)
        )
        minimum = math.ceil(elixir_cost * MIN_RATIO)
        maximum = elixir_cost

        expected = {
            "ingredients_cost": ingredients_cost,
            "recipe_cost": recipe_cost,
            "elixir_cost": elixir_cost,
        }
        for key, want in expected.items():
            got = recipe.get(key)
            if got != want:
                print(f"FAIL {label}: {key} stored={got} expected={want}")
                local_fail += 1

        if ingredients_cost < minimum or ingredients_cost > maximum:
            print(
                f"FAIL {label}: ingredients_cost {ingredients_cost} "
                f"outside [{minimum}, {maximum}] "
                f"({MIN_RATIO:.0%}..100% of elixir_cost {elixir_cost})"
            )
            local_fail += 1

        if not is_material and recipe_cost > SPIRIT_COST_THRESHOLD and "spirit" not in ings:
            print(
                f"FAIL {label}: recipe_cost {recipe_cost} > "
                f"{SPIRIT_COST_THRESHOLD} requires spirit component"
            )
            local_fail += 1

        if not is_material and MIDTIER_ELIXIR_COST_MIN < elixir_cost < MIDTIER_ELIXIR_COST_MAX:
            missing_mid = [x for x in ("spirit", "flask") if x not in ings]
            if missing_mid:
                print(
                    f"FAIL {label}: elixir_cost {elixir_cost} in "
                    f"({MIDTIER_ELIXIR_COST_MIN}, {MIDTIER_ELIXIR_COST_MAX}) "
                    f"requires spirit and flask; missing {missing_mid}"
                )
                local_fail += 1

        failures += local_fail
        if local_fail == 0:
            print(
                f"OK   {label}: ingredients_cost={ingredients_cost} "
                f"band=[{minimum},{maximum}] elixir_cost={elixir_cost} "
                f"recipe_cost={recipe_cost}"
            )

    if failures:
        print(f"{failures} check(s) failed", file=sys.stderr)
        return 1
    print("all recipes valid")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
