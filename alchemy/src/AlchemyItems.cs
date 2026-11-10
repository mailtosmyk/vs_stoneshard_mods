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
        PatchRecipeScrolls();
    }

    static void PatchCustomProducts()
    {
        foreach (var product in AlchemyCatalog.CustomProducts.Values)
        {
            var invParent = product.Kind switch
            {
                AlchemyProductKind.Throwable => "o_inv_bombs",
                AlchemyProductKind.Coating => "o_inv_consum_passive",
                _ => "o_inv_dishes_beverage",
            };

            Msl.AddObject(
                name: $"o_inv_{product.Id}",
                spriteName: product.Icon,
                parentName: invParent,
                isVisible: true,
                isPersistent: true,
                isAwake: true,
                collisionShapeFlags: CollisionShapeFlags.Circle);

            Msl.AddObject(
                name: $"o_loot_{product.Id}",
                spriteName: product.Icon,
                parentName: "o_consument_loot",
                isVisible: true,
                isPersistent: false,
                isAwake: true,
                collisionShapeFlags: CollisionShapeFlags.Circle);

            var (cat, subcat, material, tags) = StatsFor(product);
            Msl.InjectTableItemStats(
                product.Id,
                material,
                Msl.ItemStatsWeight.Light,
                Price: Math.Max(1, product.Price),
                Cat: cat,
                Subcat: subcat,
                tags: tags);

            var desc = string.IsNullOrWhiteSpace(product.Description)
                ? $"An alchemical preparation: {product.Name}."
                : product.Description;

            // consum_name / consum_mid (effects) / consum_desc
            InjectLocalization(product.Id, product.Name, desc, desc);
        }
    }

    static void PatchRecipeScrolls()
    {
        var products = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var recipe in AlchemyCatalog.EnabledRecipes)
            products.Add(recipe.Elixir);

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

    static (Msl.ItemStatsCategory Cat, Msl.ItemStatsSubcategory Subcat, Msl.ItemStatsMaterial Material, Msl.ItemStatsTags Tags)
        StatsFor(AlchemyProduct product) => product.Kind switch
        {
            AlchemyProductKind.Throwable => (
                Msl.ItemStatsCategory.tool,
                Msl.ItemStatsSubcategory.none,
                Msl.ItemStatsMaterial.glass,
                Msl.ItemStatsTags.commonalchemy),
            AlchemyProductKind.Coating => (
                Msl.ItemStatsCategory.additive,
                Msl.ItemStatsSubcategory.alchemy,
                Msl.ItemStatsMaterial.organic,
                Msl.ItemStatsTags.commonalchemy),
            _ => (
                Msl.ItemStatsCategory.medicine,
                Msl.ItemStatsSubcategory.potion,
                Msl.ItemStatsMaterial.glass,
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
}
