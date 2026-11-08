if (!(instance_exists(owner) && instance_exists(target)))
{
    instance_destroy()
    return;
}
if (target.object_index != o_corpse)
{
    instance_destroy()
    return;
}
unit_object = target.enemy_object_index
if ((unit_object == o_Hive) || (unit_object == o_hornethive) || (unit_object == o_crawler_hive) || (unit_object == o_harpynest))
{
    instance_destroy()
    return;
}
var _pct = restore_percent
if is_crit
    _pct = 0.55
else
    _pct = 0.5 + (0.05 * (owner.Magic_Power / 100))
_pct = clamp(_pct, 0.5, 0.55)
var _mpPct = 0.5
var _cap = (owner.LVL + 3)

// Drop previous lasting thrall (tagged nm_resurrection).
with (o_enemy)
{
    if (variable_instance_exists(id, "nm_resurrection") && nm_resurrection)
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

var _thrall = -4
with (scr_enemy_create(x, y, unit_object, false))
{
    if (LVL > _cap)
    {
        instance_destroy()
    }
    else
    {
        nm_resurrection = true
        nm_summon = true
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
        combat_array = ["Brigand", "Undead", "Vampire", "Rockeaters", "Crawlers", "Crawler", "Dogs", "Wolves", "Bears", "Gulons", "Trolls", "Bat Flock", "Snake", "Spiderling", "Harpy", "carnivore", "omnivore", "herbivore", "Hive"]
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
        HP = max(1, floor(max_hp * _pct))
        MP = max(0, floor(max_mp * _mpPct))
        if instance_exists(owner)
            callerId = owner.callerId
        _thrall = id
    }
}

var _servant = asset_get_index("o_b_nm_servant")
if (instance_exists(_thrall) && _servant != -1)
    scr_effect_create(_servant, -1, _thrall, owner)

with (target)
    instance_destroy()

if instance_exists(owner)
{
    if is_player(owner)
    {
        scr_psy_change("Sanity", -0.25)
        scr_allturn()
    }
}
instance_destroy()
