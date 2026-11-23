function scr_sm_xp_add(argument0, argument1)
{
    // argument0 = species slug, argument1 = xp amount
    if (!instance_exists(o_player) || (argument1 <= 0))
        return
    var _prevLvl = scr_sm_level_get(argument0)
    var _key = ("sm_xp_" + string(argument0))
    var _next = (scr_sm_xp_get(argument0) + argument1)
    if (!variable_instance_exists(o_player, _key))
        variable_instance_set(o_player, _key, _next)
    else
        variable_instance_set(o_player, _key, _next)
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