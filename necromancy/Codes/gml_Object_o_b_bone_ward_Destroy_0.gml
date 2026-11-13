event_inherited()
if (!instance_exists(target))
    exit

var _owner = owner
if (!instance_exists(_owner))
    _owner = target

var _center = target

// Always show the shatter burst on the warded unit (even with no adjacent enemies).
with (_center)
{
    with (scr_guiAnimation_ext(x, y, s_signofdarkness_impact))
    {
        scr_audio_play_at(snd_dark_blessing_buff_impact)
        depth = (other.depth - 1)
    }
}

// Deal shard damage immediately on the buff instance (alarms are unreliable in Destroy).
scr_damage_init()
Unholy_Damage = 4.0
Piercing_Damage = 3.0
bArmor_Piercing = 0.0

with (o_enemy)
{
    if (visible && is_player_enemy && HP > 0)
    {
        if (scr_tile_distance(id, _center) <= 1)
        {
            other.target = id
            with (scr_guiAnimation_ext(x, y, s_weapondamage_unholy))
                depth = (other.depth - 1)
            with (other)
                scr_skill_damage(target)
            if scr_chance_value(50.0)
                scr_effect_create(o_db_daze, 1.0, id, _owner)
        }
    }
}
