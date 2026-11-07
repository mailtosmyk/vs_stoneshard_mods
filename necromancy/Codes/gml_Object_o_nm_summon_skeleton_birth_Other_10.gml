if ((!is_execute) && instance_exists(owner))
{
    scr_skill_summon_get_tile()
    xx = point_x
    yy = point_y
    is_execute = true

    // Parent o_skeleton has no scr_param / bSTR — spawn a concrete skeleton unit.
    var _skelObj = asset_get_index("o_skeleton_militiaman")
    if (_skelObj == -1)
        _skelObj = asset_get_index("o_skeleton_footman")

    // Drop previous summon timer (Destroy unbinds the linked skeleton).
    var _timer = asset_get_index("o_b_nm_skeleton_timer")
    if (_timer != -1 && instance_exists(owner))
    {
        with (scr_instance_exists_in_list(_timer, owner.buffs))
            instance_destroy()
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
            is_neutral = true
            isSummoning = true
            HP = max(1, floor((max_hp * _hpPct) * 0.01))
            MP = max(0, floor((max_mp * _mpPct) * 0.01))
            ArmorDurability = 0
            if instance_exists(owner)
                callerId = owner.callerId
            _skel = id
        }
    }

    if (instance_exists(_skel) && instance_exists(o_player))
    {
        with (o_player)
        {
            var _pos = ds_list_find_index(enemylist, _skel)
            if (_pos >= 0)
                ds_list_delete(enemylist, _pos)
        }
    }

    if (instance_exists(_skel) && _timer != -1 && instance_exists(owner))
    {
        var _buff = scr_effect_create(_timer, _turns, owner, owner)
        if instance_exists(_buff)
        {
            with (_buff)
                nm_skel = _skel
        }
    }

    if instance_exists(o_player)
    {
        if (scr_atr("Sanity") > 70)
            scr_psy_change("Sanity", -0.25)
        scr_allturn()
    }
}
