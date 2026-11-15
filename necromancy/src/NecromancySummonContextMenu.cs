using ModShardLauncher;

namespace Necromancy;

/// <summary>
/// RMB on player summons: Inspect (Explore) + tile Swap instead of the enemy debug menu.
/// Idempotent with Summoner's matching patch (shared markers / both summon flags).
/// </summary>
public static class NecromancySummonContextMenu
{
    const string Marker = "vs_summon_context_menu";
    const string SwapMarker = "vs_summon_swap_idle";

    const string VanillaEnemyMenu =
        "scr_create_context_menu(\"Attack\", \"Explore\", \"Change_Dummy\", \"Move_Dummy\", \"Destroy_Entity\")";

    const string VanillaSwapGate =
        "if (other.is_neutral && other.state != \"idle\" && scr_tile_distance(other.id, o_player) <= 1)";

    public static void Patch()
    {
        PatchEnemyMouseMenu();
        PatchSwapAllowsIdleSummons();
    }

    static void PatchEnemyMouseMenu()
    {
        Msl.LoadGML("gml_Object_o_enemy_Mouse_5")
            .Apply(lines =>
            {
                var list = lines as IList<string> ?? lines.ToList();
                if (list.Any(l => l.Contains(Marker, StringComparison.Ordinal)))
                    return list;

                var idx = -1;
                for (var i = 0; i < list.Count; i++)
                {
                    if (list[i].Contains(VanillaEnemyMenu, StringComparison.Ordinal))
                    {
                        idx = i;
                        break;
                    }
                }

                if (idx < 0)
                    return list;

                var indent = LeadingWhitespace(list[idx]);
                // Explore = Inspect; Swap = exchange tiles with the PC. No Attack —
                // that would let the player strike their own thrall.
                var block = new[]
                {
                    $"{indent}// {Marker}",
                    $"{indent}if (((variable_instance_exists(id, \"nm_summon\") && nm_summon) || (variable_instance_exists(id, \"sm_summon\") && sm_summon)))",
                    $"{indent}    scr_create_context_menu(\"Explore\", \"Swap\")",
                    $"{indent}else",
                    $"{indent}    {VanillaEnemyMenu}",
                };

                var output = new List<string>(list.Count + block.Length);
                output.AddRange(list.Take(idx));
                output.AddRange(block);
                output.AddRange(list.Skip(idx + 1));
                return output;
            })
            .Save();
    }

    static void PatchSwapAllowsIdleSummons()
    {
        // Vanilla Swap requires state != "idle"; bound summons sit in idle and would
        // never show the option without this gate.
        Msl.LoadGML("gml_GlobalScript_scr_create_context_menu")
            .Apply(lines =>
            {
                var list = lines as IList<string> ?? lines.ToList();
                if (list.Any(l => l.Contains(SwapMarker, StringComparison.Ordinal)))
                    return list;

                var idx = -1;
                for (var i = 0; i < list.Count; i++)
                {
                    if (list[i].Contains(VanillaSwapGate, StringComparison.Ordinal)
                        || list[i].Trim() == VanillaSwapGate)
                    {
                        idx = i;
                        break;
                    }
                }

                if (idx < 0)
                    return list;

                var indent = LeadingWhitespace(list[idx]);
                var output = new List<string>(list.Count + 1);
                output.AddRange(list.Take(idx));
                output.Add($"{indent}// {SwapMarker}");
                output.Add(
                    $"{indent}if (other.is_neutral && (other.state != \"idle\" || (variable_instance_exists(other.id, \"nm_summon\") && other.nm_summon) || (variable_instance_exists(other.id, \"sm_summon\") && other.sm_summon)) && scr_tile_distance(other.id, o_player) <= 1)");
                output.AddRange(list.Skip(idx + 1));
                return output;
            })
            .Save();
    }

    static string LeadingWhitespace(string line)
    {
        var i = 0;
        while (i < line.Length && char.IsWhiteSpace(line[i]))
            i++;
        return line[..i];
    }
}
