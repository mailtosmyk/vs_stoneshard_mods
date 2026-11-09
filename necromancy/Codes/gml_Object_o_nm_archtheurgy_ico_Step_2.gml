if (instance_exists(o_nm_death_touch_ico) && o_nm_death_touch_ico.is_open)
    can_learn = 1
else
    can_learn = 0
if instance_exists(owner)
    damage = math_round((owner.Magic_Power * 0.033))
