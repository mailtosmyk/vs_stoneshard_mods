if (instance_exists(owner) && instance_exists(target))
{
    var _zx = scr_round_cell(target.x) + 13
    var _zy = scr_round_cell(target.y) + 13
    var _buff = asset_get_index("o_b_nm_grave_blight")
    if (_buff != -1)
    {
        with (scr_instance_exists_in_list(_buff, owner.buffs))
            instance_destroy()
        with (scr_effect_create(_buff, turns, owner, owner))
        {
            zone_x = _zx
            zone_y = _zy
        }
    }
    var _zone = asset_get_index("o_nm_grave_blight_zone")
    if (_zone != -1)
    {
        with (_zone)
            instance_destroy()
        with (instance_create_depth(_zx, _zy, 0, _zone))
        {
            owner = other.owner
            duration = other.turns
        }
    }
}
event_inherited()
if instance_exists(owner)
{
    if is_player(owner)
    {
        var _san = 0.15
        var _sealed = asset_get_index("o_nm_sealed_mind_ico")
        if (_sealed != -1 && instance_exists(_sealed) && _sealed.is_open)
            _san *= 0.5
        scr_psy_change("Sanity", -_san)
        scr_allturn()
    }
}
