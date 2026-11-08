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

    combat_array = ["Brigand", "Undead", "Vampire", "Rockeaters", "Crawlers", "Crawler", "Dogs", "Wolves", "Bears", "Gulons", "Trolls", "Bat Flock", "Snake", "Spiderling", "Harpy", "carnivore", "omnivore", "herbivore", "Hive"]
    threat_array = []
    flee_array = []

    if (instance_exists(o_player) && ds_list_find_index(o_player.enemylist, id) < 0)
        ds_list_add(o_player.enemylist, id)
}
