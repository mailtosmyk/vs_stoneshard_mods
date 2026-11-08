if (instance_exists(owner) && instance_exists(target))
{
    Unholy_Damage = 2
    with (target)
        scr_guiAnimation_ext(x, y, s_weapondamage_unholy)
    scr_skill_damage(target)
}
instance_destroy()
