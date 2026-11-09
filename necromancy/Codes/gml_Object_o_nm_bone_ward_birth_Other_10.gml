if (!is_execute)
{
    is_execute = true
    var _turns = 8
    var _corpse = asset_get_index("o_corpse")
    if (_corpse != -1)
    {
        with (_corpse)
        {
            if (point_distance(x, y, other.owner.x, other.owner.y) <= 36)
                _turns = 9
        }
    }
    with (instance_create_depth(owner.x, owner.y, 0, spell))
    {
        owner = other.owner
        target = other.owner
        name = other.name
        turns = _turns
    }
    var _buff = asset_get_index("o_b_bone_ward")
    if ((_buff != -1) && instance_exists(owner))
    {
        if scr_instance_exists_in_list(_buff, owner.buffs)
        {
            with (_buff)
            {
                if (target == other.owner)
                {
                    nm_force_shatter = 1
                    instance_destroy()
                }
            }
        }
        scr_effect_create(_buff, _turns, owner, owner)
    }
    if instance_exists(owner)
    {
        if is_player(owner)
        {
            var _san = 0.1
            var _sealed = asset_get_index("o_nm_sealed_mind_ico")
            if (_sealed != -1 && instance_exists(_sealed) && _sealed.is_open)
                _san *= 0.5
            scr_psy_change("Sanity", -_san)
            scr_allturn()
        }
    }
}
