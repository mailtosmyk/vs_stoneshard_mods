if instance_exists(owner)
{
    var _turns = 20.0
    ds_map_replace(data, "Duration", _turns)
}
event_inherited()
