var _bless = (instance_exists(o_nm_deaths_blessing_ico) && o_nm_deaths_blessing_ico.is_open)
var _res = (instance_exists(o_nm_resurrection_ico) && o_nm_resurrection_ico.is_open)
if (_bless || _res)
    can_learn = 1
else
    can_learn = 0
