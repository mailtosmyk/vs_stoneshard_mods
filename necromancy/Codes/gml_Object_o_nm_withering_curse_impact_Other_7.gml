if scr_instance_exists_in_list(o_db_withering_curse, target.buffs)
{
    with (o_db_withering_curse)
    {
        if (target == other.target)
            duration = ceil((other.f_duration * (1 - (other.target.Fortitude / 100))))
    }
}
else
    scr_effect_create(o_db_withering_curse, f_duration, target, owner)
event_inherited()
if instance_exists(owner)
{
    if is_player(owner)
    {
        scr_psy_change("Sanity", -0.15)
        scr_allturn()
    }
}
