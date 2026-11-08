event_inherited()
var _caster = owner
if (!instance_exists(_caster))
    _caster = target
var _debuff = asset_get_index("o_db_nm_grave_blight")
var _hit = asset_get_index("o_nm_grave_blight_tick")
var _zx = zone_x
var _zy = zone_y
with (o_enemy)
{
    if (is_player_enemy || (!is_neutral))
    {
        if (point_distance(x, y, _zx, _zy) <= 40)
        {
            if (_hit != -1)
            {
                with (instance_create_depth(x, y, 0, _hit))
                {
                    owner = _caster
                    target = other.id
                }
            }
            if (_debuff != -1)
            {
                var _ex = scr_instance_exists_in_list(_debuff, buffs)
                if (!_ex)
                {
                    with (scr_effect_create(_debuff, 2, id, _caster))
                        stage = 1
                }
                else
                {
                    with (_ex)
                    {
                        duration = max(duration, 2)
                        if (stage < max_stage)
                            stage += 1
                        alarm[3] = 1
                    }
                }
            }
        }
    }
}
