event_inherited()
name = ds_map_find_value_ext(global.actionsLogMissile, "unholy", "unholy")
is_missile = true
scr_light_off()
scr_set_lt()
image_speed = 0.5
speed = 10
scr_audio_play_at(choose(snd_arcane_casting_1, snd_arcane_casting_2))
scr_audio_play_at(snd_arcane_bolt_casting_1)
scr_damage_init()
can_reflect = true
