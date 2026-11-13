if (!(instance_exists(owner) && instance_exists(target)))
{
    event_inherited()
    return;
}

var _mpBonus = 0
var _charge = asset_get_index("o_b_nm_essence_charge")
if (_charge != -1)
{
    var _existing = scr_instance_exists_in_list(_charge, owner.buffs)
    if _existing
    {
        _mpBonus = 15
        with (_existing)
            event_user(4)
    }
}

var _base = 12
var _curse = asset_get_index("o_db_withering_curse")
var _hasCurse = (_curse != -1 && scr_instance_exists_in_list(_curse, target.buffs))
if _hasCurse
    _base = math_round(_base * 1.25)

var _mp = owner.Magic_Power + _mpBonus
Unholy_Damage = max(1, math_round((_base * _mp) * 0.01))
if is_crit
    Unholy_Damage = math_round(Unholy_Damage * max(1, (owner.Miracle_Power / 100)))

with (target)
    scr_guiAnimation_ext(x, y, s_signofdarkness_impact)
scr_audio_play_at(snd_skill_death_touch_hit)

var _immob = (36 + (_mp * 0.3))
with (target)
{
    if object_is_ancestor(object_index, o_unit)
    {
        if scr_chance_value(max((_immob - Knockback_Resistance), 0))
            scr_effect_create(o_db_immob, 2, id, other.owner)
    }
}

scr_skill_damage(target)
var _dealt = damage_done
if (_dealt <= 0)
    _dealt = Unholy_Damage

if (_hasCurse && instance_exists(target))
{
    var _c = scr_instance_exists_in_list(_curse, target.buffs)
    if _c
    {
        with (_c)
        {
            duration = max(duration, 10)
            max_stage = max(max_stage, 1)
            var _sep = asset_get_index("o_nm_black_sepulchre_ico")
            if (_sep != -1 && instance_exists(_sep) && _sep.is_open)
                max_stage = max(max_stage, 3)
            else
            {
                var _blight = asset_get_index("o_nm_grave_blight_ico")
                if (_blight != -1 && instance_exists(_blight) && _blight.is_open)
                    max_stage = max(max_stage, 2)
            }
            have_stages = (max_stage > 1)
            if (stage < max_stage)
                stage += 1
        }
    }
}

var _healRatio = 0.5
var _heal = math_round(_dealt * _healRatio)
var _servant = asset_get_index("o_b_nm_servant")
var _caster = owner
var _skillName = name
if instance_exists(_caster)
{
    // Heal caster if in vision of self (always).
    scr_restore_hp(_caster, _heal, _skillName)
    with (o_enemy)
    {
        if (visible && HP > 0 && _servant != -1 && scr_instance_exists_in_list(_servant, buffs))
        {
            if (scr_tile_distance(id, _caster) <= _caster.VSN)
                scr_restore_hp(id, _heal, _skillName)
        }
    }
}

event_inherited()
if instance_exists(owner)
{
    if is_player(owner)
    {
        var _san = 0.2
        var _sealed = asset_get_index("o_nm_sealed_mind_ico")
        if (_sealed != -1 && instance_exists(_sealed) && _sealed.is_open)
            _san *= 0.5
        scr_psy_change("Sanity", -_san)
        scr_allturn()
    }
}
