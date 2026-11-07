event_inherited()
with (target)
{
    ds_map_clear(other.data)
    ds_map_add(other.data, "Physical_Resistance", 12)
    ds_map_add(other.data, "Bleeding_Resistance", 15)
    ds_map_add(other.data, "Magic_Resistance", 10)
    ds_map_add(other.data, "Knockback_Resistance", 10)
    stats_is_change = true
    scr_atr_calc(id)
}
