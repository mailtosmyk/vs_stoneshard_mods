using ModShardLauncher;

namespace Summoner;

/// <summary>
/// Dismiss all summons when the player changes map tile / room (XP kept, no Morale loss).
/// </summary>
public static class SummonerMapHooks
{
    public static void Patch()
    {
        // IMPORTANT: do not InsertBelow the `function ...` line.
        // That produces a single-statement function and leaves the original body as
        // top-level GlobalScript code, which runs at load and crashes on unset globals
        // (playerGridX via scr_globaltile_situations_validate).
        Msl.LoadGML("gml_GlobalScript_scr_locationPositionChangeTrigger")
            .MatchFrom("if (argument2 != -1 && argument3 != -1)")
            .InsertAbove("    scr_sm_unsummon_all()\n")
            .Save();

        Msl.LoadGML("gml_GlobalScript_scr_smoothRoomChange")
            .MatchFrom("if (argument1 == undefined)")
            .InsertAbove("    scr_sm_unsummon_all()\n")
            .Save();
    }
}
