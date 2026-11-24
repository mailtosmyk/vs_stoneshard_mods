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

    // Drop Unaware / friendly / caravan targets.
    // Keep Hunt Command marks and anyone who already hit this summon.
    var _foe = target
    if (instance_exists(_foe) && object_is_ancestor(_foe.object_index, o_unit) && variable_instance_exists(_foe, "state"))
    {
        var _drop = false
        if (_foe.state == "idle")
            _drop = true
        if (variable_instance_exists(_foe, "faction_key") && _foe.faction_key == "Player")
            _drop = true
        if (variable_instance_exists(_foe, "sm_summon") && _foe.sm_summon)
            _drop = true
        if (variable_instance_exists(_foe, "nm_summon") && _foe.nm_summon)
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
    }
    else if (instance_exists(_foe) && (!object_is_ancestor(_foe.object_index, o_unit)))
    {
        target = -4
    }
}
