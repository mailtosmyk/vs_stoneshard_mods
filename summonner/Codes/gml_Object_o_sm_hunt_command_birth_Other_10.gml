if ((!is_execute) && instance_exists(owner) && instance_exists(point))
{
    is_execute = true
    var _mark = asset_get_index("o_b_sm_hunt_mark")
    if (_mark != -1)
    {
        // One mark per caster: drop previous Hunt marks from this owner.
        with (_mark)
        {
            if (instance_exists(owner) && (owner == other.owner))
                instance_destroy()
        }
        scr_effect_create(_mark, 6, point, owner)
    }
    with (owner)
    {
        sm_hunt_target = other.point
        sm_hunt_turns = 6
    }
    if is_player(owner)
        scr_allturn()
}
