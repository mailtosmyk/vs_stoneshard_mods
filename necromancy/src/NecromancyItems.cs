using ModShardLauncher;
using ModShardLauncher.Mods;
using Necromancy.Skills;
using UndertaleModLib.Models;

namespace Necromancy;

/// <summary>
/// Registers enabled treatise books / loot from necromancy.json <c>items</c>.
/// </summary>
public static class NecromancyItems
{
    public static void Patch(ModFile modFiles)
    {
        var books = NecromancySkills.EnabledBooks.ToArray();
        if (books.Length == 0)
            return;

        foreach (var book in books)
            CreateBook(book, modFiles);

        InjectLoot(books);
    }

    static void CreateBook(NecromancyItem book, ModFile modFiles)
    {
        var inv = Msl.AddObject(
            name: book.InvObjectName,
            spriteName: book.InvSpriteName,
            parentName: "o_inv_magic_treatise",
            isVisible: true,
            isPersistent: true,
            isAwake: true,
            collisionShapeFlags: CollisionShapeFlags.Circle);

        var loot = Msl.AddObject(
            name: book.LootObjectName,
            spriteName: book.LootSpriteName,
            parentName: "o_consument_loot",
            isVisible: true,
            isPersistent: false,
            isAwake: true,
            collisionShapeFlags: CollisionShapeFlags.Circle);

        NecromancyTier1Skills.ApplyEventAndReplace(inv, modFiles,
            $"gml_Object_{book.InvObjectName}_Create_0.gml",
            $"gml_Object_{book.InvObjectName}_Other_10.gml",
            $"gml_Object_{book.InvObjectName}_Draw_73.gml",
            $"gml_Object_{book.InvObjectName}_PreCreate_0.gml");

        NecromancyTier1Skills.ApplyEventAndReplace(loot, modFiles,
            $"gml_Object_{book.LootObjectName}_Create_0.gml",
            $"gml_Object_{book.LootObjectName}_PreCreate_0.gml");
    }

    /// <summary>
    /// Witch-house container sometimes drops a geomancy treatise — chance for necromancy books too.
    /// </summary>
    static void InjectLoot(NecromancyItem[] books)
    {
        var t2 = books.FirstOrDefault(b => b.Tier == 2);
        var t3 = books.FirstOrDefault(b => b.Tier == 3);
        if (t2 is null && t3 is null)
            return;

        var lines = new List<string>();
        if (t2 is not null)
            lines.Add($"if scr_chance_value(50) {{scr_inventory_add_item({t2.InvObjectName})}}");
        if (t3 is not null)
        {
            var prefix = t2 is not null ? "else if" : "if";
            lines.Add($"{prefix} scr_chance_value(75) {{scr_inventory_add_item({t3.InvObjectName})}}");
        }

        try
        {
            Msl.LoadGML("gml_Object_o_whitchousecontainer02_Other_13")
                .MatchFrom("scr_inventory_add_item(o_inv_treatise_geo2)")
                .InsertBelow(string.Join("\n", lines))
                .Save();
        }
        catch
        {
            // Container script may differ by game version; books still exist as items.
        }
    }
}
