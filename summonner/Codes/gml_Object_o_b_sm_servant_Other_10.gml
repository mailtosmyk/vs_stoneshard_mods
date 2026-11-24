event_inherited()
if ((!instance_exists(target)) || (!instance_exists(o_player)))
    exit
// Keep owner = player so o_enemy Destroy credits summon kills for player XP.
with (target)
{
    if (instance_exists(o_player))
        owner = o_player.id
    else
        owner = other.owner
    if (instance_exists(o_player) && (ds_list_find_index(o_player.enemylist, id) < 0))
        ds_list_add(o_player.enemylist, id)

    // Drop Unaware (idle) targets — do not break stealth / start fights.
    // Keep Hunt Command marks and anyone who already hit this summon.
    if (instance_exists(target) && target.state == "idle")
    {
        var _keep = false
        if (last_attacker == target)
            _keep = true
        if (instance_exists(o_player) && variable_instance_exists(o_player, "sm_hunt_target") && o_player.sm_hunt_target == target)
            _keep = true
        if (!_keep)
        {
            target = -4
            if (state == "attack" || state == "npc combat")
            {
                scr_stateIndicatorDestroy(id)
                state = "idle"
                alarmPercent = 0.0
                threat_time = 0.0
            }
        }
    }
}
