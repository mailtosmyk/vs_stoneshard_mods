// Ally AI patterned on Syndicate retinue (o_syndicate_bandit Other_10):
// fight with inherited enemy AI, otherwise follow the player via scr_turn.
// This event only runs when we remain on o_player.enemylist (scr_unitTurnEvents).

// Sticky Player-faction bind (vanilla AI can flip these).
// Keep owner = player so o_enemy Destroy credits summon kills for XP
// (vanilla path only auto-credits o_summoned_entity; we patch nm_summon + owner).
if (instance_exists(o_player))
    owner = o_player.id
faction_key = "Player"
subfaction_key = "Player"
faction = "Player"
// Must match vanilla table_ai / subfaction_key names (scr_nearest_in_array + faction_map).
combat_array = ["Brigand", "Undead", "Vampire", "Wolf", "Bear", "Gulon", "Dog", "Wild Dog", "Young Troll", "Ancient Troll", "Crawler", "Bat Flock", "Snake", "Harpy", "Hive", "Buzzer Hive", "Forest Buzzer", "carnivore", "omnivore", "Moose", "Saiga", "Deer", "Fox", "Boar", "Bison", "GrandMagistrate", "Mercenary", "RottenWillow", "Neutral"]
threat_array = []
flee_array = []
is_player_enemy = false
is_neutral = true
is_hostile = false
havePenaltyAttack = false
is_follower = true
ai_is_on = true
can_drop_loot = false

// Re-register on the turn queue if something stripped us out.
if (instance_exists(o_player))
{
    if (ds_list_find_index(o_player.enemylist, id) < 0)
        ds_list_add(o_player.enemylist, id)
}

var _has_enemy = false
if (instance_exists(target))
{
    if (object_is_ancestor(target.object_index, o_enemy))
    {
        var _skip = false
        if (variable_instance_exists(target, "nm_summon") && target.nm_summon)
            _skip = true
        if (variable_instance_exists(target, "faction_key") && target.faction_key == "Player")
            _skip = true
        if (!_skip && (!variable_instance_exists(target, "HP") || target.HP > 0))
        {
            if (scr_tile_distance(id, target) <= 12.0)
                _has_enemy = true
            if (variable_instance_exists(target, "target") && target.target == id)
                _has_enemy = true
        }
    }
}

if (state == "npc combat" && (!_has_enemy))
{
    scr_stateIndicatorDestroy(id)
    target = -4
    state = "idle"
    alarmPercent = 0.0
    threat_time = 0.0
    last_attacker = -4
    is_damage_taken = false
}

var _enemy = -4
var _handled_follow = false
if (instance_exists(o_player) && (!_has_enemy))
{
    _enemy = scr_nearest_in_array(combat_array, true)
    if (instance_exists(_enemy))
    {
        nm_ally_candidate_bad = false
        with (_enemy)
        {
            if (variable_instance_exists(id, "faction_key") && faction_key == "Player")
                other.nm_ally_candidate_bad = true
            if (variable_instance_exists(id, "nm_summon") && nm_summon)
                other.nm_ally_candidate_bad = true
            if (id == other.id)
                other.nm_ally_candidate_bad = true
        }
        if (nm_ally_candidate_bad)
            _enemy = -4
        nm_ally_candidate_bad = false
    }
}

if (instance_exists(_enemy))
{
    target = _enemy
    state = "npc combat"
    _has_enemy = true
}

if (_has_enemy)
{
    // Inherited enemy Other_10 handles pathing, melee, and skill use.
    event_inherited()
}
else if (instance_exists(o_player))
{
    _handled_follow = true
    scr_stateIndicatorDestroy(id)
    is_player_enemy = false
    is_neutral = true
    is_hostile = false
    havePenaltyAttack = false
    is_follower = true
    move_target = o_player.id
    owner = o_player.id
    target = -4
    substate = "following"
    state = "move_to_exit"
    alarmPercent = 0.0
    threat_time = 0.0
    last_attacker = -4
    is_damage_taken = false
    scr_enemy_pf_update()

    if (HP > 0 && scr_tile_distance(id, o_player.id) > 1.0 && (!is_moving) && point_distance(x, y, xx, yy) <= 1.0)
    {
        var _follow_tile = scr_tile_around_target(o_player.id, 1)
        if (is_array(_follow_tile) && array_length(_follow_tile) > 1.0)
            scr_turn(o_player.x + _follow_tile[0], o_player.y + _follow_tile[1], true)
        else
            scr_turn(o_player.x, o_player.y, true)
    }
}

if (instance_exists(o_player) && (!_handled_follow))
{
    is_player_enemy = false
    is_neutral = true
    is_hostile = false
    havePenaltyAttack = false
    if (target == o_player.id)
    {
        scr_stateIndicatorDestroy(id)
        target = -4
    }
}

if (HP > 0 && state == "flee")
{
    scr_stateIndicatorDestroy(id)
    state = "idle"
    target = -4
    alarmPercent = 0.0
    threat_time = 0.0
    last_attacker = -4
    is_damage_taken = false
}
