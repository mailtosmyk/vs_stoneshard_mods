if instance_exists(owner)
{
    var _wil = owner.WIL
    var _pct = clamp(8 + floor(_wil / 5), 8, 20)
    var _turns = clamp(12 + floor(_wil / 5), 12, 30)
    ds_map_replace(data, "HP_Percent", _pct)
    ds_map_replace(data, "MP_Percent", _pct)
    ds_map_replace(data, "Duration", _turns)
}
event_inherited()
