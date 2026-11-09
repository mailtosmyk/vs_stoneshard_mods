if (instance_exists(o_nm_mastery_of_binding_ico) && o_nm_mastery_of_binding_ico.is_open)
    can_learn = 1
else
    can_learn = 0

// +100% XP from Undead kills while learned.
if is_open
{
    with (o_enemy)
    {
        if ((faction_key == "Undead" || faction_key == "Vampire") && ((!variable_instance_exists(id, "nm_xp_doubled")) || (!nm_xp_doubled)))
        {
            gain_xp *= 2
            nm_xp_doubled = 1
        }
    }
}

// Forget temporary stolen skill when the thrall dies.
if (is_open && variable_instance_exists(id, "nm_stolen_thrall") && nm_stolen_thrall != -4)
{
    if (!instance_exists(nm_stolen_thrall))
    {
        if (nm_stolen_ico != -4 && instance_exists(nm_stolen_ico))
        {
            with (nm_stolen_ico)
            {
                if (variable_instance_exists(id, "nm_blooming_temp") && nm_blooming_temp)
                {
                    is_open = 0
                    nm_blooming_temp = 0
                }
            }
        }
        nm_stolen_ico = -4
        nm_stolen_thrall = -4
    }
}
