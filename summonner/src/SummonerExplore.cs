using ModShardLauncher;

namespace Summoner;

/// <summary>
/// Inspect (Explore) window: show companion Level, XP, and level-up parameter bonuses.
/// </summary>
public static class SummonerExplore
{
    const string CreateMarker = "vs_sm_explore_create";
    const string LayoutMarker = "vs_sm_explore_layout";
    const string DrawMarker = "vs_sm_explore_draw";

    public static void Patch()
    {
        PatchCreate();
        PatchLayout();
        PatchDraw();
    }

    static void PatchCreate()
    {
        Msl.LoadGML("gml_Object_o_exploreRender_Create_0")
            .Apply(lines =>
            {
                var list = lines as IList<string> ?? lines.ToList();
                if (list.Any(l => l.Contains(CreateMarker, StringComparison.Ordinal)))
                    return list;

                var idx = -1;
                for (var i = 0; i < list.Count; i++)
                {
                    if (list[i].Trim() == "descriptionHeight = 0")
                    {
                        idx = i;
                        break;
                    }
                }

                if (idx < 0)
                    return list;

                var indent = LeadingWhitespace(list[idx]);
                var output = new List<string>(list.Count + 2);
                output.AddRange(list.Take(idx + 1));
                output.Add($"{indent}// {CreateMarker}");
                output.Add($"{indent}companionProgressHeight = 0");
                output.AddRange(list.Skip(idx + 1));
                return output;
            })
            .Save();
    }

    static void PatchLayout()
    {
        // After morale block, before buffs — reserve height for companion progression.
        Msl.LoadGML("gml_Object_o_exploreRender_Other_20")
            .Apply(lines =>
            {
                var list = lines as IList<string> ?? lines.ToList();
                if (list.Any(l => l.Contains(LayoutMarker, StringComparison.Ordinal)))
                    return list;

                var idx = -1;
                for (var i = 0; i < list.Count; i++)
                {
                    if (list[i].Contains("other.contentHeight += (other.mainParamsHeight + other.armorDurabilityHeight + other.moraleHeight)", StringComparison.Ordinal))
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
                    $"{indent}// {LayoutMarker}",
                    $"{indent}other.companionProgressHeight = 0",
                    $"{indent}if (variable_instance_exists(id, \"sm_summon\") && sm_summon)",
                    $"{indent}{{",
                    $"{indent}    var _smLines = 2",
                    $"{indent}    if (variable_instance_exists(id, \"sm_bonus_hp\") && sm_bonus_hp != 0)",
                    $"{indent}        _smLines += 1",
                    $"{indent}    if (variable_instance_exists(id, \"sm_bonus_dmg\") && sm_bonus_dmg != 0)",
                    $"{indent}        _smLines += 1",
                    $"{indent}    if (variable_instance_exists(id, \"sm_bonus_armor\") && sm_bonus_armor != 0)",
                    $"{indent}        _smLines += 1",
                    $"{indent}    if (variable_instance_exists(id, \"sm_bonus_str\") && sm_bonus_str != 0)",
                    $"{indent}        _smLines += 1",
                    $"{indent}    if (variable_instance_exists(id, \"sm_bonus_agl\") && sm_bonus_agl != 0)",
                    $"{indent}        _smLines += 1",
                    $"{indent}    if (variable_instance_exists(id, \"sm_bonus_prc\") && sm_bonus_prc != 0)",
                    $"{indent}        _smLines += 1",
                    $"{indent}    if (variable_instance_exists(id, \"sm_bonus_vit\") && sm_bonus_vit != 0)",
                    $"{indent}        _smLines += 1",
                    $"{indent}    other.companionProgressHeight = _fontDmgHeight * _smLines",
                    $"{indent}    other.contentHeight += (_spaceHeight + _fontDmgHeight + _lineHeight + other.companionProgressHeight)",
                    $"{indent}}}",
                };

                var output = new List<string>(list.Count + block.Length);
                output.AddRange(list.Take(idx + 1));
                output.AddRange(block);
                output.AddRange(list.Skip(idx + 1));
                return output;
            })
            .Save();
    }

    static void PatchDraw()
    {
        // Draw after morale, before buffs.
        Msl.LoadGML("gml_Object_o_exploreRender_Other_21")
            .Apply(lines =>
            {
                var list = lines as IList<string> ?? lines.ToList();
                if (list.Any(l => l.Contains(DrawMarker, StringComparison.Ordinal)))
                    return list;

                var idx = -1;
                for (var i = 0; i < list.Count; i++)
                {
                    if (list[i].Contains("if other.buffsHeight", StringComparison.Ordinal))
                    {
                        idx = i;
                        break;
                    }
                }

                if (idx < 0)
                    return list;

                var indent = LeadingWhitespace(list[idx]);
                var green = "make_colour_rgb(125, 185, 95)";
                var block = new[]
                {
                    $"{indent}// {DrawMarker}",
                    $"{indent}if other.companionProgressHeight",
                    $"{indent}{{",
                    $"{indent}    _offsetY += _spaceHeight",
                    $"{indent}    scr_drawText((_contentX + _contentWidth / 2), (_contentY + _offsetY), \"Progression\", c_white, fa_center, fa_top, global.f_dmg, other.textScale)",
                    $"{indent}    _offsetY += _fontDmgHeight",
                    $"{indent}    scr_hoversDrawLine(_contentX, (_contentY + _offsetY), _contentWidth, _lineHeight, other.surfaceScale)",
                    $"{indent}    _offsetY += _lineHeight",
                    $"{indent}    var _smLvl = 1",
                    $"{indent}    if (variable_instance_exists(id, \"sm_level\"))",
                    $"{indent}        _smLvl = sm_level",
                    $"{indent}    else if (variable_instance_exists(id, \"sm_species\"))",
                    $"{indent}        _smLvl = scr_sm_level_get(sm_species)",
                    $"{indent}    scr_drawText(_contentX, (_contentY + _offsetY), ds_map_find_value(global.attribute, \"LVL\"), c_white, fa_left, fa_top, global.f_dmg, other.textScale)",
                    $"{indent}    scr_drawText((_contentX + _contentWidth), (_contentY + _offsetY), string(_smLvl), c_white, fa_right, fa_top, global.f_dmg, other.textScale)",
                    $"{indent}    _offsetY += _fontDmgHeight",
                    $"{indent}    var _smXp = 0",
                    $"{indent}    var _smXpNext = 0",
                    $"{indent}    if (variable_instance_exists(id, \"sm_species\"))",
                    $"{indent}    {{",
                    $"{indent}        _smXp = scr_sm_xp_get(sm_species)",
                    $"{indent}        _smXpNext = scr_sm_xp_next(sm_species)",
                    $"{indent}    }}",
                    $"{indent}    var _smXpText = string(floor(_smXp))",
                    $"{indent}    if (_smXpNext > 0)",
                    $"{indent}        _smXpText = (string(floor(_smXp)) + \" / \" + string(floor(_smXpNext)))",
                    $"{indent}    scr_drawText(_contentX, (_contentY + _offsetY), ds_map_find_value(global.attribute, \"XP\"), c_white, fa_left, fa_top, global.f_dmg, other.textScale)",
                    $"{indent}    scr_drawText((_contentX + _contentWidth), (_contentY + _offsetY), _smXpText, c_white, fa_right, fa_top, global.f_dmg, other.textScale)",
                    $"{indent}    _offsetY += _fontDmgHeight",
                    $"{indent}    if (variable_instance_exists(id, \"sm_bonus_hp\") && sm_bonus_hp != 0)",
                    $"{indent}    {{",
                    $"{indent}        scr_drawText(_contentX, (_contentY + _offsetY), ds_map_find_value(global.attribute, \"HP\"), c_white, fa_left, fa_top, global.f_dmg, other.textScale)",
                    $"{indent}        scr_drawText((_contentX + _contentWidth), (_contentY + _offsetY), (\"+\" + string(sm_bonus_hp)), {green}, fa_right, fa_top, global.f_dmg, other.textScale)",
                    $"{indent}        _offsetY += _fontDmgHeight",
                    $"{indent}    }}",
                    $"{indent}    if (variable_instance_exists(id, \"sm_bonus_dmg\") && sm_bonus_dmg != 0)",
                    $"{indent}    {{",
                    $"{indent}        scr_drawText(_contentX, (_contentY + _offsetY), ds_map_find_value(global.attribute, \"Weapon_Damage\"), c_white, fa_left, fa_top, global.f_dmg, other.textScale)",
                    $"{indent}        scr_drawText((_contentX + _contentWidth), (_contentY + _offsetY), (\"+\" + string(sm_bonus_dmg)), {green}, fa_right, fa_top, global.f_dmg, other.textScale)",
                    $"{indent}        _offsetY += _fontDmgHeight",
                    $"{indent}    }}",
                    $"{indent}    if (variable_instance_exists(id, \"sm_bonus_armor\") && sm_bonus_armor != 0)",
                    $"{indent}    {{",
                    $"{indent}        scr_drawText(_contentX, (_contentY + _offsetY), ds_map_find_value(global.attribute, \"Physical_Resistance\"), c_white, fa_left, fa_top, global.f_dmg, other.textScale)",
                    $"{indent}        scr_drawText((_contentX + _contentWidth), (_contentY + _offsetY), (\"+\" + string(sm_bonus_armor)), {green}, fa_right, fa_top, global.f_dmg, other.textScale)",
                    $"{indent}        _offsetY += _fontDmgHeight",
                    $"{indent}    }}",
                    $"{indent}    if (variable_instance_exists(id, \"sm_bonus_str\") && sm_bonus_str != 0)",
                    $"{indent}    {{",
                    $"{indent}        scr_drawText(_contentX, (_contentY + _offsetY), ds_map_find_value(global.attribute, \"STR\"), c_white, fa_left, fa_top, global.f_dmg, other.textScale)",
                    $"{indent}        scr_drawText((_contentX + _contentWidth), (_contentY + _offsetY), (\"+\" + string(sm_bonus_str)), {green}, fa_right, fa_top, global.f_dmg, other.textScale)",
                    $"{indent}        _offsetY += _fontDmgHeight",
                    $"{indent}    }}",
                    $"{indent}    if (variable_instance_exists(id, \"sm_bonus_agl\") && sm_bonus_agl != 0)",
                    $"{indent}    {{",
                    $"{indent}        scr_drawText(_contentX, (_contentY + _offsetY), ds_map_find_value(global.attribute, \"AGL\"), c_white, fa_left, fa_top, global.f_dmg, other.textScale)",
                    $"{indent}        scr_drawText((_contentX + _contentWidth), (_contentY + _offsetY), (\"+\" + string(sm_bonus_agl)), {green}, fa_right, fa_top, global.f_dmg, other.textScale)",
                    $"{indent}        _offsetY += _fontDmgHeight",
                    $"{indent}    }}",
                    $"{indent}    if (variable_instance_exists(id, \"sm_bonus_prc\") && sm_bonus_prc != 0)",
                    $"{indent}    {{",
                    $"{indent}        scr_drawText(_contentX, (_contentY + _offsetY), ds_map_find_value(global.attribute, \"PRC\"), c_white, fa_left, fa_top, global.f_dmg, other.textScale)",
                    $"{indent}        scr_drawText((_contentX + _contentWidth), (_contentY + _offsetY), (\"+\" + string(sm_bonus_prc)), {green}, fa_right, fa_top, global.f_dmg, other.textScale)",
                    $"{indent}        _offsetY += _fontDmgHeight",
                    $"{indent}    }}",
                    $"{indent}    if (variable_instance_exists(id, \"sm_bonus_vit\") && sm_bonus_vit != 0)",
                    $"{indent}    {{",
                    $"{indent}        scr_drawText(_contentX, (_contentY + _offsetY), ds_map_find_value(global.attribute, \"Vitality\"), c_white, fa_left, fa_top, global.f_dmg, other.textScale)",
                    $"{indent}        scr_drawText((_contentX + _contentWidth), (_contentY + _offsetY), (\"+\" + string(sm_bonus_vit)), {green}, fa_right, fa_top, global.f_dmg, other.textScale)",
                    $"{indent}        _offsetY += _fontDmgHeight",
                    $"{indent}    }}",
                    $"{indent}}}",
                    "",
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
