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
{
    var _sanLoss = 0.01
    var _sealed = asset_get_index("o_nm_sealed_mind_ico")
    if (_sealed != -1 && instance_exists(_sealed) && _sealed.is_open)
        _sanLoss *= 0.5
    scr_psy_change("Sanity", -_sanLoss)
}
