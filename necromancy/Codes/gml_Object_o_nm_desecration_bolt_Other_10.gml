if ((!instance_exists(owner)) || (!instance_exists(target)))
    return;
event_inherited()
if (deal_damage > 0)
{
    var _name = scr_id_get_name(id)
    var _burn = math_round(deal_damage * 0.33)
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
            {
                duration = max(duration, 10)
            }
        }
    }
}
event_user(1)
