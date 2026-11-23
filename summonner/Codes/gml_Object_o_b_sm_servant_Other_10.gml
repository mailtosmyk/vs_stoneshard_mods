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
}
