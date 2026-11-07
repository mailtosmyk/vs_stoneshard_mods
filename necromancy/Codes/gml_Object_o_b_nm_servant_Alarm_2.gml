event_inherited()
with (target)
{
    owner = other.owner
    is_neutral = true
    is_player_enemy = false
    scr_delete_from_enemy_list("Player")
    can_drop_loot = false
    loot_script = -4

    // Keep Player out of combat attitudes (enemy AI can refresh arrays).
    var _ca = []
    var _i = 0.0
    while (_i < array_length(combat_array))
    {
        if (combat_array[_i] != "Player")
            _ca[array_length(_ca)] = combat_array[_i]
        _i += 1.0
    }
    combat_array = _ca

    var _ta = []
    _i = 0.0
    while (_i < array_length(threat_array))
    {
        if (threat_array[_i] != "Player")
            _ta[array_length(_ta)] = threat_array[_i]
        _i += 1.0
    }
    threat_array = _ta
}
