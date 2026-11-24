using ModShardLauncher;

namespace Summoner;

/// <summary>
/// Summons must not aggro Unaware (idle) NPCs — preserves stealth.
/// Hunt Command marks still override so the player can force a target.
/// </summary>
public static class SummonerAi
{
    const string Marker = "vs_sm_skip_unaware";

    public static void Patch()
    {
        Msl.LoadGML("gml_GlobalScript_scr_nearest_in_array")
            .Apply(SkipUnawareTargets)
            .Save();
    }

    static IEnumerable<string> SkipUnawareTargets(IEnumerable<string> lines)
    {
        var list = lines as IList<string> ?? lines.ToList();
        if (list.Any(l => l.Contains(Marker, StringComparison.Ordinal)))
            return list;

        var idx = -1;
        for (var i = 0; i < list.Count - 1; i++)
        {
            if (list[i].Contains(
                    "else if (state != \"KO\" && state != \"jailed\" && (!is_ignored_by_enemies))",
                    StringComparison.Ordinal)
                && list[i + 1].Contains("array_push(_target_array, id)", StringComparison.Ordinal))
            {
                idx = i;
                break;
            }
        }

        if (idx < 0)
            return list;

        var indent = LeadingWhitespace(list[idx]);
        var bodyIndent = indent + "    ";
        var block = new[]
        {
            $"{indent}else if (state != \"KO\" && state != \"jailed\" && (!is_ignored_by_enemies))",
            $"{indent}{{",
            $"{bodyIndent}// {Marker}",
            $"{bodyIndent}var _sm_skip_unaware = false",
            $"{bodyIndent}if (state == \"idle\")",
            $"{bodyIndent}{{",
            $"{bodyIndent}    if (variable_instance_exists(_id, \"sm_summon\") && _id.sm_summon)",
            $"{bodyIndent}    {{",
            $"{bodyIndent}        _sm_skip_unaware = true",
            $"{bodyIndent}        if (instance_exists(o_player) && variable_instance_exists(o_player, \"sm_hunt_target\") && o_player.sm_hunt_target == id)",
            $"{bodyIndent}            _sm_skip_unaware = false",
            $"{bodyIndent}    }}",
            $"{bodyIndent}}}",
            $"{bodyIndent}if (!_sm_skip_unaware)",
            $"{bodyIndent}    array_push(_target_array, id)",
            $"{indent}}}",
        };

        var output = new List<string>(list.Count + block.Length);
        output.AddRange(list.Take(idx));
        output.AddRange(block);
        output.AddRange(list.Skip(idx + 2));
        return output;
    }

    static string LeadingWhitespace(string line)
    {
        var i = 0;
        while (i < line.Length && char.IsWhiteSpace(line[i]))
            i++;
        return line[..i];
    }
}
