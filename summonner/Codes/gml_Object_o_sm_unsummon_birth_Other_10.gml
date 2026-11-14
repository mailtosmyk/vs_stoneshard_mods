if ((!is_execute) && instance_exists(owner))
{
    is_execute = true
    var _target = -4
    if (instance_exists(point) && variable_instance_exists(point, "sm_summon") && point.sm_summon)
        _target = point
    if (!instance_exists(_target))
    {
        with (o_enemy)
        {
            if (variable_instance_exists(id, "sm_summon") && sm_summon)
            {
                if ((!instance_exists(_target)) || (point_distance(x, y, other.owner.x, other.owner.y) < point_distance(_target.x, _target.y, other.owner.x, other.owner.y)))
                    _target = id
            }
        }
    }
    if instance_exists(_target)
    {
        with (_target)
            instance_destroy()
    }
    if is_player(owner)
        scr_allturn()
}
