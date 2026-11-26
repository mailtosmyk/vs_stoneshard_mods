function scr_sm_xp_get(argument0)
{
    // Companion XP must survive map/room changes. o_player is NOT persistent, so
    // instance vars on the player are wiped when switching tiles. Store on
    // global.characterDataMap (also saved with the character).
    var _key = ("sm_xp_" + string(argument0))

    if (variable_global_exists("characterDataMap") && ds_exists(global.characterDataMap, 1))
    {
        if (ds_map_exists(global.characterDataMap, _key))
            return ds_map_find_value(global.characterDataMap, _key)
    }

    // Migrate leftover per-instance XP from older builds (same session only).
    if (instance_exists(o_player) && variable_instance_exists(o_player, _key))
    {
        var _legacy = variable_instance_get(o_player, _key)
        if (variable_global_exists("characterDataMap") && ds_exists(global.characterDataMap, 1))
            ds_map_set(global.characterDataMap, _key, _legacy)
        return _legacy
    }

    return 0
}
