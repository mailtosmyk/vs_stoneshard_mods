---
name: alchemy-recipe-spirit-required
description: >-
  Enforces that Alchemy recipes with cost more than 100 require spirit as an
  ingredient. Use whenever creating, editing, or balancing recipes in
  ingredients.json for the Alchemy mod.
---

# Alchemy recipe spirit requirement

## Hard rule

for each recipe with cost more than 100 spirit must be required component

If `recipe_cost > 100`, then `spirit` must appear in `ingredients` (one of up to 4 slots; repeats allowed).

## When creating or editing a recipe

1. Follow `alchemy-recipe-cost` for the 75%..100% ingredients band and cost fields.
2. Compute costs from catalog prices:
   - `ingredients_cost` = sum of ingredient prices
   - `recipe_cost` = `ingredients_cost` + (flask price only if `flask` is not already in `ingredients`) + (spirit price only if `spirit` is not already in `ingredients`)
   - `elixir_cost` = target outcome price
3. If `recipe_cost > 100` and `spirit` is missing, add or swap in `spirit`, then recompute costs and re-check the cost band.
4. Also follow `alchemy-recipe-spirit-flask-midtier` when `80 < elixir_cost < 150`.
5. Validate:

```bash
python3 .cursor/skills/alchemy-recipe-cost/scripts/validate_recipe_costs.py
```
