using ModShardLauncher;

namespace Summoner;

/// <summary>
/// Pack Link: while the player buff is up, a share of damage dealt by sm_summon units heals the player.
/// Assembly-only hook after vanilla lifesteal/manasteal in scr_physical_damage — ReplaceGML cannot
/// round-trip decompiled <c>self.on_damage_method(...)</c> without breaking the method call.
/// </summary>
public static class SummonerPackLink
{
    const string HealCall = "call.i gml_Script_scr_sm_pack_link_heal(argc=1)";

    const string ManastealCall = "call.i gml_Script_scr_manasteal(argc=2)";

    public static void Patch()
    {
        Msl.LoadAssemblyAsString("gml_GlobalScript_scr_physical_damage")
            .Apply(InsertHealCall)
            .Save();
    }

    static IEnumerable<string> InsertHealCall(IEnumerable<string> input)
    {
        var lines = input as IList<string> ?? input.ToList();
        if (lines.Any(l => l.Contains("scr_sm_pack_link_heal", StringComparison.Ordinal)))
            return lines;

        var manaIdx = -1;
        for (var i = 0; i < lines.Count; i++)
        {
            if (lines[i].Contains(ManastealCall, StringComparison.Ordinal))
            {
                manaIdx = i;
                break;
            }
        }

        if (manaIdx < 0)
        {
            throw new InvalidOperationException(
                "Summoner: scr_manasteal site not found in gml_GlobalScript_scr_physical_damage");
        }

        // After manasteal's popz.v, still inside (id != target). Leaves on_damage_method callv intact.
        var insertAt = manaIdx + 1;
        if (insertAt < lines.Count && lines[insertAt].Trim() == "popz.v")
            insertAt++;

        var block = new[]
        {
            "push.v arg.argument1",
            HealCall,
            "popz.v",
        };

        var output = new List<string>(lines.Count + block.Length);
        output.AddRange(lines.Take(insertAt));
        output.AddRange(block);
        output.AddRange(lines.Skip(insertAt));
        return output;
    }
}
