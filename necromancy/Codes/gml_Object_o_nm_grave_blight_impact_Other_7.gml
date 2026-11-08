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
        scr_psy_change("Sanity", -0.15)
        scr_allturn()
    }
}
