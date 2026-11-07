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

// Auto: fight nearest visible hostile near the player, otherwise follow.
// state = -4 suppresses the new FSM (idle would patrol / still treat Player as hostile).
var _srv = asset_get_index("o_b_nm_servant")
var _best = -4
var _best_d = 100000000.0
with (o_enemy)
{
    if (id == other.target || HP <= 0)
    {
    }
    else if (is_neutral || (!is_player_enemy))
    {
    }
    else if (_srv != -1 && instance_exists(scr_instance_exists_in_list(_srv, buffs)))
    {
    }
    else if (scr_round_cell(point_distance(x, y, o_player.x, o_player.y)) < 182)
    {
        if (visible && is_visible())
        {
            var _d = point_distance(x, y, other.target.x, other.target.y)
            if (_d < _best_d)
            {
                _best_d = _d
                _best = id
            }
        }
    }
}

if (instance_exists(_best))
{
    with (target)
    {
        target = _best
        state = "npc combat"
    }
}
else
{
    with (target)
    {
        state = -4
        if (ds_list_empty(lock_turn) && instance_exists(o_controller) && instance_exists(o_player))
        {
            if (scr_tile_distance(id, o_player) > 1)
            {
                // Current Stoneshard movement API (replaces mp_grid_path / xx,yy).
                scr_enemy_priority_path_target(o_player.x, o_player.y)
            }
        }
    }
}
