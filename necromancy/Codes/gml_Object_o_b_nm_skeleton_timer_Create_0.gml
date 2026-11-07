// Must init before event_inherited/scr_buff_atr — Alarm 2 can fire during create.
nm_skel = -4
event_inherited()
scr_buff_atr()
stack = 1
buff_snd = -4
have_duration = 1
draw_duration = true
max_duration = 99
