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
            is_crit = other.is_crit
            // o_spellbirth never sets is_fumble — only is_crit (see spellbirth Other_10).
        }
        with (owner)
            scr_unitInterractImpact(id, other.point)
    }
}
