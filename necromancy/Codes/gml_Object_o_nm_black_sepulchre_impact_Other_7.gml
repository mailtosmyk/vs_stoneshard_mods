if (!instance_exists(owner))
{
    event_inherited()
    return;
}

var _curse = asset_get_index("o_db_withering_curse")
var _main = -4
var _stacks = 0
var _cx = x
var _cy = y
if instance_exists(target)
{
    _cx = target.x
    _cy = target.y
}

if (_curse != -1)
{
    // Prefer a unit under the cast point that already has the curse.
    if (instance_exists(target) && object_is_ancestor(target.object_index, o_unit))
    {
        var _onTarget = scr_instance_exists_in_list(_curse, target.buffs)
        if _onTarget
        {
            _main = target
            _stacks = max(1, _onTarget.stage)
        }
    }

    // Tile cast / no cursed unit under cursor: strongest cursed enemy in 3x3.
    if (!instance_exists(_main))
    {
        var _best = -4
        var _bestStage = 0
        var _tTileX = floor(_cx / 26)
        var _tTileY = floor(_cy / 26)
        with (o_enemy)
        {
            if (visible && is_player_enemy && HP > 0)
            {
                var _dx = abs(floor(x / 26) - _tTileX)
                var _dy = abs(floor(y / 26) - _tTileY)
                if (max(_dx, _dy) <= 1)
                {
                    var _c = scr_instance_exists_in_list(_curse, buffs)
                    if (_c && _c.stage > _bestStage)
                    {
                        _best = id
                        _bestStage = _c.stage
                    }
                }
            }
        }
        if instance_exists(_best)
        {
            _main = _best
            _stacks = _bestStage
        }
    }
}

if ((!instance_exists(_main)) || _stacks <= 0)
{
    event_inherited()
    if instance_exists(owner)
    {
        if is_player(owner)
            scr_allturn()
    }
    return;
}

var _mainDmg = max(1, math_round(((6 * _stacks) * owner.Magic_Power) * 0.01))
var _splash = max(1, math_round(((3 * _stacks) * owner.Magic_Power) * 0.01))
var _owner = owner
var _mx = _main.x
var _my = _main.y

with (scr_instance_exists_in_list(_curse, _main.buffs))
    instance_destroy()

with (_main)
{
    with (scr_guiAnimation_ext(x, y, s_signofdarkness_impact))
    {
        scr_audio_play_at(snd_sigilofdarkness_cast)
        depth = (other.depth - 1)
    }
}

Unholy_Damage = _mainDmg
target = _main
scr_skill_damage(_main)

with (o_enemy)
{
    if (visible && is_player_enemy && HP > 0 && id != _main)
    {
        if (scr_tile_distance(id, _main) <= 1)
        {
            other.Unholy_Damage = _splash
            other.target = id
            with (scr_guiAnimation_ext(x, y, s_weapondamage_unholy))
                depth = (other.depth - 1)
            with (other)
                scr_skill_damage(target)
        }
    }
}
event_inherited()

if instance_exists(_owner)
{
    if is_player(_owner)
    {
        var _san = 0.15
        var _sealed = asset_get_index("o_nm_sealed_mind_ico")
        if (_sealed != -1 && instance_exists(_sealed) && _sealed.is_open)
            _san *= 0.5
        with (_owner)
            scr_psy_change("Sanity", -_san)
        with (_owner)
            scr_allturn()
    }
}
