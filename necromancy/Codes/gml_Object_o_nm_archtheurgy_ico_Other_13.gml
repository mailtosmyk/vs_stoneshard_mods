event_inherited()
if is_open
{
    if scr_chance_value(33)
    {
        var _buff = asset_get_index("o_b_nm_ascension")
        if (_buff != -1 && instance_exists(o_player))
            scr_effect_create(_buff, 6, o_player, o_player)
    }
}
