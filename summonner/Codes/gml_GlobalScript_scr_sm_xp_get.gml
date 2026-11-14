function scr_sm_xp_get(argument0)
{
    if (!instance_exists(o_player))
        return 0
    var _key = ("sm_xp_" + string(argument0))
    if (!variable_instance_exists(o_player, _key))
        return 0
    return variable_instance_get(o_player, _key)
}
