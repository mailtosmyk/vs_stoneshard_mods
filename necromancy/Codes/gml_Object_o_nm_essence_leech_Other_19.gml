with (o_player)
    scr_setside(o_floor_target)
var _cur = s_cursor_skill
if (pointed == "Target Object" || pointed == "Target Ally")
{
    _cur = s_cursor
    with (o_skill_tile_indicator)
        visible = false
    with (o_corpse)
    {
        if (collision_point(mouse_x, mouse_y, id, false, false) && visible)
        {
            _cur = s_cursor_skill
            other.target_group = o_corpse
        }
    }
    with (o_enemy)
    {
        if (collision_point(mouse_x, mouse_y, id, false, false) && visible)
        {
            var _servant = asset_get_index("o_b_nm_servant")
            if (_servant != -1 && scr_instance_exists_in_list(_servant, buffs))
            {
                _cur = s_cursor_skill
                other.target_group = o_enemy
            }
            else
            {
                other.target_group = o_corpse
                _cur = s_cursor
            }
        }
    }
}
with (o_floor_target)
    cursor = _cur
