if instance_exists(owner)
{
    var _wil = owner.WIL
    ds_map_replace(data, "Duration", (5 + floor(_wil / 5)))
    ds_map_replace(data, "HP_Percent", (33 + floor(_wil / 2)))
    ds_map_replace(data, "MP_Percent", (25 + floor(_wil / 3)))
}
event_inherited()
