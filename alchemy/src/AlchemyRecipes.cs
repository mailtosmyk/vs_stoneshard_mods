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

        // MSL InjectTableRecipesCraft omits the trailing ';' so rows have 11 cols while the
        // header has 12. o_textLoader then crashes: Variable Index [11] out of range [11].
        PadRecipeCraftRowsToHeaderWidth();
    }

    static void PadRecipeCraftRowsToHeaderWidth()
    {
        Msl.LoadGML("gml_GlobalScript_table_recipes_craft")
            .Apply(lines => lines.Select(PadShortRecipeCraftRows))
            .Save();
    }

    static string PadShortRecipeCraftRows(string line)
    {
        // Header: NAME;CAT;Recipe1..Recipe6;AMOUNT;XP;SOURCE;  => 12 fields
        const int headerWidth = 12;
        var sb = new System.Text.StringBuilder(line.Length + 128);
        var i = 0;
        while (i < line.Length)
        {
            if (line[i] != '"')
            {
                sb.Append(line[i]);
                i++;
                continue;
            }

            var close = FindClosingQuote(line, i);
            if (close < 0)
            {
                sb.Append(line.AsSpan(i));
                break;
            }

            var inner = line.Substring(i + 1, close - (i + 1));
            if (inner.Contains(';'))
            {
                var parts = inner.Split(';');
                // Data rows look like id;cat;... (skip section markers with empty NAME)
                if (parts.Length > 0 && parts.Length < headerWidth && !string.IsNullOrEmpty(parts[0]))
                    inner = string.Join(";", parts.Concat(Enumerable.Repeat(string.Empty, headerWidth - parts.Length)));
            }

            sb.Append('"').Append(inner).Append('"');
            i = close + 1;
        }

        return sb.ToString();
    }

    static int FindClosingQuote(string line, int openQuote)
    {
        for (var i = openQuote + 1; i < line.Length; i++)
        {
            if (line[i] == '\\' && i + 1 < line.Length)
            {
                i++;
                continue;
            }

            if (line[i] == '"')
                return i;
        }

        return -1;
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
