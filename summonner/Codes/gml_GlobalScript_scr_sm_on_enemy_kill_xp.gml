function scr_sm_on_enemy_kill_xp()
{
    // Called from o_enemy Destroy (self = victim). Player kill XP is handled by the vanilla +
    // assembly patch; this credits the attacker's species XP track on the player.
    var _att = last_attacker
    if (!instance_exists(_att))
        return
    if (!variable_instance_exists(_att, "sm_summon") || (!_att.sm_summon))
        return
    if (!variable_instance_exists(_att, "sm_species"))
        return
    var _species = _att.sm_species
    var _owner = -4
    if (variable_instance_exists(_att, "owner"))
        _owner = _att.owner
    if ((!instance_exists(_owner)) || (!is_player(_owner)))
    {
        if (!instance_exists(o_player))
            return
        _owner = o_player
    }
    if (gain_xp <= 0)
        return
    var _amt = gain_xp
    var _ico = asset_get_index("o_sm_pack_instincts_ico")
    if ((_ico != -1) && instance_exists(_ico) && _ico.is_open)
        _amt = floor(_amt * 1.1)
    _ico = asset_get_index("o_sm_kindred_spirit_ico")
    if ((_ico != -1) && instance_exists(_ico) && _ico.is_open)
        _amt = floor(_amt * 1.25)
    if (variable_instance_exists(_owner, "sm_Summon_XP"))
        _amt = floor(_amt * (1 + (_owner.sm_Summon_XP / 100)))
    scr_sm_xp_add(_species, _amt)
}