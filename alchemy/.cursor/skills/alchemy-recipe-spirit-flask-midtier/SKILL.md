---
name: alchemy-recipe-spirit-flask-midtier
description: >-
  Enforces that Alchemy elixirs with cost > 80 and < 150 require spirit and
  flask as ingredients. Use whenever creating, editing, or balancing recipes
  in ingredients.json for the Alchemy mod.
---

# Alchemy mid-tier spirit and flask requirement

## Hard rule

for each elixir with cost > 80 and < 150 spirit and flask is required ingredients

If `80 < elixir_cost < 150`, then both `spirit` and `flask` must appear in `ingredients` (up to 4 slots; repeats allowed).

## When creating or editing a recipe

1. Follow `alchemy-recipe-cost` for the 75%..100% ingredients band and cost fields.
2. Compute costs from catalog prices:
   - `ingredients_cost` = sum of ingredient prices
   - `recipe_cost` = `ingredients_cost` + (flask price only if `flask` is not already in `ingredients`) + (spirit price only if `spirit` is not already in `ingredients`)
   - `elixir_cost` = target outcome price
3. If `80 < elixir_cost < 150` and `spirit` or `flask` is missing, add or swap them in, then recompute costs and re-check the cost band.
4. Also follow `alchemy-recipe-spirit-required` when `recipe_cost > 100`.
5. Validate:

```bash
python3 .cursor/skills/alchemy-recipe-cost/scripts/validate_recipe_costs.py
```
