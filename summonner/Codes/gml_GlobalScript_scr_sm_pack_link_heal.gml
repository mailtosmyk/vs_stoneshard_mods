function scr_sm_pack_link_heal(argument0)
{
    // Called from scr_physical_damage (self = attacker). argument0 = damage dealt.
    if (argument0 <= 0)
        return
    if (!variable_instance_exists(id, "sm_summon") || (!sm_summon))
        return
    if (!instance_exists(o_player))
        return
    var _smLink = asset_get_index("o_b_sm_pack_link")
    if (_smLink == -1)
        return
    if (!variable_instance_exists(o_player, "buffs"))
        return
    if (!scr_instance_exists_in_list(_smLink, o_player.buffs))
        return
    var _smHeal = math_round(argument0 * 0.20)
    if (_smHeal <= 0)
        return
    with (o_player)
        scr_restore_hp(id, _smHeal, "Pack Link")
}
