event_inherited()
if ((!instance_exists(target)) || (!instance_exists(o_player)))
    return;

// Sticky ally bind only — movement/combat AI is on o_nm_skeleton_ally Other_10
// (Syndicate retinue pattern: unit Other_10 + event_inherited / scr_turn).
with (target)
{
    // Prefer live player id so kill XP attribution stays valid even if buff.owner drifts.
    if (instance_exists(o_player))
        owner = o_player.id
    else
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
    nm_summon = true

    if (target == o_player.id)
        target = -4

    // Keep the pet on the player turn queue (required for Other_10 AI ticks).
    if (instance_exists(o_player) && ds_list_find_index(o_player.enemylist, id) < 0)
        ds_list_add(o_player.enemylist, id)
}
