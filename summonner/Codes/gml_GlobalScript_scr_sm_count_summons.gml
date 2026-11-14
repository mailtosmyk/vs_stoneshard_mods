function scr_sm_count_summons()
{
    var _n = 0
    with (o_enemy)
    {
        if (variable_instance_exists(id, "sm_summon") && sm_summon)
            _n += 1
    }
    return _n
}
