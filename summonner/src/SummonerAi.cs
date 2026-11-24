using ModShardLauncher;

namespace Summoner;

/// <summary>
/// Summons must not aggro Unaware (idle) NPCs or caravan / town friendlies.
/// Hunt Command marks still override so the player can force a target.
/// </summary>
public static class SummonerAi
{
    const string Marker = "vs_sm_skip_unaware";

    public static void Patch()
    {
        Msl.LoadGML("gml_GlobalScript_scr_nearest_in_array")
            .Apply(SkipFriendlyAndUnawareTargets)
            .Save();
    }

    static IEnumerable<string> SkipFriendlyAndUnawareTargets(IEnumerable<string> lines)
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

        // Already expanded by a prior patch build — refresh body if marker lives inside braces.
        if (idx < 0)
        {
            for (var i = 0; i < list.Count; i++)
            {
                if (list[i].Contains(Marker, StringComparison.Ordinal))
                    return list;
            }

            return list;
        }

        var indent = LeadingWhitespace(list[idx]);
        var bodyIndent = indent + "    ";
        var block = new[]
        {
            $"{indent}else if (state != \"KO\" && state != \"jailed\" && (!is_ignored_by_enemies))",
            $"{indent}{{",
            $"{bodyIndent}// {Marker}",
            $"{bodyIndent}var _sm_skip = false",
            $"{bodyIndent}if (variable_instance_exists(_id, \"sm_summon\") && _id.sm_summon)",
            $"{bodyIndent}{{",
            $"{bodyIndent}    if (state == \"idle\")",
            $"{bodyIndent}        _sm_skip = true",
            $"{bodyIndent}    if (variable_instance_exists(id, \"faction_key\") && faction_key == \"Player\")",
            $"{bodyIndent}        _sm_skip = true",
            $"{bodyIndent}    if (variable_instance_exists(id, \"sm_summon\") && sm_summon)",
            $"{bodyIndent}        _sm_skip = true",
            $"{bodyIndent}    if (variable_instance_exists(id, \"nm_summon\") && nm_summon)",
            $"{bodyIndent}        _sm_skip = true",
            $"{bodyIndent}    if (variable_instance_exists(id, \"is_follower\") && is_follower)",
            $"{bodyIndent}        _sm_skip = true",
            $"{bodyIndent}    if (variable_instance_exists(id, \"is_player_enemy\") && (!is_player_enemy) && variable_instance_exists(id, \"is_neutral\") && is_neutral)",
            $"{bodyIndent}        _sm_skip = true",
            $"{bodyIndent}    if (_sm_skip && instance_exists(o_player) && variable_instance_exists(o_player, \"sm_hunt_target\") && o_player.sm_hunt_target == id)",
            $"{bodyIndent}        _sm_skip = false",
            $"{bodyIndent}}}",
            $"{bodyIndent}if (!_sm_skip)",
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
