function scr_sm_apply_level_stats(argument0, argument1)
{
    // argument0 = unit, argument1 = species slug
    // Absolute tables from summonner.json skill.progression (hp / damage / armor per level).
    if (!instance_exists(argument0))
        return;
    var _lvl = scr_sm_level_get(argument1)
    var _idx = clamp((_lvl - 1), 0, 9)

    var _hp = [55.0, 58.0, 62.0, 65.0, 68.0, 72.0, 75.0, 78.0, 81.0, 85.0]
    var _dmg = [9.0, 9.0, 10.0, 10.0, 11.0, 11.0, 11.0, 12.0, 12.0, 13.0]
    var _armor = [0.0, 0.0, 1.0, 1.0, 1.0, 2.0, 2.0, 2.0, 3.0, 3.0]
    if (argument1 == "crawler")
    {
        _hp = [45.0, 47.0, 50.0, 52.0, 55.0, 57.0, 60.0, 62.0, 65.0, 67.0]
        _dmg = [11.0, 12.0, 12.0, 13.0, 13.0, 14.0, 14.0, 15.0, 15.0, 16.0]
        _armor = [0.0, 0.0, 0.0, 1.0, 1.0, 1.0, 1.0, 1.0, 2.0, 2.0]
    }
    else if (argument1 == "bear")
    {
        _hp = [100.0, 106.0, 113.0, 120.0, 126.0, 132.0, 139.0, 146.0, 152.0, 158.0]
        _dmg = [13.0, 14.0, 14.0, 15.0, 15.0, 16.0, 16.0, 17.0, 17.0, 18.0]
        _armor = [2.0, 3.0, 3.0, 4.0, 4.0, 5.0, 5.0, 6.0, 6.0, 7.0]
    }
    else if (argument1 == "bison")
    {
        _hp = [130.0, 139.0, 148.0, 157.0, 166.0, 176.0, 185.0, 194.0, 203.0, 212.0]
        _dmg = [10.0, 10.0, 11.0, 11.0, 11.0, 12.0, 12.0, 12.0, 13.0, 13.0]
        _armor = [4.0, 5.0, 6.0, 6.0, 7.0, 8.0, 8.0, 9.0, 10.0, 11.0]
    }
    else if (argument1 == "gulon")
    {
        _hp = [85.0, 90.0, 95.0, 100.0, 105.0, 110.0, 116.0, 121.0, 126.0, 131.0]
        _dmg = [16.0, 17.0, 18.0, 18.0, 19.0, 20.0, 21.0, 22.0, 22.0, 23.0]
        _armor = [1.0, 1.0, 2.0, 2.0, 3.0, 3.0, 4.0, 4.0, 5.0, 5.0]
    }

    var _hpVal = _hp[_idx]
    var _dmgVal = _dmg[_idx]
    var _armorVal = _armor[_idx]
    var _ups = max(0, (_lvl - 1))

    with (argument0)
    {
        var _ico = asset_get_index("o_sm_pack_instincts_ico")
        if (_ico != -1 && instance_exists(_ico) && _ico.is_open)
        {
            _hpVal = floor(_hpVal * 1.04)
            _dmgVal = floor(_dmgVal * 1.03)
        }
        max_hp = max(1, floor(_hpVal))
        HP = max_hp
        if (variable_instance_exists(id, "Weapon_Damage"))
            Weapon_Damage = max(1, floor(_dmgVal))
        else
            Weapon_Damage = max(1, floor(_dmgVal))
        if (variable_instance_exists(id, "Physical_Resistance"))
            Physical_Resistance += _armorVal
        else
            Physical_Resistance = _armorVal
        if (variable_instance_exists(id, "STR"))
            STR += (1.0 * _ups)
        if (variable_instance_exists(id, "AGL"))
            AGL += (1.0 * _ups)
        if (variable_instance_exists(id, "PRC"))
            PRC += (0.5 * _ups)
        if (variable_instance_exists(id, "Vitality"))
            Vitality += (1.0 * _ups)
        sm_species = argument1
        sm_level = _lvl
        sm_armor = _armorVal
    }
}
