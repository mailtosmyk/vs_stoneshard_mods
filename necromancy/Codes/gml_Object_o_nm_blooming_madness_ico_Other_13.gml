event_inherited()
if (!is_open)
    return;

// Steal one active ability from a newly raised thrall (set nm_pending_thrall first).
if (!(variable_instance_exists(id, "nm_pending_thrall") && instance_exists(nm_pending_thrall)))
    return;

var _thrall = nm_pending_thrall
nm_pending_thrall = -4
var _obj = _thrall.object_index
var _pick = -4

if (_obj == o_skeleton_spearman)
    _pick = o_skill_piercing_lunge_ico
else if (_obj == o_skeleton_axeman || _obj == o_skeleton_maceman || _obj == o_skeleton_swordsman)
    _pick = o_skill_raise_shield_ico
else if (_obj == o_skeleton_bowman || _obj == o_skeleton_ranger || _obj == o_skeleton_crossbowman)
    _pick = o_skill_taking_aim_ico
else if (_obj == o_skeleton_footman)
    _pick = o_skill_cleave_ico
else if (_obj == o_skeleton_militiaman)
    _pick = o_skill_cut_through_ico
else if (_obj == o_skeleton_defender)
    _pick = o_skill_defensive_stance_ico
else if (_obj == o_skeleton_guard)
    _pick = o_skill_nail_down_ico
else if (_obj == o_skeleton_soldier)
    _pick = o_skill_breakthrough_ico
else if (_obj == o_skeleton_warrior)
    _pick = o_skill_ram_ico
else if (_obj == o_skeleton_halberdier)
    _pick = o_skill_pikemans_stance_ico
else if (_obj == o_skeleton_knightAxe)
    _pick = o_skill_wide_cut_ico
else if (_obj == o_skeleton_knightSword)
    _pick = o_skill_riposte_ico
else if (_obj == o_skeleton_knightHammer)
    _pick = o_skill_mighty_swing_ico
else if (_obj == o_ghoul_medium || _obj == o_ghoul_large)
    _pick = o_skill_series_of_strikes_ico
else if (_obj == o_bandit_rioter)
    _pick = o_skill_piercing_lunge_ico
else if (_obj == o_bandit_poacher || _obj == o_bandit_outlaw || _obj == o_bandit_arbalester)
    _pick = o_skill_taking_aim_ico
else if (_obj == o_bandit_crook || _obj == o_bandit_cutthroat)
    _pick = o_skill_double_jab_ico
else if (_obj == o_bandit_thug_2haxe)
    _pick = o_skill_wide_cut_ico
else if (_obj == o_bandit_thug_2hsword)
    _pick = o_skill_feint_swing_ico
else if (_obj == o_bandit_henchman_axe)
    _pick = o_skill_cut_through_ico
else if (_obj == o_bandit_henchman_mace)
    _pick = o_skill_onslaught_ico
else if (_obj == o_bandit_mancatcher)
    _pick = o_skill_dash_ico
else if (_obj == o_bandit_renegade_halberd)
    _pick = o_skill_nail_down_ico
else if (_obj == o_bandit_renegade_2hsword)
    _pick = o_skill_feint_swing_ico
else if (_obj == o_bandit_renegade_2haxe)
    _pick = o_skill_reign_in_blood_ico
else if (_obj == o_bandit_renegade_2hmace)
    _pick = o_skill_unstoppable_force_ico
else if (_obj == o_bandit_marauder_2haxe)
    _pick = o_skill_dismember_ico
else if (_obj == o_bandit_marauder_2hsword)
    _pick = o_skill_heroic_charge_ico
else if (_obj == o_bandit_marauder_2hmace)
    _pick = o_skill_skull_crusher_ico
else if (_obj == o_bandit_deserter_sword)
    _pick = o_skill_advance_ico
else if (_obj == o_bandit_deserter_axe)
    _pick = o_skill_crippling_lunge_ico
else if (_obj == o_bandit_deserter_flail)
    _pick = o_skill_armor_break_ico
else if (_obj == o_bandit_marksman)
    _pick = o_skill_headshot_ico
else if (_obj == o_bandit_duelist)
    _pick = o_skill_gaping_wound_ico
else if (_obj == o_bandit_kingpin)
    _pick = o_skill_power_kick_ico
else if (_obj == o_bandit_warlock)
    _pick = o_skill_seal_of_power_ico
else if (_obj == o_proselyte_chosen)
    _pick = o_skill_series_of_strikes_ico
else if (_obj == o_proselyte_impaler)
    _pick = o_skill_sudden_strike_ico
else if (_obj == o_proselyte_outcast)
    _pick = o_skill_elusiveness_ico
else if (_obj == o_proselyte_disciple_2haxe)
    _pick = o_skill_dismember_ico
else if (_obj == o_proselyte_disciple_2hflail)
    _pick = o_skill_skull_crusher_ico
else if (_obj == o_revenant || _obj == o_necromancer)
    _pick = o_skill_hail_of_blows_ico

if (_pick == -4)
    return;

// Drop previous temporary skill.
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

var _study = ["Necromancy", _pick]
if scr_skill_branch_can_study(_study)
    scr_skill_branch_study(_study)
with (_pick)
{
    is_open = 1
    is_lock = 0
    nm_blooming_temp = 1
}
nm_stolen_ico = _pick
nm_stolen_thrall = _thrall
