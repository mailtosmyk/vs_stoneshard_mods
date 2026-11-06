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
    public override string Version => "0.1.0";
    public override string TargetVersion => "0.9.4.25";

    public override void PatchMod()
    {
        NecromancyLocalization.PatchAll();

        // Skill definitions are available for future object / tree wiring.
        _ = NecromancySkills.All;
    }
}
