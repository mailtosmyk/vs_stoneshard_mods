if (!is_execute)
{
    is_execute = 1
    var _target = owner
    if instance_exists(point)
        _target = point
    with (instance_create_depth(owner.x, owner.y, 0, spell))
    {
        owner = other.owner
        target = _target
        name = other.name
        turns = 4
        if instance_exists(o_deadbody)
        {
            with (o_deadbody)
            {
                if (point_distance(x, y, other.owner.x, other.owner.y) <= 36)
                    other.turns += 1
            }
        }
    }
}
