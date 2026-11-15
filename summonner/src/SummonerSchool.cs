using ModShardLauncher;
using ModShardLauncher.Mods;
using Summoner.Skills;
using UndertaleModLib.Models;

namespace Summoner;

public static class SummonerSchool
{
    public const string CategoryObjectName = "o_skill_category_summoner";
    public const string Tier1Global = "global.summoner_tier1";
    public const string Tier2Global = "global.summoner_tier2";
    public const string Tier3Global = "global.summoner_tier3";
    public const string Tier4Global = "global.summoner_tier4";

    public static void Patch(ModFile modFiles)
    {
        var tier1 = SummonerSkills.ByTier(1).ToArray();
        var tier2 = SummonerSkills.ByTier(2).ToArray();
        var tier3 = SummonerSkills.ByTier(3).ToArray();
        var tier4 = SummonerSkills.ByTier(4).ToArray();
        if (tier1.Length == 0)
            throw new InvalidOperationException("summonner.json has no tier 1 skills.");

        EnsureSpriteOrigins(tier1.Concat(tier2).Concat(tier3).Concat(tier4));

        var category = Msl.AddObject(
            name: CategoryObjectName,
            spriteName: "",
            parentName: "o_sklill_category_magic",
            isVisible: true,
            isPersistent: false,
            isAwake: true,
            collisionShapeFlags: CollisionShapeFlags.Circle);

        try
        {
            category.ApplyEvent(
                modFiles,
                new MslEvent("gml_Object_o_skill_category_summoner_Create_0.gml", EventType.Create, 0),
                new MslEvent("gml_Object_o_skill_category_summoner_Other_24.gml", EventType.Other, 24));
        }
        catch (ArgumentException)
        {
        }

        WriteCategoryEvents(tier1, tier2, tier3, tier4);
        InsertCategoryIntoSkillMenu();
        InsertTierGlobals(tier1, tier2, tier3, tier4);
        UnlockTier1OnSkillPanelInit();
    }

    static void WriteCategoryEvents(
        SummonerSkill[] tier1, SummonerSkill[] tier2, SummonerSkill[] tier3, SummonerSkill[] tier4)
    {
        var icons = string.Join(", ", tier1.Concat(tier2).Concat(tier3).Concat(tier4).Select(s => s.IconObjectName));
        WriteGml(
            "gml_Object_o_skill_category_summoner_Create_0",
            "event_inherited()\n" +
            "text = \"" + SummonerSkills.SchoolName + "\"\n" +
            "skill = [" + icons + "]\n" +
            "branch_sprite = s_summoner_tree\n" +
            "is_execute = false\n");

        var layout = new System.Text.StringBuilder("event_inherited()\n");
        void Place(SummonerSkill[] skills, int[] xs, int y)
        {
            for (var i = 0; i < skills.Length; i++)
            {
                var x = i < xs.Length ? xs[i] : xs[^1] + (i - xs.Length + 1) * 38;
                layout.Append("scr_guiLayoutOffsetUpdate(")
                    .Append(skills[i].IconObjectName).Append(", ").Append(x)
                    .Append(", ").Append(y).Append(", 1)\n");
            }
        }

        Place(tier1, new[] { 24, 81, 138 }, 55);
        Place(tier2, new[] { 24, 81, 138 }, 118);
        Place(tier3, new[] { 18, 56, 94, 132 }, 172);
        Place(tier4, new[] { 24, 81, 138 }, 220);
        WriteGml("gml_Object_o_skill_category_summoner_Other_24", layout.ToString());
    }

    static void WriteGml(string gmlName, string source)
    {
        var lines = source.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
        Msl.LoadGML(gmlName).Apply(_ => lines).Save();
    }

    static void EnsureSpriteOrigins(IEnumerable<SummonerSkill> skills)
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
            }
        }

        try
        {
            var tree = Msl.GetSprite("s_summoner_tree");
            tree.OriginX = 0;
            tree.OriginY = 0;
        }
        catch
        {
        }
    }

    /// <summary>
    /// Inject into the existing <c>_metaCategoriesArray</c> instead of replacing it.
    /// ReplaceBy wipes sibling schools (Necromancy) because UTMT decompiles the array as
    /// numeric IDs and the second mod's full rewrite drops the first mod's category.
    /// Columns: 0 = weapons, 1 = utility, 2 = sorcery.
    /// </summary>
    static void InsertCategoryIntoSkillMenu()
    {
        var column = SummonerSkills.SchoolTarget.Trim().ToLowerInvariant() switch
        {
            "sorcery" or "magic" => 2,
            "utility" or "survival" or "warfare" or "mastery" => 1,
            _ => throw new InvalidOperationException(
                $"Unknown summonner.json target '{SummonerSkills.SchoolTarget}'.")
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
        SummonerSkill[] tier1, SummonerSkill[] tier2, SummonerSkill[] tier3, SummonerSkill[] tier4)
    {
        var gml = "\n" + Tier1Global + " = [\"" + SummonerSkills.SchoolName + "\", " +
                  string.Join(", ", tier1.Select(s => s.IconObjectName)) + "]\n";
        if (tier2.Length > 0)
            gml += Tier2Global + " = [\"" + SummonerSkills.SchoolName + "\", " +
                   string.Join(", ", tier2.Select(s => s.IconObjectName)) + "]\n";
        if (tier3.Length > 0)
            gml += Tier3Global + " = [\"" + SummonerSkills.SchoolName + "\", " +
                   string.Join(", ", tier3.Select(s => s.IconObjectName)) + "]\n";
        if (tier4.Length > 0)
            gml += Tier4Global + " = [\"" + SummonerSkills.SchoolName + "\", " +
                   string.Join(", ", tier4.Select(s => s.IconObjectName)) + "]\n";

        Msl.LoadGML("gml_GlobalScript_scr_skill_tier_init")
            .MatchFrom("}")
            .InsertAbove(gml)
            .Save();
    }

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
