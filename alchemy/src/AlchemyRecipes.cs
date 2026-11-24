using ModShardLauncher;

namespace Alchemy;

/// <summary>
/// Registers enabled craft recipes from <c>ingredients.json</c> into
/// <c>table_recipes_craft</c>.
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

            var advanced = recipe.Advanced || IsAdvanced(recipe);
            var xp = XpFor(recipe);
            var slots = PadIngredients(recipe.Ingredients, advanced);
            var source = recipe.Source == AlchemyCraftSource.BaseRecipe
                ? Msl.RecipesCraftSource.BaseRecipe
                : Msl.RecipesCraftSource.Trade;

            var amount = (ushort)Math.Clamp(recipe.Amount, 1, 99);

            if (advanced)
            {
                Msl.InjectTableRecipesCraft(
                    Msl.RecipesCraftHook.ADVANCED,
                    recipe.Elixir,
                    Msl.RecipesCraftCategory.advanced,
                    xp,
                    AMOUNT: amount,
                    source,
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
                    AMOUNT: amount,
                    source,
                    slots[0] ?? "-",
                    slots[1],
                    slots[2],
                    slots[3]);
            }
        }

        // MSL InjectTableRecipesCraft omits the trailing ';' so rows have 11 cols while the
        // header has 12. o_textLoader then crashes: Variable Index [11] out of range [11].
        PadRecipeCraftRowsToHeaderWidth();
        PatchBlankScrollCraftAmount();
    }

    /// <summary>
    /// Blank Scroll recipe accepts book or hemp; table amount is 3 (book yield).
    /// Hemp still yields a single scroll.
    /// </summary>
    static void PatchBlankScrollCraftAmount()
    {
        const string marker = "var _count = max(1, string_to_real(ds_map_find_value(_data, \"amount\")))";
        var adjust = string.Join(
            "\n",
            marker,
            "if (idName == \"blank_scroll\")",
            "{",
            "    var _alch_from_book = false",
            "    var _alch_n = array_length(item_array)",
            "    for (var _alch_i = 0; _alch_i < _alch_n; _alch_i++)",
            "    {",
            "        var _alch_ing = item_array[_alch_i]",
            "        if (instance_exists(_alch_ing) && ds_list_find_index(_alch_ing.category, \"book\") >= 0)",
            "            _alch_from_book = true",
            "    }",
            "    _count = _alch_from_book ? 3 : 1",
            "}");

        Msl.LoadGML("gml_Object_o_craftingConsumsMenu_Other_14")
            .MatchFrom(marker)
            .ReplaceBy(adjust)
            .Save();
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
            // Outcome flask is a basic glass reshape — not an alchemy station craft.
            if (ing is "spirit" or "oil" or "craftkit" or "workbench")
                return true;
            if (ing is "flask" && recipe.Elixir != "flask")
                return true;
        }

        // Custom elixirs/throwables/coatings/materials need the craft station.
        // Blank scrolls are a simple Base Recipe (hemp) and stay basic.
        if (!AlchemyCatalog.CustomProducts.TryGetValue(recipe.Elixir, out var product))
            return false;
        return product.Kind != AlchemyProductKind.Scroll;
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
