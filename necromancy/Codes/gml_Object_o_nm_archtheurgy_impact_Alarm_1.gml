target = scr_tile_get_instance(x, y, 0, 0)
if (instance_exists(owner) && instance_exists(target))
{
    var _dmg = math_round((owner.Magic_Power * 0.033))
    if (_dmg < 1)
        _dmg = 1
    bArmor_Piercing = 100
    var _asc = asset_get_index("o_b_nm_ascension")
    var _hasAsc = (_asc != -1 && scr_instance_exists_in_list(_asc, owner.buffs))
    if _hasAsc
        Sacred_Damage = _dmg
    else
        Unholy_Damage = _dmg
    if _hasAsc
    {
        with (target)
            scr_guiAnimation_ext(x, y, s_weapondamage_sacred)
    }
    else
    {
        with (target)
            scr_guiAnimation_ext(x, y, s_weapondamage_unholy)
    }
    scr_skill_damage(target)
}
instance_destroy()
