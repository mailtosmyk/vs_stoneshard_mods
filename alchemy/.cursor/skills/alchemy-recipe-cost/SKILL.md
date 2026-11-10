---
name: alchemy-recipe-cost
description: >-
  Enforces Alchemy recipe economy in ingredients.json: ingredients cost can't
  exceed elixir cost price and must not be lower than 75% of target elixir
  price. Use whenever creating, editing, or balancing recipes, elixirs, or
  recipe costs in the Alchemy mod.
---

# Alchemy recipe cost

## Hard rule

ingredients cost can't exceed elixir cost price and be lower than 75% of target elixir price

`ceil(elixir_cost * 0.75) <= ingredients_cost <= elixir_cost`

## When creating or editing a recipe

1. Pick up to 4 ingredient ids (repeats allowed) from the catalog in `src/ingredients.json`.
2. Set `elixir` to the target outcome id.
3. Compute costs from catalog `price` values:
   - `ingredients_cost` = sum of ingredient prices
   - `recipe_cost` = `ingredients_cost` + (flask price only if `flask` is not already in `ingredients`) + (spirit price only if `spirit` is not already in `ingredients`)
   - `elixir_cost` = target outcome price
4. Adjust ingredients until `ingredients_cost` is within the band above. Prefer thematic reagents.
5. Also follow `alchemy-recipe-spirit-required` when `recipe_cost > 100`.
6. Also follow `alchemy-recipe-spirit-flask-midtier` when `80 < elixir_cost < 150`.
7. Validate before finishing:

```bash
python3 .cursor/skills/alchemy-recipe-cost/scripts/validate_recipe_costs.py
```

Fix any reported recipe, then re-run until the script exits 0.

## Recipe fields

```json
{
  "ingredients": ["id1", "id2", "id3", "id4"],
  "enabled": false,
  "elixir": "elixir_id",
  "ingredients_cost": 0,
  "recipe_cost": 0,
  "elixir_cost": 0
}
```
