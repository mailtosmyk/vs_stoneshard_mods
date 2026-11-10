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
        "More useful recipes from existing ingredients — no new skills.";
    public override string Version => AlchemyVersion.Value;
    public override string TargetVersion => "0.9.4.25";

    public override void PatchMod()
    {
        // Load catalog first so recipe/product patches share one parse.
        _ = AlchemyCatalog.EnabledRecipes;

        AlchemyItems.Patch();
        AlchemyRecipes.Patch();
        AlchemyShops.Patch();
        AlchemyDebug.Patch();
    }
}
