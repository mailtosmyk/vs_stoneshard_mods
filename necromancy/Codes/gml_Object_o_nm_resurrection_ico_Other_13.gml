if (!is_open)
    return;
var _san = scr_atr("Sanity")
var _mor = scr_atr("Morale")
if (_mor < 70)
    scr_psy_change("Morale", 0.3)
with (o_b_bless)
{
    if (target == o_player)
        instance_destroy()
}
if (_san > 60)
    scr_psy_change("Sanity", -0.01)
