event_inherited()
with (target)
{
    owner = other.owner
    is_neutral = true
    is_player_enemy = false
    scr_delete_from_enemy_list("Player")
    can_drop_loot = false
    loot_script = -4
}
