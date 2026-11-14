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
            "text = \"Summoner\"\n" +
            "skill = [" + icons + "]\n" +
            "branch_sprite = s_summoner_tree\n" +
            "is_execute = false\n" +
            "owner = gml_Script_scr_GetMobParametr\n");

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

    const string WeaponsColumn =
        "o_skill_category_sword, o_skill_category_axe, o_skill_category_mace, o_skill_category_dagger, " +
        "o_skill_category_greatsword, o_skill_category_greataxe, o_skill_category_greatmauls, " +
        "o_skill_category_polearms, o_skill_category_bows, o_skill_category_shields, " +
        "o_skill_category_staves, o_skill_category_wands";

    const string UtilityColumn =
        "o_skill_category_basic_skills, o_skill_category_dual_wielding, o_skill_category_combat, " +
        "o_skill_category_athletics, o_skill_category_basic_armor, o_skill_category_survival, " +
        "o_skill_category_mastery_of_magic, o_skill_category_alchemy, o_skill_category_sabotage";

    const string SorceryColumn =
        "o_skill_category_pyromancy, o_skill_category_geomancy, o_skill_category_electromancy, " +
        "o_skill_category_arcanistics, o_skill_category_venomancy, o_skill_category_cryomancy, " +
        "o_skill_category_astromancy, o_skill_category_psymancy";

    static void InsertCategoryIntoSkillMenu()
    {
        var weapons = WeaponsColumn;
        var utility = UtilityColumn;
        var sorcery = SorceryColumn;
        switch (SummonerSkills.SchoolTarget.Trim().ToLowerInvariant())
        {
            case "sorcery":
            case "magic":
                sorcery += ", " + CategoryObjectName;
                break;
            case "utility":
            case "survival":
            case "warfare":
            case "mastery":
                utility = utility.Replace(
                    "o_skill_category_survival",
                    "o_skill_category_survival, " + CategoryObjectName);
                break;
            default:
                throw new InvalidOperationException(
                    $"Unknown summonner.json target '{SummonerSkills.SchoolTarget}'.");
        }

        var replacement =
            "var _metaCategoriesArray = [[" + weapons + "], [" + utility + "], [" + sorcery + "]]";
        Msl.LoadGML("gml_Object_o_skillmenu_Create_0")
            .MatchFrom("var _metaCategoriesArray = ")
            .ReplaceBy(replacement)
            .Save();
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
            .InsertBelow("scr_skill_open_from_array1d(" + Tier1Global + ")")
            .Save();
    }
}
