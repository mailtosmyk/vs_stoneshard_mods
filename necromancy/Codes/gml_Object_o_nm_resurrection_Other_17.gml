if instance_exists(owner)
{
    var _turns = 20.0
    var _mastery = asset_get_index("o_nm_mastery_of_binding_ico")
    if (_mastery != -1 && instance_exists(_mastery) && _mastery.is_open)
        _turns += 10.0
    ds_map_replace(data, "Duration", _turns)
}
event_inherited()
