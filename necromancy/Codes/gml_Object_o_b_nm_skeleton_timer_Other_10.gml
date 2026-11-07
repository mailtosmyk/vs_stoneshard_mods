event_inherited()
duration = min(duration, max_duration)

// Nested checks: GML may evaluate both sides of || / &&.
if (!variable_instance_exists(id, "nm_skel"))
    return;
if (nm_skel == -4)
    return;
if (!instance_exists(nm_skel))
{
    instance_destroy()
    return;
}
if (!instance_exists(o_player))
    return;

// Keep ally bind sticky, and drive follow/combat when the servant buff is absent.
with (nm_skel)
{
    owner = other.owner
    is_neutral = true
    is_player_enemy = false
}

var _servant = asset_get_index("o_b_nm_servant")
if (_servant != -1 && instance_exists(scr_instance_exists_in_list(_servant, nm_skel.buffs)))
    return;

var _hostile = 0
with (o_enemy)
{
    if (id != other.nm_skel && HP > 0 && (!is_neutral) && is_player_enemy)
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
    nm_skel.state = "npc combat"
}
else
{
    with (nm_skel)
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
