event_inherited()
if (!instance_exists(target))
    exit
if (variable_instance_exists(target, "sm_safe_dismiss") && target.sm_safe_dismiss)
    exit
if (!instance_exists(owner))
    exit
if (!is_player(owner))
    exit
var _loss = 8
if (variable_instance_exists(target, "sm_species"))
{
    if (target.sm_species == "wolf")
        _loss = 6
    else if (target.sm_species == "crawler")
        _loss = 8
    else if (target.sm_species == "bear" || target.sm_species == "bison")
        _loss = 10
    else if (target.sm_species == "gulon")
        _loss = 12
}
var _kindred = asset_get_index("o_sm_kindred_spirit_ico")
if (_kindred != -1 && instance_exists(_kindred) && _kindred.is_open)
    _loss = floor(_loss * 0.5)
var _buff = asset_get_index("o_b_sm_summon_death")
if (_buff == -1)
    exit
var _existing = -4
with (owner)
    _existing = scr_instance_exists_in_list(_buff, buffs)
if (_existing != -4)
{
    with (_existing)
    {
        duration = 600
        if (_loss > stage)
            stage = _loss
        event_perform(ev_alarm, 2)
    }
}
else
    scr_effect_create(_buff, 600, owner, owner, _loss)
