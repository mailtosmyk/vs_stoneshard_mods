event_inherited()
if instance_exists(target)
{
    var _owner = owner
    var _tx = target.x
    var _ty = target.y
    var _shard = asset_get_index("o_nm_bone_shard_hit")
    with (o_enemy)
    {
        if ((!is_player(id)) && point_distance(x, y, _tx, _ty) <= 36)
        {
            if (_shard != -1)
            {
                with (instance_create_depth(x, y, 0, _shard))
                {
                    owner = _owner
                    target = other.id
                    alarm[1] = 1
                }
            }
        }
    }
}
