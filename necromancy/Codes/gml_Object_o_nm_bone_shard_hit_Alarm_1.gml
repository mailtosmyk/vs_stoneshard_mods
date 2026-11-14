if (instance_exists(owner) && instance_exists(target))
{
    scr_damage_init()
    is_crit = false
    if ((!variable_instance_exists(id, "name")) || (name == "N/A"))
        name = "Bone Ward"
    Unholy_Damage = 4.0
    Piercing_Damage = 3.0
    bArmor_Piercing = 0.0
    with (target)
    {
        with (scr_guiAnimation_ext(x, y, s_weapondamage_unholy))
            depth = (other.depth - 1)
    }
    scr_skill_damage(target)
    if scr_chance_value(50.0)
        scr_effect_create(o_db_daze, 1.0, target, owner)
}
instance_destroy()
