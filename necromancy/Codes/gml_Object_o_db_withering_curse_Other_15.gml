event_inherited()
ds_map_clear(data)
with (target)
{
    ds_map_add(other.data, "Fortitude", -8)
    ds_map_add(other.data, "Magic_Resistance", -8)
    ds_map_add(other.data, "max_hp", (-0.05 * max_hp))
    ds_map_add(other.data, "Damage_Received", 3)
    scr_injuryChange(id, scr_choose_body_part(), -0.25)
    scr_atr_calc(id)
}
