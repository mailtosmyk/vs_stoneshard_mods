event_inherited()
if ((!instance_exists(target)) || (!instance_exists(o_player)))
    exit
// Sticky Player-faction bind + combat list (vanilla animal AI can flip these).
with (target)
{
    if (instance_exists(o_player))
        owner = o_player.id
    else
        owner = other.owner
    sm_summon = true
    scr_faction_map_remove(id)
    faction_key = "Player"
    subfaction_key = "Player"
    faction = "Player"
    scr_faction_map_add(id)
    is_neutral = true
    is_player_enemy = false
    is_hostile = false
    havePenaltyAttack = false
    is_follower = true
    ai_is_on = true
    can_drop_loot = false
    loot_script = -4
    scr_delete_from_enemy_list("Player")
    combat_array = ["Brigand", "Undead", "Vampire", "Wolf", "Bear", "Gulon", "Dog", "Wild Dog", "Young Troll", "Ancient Troll", "Crawler", "Bat Flock", "Snake", "Harpy", "Hive", "Buzzer Hive", "Forest Buzzer", "carnivore", "omnivore", "Moose", "Saiga", "Deer", "Fox", "Boar", "Bison"]
    threat_array = []
    flee_array = []

    if (instance_exists(o_player) && (ds_list_find_index(o_player.enemylist, id) < 0))
        ds_list_add(o_player.enemylist, id)

    // Resolve whether we still have a valid hostile to fight.
    var _foe = target
    var _has_enemy = false
    if (instance_exists(_foe) && object_is_ancestor(_foe.object_index, o_unit) && variable_instance_exists(_foe, "state"))
    {
        // Never keep the player / owner as a combat target — that is follow, not a fight.
        if (is_player(_foe) || (_foe == owner) || (instance_exists(o_player) && (_foe == o_player.id)))
        {
            target = -4
            _foe = -4
        }
        else
        {
            var _drop = false
            if (_foe.state == "idle")
                _drop = true
            if (variable_instance_exists(_foe, "sm_summon") && _foe.sm_summon)
                _drop = true
            if (variable_instance_exists(_foe, "nm_summon") && _foe.nm_summon)
                _drop = true
            if (variable_instance_exists(_foe, "faction_key") && _foe.faction_key == "Player")
                _drop = true
            if (variable_instance_exists(_foe, "is_follower") && _foe.is_follower && (!(variable_instance_exists(_foe, "sm_summon") && _foe.sm_summon)))
                _drop = true
            if (variable_instance_exists(_foe, "is_player_enemy") && (!_foe.is_player_enemy) && variable_instance_exists(_foe, "is_neutral") && _foe.is_neutral)
                _drop = true

            var _keep = false
            if (last_attacker == _foe)
                _keep = true
            if (instance_exists(o_player) && variable_instance_exists(o_player, "sm_hunt_target") && o_player.sm_hunt_target == _foe)
                _keep = true
            var _huntMark = asset_get_index("o_b_sm_hunt_mark")
            if ((_huntMark != -1) && variable_instance_exists(_foe, "buffs") && scr_instance_exists_in_list(_huntMark, _foe.buffs))
                _keep = true

            if (_drop && (!_keep))
            {
                target = -4
                if (variable_instance_exists(id, "state") && (state == "attack" || state == "npc combat"))
                {
                    scr_stateIndicatorDestroy(id)
                    state = "idle"
                    alarmPercent = 0.0
                    threat_time = 0.0
                }
            }
            else
                _has_enemy = true
        }
    }
    else if (instance_exists(_foe) && (!object_is_ancestor(_foe.object_index, o_unit)))
    {
        target = -4
    }

    // Hunt Focus: +12% Weapon Damage / +10% Accuracy while fighting a Hunt-marked foe.
    var _focus = asset_get_index("o_b_sm_hunt_focus")
    var _mark = asset_get_index("o_b_sm_hunt_mark")
    var _wantFocus = false
    if (_has_enemy && instance_exists(target) && (_mark != -1) && variable_instance_exists(target, "buffs"))
        _wantFocus = scr_instance_exists_in_list(_mark, target.buffs)
    if ((_focus != -1) && _wantFocus)
    {
        if (scr_instance_exists_in_list(_focus, buffs))
        {
            with (_focus)
            {
                if (target == other.id)
                {
                    duration = 2
                    max_duration = 2
                }
            }
        }
        else
            scr_effect_create(_focus, 2, id, id)
    }

    // No valid hostile: follow the player (same pattern as Necromancy ally AI).
    if ((!_has_enemy) && instance_exists(o_player))
    {
        scr_stateIndicatorDestroy(id)
        move_target = o_player.id
        owner = o_player.id
        target = -4
        substate = "following"
        state = "move_to_exit"
        alarmPercent = 0.0
        threat_time = 0.0
        is_damage_taken = false
        scr_enemy_pf_update()

        if ((HP > 0) && (scr_tile_distance(id, o_player.id) > 1.0) && (!is_moving) && (point_distance(x, y, xx, yy) <= 1.0))
        {
            var _follow_tile = scr_tile_around_target(o_player.id, 1)
            if (is_array(_follow_tile) && (array_length(_follow_tile) > 1.0))
                scr_turn(o_player.x + _follow_tile[0], o_player.y + _follow_tile[1], true)
            else
                scr_turn(o_player.x, o_player.y, true)
        }
    }
}
