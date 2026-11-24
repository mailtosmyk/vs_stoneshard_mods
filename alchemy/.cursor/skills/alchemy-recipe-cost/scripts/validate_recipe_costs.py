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
    scroll_outcomes = {
        str(it["id"])
        for it in data.get("scrolls") or []
        if isinstance(it, dict) and it.get("id")
    }
    # Vanilla craft categories matched by item.category (not catalog item ids).
    category_ingredients = {
        "book": 15,  # synthetic: matches 3× blank_scroll
        "herb": 2,
        "vegetable": 2,
        "additive": 10,
        "medicine": 10,
        "tool": 10,
        "material": 5,
    }

    failures = 0
    for i, recipe in enumerate(data.get("recipes") or []):
        label = recipe.get("elixir", f"#{i}")
        ings = recipe.get("ingredients") or []
        local_fail = 0
        amount = int(recipe.get("amount") or 1)
        if amount < 1:
            print(f"FAIL {label}: amount must be >= 1, got {amount}")
            failures += 1
            continue

        if not 1 <= len(ings) <= 4:
            print(f"FAIL {label}: ingredients must be 1..4, got {len(ings)}")
            failures += 1
            continue

        def split_alts(slot: str) -> list[str]:
            return [a.strip() for a in slot.split(",") if a.strip()]

        flat_ings = [a for slot in ings for a in split_alts(slot)]
        missing = [x for x in flat_ings if x not in prices and x not in category_ingredients]
        if missing:
            print(f"FAIL {label}: unknown ingredient ids {missing}")
            failures += 1
            continue

        elixir_id = recipe.get("elixir")
        if elixir_id not in prices:
            print(f"FAIL {label}: unknown elixir id {elixir_id!r}")
            failures += 1
            continue

        def price_of(token: str) -> int:
            return prices[token] if token in prices else category_ingredients[token]

        # OR-alternatives in one slot: price the slot as the max option (declared cost).
        ingredients_cost = sum(max(price_of(a) for a in split_alts(slot)) for slot in ings)
        elixir_cost = prices[elixir_id]
        output_value = elixir_cost * amount
        # Ore/material smelts and scroll crafts skip flask+spirit surcharge and
        # spirit/midtier alchemy rules (scroll recipes still include spirit when needed).
        # Flask/oil-style container & scroll/ore crafts skip flask+spirit surcharge.
        basic_container_outcomes = {"flask"}
        is_material = (
            elixir_id in ore_outcomes
            or elixir_id in scroll_outcomes
            or elixir_id in basic_container_outcomes
        )
        uses_category = any(
            x in category_ingredients and x not in prices for x in flat_ings
        )
        # OR-slots with unequal alt prices (e.g. vessel/bottle) skip the tight band.
        uneven_or = any(
            len(alts := split_alts(slot)) > 1
            and min(price_of(a) for a in alts) != max(price_of(a) for a in alts)
            for slot in ings
        )
        recipe_cost = (
            ingredients_cost
            if is_material
            else recipe_cost_of(ingredients_cost, ings, spirit, flask)
        )
        minimum = math.ceil(output_value * MIN_RATIO)
        maximum = output_value

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

        # Category crafts / uneven OR crafts skip the tight band (synthetic or mixed prices).
        if (
            not uses_category
            and not uneven_or
            and (ingredients_cost < minimum or ingredients_cost > maximum)
        ):
            print(
                f"FAIL {label}: ingredients_cost {ingredients_cost} "
                f"outside [{minimum}, {maximum}] "
                f"({MIN_RATIO:.0%}..100% of output_value {output_value} "
                f"= elixir_cost {elixir_cost} × amount {amount})"
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
                f"amount={amount} recipe_cost={recipe_cost}"
            )

    if failures:
        print(f"{failures} check(s) failed", file=sys.stderr)
        return 1
    print("all recipes valid")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
