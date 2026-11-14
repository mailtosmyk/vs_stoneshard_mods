event_inherited()
if (!instance_exists(owner))
    return;

var _empty = 0
with (o_inv_right_hand)
{
    if (!instance_exists(children))
        _empty++
}
with (o_inv_left_hand)
{
    if (!instance_exists(children))
        _empty++
}
if (_empty == 2)
{
    maxKD = math_round((maxKD * 0.5))
    Miscast_Chance = clamp((Miscast_Chance * 0.5), 0, 100)
}

// Level-cap tooltip: Mastery of Binding removes the limit.
var _mastery = asset_get_index("o_nm_mastery_of_binding_ico")
if (_mastery != -1 && instance_exists(_mastery) && _mastery.is_open)
    ds_map_replace(data, "LEVL", 99)
else
    ds_map_replace(data, "LEVL", (owner.LVL + 3))
