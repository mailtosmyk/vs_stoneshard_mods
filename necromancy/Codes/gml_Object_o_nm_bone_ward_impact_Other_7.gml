if (instance_exists(target) && instance_exists(owner))
{
    var _buff = asset_get_index("o_b_bone_ward")
    if (_buff != -1)
    {
        if scr_instance_exists_in_list(_buff, target.buffs)
        {
            with (_buff)
            {
                if (target == other.target)
                {
                    nm_force_shatter = 1
                    instance_destroy()
                }
            }
        }
        scr_effect_create(_buff, turns, target, owner)
    }
}
event_inherited()
if instance_exists(owner)
{
    if is_player(owner)
    {
        scr_psy_change("Sanity", -0.1)
        scr_allturn()
    }
}
