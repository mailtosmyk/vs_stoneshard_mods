if instance_exists(owner)
{
    var _wil = owner.WIL
    var _duration = (5 + floor(_wil / 5))
    var _hp = (33 + floor(_wil / 2))
    var _mp = (25 + floor(_wil / 3))
    var _mastery = asset_get_index("o_nm_mastery_of_binding_ico")
    if (_mastery != -1 && instance_exists(_mastery) && _mastery.is_open)
    {
        _duration += 10
        _hp += 10
        _mp += 10
    }
    ds_map_replace(data, "Duration", _duration)
    ds_map_replace(data, "HP_Percent", _hp)
    ds_map_replace(data, "MP_Percent", _mp)
}
event_inherited()
