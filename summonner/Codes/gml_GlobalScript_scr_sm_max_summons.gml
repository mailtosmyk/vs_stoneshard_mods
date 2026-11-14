function scr_sm_max_summons()
{
    var _max = 1
    var _ico = asset_get_index("o_sm_pack_instincts_ico")
    if (_ico != -1 && instance_exists(_ico) && _ico.is_open)
        _max += 1
    return _max
}
