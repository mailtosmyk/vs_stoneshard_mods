if instance_exists(owner)
{
    var _cap = (owner.LVL + 3)
    ds_map_replace(data, "Duration", _cap)
}
event_inherited()
