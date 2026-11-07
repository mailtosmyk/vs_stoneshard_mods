if instance_exists(owner)
{
    if is_player(owner)
    {
        var _turns = irandom_range(8, 10)
        var _buff = asset_get_index("o_b_nm_desecration")
        if (_buff != -1)
        {
            with (_buff)
            {
                if (target == other.owner)
                {
                    destroy_anim = false
                    actionsLogDrop = false
                    instance_destroy()
                }
            }
            scr_effect_create(_buff, _turns, owner, owner)
        }
        scr_psy_change("Sanity", -0.15)
        scr_allturn()
    }
}
