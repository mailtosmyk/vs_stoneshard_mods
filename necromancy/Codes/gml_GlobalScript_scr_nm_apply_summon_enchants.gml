function scr_nm_apply_summon_enchants(argument0) //gml_Script_scr_nm_apply_summon_enchants
{
    // Apply equipped summon Health/Damage enchants onto a servant buff + recalc.
    // argument0 = summon unit instance
    if (!(instance_exists(argument0)))
        return 0

    var _hpEnch = scr_nm_summon_enchant("nm_Summon_Health")
    var _dmgEnch = scr_nm_summon_enchant("nm_Summon_Damage")
    var _servant = asset_get_index("o_b_nm_servant")
    if (_servant == -1)
        return 0

    with (scr_instance_exists_in_list(_servant, argument0.buffs))
    {
        var _baseHp = 0
        with (argument0)
        {
            if (variable_instance_exists(id, "bHP"))
                _baseHp = bHP
            else
                _baseHp = max_hp
        }
        ds_map_replace(data, "max_hp", floor((_baseHp * _hpEnch) * 0.01))
        ds_map_replace(data, "Weapon_Damage", _dmgEnch)
    }

    with (argument0)
        scr_atr_calc(id)
    return 1
}
