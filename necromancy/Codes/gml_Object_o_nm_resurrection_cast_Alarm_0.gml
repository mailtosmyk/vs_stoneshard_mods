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
var _mastery = asset_get_index("o_nm_mastery_of_binding_ico")
var _hasMastery = (_mastery != -1 && instance_exists(_mastery) && _mastery.is_open)
if _hasMastery
    _pct = 0.6
else if is_crit
    _pct = 0.55
else
    _pct = 0.5 + (0.05 * (owner.Magic_Power / 100))
if (!_hasMastery)
    _pct = clamp(_pct, 0.5, 0.55)
var _mpPct = 0.5
var _cap = (owner.LVL + 3)
if _hasMastery
    _cap += 1
var _turns = 20.0

// Drop previous thrall timer (Destroy unbinds the linked thrall).
var _timer = asset_get_index("o_b_nm_thrall_timer")
if (_timer != -1 && instance_exists(owner))
{
    with (scr_instance_exists_in_list(_timer, owner.buffs))
        instance_destroy()
}

// Drop previous lasting thrall (tagged nm_resurrection) if timer miss.
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
        nm_summon_turns = _turns
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

if (instance_exists(_thrall) && _timer != -1 && instance_exists(owner))
{
    var _buff = scr_effect_create(_timer, _turns, owner, owner)
    if instance_exists(_buff)
    {
        with (_buff)
            nm_thrall = _thrall
    }
}

with (target)
    instance_destroy()

if instance_exists(owner)
{
    if is_player(owner)
    {
        var _san = 0.25
        var _sealed = asset_get_index("o_nm_sealed_mind_ico")
        if (_sealed != -1 && instance_exists(_sealed) && _sealed.is_open)
            _san *= 0.5
        scr_psy_change("Sanity", -_san)
        scr_allturn()
        var _arch = asset_get_index("o_nm_archtheurgy_ico")
        if (_arch != -1 && instance_exists(_arch))
            with (_arch) event_user(3)
        var _bloom = asset_get_index("o_nm_blooming_madness_ico")
        if (_bloom != -1 && instance_exists(_bloom) && instance_exists(_thrall))
        {
            with (_bloom)
            {
                nm_pending_thrall = _thrall
                event_user(3)
            }
        }
    }
}
instance_destroy()
