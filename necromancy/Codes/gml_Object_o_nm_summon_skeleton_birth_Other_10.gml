if ((!is_execute) && instance_exists(owner))
{
    scr_skill_summon_get_tile()
    xx = point_x
    yy = point_y
    is_execute = true

    var _skelObj = asset_get_index("o_skeleton")
    if (_skelObj != -1)
    {
        with (_skelObj)
        {
            if (variable_instance_exists(id, "nm_summon") && nm_summon)
                instance_destroy()
        }
    }

    var _wil = scr_atr("WIL")
    var _turns = 5 + floor(_wil / 5)
    var _hpPct = clamp(33 + floor(_wil / 2), 20, 90)
    var _mpPct = clamp(25 + floor(_wil / 3), 15, 80)

    var _skel = -4
    if (_skelObj != -1)
    {
        with (scr_enemy_create((xx + 13), (yy + 13), _skelObj, false))
        {
            nm_summon = true
            nm_summon_turns = _turns
            creating_unit = _skelObj
            owner = other.owner
            can_drop_loot = false
            gain_xp *= 0.1
            faction_key = "Servant"
            subfaction_key = "Servant"
            isSummoning = true
            HP = max(1, floor((max_hp * _hpPct) * 0.01))
            MP = max(0, floor((max_mp * _mpPct) * 0.01))
            ArmorDurability = 0
            _skel = id
        }
    }

    var _timer = asset_get_index("o_b_nm_skeleton_timer")
    if (instance_exists(_skel) && _timer != -1)
        scr_effect_create(_timer, _turns, _skel, owner)

    if instance_exists(o_player)
    {
        if (scr_atr("Sanity") > 70)
            scr_psy_change("Sanity", -0.25)
        scr_allturn()
    }
}
