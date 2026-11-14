if ((!is_execute) && instance_exists(owner))
{
    is_execute = true
    with (o_enemy)
    {
        if (variable_instance_exists(id, "sm_summon") && sm_summon && HP > 0)
            scr_restore_hp(id, 12.0, other.name)
    }
    if is_player(owner)
        scr_allturn()
}
