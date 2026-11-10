using ModShardLauncher;

namespace Alchemy;

/// <summary>
/// Registers enabled craft recipes from <c>ingredients.json</c> into
/// <c>table_recipes_craft</c> with <see cref="Msl.RecipesCraftSource.Trade"/>.
/// </summary>
internal static class AlchemyRecipes
{
    public static void Patch()
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var recipe in AlchemyCatalog.EnabledRecipes)
        {
            // Craft table NAME is the product id; keep first variant when duplicates exist (spirit/oil).
            if (!seen.Add(recipe.Elixir))
                continue;

            var advanced = IsAdvanced(recipe);
            var xp = XpFor(recipe);
            var slots = PadIngredients(recipe.Ingredients, advanced);

            if (advanced)
            {
                Msl.InjectTableRecipesCraft(
                    Msl.RecipesCraftHook.ADVANCED,
                    recipe.Elixir,
                    Msl.RecipesCraftCategory.advanced,
                    xp,
                    AMOUNT: 1,
                    Msl.RecipesCraftSource.Trade,
                    slots[0] ?? "craftkit, workbench",
                    slots[1],
                    slots[2],
                    slots[3],
                    slots[4]);
            }
            else
            {
                Msl.InjectTableRecipesCraft(
                    Msl.RecipesCraftHook.BASIC,
                    recipe.Elixir,
                    Msl.RecipesCraftCategory.basic,
                    xp,
                    AMOUNT: 1,
                    Msl.RecipesCraftSource.Trade,
                    slots[0] ?? "-",
                    slots[1],
                    slots[2],
                    slots[3]);
            }
        }
    }

    static bool IsAdvanced(AlchemyRecipe recipe)
    {
        if (recipe.RecipeCost >= 60)
            return true;

        foreach (var ing in recipe.Ingredients)
        {
            if (ing is "flask" or "spirit" or "oil" or "craftkit" or "workbench")
                return true;
        }

        return AlchemyCatalog.CustomProducts.ContainsKey(recipe.Elixir);
    }

    static ushort XpFor(AlchemyRecipe recipe)
    {
        var xp = Math.Clamp(recipe.RecipeCost / 10, 2, 50);
        return (ushort)xp;
    }

    static string?[] PadIngredients(IReadOnlyList<string> ingredients, bool advanced)
    {
        // Advanced craft rows reserve Recipe1 for stations (craftkit / workbench).
        if (advanced)
        {
            var slots = new string?[5];
            slots[0] = "craftkit, workbench";
            for (var i = 0; i < 4; i++)
                slots[i + 1] = i < ingredients.Count ? ingredients[i] : null;
            return slots;
        }

        var basic = new string?[4];
        for (var i = 0; i < 4; i++)
            basic[i] = i < ingredients.Count ? ingredients[i] : null;
        return basic;
    }
}
