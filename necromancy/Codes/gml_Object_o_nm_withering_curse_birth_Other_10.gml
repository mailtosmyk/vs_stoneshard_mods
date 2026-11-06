if (!is_execute)
{
    if instance_exists(point)
    {
        is_execute = 1
        with (instance_create_depth(point.x, point.y, 0, spell))
        {
            owner = other.owner
            target = other.point
            name = other.name
            f_duration = 10
        }
        with (owner)
            scr_unitInterractImpact(id, other.point)
    }
}
