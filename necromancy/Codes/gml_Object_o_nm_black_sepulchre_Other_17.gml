if instance_exists(owner)
{
    ds_map_replace(data, "Unholy_Damage", ((6 * owner.Magic_Power) * 0.01))
}
event_inherited()
