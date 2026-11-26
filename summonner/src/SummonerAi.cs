using ModShardLauncher;

namespace Summoner;

/// <summary>
/// Summons must not aggro town/caravan friendlies (Player faction, neutrals, followers).
/// Hostiles in Vision stay valid even while idle so the pack can chain kills.
/// Hunt Command marks still override filters.
/// </summary>
public static class SummonerAi
{
    const string Marker = "vs_sm_skip_friendlies";
    const string LegacyMarker = "vs_sm_skip_unaware";

    public static void Patch()
    {
        Msl.LoadGML("gml_GlobalScript_scr_nearest_in_array")
            .Apply(SkipFriendlyTargets)
            .Save();
    }

    static IEnumerable<string> SkipFriendlyTargets(IEnumerable<string> lines)
    {
        var list = lines as IList<string> ?? lines.ToList();
        if (list.Any(l => l.Contains(Marker, StringComparison.Ordinal)))
            return list;

        // Replace a prior idle-skip patch in place if present.
        var legacyStart = -1;
        for (var i = 0; i < list.Count; i++)
        {
            if (list[i].Contains(LegacyMarker, StringComparison.Ordinal)
                || list[i].Contains(Marker, StringComparison.Ordinal))
            {
                legacyStart = FindBlockStart(list, i);
                break;
            }
        }

        if (legacyStart >= 0)
        {
            var legacyEnd = FindMatchingBrace(list, legacyStart);
            if (legacyEnd > legacyStart)
            {
                var indent = LeadingWhitespace(list[legacyStart]);
                var output = new List<string>(list.Count);
                output.AddRange(list.Take(legacyStart));
                output.AddRange(BuildBlock(indent));
                output.AddRange(list.Skip(legacyEnd + 1));
                return output;
            }
        }

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

        var baseIndent = LeadingWhitespace(list[idx]);
        var rebuilt = new List<string>(list.Count + 24);
        rebuilt.AddRange(list.Take(idx));
        rebuilt.AddRange(BuildBlock(baseIndent));
        rebuilt.AddRange(list.Skip(idx + 2));
        return rebuilt;
    }

    static string[] BuildBlock(string indent)
    {
        var bodyIndent = indent + "    ";
        return new[]
        {
            $"{indent}else if (state != \"KO\" && state != \"jailed\" && (!is_ignored_by_enemies))",
            $"{indent}{{",
            $"{bodyIndent}// {Marker}",
            $"{bodyIndent}var _sm_skip = false",
            $"{bodyIndent}if (variable_instance_exists(_id, \"sm_summon\") && _id.sm_summon)",
            $"{bodyIndent}{{",
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
            $"{bodyIndent}    var _sm_hunt_mark = asset_get_index(\"o_b_sm_hunt_mark\")",
            $"{bodyIndent}    if (_sm_skip && (_sm_hunt_mark != -1) && variable_instance_exists(id, \"buffs\") && scr_instance_exists_in_list(_sm_hunt_mark, buffs))",
            $"{bodyIndent}        _sm_skip = false",
            $"{bodyIndent}}}",
            $"{bodyIndent}if (!_sm_skip)",
            $"{bodyIndent}    array_push(_target_array, id)",
            $"{indent}}}",
        };
    }

    static int FindBlockStart(IList<string> list, int markerLine)
    {
        for (var i = markerLine; i >= 0; i--)
        {
            if (list[i].Contains(
                    "else if (state != \"KO\" && state != \"jailed\" && (!is_ignored_by_enemies))",
                    StringComparison.Ordinal))
                return i;
        }

        return -1;
    }

    static int FindMatchingBrace(IList<string> list, int start)
    {
        var depth = 0;
        for (var i = start; i < list.Count; i++)
        {
            var line = list[i];
            foreach (var ch in line)
            {
                if (ch == '{')
                    depth++;
                else if (ch == '}')
                {
                    depth--;
                    if (depth == 0)
                        return i;
                }
            }
        }

        return -1;
    }

    static string LeadingWhitespace(string line)
    {
        var i = 0;
        while (i < line.Length && char.IsWhiteSpace(line[i]))
            i++;
        return line[..i];
    }
}
