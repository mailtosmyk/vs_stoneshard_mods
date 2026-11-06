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

    public static void Patch(ModFile modFiles)
    {
        var tier1 = NecromancySkills.ByTier(1).ToArray();
        if (tier1.Length == 0)
            throw new InvalidOperationException("necromancy.json has no tier 1 skills.");

        EnsureSpriteOrigins(tier1);

        var category = Msl.AddObject(
            name: CategoryObjectName,
            spriteName: "",
            parentName: "o_sklill_category_magic",
            isVisible: true,
            isPersistent: false,
            isAwake: true,
            collisionShapeFlags: CollisionShapeFlags.Circle);

        category.ApplyEvent(
            modFiles,
            new MslEvent("gml_Object_o_skill_category_necromancy_Create_0.gml", EventType.Create, 0),
            new MslEvent("gml_Object_o_skill_category_necromancy_Other_24.gml", EventType.Other, 24));

        InsertCategoryIntoSkillMenu();
        InsertTierGlobals(tier1);
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

    // Current Stoneshard decompiles this array as numeric asset IDs, so matching
    // individual o_skill_category_* names fails. Replace the whole assignment
    // (same approach as BW-s-Necromancy) using stable object names.
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

        switch (NecromancySkills.SchoolTarget.Trim().ToLowerInvariant())
        {
            case "sorcery":
            case "magic":
                sorcery += ", " + CategoryObjectName;
                break;

            case "utility":
            case "survival":
            case "mastery":
                utility = utility.Replace(
                    "o_skill_category_mastery_of_magic",
                    "o_skill_category_mastery_of_magic, " + CategoryObjectName);
                break;

            default:
                throw new InvalidOperationException(
                    $"Unknown necromancy.json target '{NecromancySkills.SchoolTarget}'. Use \"Sorcery\" or \"Utility\".");
        }

        var replacement =
            "var _metaCategoriesArray = [[" + weapons + "], [" + utility + "], [" + sorcery + "]]";

        Msl.LoadGML("gml_Object_o_skillmenu_Create_0")
            .MatchFrom("var _metaCategoriesArray = ")
            .ReplaceBy(replacement)
            .Save();
    }

    static void InsertTierGlobals(NecromancySkill[] tier1)
    {
        var icons = string.Join(", ", tier1.Select(s => s.IconObjectName));
        var gml = "\n" + Tier1Global + " = [\"" + NecromancySkills.SchoolName + "\", " + icons + "]\n";

        Msl.LoadGML("gml_GlobalScript_scr_skill_tier_init")
            .MatchFrom("}")
            .InsertAbove(gml)
            .Save();
    }
}
