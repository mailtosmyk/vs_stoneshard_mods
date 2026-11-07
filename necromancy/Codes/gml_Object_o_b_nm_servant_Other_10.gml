event_inherited()
if ((!instance_exists(target)) || (!instance_exists(o_player)))
    return;

// Keep ally flags sticky (enemy AI can flip them).
with (target)
{
    owner = other.owner
    is_neutral = true
    is_player_enemy = false
}

// Auto: fight nearby hostiles, otherwise follow the player (BW check==3 style).
var _hostile = 0
with (o_enemy)
{
    if (id != other.target && HP > 0 && (!is_neutral) && is_player_enemy)
    {
        if (scr_round_cell(point_distance(x, y, o_player.x, o_player.y)) < 182)
        {
            if (visible && is_visible())
                _hostile++
        }
    }
}

if (_hostile)
{
    target.state = "npc combat"
}
else
{
    with (target)
    {
        state = -4
        if ds_list_empty(lock_turn)
        {
            with (o_controller)
                mp_grid_clear_cell(grid, (other.x / 26), (other.y / 26))
            scr_selfcell(0)
            var access = 0
            path = path_add()
            access = mp_grid_path(o_controller.grid, path, x, y, (((o_player.x div 26) * 26) + 13), (((o_player.y div 26) * 26) + 13), true)
            scr_selfcell(1)
            if access
            {
                var _x = path_get_point_x(path, step)
                var _y = path_get_point_y(path, step)
                if (mp_grid_get_cell(o_controller.grid, (_x / 26), (_y / 26)) == 0)
                {
                    if (xx != _x || yy != _y)
                    {
                        xx = _x
                        yy = _y
                        is_true = 1
                    }
                }
            }
        }
    }
}
