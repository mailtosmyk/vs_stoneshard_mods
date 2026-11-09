if instance_exists(owner)
{
    var _wil = owner.WIL
    ds_map_replace(data, "Unholy_Damage", ((8 + floor(_wil / 4)) * owner.Magic_Power * 0.01))
    ds_map_replace(data, "Piercing_Damage", ((6 + floor(_wil / 5)) * owner.Magic_Power * 0.01))
}
event_inherited()
