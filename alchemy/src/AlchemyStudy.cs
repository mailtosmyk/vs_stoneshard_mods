using ModShardLauncher;

namespace Alchemy;

/// <summary>
/// Lets players learn an alchemy craft schematic by placing 4 of the same
/// product plus any gem in a cooking pot or craft station.
/// </summary>
internal static class AlchemyStudy
{
    public static void Patch()
    {
        var products = AlchemyCatalog.EnabledRecipes
            .Select(r => r.Elixir)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(id => id, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (products.Length == 0)
            return;

        // Register helpers before patching callers (UTMT resolves function refs at compile).
        Msl.AddFunction(BuildFindScript(products), "scr_alch_study_elixir");
        Msl.AddFunction(BuildTryScript(), "scr_alch_study_try");

        const string enableStudy =
            "if (scr_alch_study_elixir() != \"\")\n" +
            "    canCraft = true";

        // Enable Craft when the study pattern is present (food = cooking pot, consums = craft).
        foreach (var codeName in new[]
                 {
                     "gml_Object_o_craftingFoodMenu_Other_13",
                     "gml_Object_o_craftingConsumsMenu_Other_13",
                 })
        {
            Msl.LoadGML(codeName)
                .MatchFrom("event_inherited()")
                .InsertAbove(enableStudy)
                .Save();
        }

        const string tryStudy =
            "if (scr_alch_study_try())\n" +
            "{\n" +
            "    event_inherited()\n" +
            "    exit\n" +
            "}";

        Msl.LoadGML("gml_Object_o_craftingFoodMenu_Other_14")
            .MatchFrom("var _object = __asset_get_index(\"o_inv_\" + idName)")
            .InsertAbove(tryStudy)
            .Save();

        Msl.LoadGML("gml_Object_o_craftingConsumsMenu_Other_14")
            .MatchFrom("with (consumsContainer)")
            .InsertAbove(tryStudy)
            .Save();
    }

    static string BuildFindScript(IReadOnlyList<string> products)
    {
        var list = string.Join(", ", products.Select(id => $"\"{Escape(id)}\""));
        return string.Join("\n", new[]
        {
            "function scr_alch_study_elixir()",
            "{",
            "    if (!instance_exists(o_craftingMenu))",
            "        return \"\"",
            "",
            $"    var _elixirs = [{list}]",
            "    var _owner = o_craftingMenu.id",
            "    var _gemCount = 0",
            "    var _otherCount = 0",
            "    var _elixirId = \"\"",
            "    var _elixirCount = 0",
            "",
            "    with (o_inv_slot)",
            "    {",
            "        if (owner == _owner && (!is_pot))",
            "        {",
            "            var _isGem = (ds_list_find_index(category, \"gem\") >= 0)",
            "            var _isElixir = false",
            "            var _i = 0",
            "            while (_i < array_length(_elixirs))",
            "            {",
            "                if (idName == _elixirs[_i])",
            "                {",
            "                    _isElixir = true",
            "                    break",
            "                }",
            "                else",
            "                    _i++",
            "            }",
            "",
            "            var _qty = 1",
            "            if (can_stack && stack >= 1)",
            "                _qty = stack",
            "",
            "            if (_isGem && (!_isElixir))",
            "                _gemCount += _qty",
            "            else if (_isElixir)",
            "            {",
            "                if (_elixirId == \"\")",
            "                {",
            "                    _elixirId = idName",
            "                    _elixirCount = _qty",
            "                }",
            "                else if (_elixirId == idName)",
            "                    _elixirCount += _qty",
            "                else",
            "                    _otherCount += _qty",
            "            }",
            "            else",
            "                _otherCount += _qty",
            "        }",
            "    }",
            "",
            "    if (_gemCount != 1)",
            "        return \"\"",
            "    if (_otherCount != 0)",
            "        return \"\"",
            "    if (_elixirId == \"\" || _elixirCount != 4)",
            "        return \"\"",
            "",
            "    var _opened = scr_atr(\"recipesConsumsOpened\")",
            "    if (ds_list_find_index(_opened, _elixirId) >= 0)",
            "        return \"\"",
            "",
            "    return _elixirId",
            "}",
        });
    }

    static string BuildTryScript() =>
        string.Join("\n", new[]
        {
            "function scr_alch_study_try()",
            "{",
            "    var _id = scr_alch_study_elixir()",
            "    if (_id == \"\")",
            "        return false",
            "    if (!instance_exists(o_craftingMenu))",
            "        return false",
            "",
            "    var _owner = o_craftingMenu.id",
            "    var _needElixir = 4",
            "    var _needGem = 1",
            "    var _slots = []",
            "    with (o_inv_slot)",
            "    {",
            "        if (owner == _owner && (!is_pot))",
            "            array_push(_slots, id)",
            "    }",
            "",
            "    var _n = array_length(_slots)",
            "    for (var _i = 0; _i < _n; _i++)",
            "    {",
            "        var _slot = _slots[_i]",
            "        if (!instance_exists(_slot))",
            "        {",
            "            // Slot was already consumed earlier in this loop.",
            "        }",
            "        else if (_needElixir > 0 && _slot.idName == _id)",
            "        {",
            "            while (_needElixir > 0 && instance_exists(_slot))",
            "            {",
            "                if (_slot.can_stack && _slot.stack >= 2)",
            "                {",
            "                    _slot.stack--",
            "                    with (_slot)",
            "                        scr_item_stack_build()",
            "                    _needElixir--",
            "                }",
            "                else",
            "                {",
            "                    with (_slot)",
            "                        scr_item_destroy()",
            "                    _needElixir--",
            "                    break",
            "                }",
            "            }",
            "        }",
            "        else if (_needGem > 0 && ds_list_find_index(_slot.category, \"gem\") >= 0)",
            "        {",
            "            if (_slot.can_stack && _slot.stack >= 2)",
            "            {",
            "                _slot.stack--",
            "                with (_slot)",
            "                    scr_item_stack_build()",
            "            }",
            "            else",
            "            {",
            "                with (_slot)",
            "                    scr_item_destroy()",
            "            }",
            "            _needGem--",
            "        }",
            "    }",
            "",
            "    if (_needElixir > 0 || _needGem > 0)",
            "        return false",
            "",
            "    var _list = scr_atr(\"recipesConsumsOpened\")",
            "    if (ds_list_find_index(_list, _id) < 0)",
            "    {",
            "        ds_list_add(_list, _id)",
            "        var _size = ds_list_size(_list)",
            "        if (_size >= 15)",
            "            scr_steam_achivment(\"DIY_or_Die\")",
            "        scr_characterStatsUpdateAdd(\"learnedSchematics\", 1)",
            "    }",
            "",
            "    var _name = ds_map_find_value_ext(global.consum_name, _id, _id)",
            "    var _title = ds_list_find_value(global.other_hover, 79)",
            "    instance_destroy(o_notification)",
            "    with (scr_notificationCreate(12091, _title, _name, room_speed, false))",
            "        scr_guiLayoutOffsetUpdate(id, ((global.cameraWidth - sprite_width) / 2), (global.cameraHeight / 2 - 140))",
            "    scr_actionsLog(\"learnSchematic\", [scr_actionsLogGetName(o_player), [\"~lg~\", _name]])",
            "",
            "    with (o_player)",
            "    {",
            "        scr_guiAnimation(s_gui_anim_craft, 1, 1)",
            "        scr_audio_play_at(snd_skill_craft)",
            "    }",
            "",
            "    with (o_craftingMenu)",
            "    {",
            "        event_user(11)",
            "        event_user(13)",
            "        event_user(12)",
            "    }",
            "",
            "    return true",
            "}",
        });

    static string Escape(string id) => id.Replace("\\", "\\\\").Replace("\"", "\\\"");
}
