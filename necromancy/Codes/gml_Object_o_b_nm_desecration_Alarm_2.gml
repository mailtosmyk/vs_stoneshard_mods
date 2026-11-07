event_inherited()
var _bonus = 0
if instance_exists(owner)
    _bonus = math_round(((7 * (owner.Magic_Power * 0.01)) / 4))
with (target)
{
    ds_map_clear(other.data)
    ds_map_add(other.data, "Unholy_Damage", _bonus)
}
