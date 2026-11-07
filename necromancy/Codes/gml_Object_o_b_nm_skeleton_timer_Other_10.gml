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

var _servant = asset_get_index("o_b_nm_servant")
if (_servant != -1 && instance_exists(scr_instance_exists_in_list(_servant, nm_skel.buffs)))
    return;

var _srv = _servant
var _best = -4
var _best_d = 100000000.0
with (o_enemy)
{
    if (id == other.nm_skel || HP <= 0)
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
            var _d = point_distance(x, y, other.nm_skel.x, other.nm_skel.y)
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
    with (nm_skel)
    {
        target = _best
        state = "npc combat"
        if (ds_list_empty(lock_turn) && scr_tile_distance(id, _best) > 1)
            scr_enemy_priority_path_target(_best.x, _best.y)
    }
}
else
{
    with (nm_skel)
    {
        state = -4
        if (ds_list_empty(lock_turn) && instance_exists(o_controller) && instance_exists(o_player))
        {
            if (scr_tile_distance(id, o_player) > 1)
                scr_enemy_priority_path_target(o_player.x, o_player.y)
        }
    }
}
