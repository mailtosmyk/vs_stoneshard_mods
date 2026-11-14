function scr_sm_level_get(argument0)
{
    var _xp = scr_sm_xp_get(argument0)
    var _curve = [0.0, 70.0, 158.0, 280.0, 438.0, 630.0, 858.0, 1120.0, 1418.0, 1750.0]
    var _lvl = 1
    var i = 1
    while (i < array_length(_curve))
    {
        if (_xp >= _curve[i])
            _lvl = (i + 1)
        else
            break
        i += 1
    }
    return clamp(_lvl, 1, 10)
}
