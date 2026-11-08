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
}
event_inherited()
if instance_exists(owner)
{
    if is_player(owner)
    {
        scr_psy_change("Sanity", -0.15)
        scr_allturn()
    }
}
