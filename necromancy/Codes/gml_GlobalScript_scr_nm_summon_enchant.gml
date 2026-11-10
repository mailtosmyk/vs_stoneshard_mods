function scr_nm_summon_enchant(argument0) //gml_Script_scr_nm_summon_enchant
{
    // Sum equipped gear values for a necromancy summon enchant key
    // (e.g. "nm_Summon_Health", "nm_Summon_Damage").
    var _total = 0.0
    with (o_inv_slot)
    {
        if (equipped && ds_exists(data, 1))
        {
            var _raw = ds_map_find_value(data, argument0)
            if (!__is_undefined(_raw))
                _total += real(_raw)
        }
    }
    return _total
}
