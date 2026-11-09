if instance_exists(owner)
{
    ds_map_replace(data, "Unholy_Damage", ((11 * owner.Magic_Power) * 0.01))
    ds_map_replace(data, "I_C", (36 + (owner.Magic_Power * 0.3)))
}
event_inherited()
