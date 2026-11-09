event_inherited()
with (target)
{
    if (instance_exists(o_player))
        owner = o_player.id
    else
        owner = other.owner
    nm_summon = true
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

    // Must match vanilla table_ai / subfaction_key names (scr_nearest_in_array + faction_map).
    combat_array = ["Brigand", "Undead", "Vampire", "Wolf", "Bear", "Gulon", "Dog", "Wild Dog", "Young Troll", "Ancient Troll", "Crawler", "Bat Flock", "Snake", "Harpy", "Hive", "Buzzer Hive", "Forest Buzzer", "carnivore", "omnivore", "Moose", "Saiga", "Deer", "Fox", "Boar", "Bison", "GrandMagistrate", "Mercenary", "RottenWillow", "Neutral"]
    threat_array = []
    flee_array = []

    if (instance_exists(o_player) && ds_list_find_index(o_player.enemylist, id) < 0)
        ds_list_add(o_player.enemylist, id)
}
