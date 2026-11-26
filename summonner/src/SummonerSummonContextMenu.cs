using ModShardLauncher;

namespace Summoner;

/// <summary>
/// RMB on player summons: Inspect (Explore) + tile Swap + Unsummon instead of the enemy debug menu.
/// Idempotent with Necromancy's matching patch (shared markers / both summon flags).
/// </summary>
public static class SummonerSummonContextMenu
{
    const string Marker = "vs_summon_context_menu";
    const string SwapMarker = "vs_summon_swap_idle";
    const string UnsummonMenuMarker = "vs_summon_unsummon_menu";
    const string UnsummonClickMarker = "vs_summon_unsummon_click";

    const string VanillaEnemyMenu =
        "scr_create_context_menu(\"Attack\", \"Explore\", \"Change_Dummy\", \"Move_Dummy\", \"Destroy_Entity\")";

    const string VanillaSwapGate =
        "if (other.is_neutral && other.state != \"idle\" && scr_tile_distance(other.id, o_player) <= 1)";

    const string ExploreCaseAnchor =
        "ds_list_add(context_name, \"Explore\", ds_list_find_value(global.context_menu, 28))";

    const string ExploreClickAnchor = "case \"Explore\":";

    public static void Patch()
    {
        PatchEnemyMouseMenu();
        PatchSwapAllowsIdleSummons();
        PatchUnsummonMenuOption();
        PatchUnsummonClick();
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
                // Explore = Inspect; Swap = exchange tiles with the PC; Unsummon = safe dismiss.
                // No Attack — that would let the player strike their own companion.
                var block = new[]
                {
                    $"{indent}// {Marker}",
                    $"{indent}if (((variable_instance_exists(id, \"nm_summon\") && nm_summon) || (variable_instance_exists(id, \"sm_summon\") && sm_summon)))",
                    $"{indent}    scr_create_context_menu(\"Explore\", \"Swap\", \"Unsummon\")",
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

    static void PatchUnsummonMenuOption()
    {
        // Register "Unsummon" as a valid scr_create_context_menu argument (else default drops it).
        Msl.LoadGML("gml_GlobalScript_scr_create_context_menu")
            .Apply(lines =>
            {
                var list = lines as IList<string> ?? lines.ToList();
                if (list.Any(l => l.Contains(UnsummonMenuMarker, StringComparison.Ordinal)))
                    return list;

                var idx = -1;
                for (var i = 0; i < list.Count; i++)
                {
                    if (list[i].Contains(ExploreCaseAnchor, StringComparison.Ordinal))
                    {
                        idx = i;
                        break;
                    }
                }

                if (idx < 0)
                    return list;

                // Land after the Explore case's break.
                var insertAt = idx + 1;
                while (insertAt < list.Count && !list[insertAt].TrimStart().StartsWith("break", StringComparison.Ordinal))
                    insertAt++;
                if (insertAt < list.Count)
                    insertAt++;

                var indent = LeadingWhitespace(list[idx]);
                var block = new[]
                {
                    $"{indent}// {UnsummonMenuMarker}",
                    $"{indent}case \"Unsummon\":",
                    $"{indent}    if ((variable_instance_exists(other.id, \"nm_summon\") && other.nm_summon) || (variable_instance_exists(other.id, \"sm_summon\") && other.sm_summon))",
                    $"{indent}    {{",
                    $"{indent}        ds_list_add(context_name, \"Unsummon\", \"Unsummon\")",
                    $"{indent}        ds_list_add(context_desc, 1, 0)",
                    $"{indent}    }}",
                    $"{indent}    break",
                };

                var output = new List<string>(list.Count + block.Length);
                output.AddRange(list.Take(insertAt));
                output.AddRange(block);
                output.AddRange(list.Skip(insertAt));
                return output;
            })
            .Save();
    }

    static void PatchUnsummonClick()
    {
        // Handle the Unsummon button: safe dismiss (Summoner XP / no Morale; Necromancy Unbind rules).
        Msl.LoadGML("gml_Object_o_context_button_Mouse_4")
            .Apply(lines =>
            {
                var list = lines as IList<string> ?? lines.ToList();
                if (list.Any(l => l.Contains(UnsummonClickMarker, StringComparison.Ordinal)))
                    return list;

                var idx = -1;
                for (var i = 0; i < list.Count; i++)
                {
                    if (list[i].Trim() == ExploreClickAnchor
                        || list[i].Contains(ExploreClickAnchor, StringComparison.Ordinal))
                    {
                        idx = i;
                        break;
                    }
                }

                if (idx < 0)
                    return list;

                var indent = LeadingWhitespace(list[idx]);
                var block = new[]
                {
                    $"{indent}// {UnsummonClickMarker}",
                    $"{indent}case \"Unsummon\":",
                    $"{indent}    if instance_exists(interact_id)",
                    $"{indent}    {{",
                    $"{indent}        var _nm = (variable_instance_exists(interact_id, \"nm_summon\") && interact_id.nm_summon)",
                    $"{indent}        var _sm = (variable_instance_exists(interact_id, \"sm_summon\") && interact_id.sm_summon)",
                    $"{indent}        if _nm",
                    $"{indent}        {{",
                    $"{indent}            var _mastery = asset_get_index(\"o_nm_mastery_of_binding_ico\")",
                    $"{indent}            var _free = (_mastery != -1 && instance_exists(_mastery) && _mastery.is_open)",
                    $"{indent}            if instance_exists(o_player)",
                    $"{indent}            {{",
                    $"{indent}                if _free",
                    $"{indent}                {{",
                    $"{indent}                    with (o_player)",
                    $"{indent}                        scr_restore_mp(id, (0.1 * Magic_Power), \"Mastery of Binding\")",
                    $"{indent}                }}",
                    $"{indent}                else",
                    $"{indent}                    scr_allturn()",
                    $"{indent}            }}",
                    $"{indent}            var _servant = asset_get_index(\"o_b_nm_servant\")",
                    $"{indent}            if (_servant != -1)",
                    $"{indent}            {{",
                    $"{indent}                with (scr_instance_exists_in_list(_servant, interact_id.buffs))",
                    $"{indent}                    instance_destroy()",
                    $"{indent}            }}",
                    $"{indent}            var _unbind = asset_get_index(\"o_b_nm_unbind\")",
                    $"{indent}            if (_unbind != -1)",
                    $"{indent}            {{",
                    $"{indent}                with (scr_instance_exists_in_list(_unbind, interact_id.buffs))",
                    $"{indent}                    instance_destroy()",
                    $"{indent}            }}",
                    $"{indent}            with (interact_id)",
                    $"{indent}            {{",
                    $"{indent}                corpse_type = noone",
                    $"{indent}                is_corpse_spawn = false",
                    $"{indent}                instance_destroy()",
                    $"{indent}            }}",
                    $"{indent}        }}",
                    $"{indent}        else if _sm",
                    $"{indent}        {{",
                    $"{indent}            with (interact_id)",
                    $"{indent}            {{",
                    $"{indent}                sm_safe_dismiss = true",
                    $"{indent}                corpse_type = noone",
                    $"{indent}                is_corpse_spawn = false",
                    $"{indent}                instance_destroy()",
                    $"{indent}            }}",
                    $"{indent}            if instance_exists(o_player)",
                    $"{indent}                scr_allturn()",
                    $"{indent}        }}",
                    $"{indent}    }}",
                    $"{indent}    break",
                };

                var output = new List<string>(list.Count + block.Length);
                output.AddRange(list.Take(idx));
                output.AddRange(block);
                output.AddRange(list.Skip(idx));
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
