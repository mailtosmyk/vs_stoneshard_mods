if instance_exists(owner)
{
    var _cap = (owner.LVL + 3)
    var _mastery = asset_get_index("o_nm_mastery_of_binding_ico")
    if (_mastery != -1 && instance_exists(_mastery) && _mastery.is_open)
        _cap += 1
    ds_map_replace(data, "Duration", _cap)
}
event_inherited()
