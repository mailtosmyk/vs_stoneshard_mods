if ((!is_execute) && instance_exists(owner))
{
    is_execute = true
    var _buff = asset_get_index("o_b_sm_savage_bond")
    if (_buff != -1)
        scr_effect_create(_buff, 8.0, owner, owner)
    if is_player(owner)
        scr_allturn()
}
