if (instance_exists(o_nm_corpse_burst_ico) && o_nm_corpse_burst_ico.is_open)
    can_learn = 1
else
    can_learn = 0

var _moral = scr_atr("Morale")
if is_open
{
    if instance_exists(o_player)
    {
        with (o_player)
        {
            // Morale > 50 and a usable Max Health pool (max_hp not collapsed).
            if (max_hp >= 2 && _moral >= 50)
                global.playerNoDeathMode = 1
            else
                global.playerNoDeathMode = 0
        }
    }
}
else
    global.playerNoDeathMode = 0

if is_open
    event_user(3)
