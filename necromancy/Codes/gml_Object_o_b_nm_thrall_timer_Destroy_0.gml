event_inherited()
if (!variable_instance_exists(id, "nm_thrall"))
    return;
if (nm_thrall == -4)
    return;
if instance_exists(nm_thrall)
{
    with (nm_thrall)
    {
        if (variable_instance_exists(id, "nm_resurrection") && nm_resurrection)
            instance_destroy()
    }
}
