with (o_player)
    scr_setside(o_floor_target)
var _cur = s_cursor_skill
if (pointed == "Target Object" || pointed == "Target Ally")
{
    _cur = 1675
    with (o_skill_tile_indicator)
        visible = false
    with (o_enemy)
    {
        if (collision_point(mouse_x, mouse_y, id, false, false) && visible && is_player_enemy)
        {
            _cur = s_cursor_skill
            other.target_group = o_enemy
        }
    }
}
with (o_floor_target)
    cursor = _cur
