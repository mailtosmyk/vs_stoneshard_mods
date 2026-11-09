if (instance_exists(target) && instance_exists(owner))
{
    var _crit = (is_crit ? max(1, (owner.Miracle_Power / 100)) : 1)
    var _healPct = (4 + ((owner.Magic_Power * 0.05) * _crit))
    var _servant = asset_get_index("o_b_nm_servant")
    if (_servant != -1 && scr_instance_exists_in_list(_servant, target.buffs))
        _healPct *= 1.5
    var _heal = ((_healPct / 100) * target.max_hp)
    var _body = (_heal * 0.25)
    with (target)
    {
        var key = ds_map_find_first(Body_Parts_map)
        repeat ds_map_size(Body_Parts_map)
        {
            scr_injuryChange(id, key, (_body / max(1, ds_map_size(Body_Parts_map))))
            key = ds_map_find_next(Body_Parts_map, key)
        }
    }
    scr_restore_hp(target, _heal, scr_id_get_name(id))
    var _buff = asset_get_index("o_b_nm_deaths_blessing")
    if (_buff != -1)
        scr_effect_create(_buff, turns, target, owner)
    var _arch = asset_get_index("o_nm_archtheurgy_ico")
    if (_arch != -1 && instance_exists(_arch))
        with (_arch) event_user(3)
}
event_inherited()
if instance_exists(owner)
{
    if is_player(owner)
    {
        var _san = 0.15
        var _sealed = asset_get_index("o_nm_sealed_mind_ico")
        if (_sealed != -1 && instance_exists(_sealed) && _sealed.is_open)
            _san *= 0.5
        scr_psy_change("Sanity", -_san)
        scr_allturn()
    }
}
