if ((!is_execute) && instance_exists(owner))
{
    scr_skill_summon_get_tile()
    xx = point_x
    yy = point_y
    is_execute = true

    var _stage = 0
    var _charge = asset_get_index("o_b_nm_essence_charge")
    if (_charge != -1)
    {
        with (scr_instance_exists_in_list(_charge, owner.buffs))
        {
            _stage = stage
            instance_destroy()
        }
    }

    var _unit = o_ghost_squire
    switch _stage
    {
        case 0:
            _unit = choose(o_ghost_squire, o_ghost_monk)
            break
        case 1:
            _unit = choose(o_ghost_squire, o_ghost_monk)
            break
        case 2:
            _unit = choose(o_ghost_cleric, o_ghost_sergeant)
            break
        default:
            _unit = choose(o_ghost_seer, o_ghost_knight)
            break
    }

    // Drop previous wraith summon marked nm_wraith.
    with (o_enemy)
    {
        if (variable_instance_exists(id, "nm_wraith") && nm_wraith)
        {
            var _sv = asset_get_index("o_b_nm_servant")
            if (_sv != -1)
            {
                with (scr_instance_exists_in_list(_sv, buffs))
                    instance_destroy()
            }
            instance_destroy()
        }
    }

    var _cap = (owner.LVL + 3)
    var _mastery = asset_get_index("o_nm_mastery_of_binding_ico")
    if (_mastery != -1 && instance_exists(_mastery) && _mastery.is_open)
        _cap += 1

    scr_guiAnimation_ext((xx + 13), (yy + 13), s_wraithsummon_hit, 1, 1, 0, 16777215, 0)
    scr_audio_play_at(snd_skill_wraith_summoning_hit)

    var _wraith = -4
    with (scr_enemy_create((xx + 13), (yy + 13), _unit, false))
    {
        if (LVL > _cap)
        {
            instance_destroy()
        }
        else
        {
            nm_wraith = true
            nm_summon = true
            creating_unit = _unit
            owner = other.owner
            can_drop_loot = false
            loot_script = -4
            gain_xp *= 0.1
            ArmorDurability = 0
            scr_faction_map_remove(id)
            faction_key = "Player"
            subfaction_key = "Player"
            faction = "Player"
            scr_faction_map_add(id)
            combat_array = ["Brigand", "Undead", "Vampire", "Wolf", "Bear", "Gulon", "Dog", "Wild Dog", "Young Troll", "Ancient Troll", "Crawler", "Bat Flock", "Snake", "Harpy", "Hive", "Buzzer Hive", "Forest Buzzer", "carnivore", "omnivore", "Moose", "Saiga", "Deer", "Fox", "Boar", "Bison", "GrandMagistrate", "Mercenary", "RottenWillow", "Neutral"]
            threat_array = []
            flee_array = []
            is_player_enemy = false
            is_neutral = true
            is_hostile = false
            havePenaltyAttack = false
            is_follower = true
            ai_is_on = true
            target = -4
            last_attacker = -4
            is_damage_taken = false
            state = "idle"
            scr_delete_from_enemy_list("Player")
            if instance_exists(owner)
                callerId = owner.callerId
            _wraith = id
        }
    }

    var _servant = asset_get_index("o_b_nm_servant")
    if (instance_exists(_wraith) && _servant != -1)
        scr_effect_create(_servant, -1, _wraith, owner)

    if instance_exists(o_player)
    {
        if (scr_atr("Sanity") > 60)
        {
            var _san = 0.3
            var _sealed = asset_get_index("o_nm_sealed_mind_ico")
            if (_sealed != -1 && instance_exists(_sealed) && _sealed.is_open)
                _san *= 0.5
            scr_psy_change("Sanity", -_san)
        }
        scr_allturn()
    }
}
