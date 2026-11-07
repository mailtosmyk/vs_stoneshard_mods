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
}
