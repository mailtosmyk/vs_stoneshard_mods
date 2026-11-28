using ModShardLauncher;

namespace Alchemy;

/// <summary>
/// Ensures Alchemy Base Recipes stay in <c>recipesConsumsOpened</c> after load.
/// Character init only snapshots Base Recipes at create time, so saves made before
/// blank scroll / flask (etc.) were marked base would otherwise never unlock them.
/// </summary>
internal static class AlchemyKnownRecipes
{
    public static void Patch()
    {
        var ids = AlchemyCatalog.BaseRecipeIds;
        if (ids.Count == 0)
            return;

        var arrayLiteral = string.Join(", ", ids.Select(id => $"\"{id}\""));
        var block = string.Join(
            "\n",
            "var _alch_opened = scr_atr(\"recipesConsumsOpened\")",
            "if (!is_undefined(_alch_opened) && ds_exists(_alch_opened, 2))",
            "{",
            $"    var _alch_base = [{arrayLiteral}]",
            "    var _alch_n = array_length(_alch_base)",
            "    for (var _alch_i = 0; _alch_i < _alch_n; _alch_i++)",
            "    {",
            "        var _alch_id = _alch_base[_alch_i]",
            "        if (ds_list_find_index(_alch_opened, _alch_id) < 0)",
            "            ds_list_add(_alch_opened, _alch_id)",
            "    }",
            "}");

        try
        {
            // Same Create path as new game and save load — characterDataMap is ready.
            Msl.LoadGML("gml_Object_o_player_Create_0")
                .MatchFrom("event_inherited()")
                .InsertBelow(block)
                .Save();
        }
        catch
        {
            // Player create script may differ by game version.
        }
    }
}
