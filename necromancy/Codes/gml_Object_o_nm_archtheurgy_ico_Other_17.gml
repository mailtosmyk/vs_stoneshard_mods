if instance_exists(owner)
    ds_map_replace(text_map, "Unholy_Damage", math_round((0.033 * owner.Magic_Power)))
event_inherited()
