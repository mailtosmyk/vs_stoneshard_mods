if ((!is_execute) && instance_exists(owner))
{
    scr_skill_summon_get_tile()
    xx = point_x
    yy = point_y
    is_execute = true

    if (scr_sm_count_summons() >= scr_sm_max_summons())
    {
        // At cap: safe-dismiss oldest summon of any species, then continue.
        var _dropped = false
        with (o_enemy)
        {
            if ((!_dropped) && variable_instance_exists(id, "sm_summon") && sm_summon)
            {
                sm_safe_dismiss = true
                instance_destroy()
                _dropped = true
            }
        }
    }

    var _obj = sm_animal
    if (_obj == -1 || (!object_exists(_obj)))
        _obj = o_wolf
    var _unit = -4
    if object_exists(_obj)
    {
        with (scr_enemy_create((xx + 13), (yy + 13), _obj, false))
        {
            creating_unit = _obj
            _unit = id
        }
    }
    if instance_exists(_unit)
        scr_sm_bind_summon(_unit, owner, sm_species)

    if instance_exists(o_player)
        scr_allturn()
}
