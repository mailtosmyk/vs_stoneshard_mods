event_inherited()
if ((!instance_exists(target)) || (!instance_exists(o_player)))
    return;

// Keep ally flags sticky (enemy AI can flip them).
with (target)
{
    owner = other.owner
    is_neutral = true
    is_player_enemy = false

    // Sticky: never let the pet treat the caster as a combat target.
    var _ca = []
    var _i = 0.0
    while (_i < array_length(combat_array))
    {
        if (combat_array[_i] != "Player")
            _ca[array_length(_ca)] = combat_array[_i]
        _i += 1.0
    }
    combat_array = _ca
}

// Auto: fight nearest visible hostile near the player, otherwise follow.
// state = -4 suppresses idle patrol / leftover Player targeting in the new FSM.
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
        // Close the gap when not adjacent; update_FSM event_user(2) handles melee.
        if (ds_list_empty(lock_turn) && scr_tile_distance(id, _best) > 1)
            scr_enemy_priority_path_target(_best.x, _best.y)
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
                scr_enemy_priority_path_target(o_player.x, o_player.y)
        }
    }
}
