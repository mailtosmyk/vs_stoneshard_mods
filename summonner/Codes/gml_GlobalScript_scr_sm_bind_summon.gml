function scr_sm_bind_summon(argument0, argument1, argument2)
{
    // argument0 unit, argument1 owner, argument2 species
    if (!instance_exists(argument0))
        return;
    with (argument0)
    {
        sm_summon = true
        sm_species = argument2
        owner = argument1
        can_drop_loot = false
        loot_script = -4
        gain_xp *= 0.1
        scr_faction_map_remove(id)
        faction_key = "Player"
        subfaction_key = "Player"
        faction = "Player"
        scr_faction_map_add(id)
        combat_array = ["Brigand", "Undead", "Vampire", "Wolf", "Bear", "Gulon", "Dog", "Wild Dog", "Young Troll", "Ancient Troll", "Crawler", "Bat Flock", "Snake", "Harpy", "Hive", "Forest Buzzer", "carnivore", "omnivore", "Moose", "Saiga", "Deer", "Fox", "Boar", "Bison", "GrandMagistrate", "Mercenary", "RottenWillow", "Neutral"]
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
        state = "idle"
        scr_delete_from_enemy_list("Player")
        if instance_exists(owner)
            callerId = owner.callerId
    }
    scr_sm_apply_level_stats(argument0, argument2)
    var _servant = asset_get_index("o_b_sm_servant")
    if (_servant != -1)
        scr_effect_create(_servant, -1, argument0, argument1)
}
