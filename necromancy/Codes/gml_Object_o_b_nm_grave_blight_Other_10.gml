event_inherited()
var _caster = owner
if (!instance_exists(_caster))
    _caster = target
if (!instance_exists(_caster))
    exit

var _debuff = asset_get_index("o_db_nm_grave_blight")
var _hit = asset_get_index("o_nm_grave_blight_tick")
var _zx = zone_x
var _zy = zone_y
var _radius = zone_radius
var _zTileX = floor(_zx / 26)
var _zTileY = floor(_zy / 26)
// Enemy blight debuff lasts 1.5x the area effect duration.
// Use .0 literals so ReplaceGML does not resolve bare ints to asset ids.
var _dbTurns = ceil(max_duration * 1.5)
var _blindTurns = 2.0

with (o_enemy)
{
    if (is_player_enemy || (!is_neutral))
    {
        var _dx = abs(floor(x / 26) - _zTileX)
        var _dy = abs(floor(y / 26) - _zTileY)
        if (max(_dx, _dy) <= _radius)
        {
            if (_hit != -1)
            {
                with (instance_create_depth(x, y, 0, _hit))
                {
                    owner = _caster
                    target = other.id
                }
            }
            scr_effect_create(o_db_blind, _blindTurns, id, _caster)
            if (_debuff != -1)
            {
                var _ex = scr_instance_exists_in_list(_debuff, buffs)
                if (!_ex)
                {
                    with (scr_effect_create(_debuff, _dbTurns, id, _caster))
                        stage = 1
                }
                else
                {
                    with (_ex)
                    {
                        duration = max(duration, _dbTurns)
                        if (stage < max_stage)
                            stage += 1
                        alarm[3] = 1
                    }
                }
            }
        }
    }
}
