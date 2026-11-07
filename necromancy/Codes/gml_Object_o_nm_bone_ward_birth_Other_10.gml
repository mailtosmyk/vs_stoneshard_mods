if (!is_execute)
{
    is_execute = true
    var _turns = 4
    var _corpse = asset_get_index("o_corpse")
    if (_corpse != -1)
    {
        with (_corpse)
        {
            if (point_distance(x, y, other.owner.x, other.owner.y) <= 36)
                _turns = 5
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
            scr_psy_change("Sanity", -0.1)
            scr_allturn()
        }
    }
}
