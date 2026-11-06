event_inherited()
if instance_exists(target)
{
    var _owner = owner
    var _tx = target.x
    var _ty = target.y
    with (o_enemy)
    {
        if ((!is_player(id)) && point_distance(x, y, _tx, _ty) <= 36)
        {
            with (instance_create_depth(x, y, 0, o_nm_bone_shard_hit))
            {
                owner = _owner
                target = other.id
                alarm[1] = 1
            }
        }
    }
}
