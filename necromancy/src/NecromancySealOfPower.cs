using ModShardLauncher;
using ModShardLauncher.Mods;

namespace Necromancy;

/// <summary>
/// Extends Magic Mastery Seal of Power's necromancy transform (o_b_magic_unholy)
/// with Magic Power, Unholy Damage, and summon Health/Damage bonuses.
/// </summary>
public static class NecromancySealOfPower
{
    public static void Patch(ModFile modFiles)
    {
        Msl.AddFunction(modFiles.GetCode("gml_GlobalScript_scr_nm_refresh_summon_enchants.gml"),
            "scr_nm_refresh_summon_enchants");

        // Vanilla unholy seal: WIL*0.2 Unholy, 20*stage Magic Power, -10*stage Damage Received.
        // Necromancy kit: +15% Magic Power, +2 Unholy to strikes/shots, summon bonuses via apply helper.
        var unholyStats = new[]
        {
            "event_inherited()",
            "if instance_exists(target)",
            "{",
            "    ds_map_replace(data, \"Unholy_Damage\", 2)",
            "    ds_map_replace(data, \"Magic_Power\", 15)",
            "}",
            "",
        };
        Msl.LoadGML("gml_Object_o_b_magic_unholy_Other_15")
            .Apply(_ => unholyStats)
            .Save();

        // Re-apply summon HP/Damage as soon as the seal transforms onto the caster.
        Msl.LoadGML("gml_Object_o_b_magic_unholy_Alarm_2")
            .MatchFrom("event_inherited()")
            .InsertBelow("scr_nm_refresh_summon_enchants()\n")
            .Save();
    }
}
