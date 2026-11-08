var _zone = asset_get_index("o_nm_grave_blight_zone")
if (_zone != -1)
{
    with (_zone)
    {
        if (owner == other.target)
            instance_destroy()
    }
}
event_inherited()
