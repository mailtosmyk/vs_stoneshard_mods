// Parent Destroy removes this buff from target.buffs — run that first while
// target still points at the warded unit.
event_inherited()
if (!instance_exists(target))
    exit

var _owner = owner
if (!instance_exists(_owner))
    _owner = target

var _center = target
var _cx = _center.x
var _cy = _center.y

// Always show the shatter burst on the warded unit (even with no adjacent enemies).
with (scr_guiAnimation_ext(_cx, _cy, s_signofdarkness_impact))
{
    scr_audio_play_at(snd_dark_blessing_buff_impact)
    depth = ((-_cy) - 1)
}

// Deal shard damage immediately on this instance. Spawning helpers with alarm[1]
// from Destroy is unreliable (hit VFX/damage often never run).
scr_damage_init()
is_crit = false
name = "Bone Ward"
owner = _owner
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
