function scr_sm_xp_add(argument0, argument1)
{
    // argument0 = species slug, argument1 = xp amount
    if (argument1 <= 0)
        return
    var _prevLvl = scr_sm_level_get(argument0)
    var _key = ("sm_xp_" + string(argument0))
    var _next = (scr_sm_xp_get(argument0) + argument1)

    // Persist on characterDataMap so XP survives room changes and save/load.
    if (variable_global_exists("characterDataMap") && ds_exists(global.characterDataMap, 1))
        ds_map_set(global.characterDataMap, _key, _next)
    else if (instance_exists(o_player))
    {
        // Fallback before character map exists (very early boot).
        variable_instance_set(o_player, _key, _next)
    }

    var _newLvl = scr_sm_level_get(argument0)
    if (_newLvl > _prevLvl)
    {
        with (o_enemy)
        {
            if (variable_instance_exists(id, "sm_summon") && sm_summon && variable_instance_exists(id, "sm_species") && (sm_species == argument0))
                scr_sm_apply_level_stats(id, argument0)
        }
    }
}
