if ((!is_execute) && instance_exists(owner) && instance_exists(point))
{
    is_execute = true
    // Mark stored on player for summons to prefer (MVP: damage buff via temp).
    with (owner)
    {
        sm_hunt_target = other.point
        sm_hunt_turns = 6
    }
    if is_player(owner)
        scr_allturn()
}
