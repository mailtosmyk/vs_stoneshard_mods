if (!(instance_exists(owner) && instance_exists(target)))
{
    event_inherited()
    return;
}

var _tx = target.x
var _ty = target.y
var _tTileX = floor(_tx / 26)
var _tTileY = floor(_ty / 26)
var _wasAlly = false
var _servant = asset_get_index("o_b_nm_servant")
if (object_is_ancestor(target.object_index, o_enemy) && _servant != -1 && scr_instance_exists_in_list(_servant, target.buffs))
    _wasAlly = true

var _wil = owner.WIL
var _radius = 1
if (_wil >= 18)
    _radius = 3
else if (_wil >= 12)
    _radius = 2

var _unholy = max(1, math_round(((8 + floor(_wil / 4.0)) * owner.Magic_Power) * 0.01))
var _pierce = max(1, math_round(((6 + floor(_wil / 5.0)) * owner.Magic_Power) * 0.01))
var _bleedChance = clamp(25 + (_wil * 2), 25, 75)

with (target)
{
    scr_guiAnimation_ext(x, y, s_signofdarkness_impact)
    scr_audio_play_at(snd_meat_blood_death_1)
    if (object_index == o_corpse)
    {
        instance_create_depth(x, y, 0, o_explosion_hole)
        instance_destroy()
    }
    else
    {
        if (_servant != -1)
        {
            with (scr_instance_exists_in_list(_servant, buffs))
                instance_destroy()
        }
        var _unbind = asset_get_index("o_b_nm_unbind")
        if (_unbind != -1)
        {
            with (scr_instance_exists_in_list(_unbind, buffs))
                instance_destroy()
        }
        instance_destroy()
    }
}

with (o_enemy)
{
    if (visible && is_player_enemy && HP > 0)
    {
        var _dx = abs(floor(x / 26) - _tTileX)
        var _dy = abs(floor(y / 26) - _tTileY)
        if (max(_dx, _dy) <= _radius)
        {
            other.Unholy_Damage = _unholy
            other.Piercing_Damage = _pierce
            other.bArmor_Piercing = 0
            other.target = id
            with (scr_guiAnimation_ext(x, y, s_weapondamage_unholy))
                depth = (other.depth - 1)
            with (other)
                scr_skill_damage(target)
            if scr_chance_value(_bleedChance)
                scr_injuryChange(id, scr_choose_body_part(), -3)
        }
    }
}

event_inherited()
if instance_exists(owner)
{
    if is_player(owner)
    {
        var _san = (_wasAlly ? 0.25 : 0.1)
        var _sealed = asset_get_index("o_nm_sealed_mind_ico")
        if (_sealed != -1 && instance_exists(_sealed) && _sealed.is_open)
            _san *= 0.5
        scr_psy_change("Sanity", -_san)
        scr_allturn()
    }
}
