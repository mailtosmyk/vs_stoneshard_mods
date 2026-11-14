scr_damage_init()
is_crit = false
name = "Bone Ward"
owner = o_player
target = -4
// Legacy deferred path (Destroy now damages immediately). Keep one-step delay
// only if a caller still creates this helper without applying damage itself.
alarm[1] = 1
