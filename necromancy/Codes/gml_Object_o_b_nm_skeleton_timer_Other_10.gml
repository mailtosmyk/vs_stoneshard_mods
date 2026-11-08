event_inherited()
duration = min(duration, max_duration)

// Nested checks: GML may evaluate both sides of || / &&.
if (!variable_instance_exists(id, "nm_skel"))
    return;
if (nm_skel == -4)
    return;
if (!instance_exists(nm_skel))
{
    instance_destroy()
    return;
}
if (!instance_exists(o_player))
    return;

// Keep ally bind sticky. Combat/follow AI lives on o_nm_skeleton_ally.
with (nm_skel)
{
    owner = other.owner
    faction_key = "Player"
    subfaction_key = "Player"
    faction = "Player"
    is_neutral = true
    is_player_enemy = false
    is_hostile = false
    havePenaltyAttack = false
    is_follower = true
    ai_is_on = true
    can_drop_loot = false

    if (target == o_player.id)
        target = -4

    if (instance_exists(o_player) && ds_list_find_index(o_player.enemylist, id) < 0)
        ds_list_add(o_player.enemylist, id)
}
