if ((!is_execute) && instance_exists(owner))
{
    scr_skill_summon_get_tile()
    xx = point_x
    yy = point_y
    is_execute = true

    // o_skeleton is an abstract parent (no sprite / no scr_param → no bSTR).
    // Spawn a concrete skeleton whose Create calls scr_param(...).
    var _skelObj = o_skeleton_militiaman
    if (!object_exists(_skelObj))
        _skelObj = o_skeleton_footman
    if (!object_exists(_skelObj))
        _skelObj = o_skeleton_spearman

    // Drop previous summon timer (Destroy unbinds the linked skeleton).
    var _timer = asset_get_index("o_b_nm_skeleton_timer")
    if (_timer != -1)
    {
        with (scr_instance_exists_in_list(_timer, owner.buffs))
            instance_destroy()
    }

    var _wil = scr_atr("WIL")
    // Use .0 literals so ReplaceGML does not resolve bare ints to asset ids.
    var _turns = 5.0 + floor(_wil / 5.0)
    var _hpPct = clamp(33.0 + floor(_wil / 2.0), 20.0, 90.0)
    var _mpPct = clamp(25.0 + floor(_wil / 3.0), 15.0, 80.0)

    var _skel = -4
    if (object_exists(_skelObj))
    {
        with (scr_enemy_create((xx + 13), (yy + 13), _skelObj, false))
        {
            nm_summon = true
            nm_summon_turns = _turns
            creating_unit = _skelObj
            owner = other.owner
            can_drop_loot = false
            gain_xp *= 0.1
            ArmorDurability = 0

            // Vanilla ally bind (see o_summoned_entity / o_b_summoned_entity).
            is_neutral = true
            is_player_enemy = false
            target = -4
            scr_delete_from_enemy_list("Player")

            // Register under Player so hostiles that hunt the player also engage the pet.
            scr_faction_map_remove(id)
            subfaction_key = "Player"
            scr_faction_map_add(id)

            HP = max(1, floor((max_hp * _hpPct) * 0.01))
            MP = max(0, floor((max_mp * _mpPct) * 0.01))
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

    // Servant AI buff lives on the skeleton (ticks on its turns).
    var _servant = asset_get_index("o_b_nm_servant")
    if (instance_exists(_skel) && _servant != -1)
        scr_effect_create(_servant, -1, _skel, owner)

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
