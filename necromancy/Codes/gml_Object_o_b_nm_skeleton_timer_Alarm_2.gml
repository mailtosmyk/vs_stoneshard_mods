event_inherited()
// nm_skel is assigned after scr_effect_create returns; skip until then.
if (nm_skel != -4 && (!instance_exists(nm_skel)))
    instance_destroy()
