using ModShardLauncher;

namespace Alchemy;

/// <summary>
/// Puts craft recipe scrolls into herbalist / alchemist shop stock via
/// <c>scr_selling_loot_singular_stock</c> (persistent unique stock).
/// </summary>
internal static class AlchemyShops
{
    public static void Patch()
    {
        var recipes = AlchemyCatalog.EnabledRecipes
            .Where(r => r.Source != AlchemyCraftSource.BaseRecipe)
            .GroupBy(r => r.Elixir, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .ToArray();

        if (recipes.Length == 0)
            return;

        var herbalist = recipes.Where(IsHerbalistStock).Select(ObjectName).ToArray();
        var alchemist = recipes.Where(IsAlchemistStock).Select(ObjectName).ToArray();

        InjectSingularStock(
            "gml_Object_o_npc_herbalist_osbrook_Create_0",
            "occupation = \"herbalist\"",
            herbalist);

        InjectSingularStock(
            "gml_Object_o_npc_herbalistMS_Create_0",
            "occupation = \"herbalist\"",
            herbalist);

        InjectSingularStock(
            "gml_Object_o_npc_alchemist_brynn_Create_0",
            "occupation = \"alchemist\"",
            alchemist);
    }

    static bool IsHerbalistStock(AlchemyRecipe recipe)
    {
        // Field / restorative brews and basic craftables — keep legendary & combat kits for Brynn.
        if (recipe.RecipeCost >= 500)
            return false;
        if (AlchemyCatalog.CustomProducts.TryGetValue(recipe.Elixir, out var product)
            && product.Kind is AlchemyProductKind.Throwable or AlchemyProductKind.Coating)
            return false;
        return recipe.RecipeCost <= 150;
    }

    static bool IsAlchemistStock(AlchemyRecipe recipe) => true;

    static string ObjectName(AlchemyRecipe recipe) => $"o_inv_recipe_{recipe.Elixir}";

    static void InjectSingularStock(string scriptName, string anchor, IReadOnlyList<string> objects)
    {
        if (objects.Count == 0)
            return;

        // Chunk calls to keep inserted GML readable.
        const int chunkSize = 12;
        var lines = new List<string>();
        for (var i = 0; i < objects.Count; i += chunkSize)
        {
            var chunk = objects.Skip(i).Take(chunkSize);
            lines.Add($"scr_selling_loot_singular_stock({string.Join(", ", chunk)})");
        }

        try
        {
            Msl.LoadGML(scriptName)
                .MatchFrom(anchor)
                .InsertBelow(string.Join("\n", lines))
                .Save();
        }
        catch
        {
            // NPC script text can differ by game version; recipes still register.
        }
    }
}
