event_inherited()
// nm_skel is assigned after scr_effect_create returns; skip until then.
if ((!variable_instance_exists(id, "nm_skel")) || nm_skel == -4)
    return;
if (!instance_exists(nm_skel))
    instance_destroy()
