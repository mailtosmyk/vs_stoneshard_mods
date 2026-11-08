event_inherited()
ds_map_clear(data)
var _stage = max(1, stage)
var _worsen = 0.25
if (max_stage > 1)
    _worsen = 0.35
with (target)
{
    ds_map_add(other.data, "Fortitude", (-8 * _stage))
    ds_map_add(other.data, "Magic_Resistance", (-8 * _stage))
    ds_map_add(other.data, "max_hp", ((-0.05 * _stage) * max_hp))
    ds_map_add(other.data, "Damage_Received", (3 * _stage))
    scr_injuryChange(id, scr_choose_body_part(), (-_worsen * _stage))
    scr_atr_calc(id)
}
