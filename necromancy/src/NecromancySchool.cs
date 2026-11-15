using ModShardLauncher;
using ModShardLauncher.Mods;
using Necromancy.Skills;
using UndertaleModLib.Models;

namespace Necromancy;

/// <summary>
/// Registers the Necromancy skill category, tier unlock arrays, and menu layout
/// for skills defined in necromancy.json.
/// </summary>
public static class NecromancySchool
{
    public const string CategoryObjectName = "o_skill_category_necromancy";
    public const string Tier1Global = "global.necromancy_tier1";
    public const string Tier2Global = "global.necromancy_tier2";
    public const string Tier3Global = "global.necromancy_tier3";
    public const string Tier4Global = "global.necromancy_tier4";

    public static void Patch(ModFile modFiles)
    {
        var tier1 = NecromancySkills.ByTier(1).ToArray();
        var tier2 = NecromancySkills.ByTier(2).ToArray();
        var tier3 = NecromancySkills.ByTier(3).ToArray();
        var tier4 = NecromancySkills.ByTier(4).ToArray();
        if (tier1.Length == 0)
            throw new InvalidOperationException("necromancy.json has no tier 1 skills.");

        EnsureSpriteOrigins(tier1.Concat(tier2).Concat(tier3).Concat(tier4));

        var category = Msl.AddObject(
            name: CategoryObjectName,
            spriteName: "",
            parentName: "o_sklill_category_magic",
            isVisible: true,
            isPersistent: false,
            isAwake: true,
            collisionShapeFlags: CollisionShapeFlags.Circle);

        // ApplyEvent skips existing subtypes on re-patch; always overwrite bodies from JSON
        // so removed skills cannot leave stale icon refs in Create/Other_24.
        try
        {
            category.ApplyEvent(
                modFiles,
                new MslEvent("gml_Object_o_skill_category_necromancy_Create_0.gml", EventType.Create, 0),
                new MslEvent("gml_Object_o_skill_category_necromancy_Other_24.gml", EventType.Other, 24));
        }
        catch (ArgumentException)
        {
            // Events already present — ForceReplace below refreshes them.
        }

        WriteCategoryEvents(tier1, tier2, tier3, tier4);

        InsertCategoryIntoSkillMenu();
        InsertTierGlobals(tier1, tier2, tier3, tier4);
        UnlockTier1OnSkillPanelInit();
    }

    /// <summary>
    /// Icon strip + layout derived from necromancy.json (not stale Codes/*.gml).
    /// Coordinates match s_nmancy_reworked lock centers (162×257):
    /// Tier 1 at Y=55, Tier 2 at Y=118, Tier 3 at Y=172, Tier 4 at Y=220.
    /// </summary>
    static void WriteCategoryEvents(
        NecromancySkill[] tier1,
        NecromancySkill[] tier2,
        NecromancySkill[] tier3,
        NecromancySkill[] tier4)
    {
        var icons = string.Join(", ", tier1.Concat(tier2).Concat(tier3).Concat(tier4).Select(s => s.IconObjectName));
        WriteGml(
            "gml_Object_o_skill_category_necromancy_Create_0",
            "event_inherited()\n" +
            "text = \"Necromancy\"\n" +
            "skill = [" + icons + "]\n" +
            "branch_sprite = s_nmancy_reworked\n" +
            "is_execute = false\n");

        // Branches:
        //   Left   Dark Bolt → Essence Leech → Death Touch → Corpse Burst
        //   Center Bone Ward → Blessing/Blight → Black Sepulchre → Immortality/Blooming
        //   Right  Summon Skeleton → Resurrection → Mastery → Wraith
        // Centers: T1/T3 24/81/138; T2/T4 24/62/100/138.
        var layout = new System.Text.StringBuilder("event_inherited()\n");
        var t1Xs = new[] { 24, 81, 138 };
        for (var i = 0; i < tier1.Length; i++)
        {
            var x = i < t1Xs.Length ? t1Xs[i] : t1Xs[^1] + (i - t1Xs.Length + 1) * 48;
            layout.Append("scr_guiLayoutOffsetUpdate(")
                .Append(tier1[i].IconObjectName)
                .Append(", ")
                .Append(x)
                .Append(", 55, 1)\n");
        }

        var t2X = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["essence_leech"] = 24,
            ["deaths_blessing"] = 62,
            ["grave_blight"] = 100,
            ["resurrection"] = 138,
        };
        var fallbackXs = new[] { 24, 62, 100, 138 };
        for (var i = 0; i < tier2.Length; i++)
        {
            var x = t2X.TryGetValue(tier2[i].Slug, out var mapped)
                ? mapped
                : (i < fallbackXs.Length ? fallbackXs[i] : fallbackXs[^1] + (i - fallbackXs.Length + 1) * 38);
            layout.Append("scr_guiLayoutOffsetUpdate(")
                .Append(tier2[i].IconObjectName)
                .Append(", ")
                .Append(x)
                .Append(", 118, 1)\n");
        }

        var t3X = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["death_touch"] = 24,
            ["black_sepulchre"] = 81,
            ["mastery_of_binding"] = 138,
        };
        for (var i = 0; i < tier3.Length; i++)
        {
            var x = t3X.TryGetValue(tier3[i].Slug, out var mapped)
                ? mapped
                : (i < fallbackXs.Length ? fallbackXs[i] : fallbackXs[^1] + (i - fallbackXs.Length + 1) * 38);
            layout.Append("scr_guiLayoutOffsetUpdate(")
                .Append(tier3[i].IconObjectName)
                .Append(", ")
                .Append(x)
                .Append(", 172, 1)\n");
        }

        var t4X = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["corpse_burst"] = 24,
            ["immortality"] = 62,
            ["blooming_madness"] = 100,
            ["wraith_summoning"] = 138,
        };
        for (var i = 0; i < tier4.Length; i++)
        {
            var x = t4X.TryGetValue(tier4[i].Slug, out var mapped)
                ? mapped
                : (i < fallbackXs.Length ? fallbackXs[i] : fallbackXs[^1] + (i - fallbackXs.Length + 1) * 38);
            layout.Append("scr_guiLayoutOffsetUpdate(")
                .Append(tier4[i].IconObjectName)
                .Append(", ")
                .Append(x)
                .Append(", 220, 1)\n");
        }

        WriteGml("gml_Object_o_skill_category_necromancy_Other_24", layout.ToString());
    }

    static void WriteGml(string gmlName, string source)
    {
        var lines = source.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
        Msl.LoadGML(gmlName)
            .Apply(_ => lines)
            .Save();
    }

    static void EnsureSpriteOrigins(IEnumerable<NecromancySkill> skills)
    {
        foreach (var skill in skills)
        {
            try
            {
                var sprite = Msl.GetSprite(skill.SpriteName);
                sprite.OriginX = 12;
                sprite.OriginY = 12;
            }
            catch
            {
                // Sprite may be missing until art is added; skill still registers.
            }
        }
    }

    /// <summary>
    /// Inject into the existing <c>_metaCategoriesArray</c> instead of replacing it.
    /// Full ReplaceBy drops sibling schools (Summoner): UTMT decompiles IDs as numbers, so
    /// the second mod cannot see the first mod's named category and rewrites vanilla-only.
    /// Columns: 0 = weapons, 1 = utility, 2 = sorcery.
    /// </summary>
    static void InsertCategoryIntoSkillMenu()
    {
        var column = NecromancySkills.SchoolTarget.Trim().ToLowerInvariant() switch
        {
            "sorcery" or "magic" => 2,
            "utility" or "survival" or "mastery" => 1,
            _ => throw new InvalidOperationException(
                $"Unknown necromancy.json target '{NecromancySkills.SchoolTarget}'. Use \"Sorcery\" or \"Utility\".")
        };

        Msl.LoadGML("gml_Object_o_skillmenu_Create_0")
            .Apply(lines => InjectMetaCategory(lines, CategoryObjectName, column))
            .Save();
    }

    static IEnumerable<string> InjectMetaCategory(
        IEnumerable<string> lines, string categoryObjectName, int column)
    {
        const string marker = "var _metaCategoriesArray = ";
        foreach (var line in lines)
        {
            if (!line.Contains(marker, StringComparison.Ordinal) ||
                line.Contains(categoryObjectName, StringComparison.Ordinal))
            {
                yield return line;
                continue;
            }

            yield return InjectIntoMetaCategoriesArray(line, categoryObjectName, column);
        }
    }

    static string InjectIntoMetaCategoriesArray(string line, string categoryObjectName, int column)
    {
        var start = line.IndexOf("[[", StringComparison.Ordinal);
        if (start < 0)
            return line;

        var i = start + 1; // '[' of column 0
        for (var col = 0; col <= column; col++)
        {
            if (i < 0 || i >= line.Length)
                return line;
            if (line[i] != '[')
                i = line.IndexOf('[', i);
            if (i < 0)
                return line;

            var depth = 0;
            for (var j = i; j < line.Length; j++)
            {
                if (line[j] == '[')
                    depth++;
                else if (line[j] == ']')
                {
                    depth--;
                    if (depth != 0)
                        continue;
                    if (col == column)
                        return line[..j] + ", " + categoryObjectName + line[j..];
                    i = j + 1;
                    while (i < line.Length && (line[i] == ',' || char.IsWhiteSpace(line[i])))
                        i++;
                    break;
                }
            }
        }

        return line;
    }

    static void InsertTierGlobals(
        NecromancySkill[] tier1,
        NecromancySkill[] tier2,
        NecromancySkill[] tier3,
        NecromancySkill[] tier4)
    {
        var t1Icons = string.Join(", ", tier1.Select(s => s.IconObjectName));
        var gml = "\n" + Tier1Global + " = [\"" + NecromancySkills.SchoolName + "\", " + t1Icons + "]\n";
        if (tier2.Length > 0)
        {
            var t2Icons = string.Join(", ", tier2.Select(s => s.IconObjectName));
            gml += Tier2Global + " = [\"" + NecromancySkills.SchoolName + "\", " + t2Icons + "]\n";
        }

        if (tier3.Length > 0)
        {
            var t3Icons = string.Join(", ", tier3.Select(s => s.IconObjectName));
            gml += Tier3Global + " = [\"" + NecromancySkills.SchoolName + "\", " + t3Icons + "]\n";
        }

        if (tier4.Length > 0)
        {
            var t4Icons = string.Join(", ", tier4.Select(s => s.IconObjectName));
            gml += Tier4Global + " = [\"" + NecromancySkills.SchoolName + "\", " + t4Icons + "]\n";
        }

        Msl.LoadGML("gml_GlobalScript_scr_skill_tier_init")
            .MatchFrom("}")
            .InsertAbove(gml)
            .Save();
    }

    /// <summary>
    /// o_skill_fast_panel Alarm_0 locks every skill in every category, then unlocks
    /// class starting tiers via scr_skill_open_from_array1d. Without that unlock,
    /// necromancy tier-1 stays locked and shows "Read the corresponding treatise...".
    /// Tier 2/3 stay gated by attributes + prerequisite skill Step_2 checks.
    /// </summary>
    static void UnlockTier1OnSkillPanelInit()
    {
        Msl.LoadGML("gml_Object_o_skill_fast_panel_Alarm_0")
            .MatchFrom("scr_skill_open_from_array1d(global.survival_tier1)")
            .InsertBelow(
                "scr_skill_open_from_array1d(" + Tier1Global + ")\n" +
                "with (" + CategoryObjectName + ")\n" +
                "    is_lock = false")
            .Save();
    }
}
