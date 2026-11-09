if ((!is_execute) && instance_exists(owner))
{
    scr_skill_summon_get_tile()
    xx = point_x
    yy = point_y
    is_execute = true

    // Custom ally object (Syndicate-style Other_10). Falls back to a concrete skeleton
    // if the ally object failed to register.
    var _skelObj = asset_get_index("o_nm_skeleton_ally")
    if (_skelObj == -1 || (!object_exists(_skelObj)))
        _skelObj = o_skeleton_militiaman
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
    var _turns = 2.0 * (10.0 + floor(_wil / 5.0))
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
            loot_script = -4
            gain_xp *= 0.1
            ArmorDurability = 0

            // Syndicate-style Player retinue bind.
            scr_faction_map_remove(id)
            faction_key = "Player"
            subfaction_key = "Player"
            faction = "Player"
            scr_faction_map_add(id)
            // Must match vanilla table_ai / subfaction_key names (scr_nearest_in_array + faction_map).
            combat_array = ["Brigand", "Undead", "Vampire", "Wolf", "Bear", "Gulon", "Dog", "Wild Dog", "Young Troll", "Ancient Troll", "Crawler", "Bat Flock", "Snake", "Harpy", "Hive", "Buzzer Hive", "Forest Buzzer", "carnivore", "omnivore", "Moose", "Saiga", "Deer", "Fox", "Boar", "Bison", "GrandMagistrate", "Mercenary", "RottenWillow", "Neutral"]
            threat_array = []
            flee_array = []
            is_player_enemy = false
            is_neutral = true
            is_hostile = false
            havePenaltyAttack = false
            is_follower = true
            // Must stay true: o_player Alarm_4 only runs event_user(0)/Other_10 for
            // instances on enemylist (see scr_enemy_create + scr_unitTurnEvents).
            ai_is_on = true
            target = -4
            last_attacker = -4
            is_damage_taken = false
            state = "idle"
            alarmPercent = 0
            threat_time = 0
            scr_delete_from_enemy_list("Player")

            HP = max(1, floor((max_hp * _hpPct) * 0.01))
            MP = max(0, floor((max_mp * _mpPct) * 0.01))
            if instance_exists(owner)
                callerId = owner.callerId
            _skel = id
        }
    }

    // Do NOT remove from o_player.enemylist. scr_enemy_create adds the unit there, and
    // that list is the only turn queue for enemy AI. Syndicate retinue stays on it too;
    // is_player_enemy/is_neutral keep the pet from counting as a hostile.

    // Marker buff on the skeleton (sticky ally flags; AI lives on o_nm_skeleton_ally).
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
