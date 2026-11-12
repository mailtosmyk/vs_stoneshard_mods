using ModShardLauncher;
using ModShardLauncher.Mods;
using Necromancy.Skills;

namespace Necromancy;

/// <summary>
/// ModShardLauncher entry point for the Necromancy skill school.
/// Place this project under MSL ModSources (or symlink) and Compile from MSL.
/// </summary>
public class Necromancy : Mod
{
    public override string Author => "Vitali Smyk <mailtosmyk@gmail.com>";
    public override string Name => "Necromancy";
    public override string Description =>
        "Occult skill school: raise the dead, curse foes, and bind unholy power.";
    public override string Version => NecromancyVersion.Value;
    public override string TargetVersion => "0.9.4.25";

    public override void PatchMod()
    {
        NecromancyLocalization.PatchAll();
        // Register summon-enchant helpers before any object GML that calls them
        // (UTMT resolves function refs at compile time; late AddFunction leaves broken calls).
        NecromancyEnchants.Patch(ModFiles);
        NecromancySealOfPower.Patch(ModFiles);
        NecromancyTier1Skills.Patch(ModFiles);
        NecromancyTier2Skills.Patch(ModFiles);
        NecromancyTier3Skills.Patch(ModFiles);
        NecromancyTier4Skills.Patch(ModFiles);
        NecromancySchool.Patch(ModFiles);
        NecromancyItems.Patch(ModFiles);

        // Ensure JSON registry is loaded (also used by future tiers).
        _ = NecromancySkills.All;
        _ = NecromancySkills.Items;
    }
}
