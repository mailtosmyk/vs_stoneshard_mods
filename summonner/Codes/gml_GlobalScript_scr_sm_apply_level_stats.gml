function scr_sm_apply_level_stats(argument0, argument1)
{
    // argument0 = unit, argument1 = species slug
    if (!instance_exists(argument0))
        return;
    var _lvl = scr_sm_level_get(argument1)
    var _ups = max(0, (_lvl - 1))
    with (argument0)
    {
        var _hpMult = (1 + (6.0 * 0.01 * _ups))
        var _dmgMult = (1 + (4.0 * 0.01 * _ups))
        var _ico = asset_get_index("o_sm_pack_instincts_ico")
        if (_ico != -1 && instance_exists(_ico) && _ico.is_open)
        {
            _hpMult *= 1.04
            _dmgMult *= 1.03
        }
        max_hp = max(1, floor(max_hp * _hpMult))
        HP = max_hp
        if (variable_instance_exists(id, "Weapon_Damage"))
            Weapon_Damage = max(1, floor(Weapon_Damage * _dmgMult))
        if (variable_instance_exists(id, "STR"))
            STR += (1.0 * _ups)
        if (variable_instance_exists(id, "AGL"))
            AGL += (1.0 * _ups)
        if (variable_instance_exists(id, "PRC"))
            PRC += (0.5.0 * _ups)
        if (variable_instance_exists(id, "Vitality"))
            Vitality += (1.0 * _ups)
        sm_species = argument1
        sm_level = _lvl
    }
}
