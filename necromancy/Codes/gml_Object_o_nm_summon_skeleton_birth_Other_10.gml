if (!is_execute)
{
    if instance_exists(owner)
    {
        scr_skill_summon_get_tile()
        xx = point_x
        yy = point_y
        is_execute = 1

        with (o_skeleton)
        {
            if (variable_instance_exists(id, "nm_summon") && nm_summon)
                instance_destroy()
        }

        var _wil = owner.WIL
        var _turns = (5 + floor(_wil / 5))
        var _hpPct = clamp((33 + floor(_wil / 2)), 20, 90)
        var _mpPct = clamp((25 + floor(_wil / 3)), 15, 80)

        var _skel = -4
        with (scr_enemy_create((xx + 13), (yy + 13), o_skeleton, 0))
        {
            nm_summon = 1
            nm_summon_turns = _turns
            creating_unit = o_skeleton
            owner = other.owner
            can_drop_loot = 0
            gain_xp *= 0.1
            faction_key = "Servant"
            subfaction_key = "Servant"
            HP = max(1, floor((max_hp * _hpPct) * 0.01))
            MP = max(0, floor((max_mp * _mpPct) * 0.01))
            ArmorDurability = 0
            _skel = id
        }
        if instance_exists(_skel)
            scr_effect_create(o_b_nm_skeleton_timer, _turns, _skel, owner)

        if is_player(owner)
        {
            if (owner.Sanity > 70)
                scr_psy_change("Sanity", -0.25)
            scr_allturn()
        }
    }
}
