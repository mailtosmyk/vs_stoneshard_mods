event_inherited()
// Clear the player's hunt mark pointer when this mark expires / is replaced.
if (instance_exists(o_player) && variable_instance_exists(o_player, "sm_hunt_target") && instance_exists(target) && (o_player.sm_hunt_target == target))
{
    o_player.sm_hunt_target = -4
    o_player.sm_hunt_turns = 0
}
