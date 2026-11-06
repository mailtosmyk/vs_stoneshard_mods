if instance_exists(owner)
{
    if is_player(owner)
    {
        var _turns = irandom_range(8, 10)
        var _bonus = math_round(((7 * (owner.Magic_Power * 0.01)) / 4))
        scr_temp_effect_update(object_index, owner, "Unholy_Damage", _bonus, _turns, 1)
        scr_psy_change("Sanity", -0.15)
        scr_allturn()
    }
}
