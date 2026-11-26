if ((!is_execute) && instance_exists(owner))
{
    is_execute = true
    // Base 10% of companion Max HP, +1% per 5 Willpower.
    var _owner = owner
    var _wil = 10
    if (variable_instance_exists(_owner, "WIL"))
        _wil = _owner.WIL
    var _healPct = (10 + floor(_wil / 5))
    var _vsn = 8
    if (variable_instance_exists(_owner, "VSN"))
        _vsn = _owner.VSN
    var _skillName = name

    with (o_enemy)
    {
        if (variable_instance_exists(id, "sm_summon") && sm_summon && (HP > 0) && instance_exists(_owner) && (scr_tile_distance(id, _owner) <= _vsn))
        {
            var _heal = max(1, math_round((_healPct / 100) * max_hp))
            // Improve bodypart condition (injury stages) — scr_restore_hp only does this on overheal.
            if (variable_instance_exists(id, "Body_Parts_map") && ds_exists(Body_Parts_map, 1))
            {
                var _body = (_heal * 0.25)
                var _parts = max(1, ds_map_size(Body_Parts_map))
                var _per = (_body / _parts)
                var _key = ds_map_find_first(Body_Parts_map)
                repeat _parts
                {
                    scr_injuryChange(id, _key, _per)
                    _key = ds_map_find_next(Body_Parts_map, _key)
                }
            }
            scr_restore_hp(id, _heal, _skillName)
        }
    }
    if is_player(_owner)
        scr_allturn()
}
