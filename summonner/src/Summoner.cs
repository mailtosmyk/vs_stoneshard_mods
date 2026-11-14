using ModShardLauncher;
using ModShardLauncher.Mods;
using Summoner.Skills;

namespace Summoner;

public class Summoner : Mod
{
    public override string Author => "Vitali Smyk <mailtosmyk@gmail.com>";
    public override string Name => "Summoner";
    public override string Description =>
        "Beastbinder school: raise living companions that keep their own XP and level.";
    public override string Version => SummonerVersion.Value;
    public override string TargetVersion => "0.9.4.25";

    public override void PatchMod()
    {
        SummonerLocalization.PatchAll();
        SummonerHelpers.Patch(ModFiles);
        SummonerSkillsPatch.Patch(ModFiles);
        SummonerSchool.Patch(ModFiles);
        SummonerMapHooks.Patch();
        _ = SummonerSkills.All;
    }
}
