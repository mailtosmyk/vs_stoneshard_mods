using ModShardLauncher;

namespace Summoner;

/// <summary>
/// Dismiss all summons when the player changes map tile / room (XP kept, no Morale loss).
/// </summary>
public static class SummonerMapHooks
{
    public static void Patch()
    {
        // Global-map tile / location position changes.
        try
        {
            Msl.LoadGML("gml_GlobalScript_scr_locationPositionChangeTrigger")
                .MatchFrom("function scr_locationPositionChangeTrigger")
                .InsertBelow("    scr_sm_unsummon_all()\n")
                .Save();
        }
        catch
        {
            Msl.LoadGML("gml_GlobalScript_scr_locationPositionChangeTrigger")
                .MatchFrom("{")
                .InsertBelow("    scr_sm_unsummon_all()\n")
                .Save();
        }

        // Smooth room transitions (entering another location room).
        try
        {
            Msl.LoadGML("gml_GlobalScript_scr_smoothRoomChange")
                .MatchFrom("function scr_smoothRoomChange")
                .InsertBelow("    scr_sm_unsummon_all()\n")
                .Save();
        }
        catch
        {
            Msl.LoadGML("gml_GlobalScript_scr_smoothRoomChange")
                .MatchFrom("{")
                .InsertBelow("    scr_sm_unsummon_all()\n")
                .Save();
        }
    }
}
