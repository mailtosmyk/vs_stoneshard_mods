using ModShardLauncher;
using ModShardLauncher.Mods;
using UndertaleModLib.Models;

namespace Alchemy;

/// <summary>
/// Registers custom brew/throwable/coating product objects and craft recipe scrolls.
/// </summary>
internal static class AlchemyItems
{
    const string RecipeInvSprite = "s_inv_scheme_herbalextr";
    const string RecipeLootSprite = "s_loot_scheme";

    public static void Patch()
    {
        PatchCustomProducts();
        PatchVanillaMaterialPrices();
        PatchRecipeScrolls();
        // MSL InjectTableItemStats still emits the pre-0.9.4 column layout (97 cols).
        // Current table_items_stats header is 107 cols; short rows crash
        // scr_table_array_filter with "Variable Index [97] out of range [97]".
        PadItemStatsRowsToHeaderWidth();
    }

    static void PatchCustomProducts()
    {
        foreach (var product in AlchemyCatalog.CustomProducts.Values)
        {
            var invParent = product.Kind switch
            {
                AlchemyProductKind.Throwable => "o_inv_bombs",
                AlchemyProductKind.Coating or AlchemyProductKind.Material => "o_inv_consum_passive",
                _ => "o_inv_dishes_beverage",
            };

            var lootSprite = LootSpriteFor(product);

            var inv = Msl.AddObject(
                name: $"o_inv_{product.Id}",
                spriteName: product.Icon,
                parentName: invParent,
                isVisible: true,
                isPersistent: true,
                isAwake: true,
                collisionShapeFlags: CollisionShapeFlags.Circle);

            Msl.AddObject(
                name: $"o_loot_{product.Id}",
                spriteName: lootSprite,
                parentName: "o_consument_loot",
                isVisible: true,
                isPersistent: false,
                isAwake: true,
                collisionShapeFlags: CollisionShapeFlags.Circle);

            // o_inv_bombs / o_inv_consum_passive never call scr_consum_atr, so `name`
            // stays unset and o_inv_consum Alarm_0 crashes on loot/add.
            // Drinkables inherit o_inv_dishes_beverage which already calls it.
            if (product.Kind is AlchemyProductKind.Throwable or AlchemyProductKind.Coating
                or AlchemyProductKind.Material)
                EnsureConsumCreate(inv, product.Id);

            var (cat, subcat, material, weight, tags) = StatsFor(product);
            Msl.InjectTableItemStats(
                product.Id,
                material,
                weight,
                Price: Math.Max(1, product.Price),
                Cat: cat,
                Subcat: subcat,
                tags: tags);

            var desc = string.IsNullOrWhiteSpace(product.Description)
                ? product.Kind == AlchemyProductKind.Material
                    ? product.Name
                    : $"An alchemical preparation: {product.Name}."
                : product.Description;

            // consum_name / consum_mid (effects) / consum_desc
            InjectLocalization(product.Id, product.Name, desc, desc);
        }
    }

    /// <summary>
    /// Align vanilla iron_ingot sell price with the nugget×2+10% smelting economy.
    /// </summary>
    static void PatchVanillaMaterialPrices()
    {
        const string id = "iron_ingot";
        if (!AlchemyCatalog.Root.Ore.Any(o =>
                string.Equals(o.Id, id, StringComparison.OrdinalIgnoreCase) && o.Price is int))
            return;

        var price = AlchemyCatalog.Root.Ore
            .First(o => string.Equals(o.Id, id, StringComparison.OrdinalIgnoreCase))
            .Price!.Value;

        Msl.LoadGML("gml_GlobalScript_table_items_stats")
            .Apply(lines => lines.Select(line =>
                line.Replace("iron_ingot;;70;70;", $"iron_ingot;;{price};{price};", StringComparison.Ordinal)))
            .Save();
    }

    static string LootSpriteFor(AlchemyProduct product)
    {
        if (product.Kind != AlchemyProductKind.Material)
            return product.Icon;
        if (product.Icon.StartsWith("s_inv_", StringComparison.Ordinal))
            return "s_loot_" + product.Icon["s_inv_".Length..];
        return product.Icon;
    }

    /// <summary>
    /// Vanilla bombs/coatings set <c>name</c> via <c>scr_consum_atr</c> in Create.
    /// Without that, <c>o_inv_consum</c> Alarm_0 reads an unset <c>name</c>.
    /// </summary>
    static void EnsureConsumCreate(UndertaleGameObject inv, string productId)
    {
        var gml = string.Join(
            "\n",
            "event_inherited()",
            $"scr_consum_atr(\"{productId}\")",
            "is_execute = false");

        try
        {
            Msl.AddNewEvent(inv, gml, EventType.Create, 0);
        }
        catch (ArgumentException)
        {
            // Event already present from a previous patch — refresh body below.
        }

        var codeName = Msl.EventName(inv.Name.Content, EventType.Create, 0);
        var lines = gml.Split('\n');
        Msl.LoadGML(codeName)
            .Apply(_ => lines)
            .Save();
    }

    static void PatchRecipeScrolls()
    {
        var products = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var recipe in AlchemyCatalog.EnabledRecipes)
        {
            // Base recipes are known from the start — no schematic item.
            if (recipe.Source == AlchemyCraftSource.BaseRecipe)
                continue;
            products.Add(recipe.Elixir);
        }

        foreach (var productId in products)
        {
            Msl.AddObject(
                name: $"o_inv_recipe_{productId}",
                spriteName: RecipeInvSprite,
                parentName: "o_inv_recipes_craft",
                isVisible: true,
                isPersistent: true,
                isAwake: true,
                collisionShapeFlags: CollisionShapeFlags.Circle);

            Msl.AddObject(
                name: $"o_loot_recipe_{productId}",
                spriteName: RecipeLootSprite,
                parentName: "o_loot_recipes_craft",
                isVisible: true,
                isPersistent: false,
                isAwake: true,
                collisionShapeFlags: CollisionShapeFlags.Circle);

            var price = ScrollPrice(productId);
            Msl.InjectTableItemStats(
                $"recipe_{productId}",
                Msl.ItemStatsMaterial.paper,
                Msl.ItemStatsWeight.VeryLight,
                Price: price,
                Cat: Msl.ItemStatsCategory.schematic,
                tags: Msl.ItemStatsTags.special);

            // Scroll tooltip name comes from consum_name[productId].
            // Custom products already registered that row; vanilla ids already exist.
        }
    }

    static (Msl.ItemStatsCategory Cat, Msl.ItemStatsSubcategory Subcat, Msl.ItemStatsMaterial Material,
            Msl.ItemStatsWeight Weight, Msl.ItemStatsTags Tags)
        StatsFor(AlchemyProduct product) => product.Kind switch
        {
            AlchemyProductKind.Throwable => (
                Msl.ItemStatsCategory.tool,
                Msl.ItemStatsSubcategory.none,
                Msl.ItemStatsMaterial.glass,
                Msl.ItemStatsWeight.Light,
                Msl.ItemStatsTags.commonalchemy),
            AlchemyProductKind.Coating => (
                Msl.ItemStatsCategory.additive,
                Msl.ItemStatsSubcategory.alchemy,
                Msl.ItemStatsMaterial.organic,
                Msl.ItemStatsWeight.Light,
                Msl.ItemStatsTags.commonalchemy),
            AlchemyProductKind.Material => (
                Msl.ItemStatsCategory.valuable,
                Msl.ItemStatsSubcategory.none,
                product.Id.Contains("gold", StringComparison.OrdinalIgnoreCase)
                    ? Msl.ItemStatsMaterial.gold
                    : Msl.ItemStatsMaterial.metal,
                Msl.ItemStatsWeight.Medium,
                Msl.ItemStatsTags.common),
            _ => (
                Msl.ItemStatsCategory.medicine,
                Msl.ItemStatsSubcategory.potion,
                Msl.ItemStatsMaterial.glass,
                Msl.ItemStatsWeight.Light,
                Msl.ItemStatsTags.commonalchemy),
        };

    static int ScrollPrice(string productId)
    {
        var recipe = AlchemyCatalog.EnabledRecipes.FirstOrDefault(r =>
            string.Equals(r.Elixir, productId, StringComparison.OrdinalIgnoreCase));
        if (recipe is null)
            return 40;
        return Math.Clamp(recipe.RecipeCost, 25, 500);
    }

    static void InjectLocalization(string id, string name, string type, string description)
    {
        var nameDict = new Dictionary<ModLanguage, string>
        {
            { ModLanguage.English, name },
            { ModLanguage.Russian, name },
        };
        var typeDict = new Dictionary<ModLanguage, string>
        {
            { ModLanguage.English, type },
            { ModLanguage.Russian, type },
        };
        var descDict = new Dictionary<ModLanguage, string>
        {
            { ModLanguage.English, description },
            { ModLanguage.Russian, description },
        };
        Msl.InjectTableItemLocalization(id, nameDict, typeDict, descDict);
    }

    /// <summary>
    /// Current items_stats inserts <c>is_container</c> + purse/bottle/upgrade/fodder/stack/
    /// fireproof/dropsOnce (10 fields) before <c>tags</c>. Old MSL rows put tags too early.
    /// </summary>
    const int ItemsStatsHeaderWidth = 107;
    const int LegacyItemsStatsWidth = 97;
    const int LegacyTagsIndex = 95;

    static void PadItemStatsRowsToHeaderWidth()
    {
        Msl.LoadGML("gml_GlobalScript_table_items_stats")
            .Apply(lines => lines.Select(PadAllShortItemStatsRows))
            .Save();
    }

    /// <summary>
    /// Walk every quoted CSV entry. Last array element ends with <c>"]</c> not <c>",</c>,
    /// so end detection must use the closing quote, not a comma.
    /// </summary>
    static string PadAllShortItemStatsRows(string line)
    {
        var sb = new System.Text.StringBuilder(line.Length + 256);
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
            if (inner.Contains(';') && inner.Count(c => c == ';') >= 50)
            {
                var parts = inner.Split(';');
                if (parts.Length < ItemsStatsHeaderWidth)
                    inner = string.Join(";", FixShortItemStatsParts(parts));
            }

            sb.Append('"').Append(inner).Append('"');
            i = close + 1;
        }

        return sb.ToString();
    }

    static string[] FixShortItemStatsParts(string[] parts)
    {
        if (parts.Length == LegacyItemsStatsWidth)
        {
            // Insert 10 empty columns before legacy tags so tags land on header index 105.
            return parts.Take(LegacyTagsIndex)
                .Concat(Enumerable.Repeat(string.Empty, ItemsStatsHeaderWidth - LegacyItemsStatsWidth))
                .Concat(parts.Skip(LegacyTagsIndex))
                .ToArray();
        }

        // Unknown short layout — pad empties at end so filter does not OOB.
        return parts
            .Concat(Enumerable.Repeat(string.Empty, ItemsStatsHeaderWidth - parts.Length))
            .ToArray();
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
}
