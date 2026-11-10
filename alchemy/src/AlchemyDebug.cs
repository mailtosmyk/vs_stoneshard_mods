using ModShardLauncher;

namespace Alchemy;

/// <summary>
/// When <c>debug: true</c> is set on an elixir or recipe in <c>ingredients.json</c>,
/// grants the matching inventory object on player create.
/// </summary>
internal static class AlchemyDebug
{
    public static void Patch()
    {
        var lines = new List<string>();

        foreach (var id in AlchemyCatalog.DebugElixirIds)
            lines.Add($"scr_inventory_add_item(o_inv_{id})");

        foreach (var id in AlchemyCatalog.DebugRecipeIds)
            lines.Add($"scr_inventory_add_item(o_inv_recipe_{id})");

        if (lines.Count == 0)
            return;

        var block = string.Join(
            "\n",
            "if (instance_exists(o_inventory))",
            "{",
            "    with (o_inventory)",
            "    {",
            string.Join("\n", lines.Select(l => "        " + l)),
            "    }",
            "}");

        try
        {
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
