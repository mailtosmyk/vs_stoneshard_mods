event_inherited()
if instance_exists(nm_skel)
{
    with (nm_skel)
    {
        if (variable_instance_exists(id, "nm_summon") && nm_summon)
            instance_destroy()
    }
}
