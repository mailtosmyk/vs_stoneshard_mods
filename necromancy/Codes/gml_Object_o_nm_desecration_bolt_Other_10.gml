if ((!instance_exists(owner)) || (!instance_exists(target)))
    return;
Unholy_Damage = 7
event_inherited()
if (damage_done > 0)
{
    var _name = scr_actionsLogGetName(id)
    var _burn = math_round(damage_done * 0.33)
    if instance_exists(target)
    {
        if object_is_ancestor(target.object_index, o_unit)
        {
            with (target)
                scr_restore_mp(id, (-_burn), _name)
            with (owner)
                scr_restore_mp(id, _burn, _name)
        }
        with (o_db_withering_curse)
        {
            if (target == other.target)
                duration = max(duration, 10)
        }
    }
}
event_user(1)
