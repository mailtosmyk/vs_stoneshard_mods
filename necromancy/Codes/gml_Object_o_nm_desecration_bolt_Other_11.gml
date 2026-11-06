if instance_exists(target)
{
    var dir = point_direction(x, y, target.x, target.y)
    with (instance_create_depth(target.x, target.y, 0, o_spellimpact))
    {
        sprite_index = s_weapondamage_unholy
        image_speed = 1
        scr_audio_play_at(choose(snd_arcane_bolt_impact_1, snd_arcane_bolt_impact_2, snd_arcane_bolt_impact_3, snd_arcane_bolt_impact_4))
        scr_set_lt()
    }
}
