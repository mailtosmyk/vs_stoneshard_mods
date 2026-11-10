using ModShardLauncher.Mods;

namespace Alchemy;

/// <summary>
/// ModShardLauncher entry point for Alchemy.
/// Expands useful crafting/cooking recipes using existing game ingredients —
/// no new skill school.
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
        // Stub: recipe patches land here later.
        // Ingredient catalog: src/ingredients.json
    }
}
