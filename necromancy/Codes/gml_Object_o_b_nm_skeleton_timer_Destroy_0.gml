event_inherited()
if instance_exists(target)
{
    with (target)
    {
        if (variable_instance_exists(id, "nm_summon") && nm_summon)
            instance_destroy()
    }
}
