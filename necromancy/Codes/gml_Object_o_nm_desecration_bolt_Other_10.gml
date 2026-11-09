if ((!instance_exists(owner)) || (!instance_exists(target)))
    return;
Unholy_Damage = 8
event_inherited()
if (damage_done > 0)
{
    var _name = scr_actionsLogGetName(id)
    var _burn = math_round(damage_done * 0.33)
    if instance_exists(target)
    {
        if object_is_ancestor(target.object_index, o_unit)
        {
            with (target)
                scr_restore_mp(id, (-_burn), _name)
            with (owner)
                scr_restore_mp(id, _burn, _name)
        }
        // Apply / refresh Withering Curse. Grave Blight unlocks stacking up to II.
        var _curse = asset_get_index("o_db_withering_curse")
        if (_curse != -1 && object_is_ancestor(target.object_index, o_unit))
        {
            var _maxStage = 1
            // asset_get_index: bare ico names are unresolved when this Tier-1 bolt
            // is compiled (icos are created later with Tier 2/3).
            var _blightIco = asset_get_index("o_nm_grave_blight_ico")
            if (_blightIco != -1 && instance_exists(_blightIco) && _blightIco.is_open)
                _maxStage = 2
            var _sepIco = asset_get_index("o_nm_black_sepulchre_ico")
            if (_sepIco != -1 && instance_exists(_sepIco) && _sepIco.is_open)
                _maxStage = 3
            var _existing = scr_instance_exists_in_list(_curse, target.buffs)
            if (!_existing)
            {
                with (scr_effect_create(_curse, 10, target, owner))
                {
                    stage = 1
                    max_stage = _maxStage
                    have_stages = (_maxStage > 1)
                }
            }
            else
            {
                with (_existing)
                {
                    duration = max(duration, 10)
                    max_stage = max(max_stage, _maxStage)
                    have_stages = (max_stage > 1)
                    if (max_stage > 1 && stage < max_stage)
                        stage += 1
                }
            }
        }
    }
}
event_user(1)
