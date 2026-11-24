function scr_sm_xp_next(argument0)
{
    // XP required to reach the next level for this species (0 if already max).
    var _curve = [0.0, 70.0, 158.0, 280.0, 438.0, 630.0, 858.0, 1120.0, 1418.0, 1750.0]
    if (argument0 == "wolf")
        _curve = [0.0, 60.0, 134.0, 238.0, 372.0, 536.0, 729.0, 952.0, 1205.0, 1488.0]
    else if (argument0 == "crawler")
        _curve = [0.0, 70.0, 158.0, 280.0, 438.0, 630.0, 858.0, 1120.0, 1418.0, 1750.0]
    else if (argument0 == "bear")
        _curve = [0.0, 74.0, 166.0, 294.0, 460.0, 662.0, 901.0, 1176.0, 1489.0, 1838.0]
    else if (argument0 == "bison")
        _curve = [0.0, 77.0, 174.0, 308.0, 482.0, 693.0, 944.0, 1232.0, 1560.0, 1925.0]
    else if (argument0 == "gulon")
        _curve = [0.0, 84.0, 190.0, 336.0, 526.0, 756.0, 1030.0, 1344.0, 1702.0, 2100.0]

    var _lvl = scr_sm_level_get(argument0)
    if (_lvl >= 10)
        return 0
    return _curve[_lvl]
}
