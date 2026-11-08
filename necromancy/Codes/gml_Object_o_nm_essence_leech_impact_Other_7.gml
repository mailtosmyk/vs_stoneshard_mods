if (!(instance_exists(owner) && instance_exists(target)))
{
    event_inherited()
    return;
}
var _hp = 0
var _mp = 0
var _grant = 0
var _servant = asset_get_index("o_b_nm_servant")
with (target)
{
    if (object_index == o_corpse)
    {
        scr_guiAnimation_ext(x, y, s_signofdarkness_impact)
        diss = 80
        scr_unitInterractImpact()
        scr_audio_play_at(snd_meat_blood_death_1)
        instance_destroy()
        instance_create_depth(x, y, 0, o_explosion_hole)
        _grant = 1
    }
    else if (object_is_ancestor(object_index, o_enemy) && _servant != -1 && scr_instance_exists_in_list(_servant, buffs))
    {
        _hp = (0.08 * max_hp)
        _mp = (0.08 * max_mp)
        scr_pure_damage(id, _hp)
        scr_restore_mp(id, (-_mp), other.name)
        _grant = 1
    }
}
with (owner)
{
    if (_hp || _mp)
    {
        if _hp
            scr_restore_hp(id, (_hp / 2), other.name)
        if _mp
            scr_restore_mp(id, (_mp / 2), other.name)
    }
    if _grant
    {
        var _buff = asset_get_index("o_b_nm_essence_charge")
        if (_buff != -1)
        {
            var _turns = irandom_range(12, 15)
            var _existing = scr_instance_exists_in_list(_buff, buffs)
            if (!_existing)
            {
                with (scr_effect_create(_buff, _turns, id, id))
                    stage = 1
            }
            else
            {
                with (_existing)
                {
                    duration = max_duration
                    if (stage < max_stage)
                        stage += 1
                    alarm[3] = 1
                }
            }
        }
    }
}
event_inherited()
if instance_exists(owner)
{
    if is_player(owner)
    {
        scr_psy_change("Sanity", -0.15)
        scr_allturn()
    }
}
