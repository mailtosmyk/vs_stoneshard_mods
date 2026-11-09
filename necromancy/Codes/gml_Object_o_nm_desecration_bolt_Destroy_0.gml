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
        var _san = 0.15
        var _sealed = asset_get_index("o_nm_sealed_mind_ico")
        if (_sealed != -1 && instance_exists(_sealed) && _sealed.is_open)
            _san *= 0.5
        scr_psy_change("Sanity", -_san)
        scr_allturn()
    }
}
