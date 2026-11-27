using ModShardLauncher;

namespace Summoner;

/// <summary>
/// Pack Link: while the player buff is up, a share of damage dealt by sm_summon units heals the player.
/// Hooks after vanilla lifesteal in scr_physical_damage (attacker is <c>id</c>).
/// </summary>
public static class SummonerPackLink
{
    const string Marker = "vs_sm_pack_link_heal";

    const string LifestealBlockEnd =
        "scr_manasteal(argument1, argument0)";

    public static void Patch()
    {
        Msl.LoadGML("gml_GlobalScript_scr_physical_damage")
            .Apply(lines =>
            {
                // Undertale decompiles method-var calls as self.fn(...), which ReplaceGML cannot
                // parse unless MSL EnableWithBlockFixes is on. Normalize so Pack Link always saves.
                var list = lines
                    .Select(l => l.Replace("self.on_damage_method(", "on_damage_method(", StringComparison.Ordinal))
                    .ToList();
                if (list.Any(l => l.Contains(Marker, StringComparison.Ordinal)))
                    return list;

                var idx = -1;
                for (var i = 0; i < list.Count; i++)
                {
                    if (list[i].Contains(LifestealBlockEnd, StringComparison.Ordinal))
                    {
                        idx = i;
                        break;
                    }
                }

                if (idx < 0)
                    return list;

                // Insert after the closing brace of the (id != argument0) block that contains lifesteal.
                var insertAt = idx + 1;
                while (insertAt < list.Count && list[insertAt].Trim() != "}")
                    insertAt++;
                if (insertAt < list.Count)
                    insertAt++;

                var indent = LeadingWhitespace(list[idx]);
                // Match the indent of the surrounding if (_is_unit) body (one level out from lifesteal calls).
                if (indent.Length >= 4)
                    indent = indent[..^4];

                var block = new[]
                {
                    $"{indent}// {Marker}",
                    $"{indent}if (argument1 > 0 && variable_instance_exists(id, \"sm_summon\") && sm_summon && instance_exists(o_player))",
                    $"{indent}{{",
                    $"{indent}    var _smLink = asset_get_index(\"o_b_sm_pack_link\")",
                    $"{indent}    if (_smLink != -1 && variable_instance_exists(o_player, \"buffs\") && scr_instance_exists_in_list(_smLink, o_player.buffs))",
                    $"{indent}    {{",
                    $"{indent}        var _smHeal = math_round(argument1 * 0.20)",
                    $"{indent}        if (_smHeal > 0)",
                    $"{indent}        {{",
                    $"{indent}            with (o_player)",
                    $"{indent}                scr_restore_hp(id, _smHeal, \"Pack Link\")",
                    $"{indent}        }}",
                    $"{indent}    }}",
                    $"{indent}}}",
                };

                var output = new List<string>(list.Count + block.Length);
                output.AddRange(list.Take(insertAt));
                output.AddRange(block);
                output.AddRange(list.Skip(insertAt));
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
