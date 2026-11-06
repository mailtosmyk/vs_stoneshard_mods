if (instance_exists(target) && instance_exists(owner))
{
    // Refresh: shatter previous ward first.
    if scr_instance_exists_in_list(o_b_bone_ward, target.buffs)
    {
        with (o_b_bone_ward)
        {
            if (target == other.target)
            {
                nm_force_shatter = 1
                instance_destroy()
            }
        }
    }
    scr_effect_create(o_b_bone_ward, turns, target, owner)
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
