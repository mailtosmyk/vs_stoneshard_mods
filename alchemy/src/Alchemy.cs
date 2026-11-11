using ModShardLauncher.Mods;

namespace Alchemy;

/// <summary>
/// ModShardLauncher entry point for Alchemy.
/// Expands useful crafting recipes from existing ingredients and sells their
/// schematics at herbalists / the Brynn alchemist — no new skill school.
/// </summary>
public class Alchemy : Mod
{
    public override string Author => "Vitali Smyk <mailtosmyk@gmail.com>";
    public override string Name => "Alchemy";
    public override string Description =>
        "More useful recipes from existing ingredients — no new skills. " +
        "Place 4 of the same elixir and any gem in a cooking pot or craft station to learn that elixir's recipe.";
    public override string Version => AlchemyVersion.Value;
    public override string TargetVersion => "0.9.4.25";

    public override void PatchMod()
    {
        // Load catalog first so recipe/product patches share one parse.
        _ = AlchemyCatalog.EnabledRecipes;

        // Study helpers must register before any GML that calls them.
        AlchemyStudy.Patch();
        AlchemyItems.Patch();
        // Buff objects + drink hooks need inventory objects from AlchemyItems first.
        AlchemySpellRange.Patch();
        AlchemyRecipes.Patch();
        AlchemyShops.Patch();
        AlchemyDebug.Patch();
    }
}
