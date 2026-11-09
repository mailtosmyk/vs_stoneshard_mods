var _ok = (instance_exists(o_nm_essence_leech_ico) && o_nm_essence_leech_ico.is_open)
_ok = (_ok && instance_exists(o_nm_grave_blight_ico) && o_nm_grave_blight_ico.is_open)
if _ok
    can_learn = 1
else
    can_learn = 0
