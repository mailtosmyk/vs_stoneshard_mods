event_inherited()
if (instance_exists(target) && instance_exists(o_onUnitEffectSprite))
{
    with (o_onUnitEffectSprite)
    {
        if (owner == other.target)
            instance_destroy()
    }
}
