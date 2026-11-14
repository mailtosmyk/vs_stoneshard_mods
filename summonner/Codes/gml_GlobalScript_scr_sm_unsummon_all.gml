function scr_sm_unsummon_all()
{
    with (o_enemy)
    {
        if (variable_instance_exists(id, "sm_summon") && sm_summon)
        {
            sm_safe_dismiss = true
            instance_destroy()
        }
    }
}
