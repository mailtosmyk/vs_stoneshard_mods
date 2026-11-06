if (instance_exists(owner) && instance_exists(target))
{
    Unholy_Damage = 4
    Piercing_Damage = 3
    bArmor_Piercing = 0
    with (target)
        scr_guiAnimation_ext(x, y, s_weapondamage_unholy)
    scr_skill_damage(target)
    if scr_chance_value(50)
        scr_effect_create(o_db_daze, 1, target, owner)
}
instance_destroy()
