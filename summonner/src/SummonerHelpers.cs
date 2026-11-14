using ModShardLauncher;
using ModShardLauncher.Mods;

namespace Summoner;

/// <summary>
/// Registers global GML helpers used by summon / unsummon / map-travel dismiss.
/// </summary>
public static class SummonerHelpers
{
    public static void Patch(ModFile modFiles)
    {
        Msl.AddFunction(modFiles.GetCode("gml_GlobalScript_scr_sm_unsummon_all.gml"), "scr_sm_unsummon_all");
        Msl.AddFunction(modFiles.GetCode("gml_GlobalScript_scr_sm_count_summons.gml"), "scr_sm_count_summons");
        Msl.AddFunction(modFiles.GetCode("gml_GlobalScript_scr_sm_max_summons.gml"), "scr_sm_max_summons");
        Msl.AddFunction(modFiles.GetCode("gml_GlobalScript_scr_sm_bind_summon.gml"), "scr_sm_bind_summon");
        Msl.AddFunction(modFiles.GetCode("gml_GlobalScript_scr_sm_apply_level_stats.gml"), "scr_sm_apply_level_stats");
        Msl.AddFunction(modFiles.GetCode("gml_GlobalScript_scr_sm_xp_get.gml"), "scr_sm_xp_get");
        Msl.AddFunction(modFiles.GetCode("gml_GlobalScript_scr_sm_level_get.gml"), "scr_sm_level_get");
    }
}
