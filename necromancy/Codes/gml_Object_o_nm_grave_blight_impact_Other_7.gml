if (instance_exists(owner) && instance_exists(target))
{
    var _tx = floor(target.x / 26)
    var _ty = floor(target.y / 26)
    var _zx = (_tx * 26) + 13
    var _zy = (_ty * 26) + 13
    // Base 3x3 (radius 1); grows to 5x5 (radius 2) at 150% Magic Power.
    var _radius = 1
    if (owner.Magic_Power >= 150)
        _radius = 2

    var _buff = asset_get_index("o_b_nm_grave_blight")
    if (_buff != -1)
    {
        with (scr_instance_exists_in_list(_buff, owner.buffs))
            instance_destroy()
        with (scr_effect_create(_buff, turns, owner, owner))
        {
            zone_x = _zx
            zone_y = _zy
            zone_radius = _radius
        }
    }

    var _zone = asset_get_index("o_nm_grave_blight_zone")
    if (_zone != -1)
    {
        with (_zone)
            instance_destroy()
        for (var _dx = (-_radius); _dx <= _radius; _dx++)
        {
            for (var _dy = (-_radius); _dy <= _radius; _dy++)
            {
                var _px = ((_tx + _dx) * 26) + 13
                var _py = ((_ty + _dy) * 26) + 13
                with (instance_create_depth(_px, _py, 0, _zone))
                {
                    owner = other.owner
                    duration = other.turns
                }
            }
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
