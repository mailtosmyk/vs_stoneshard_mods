event_inherited()
duration = min(duration, max_duration)

// Nested checks: GML may evaluate both sides of || / &&.
if (!variable_instance_exists(id, "nm_thrall"))
    return;
if (nm_thrall == -4)
    return;
if (!instance_exists(nm_thrall))
{
    instance_destroy()
    return;
}
