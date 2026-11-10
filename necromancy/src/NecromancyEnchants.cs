using ModShardLauncher;
using ModShardLauncher.Mods;
using Necromancy.Skills;

namespace Necromancy;

/// <summary>
/// Registers necromancy summon enchants into the vanilla gear affix pool and UI.
/// </summary>
public static class NecromancyEnchants
{
    public static void Patch(ModFile modFiles)
    {
        // Helpers are always registered so summon/servant GML can call them safely.
        RegisterHelper(modFiles);

        var enchants = NecromancySkills.EnabledEnchants.ToArray();
        if (enchants.Length == 0)
            return;

        InjectAffixPool(enchants);
        InjectAttributeLocalization(enchants);
        PatchPercentSuffix(enchants);
    }

    static void RegisterHelper(ModFile modFiles)
    {
        Msl.AddFunction(modFiles.GetCode("gml_GlobalScript_scr_nm_summon_enchant.gml"), "scr_nm_summon_enchant");
        Msl.AddFunction(modFiles.GetCode("gml_GlobalScript_scr_nm_apply_summon_enchants.gml"), "scr_nm_apply_summon_enchants");
    }

    static void InjectAffixPool(NecromancyItem[] enchants)
    {
        // Insert after Magic_Power so summon affixes sit with the rare magic cluster.
        var lines = string.Join(
            "\n",
            enchants.Select(e =>
                $"scr_weapon_prefix_generation(\"{e.Stat}\", \"{SanitizeSlot(e.Slot)}\", {ClampRarity(e.Rarity)}, {e.Value})"));

        Msl.LoadGML("gml_GlobalScript_scr_weapon_generation")
            .MatchFrom("scr_weapon_prefix_generation(\"Magic_Power\", \"Weapon\", 2, 4)")
            .InsertBelow(lines)
            .Save();
    }

    static void InjectAttributeLocalization(NecromancyItem[] enchants)
    {
        // table_attributes is one giant return array — splice CSV rows after Magic_Power.
        var insert = string.Concat(enchants.Select(e => $" {AttributeCsvRow(e).TrimEnd(',', '\n')},"));
        const string marker = "\"Magic_Power;Сила магии;Magic Power;";

        Msl.LoadGML("gml_GlobalScript_table_attributes")
            .Apply(lines => lines.Select(line =>
            {
                if (!line.Contains(marker, StringComparison.Ordinal))
                    return line;

                // Insert after the full Magic_Power quoted entry.
                var start = line.IndexOf("\"Magic_Power;", StringComparison.Ordinal);
                if (start < 0)
                    return line;
                var end = line.IndexOf("\",", start, StringComparison.Ordinal);
                if (end < 0)
                    return line;
                return line.Insert(end + 2, insert);
            }))
            .Save();
    }

    static void PatchPercentSuffix(NecromancyItem[] enchants)
    {
        // Ensure Char tooltips show "+N%" for summon enchants.
        var checks = string.Join(
            " || ",
            enchants.Select(e => $"argument0 == \"{e.Stat}\""));

        Msl.LoadGML("gml_GlobalScript_scr_atr_percent")
            .MatchFrom("return (__is_undefined(ds_map_find_value(global.attribute_percent, argument0)) ? \"\" : \"%\")")
            .InsertAbove(
                $"    if ({checks})\n        return \"%\"")
            .Save();
    }

    static string AttributeCsvRow(NecromancyItem e)
    {
        // Mirror Magic_Power column order; fill non-EN/RU with English for now.
        var en = EscapeCsv(e.Name);
        var ru = EscapeCsv(e.Name switch
        {
            "Summon Health" => "Здоровье призванных",
            "Summon Damage" => "Урон призванных",
            _ => e.Name
        });
        return $"\"{e.Stat};{ru};{en};{en};{en};{en};{en};{en};{en};{en};{en};{en};{en};\",";
    }

    static string EscapeCsv(string value) =>
        value.Replace("\"", "\\\"", StringComparison.Ordinal);

    static string SanitizeSlot(string slot) =>
        slot.Trim().ToLowerInvariant() switch
        {
            "weapon" => "Weapon",
            "armor" => "Armor",
            _ => "all"
        };

    static int ClampRarity(int rarity) => rarity <= 1 ? 1 : 2;
}
