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

    static void InsertCategoryIntoSkillMenu()
    {
        // Insert after Mastery of Magic in the utility meta-column (same slot BW uses).
        Msl.LoadGML("gml_Object_o_skillmenu_Create_0")
            .MatchFrom("o_skill_category_mastery_of_magic")
            .ReplaceBy("o_skill_category_mastery_of_magic, o_skill_category_necromancy")
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
