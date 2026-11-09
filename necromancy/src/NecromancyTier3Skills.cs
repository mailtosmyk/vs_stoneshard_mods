using ModShardLauncher;
using ModShardLauncher.Mods;
using Necromancy.Skills;
using UndertaleModLib.Models;

namespace Necromancy;

/// <summary>
/// Creates GameMaker objects + skills_stats rows for tier-3 skills from necromancy.json.
/// </summary>
public static class NecromancyTier3Skills
{
    public static void Patch(ModFile modFiles)
    {
        var tier3 = NecromancySkills.ByTier(3).ToArray();
        if (tier3.Length == 0)
            return;

        CreateSharedCombatObjects(modFiles);

        foreach (var skill in tier3)
        {
            if (skill.Type == SkillType.Active && skill.Combat is null)
                throw new InvalidOperationException($"Tier-3 skill '{skill.Slug}' is missing combat config in necromancy.json.");

            CreateSkillObjects(skill, modFiles);
        }

        var actives = tier3.Where(s => s.Type == SkillType.Active).ToArray();
        if (actives.Length > 0)
            NecromancyTier1Skills.InjectSkillsStatsRows(actives);
    }

    static void CreateSkillObjects(NecromancySkill skill, ModFile modFiles)
    {
        if (skill.Type == SkillType.Passive)
        {
            var passiveIco = Msl.AddObject(
                name: skill.IconObjectName,
                spriteName: skill.SpriteName,
                parentName: "o_skill_passive",
                isVisible: true,
                isPersistent: false,
                isAwake: true,
                collisionShapeFlags: CollisionShapeFlags.Circle);

            var passiveEvents = new List<string>
            {
                $"gml_Object_{skill.IconObjectName}_Create_0.gml",
                $"gml_Object_{skill.IconObjectName}_Step_2.gml",
                $"gml_Object_{skill.IconObjectName}_Other_17.gml",
            };
            NecromancyTier1Skills.ApplyEventAndReplace(passiveIco, modFiles, passiveEvents.ToArray());
            return;
        }

        var ico = Msl.AddObject(
            name: skill.IconObjectName,
            spriteName: skill.SpriteName,
            parentName: "o_skill_ico",
            isVisible: true,
            isPersistent: false,
            isAwake: true,
            collisionShapeFlags: CollisionShapeFlags.Circle);

        var skillObj = Msl.AddObject(
            name: skill.SkillObjectName,
            spriteName: skill.SpriteName,
            parentName: "o_skill",
            isVisible: true,
            isPersistent: false,
            isAwake: true,
            collisionShapeFlags: CollisionShapeFlags.Circle);

        var birth = Msl.AddObject(
            name: skill.BirthObjectName,
            spriteName: "s_spell_darkball_birth",
            parentName: "o_spellbirth",
            isVisible: true,
            isPersistent: false,
            isAwake: true,
            collisionShapeFlags: CollisionShapeFlags.Circle);

        NecromancyTier1Skills.ApplyEventAndReplace(ico, modFiles,
            $"gml_Object_{skill.IconObjectName}_Create_0.gml",
            $"gml_Object_{skill.IconObjectName}_Step_2.gml");

        var skillEvents = new List<string>
        {
            $"gml_Object_{skill.SkillObjectName}_Create_0.gml",
            $"gml_Object_{skill.SkillObjectName}_Other_17.gml",
        };
        if (skill.Slug is "corpse_burst" or "death_touch")
            skillEvents.Add($"gml_Object_{skill.SkillObjectName}_Other_19.gml");

        NecromancyTier1Skills.ApplyEventAndReplace(skillObj, modFiles, skillEvents.ToArray());

        NecromancyTier1Skills.ApplyEventAndReplace(birth, modFiles,
            $"gml_Object_{skill.BirthObjectName}_Create_0.gml",
            $"gml_Object_{skill.BirthObjectName}_Other_10.gml");
    }

    static void CreateSharedCombatObjects(ModFile modFiles)
    {
        var deathImpact = Msl.AddObject(
            name: "o_nm_death_touch_impact",
            spriteName: "s_death_touch",
            parentName: "o_magic_pillar",
            isVisible: true,
            isPersistent: false,
            isAwake: true,
            collisionShapeFlags: CollisionShapeFlags.Circle);

        var burstImpact = Msl.AddObject(
            name: "o_nm_corpse_burst_impact",
            spriteName: "s_corpse_burst",
            parentName: "o_magic_pillar",
            isVisible: true,
            isPersistent: false,
            isAwake: true,
            collisionShapeFlags: CollisionShapeFlags.Circle);

        var sepulchreImpact = Msl.AddObject(
            name: "o_nm_black_sepulchre_impact",
            spriteName: "s_black_sepulchre",
            parentName: "o_magic_pillar",
            isVisible: true,
            isPersistent: false,
            isAwake: true,
            collisionShapeFlags: CollisionShapeFlags.Circle);

        var unbind = Msl.AddObject(
            name: "o_b_nm_unbind",
            spriteName: "s_mastery_of_binding",
            parentName: "o_physical_buff",
            isVisible: true,
            isPersistent: false,
            isAwake: true,
            collisionShapeFlags: CollisionShapeFlags.Circle);

        NecromancyTier1Skills.ApplyEventAndReplace(deathImpact, modFiles,
            "gml_Object_o_nm_death_touch_impact_Create_0.gml",
            "gml_Object_o_nm_death_touch_impact_Other_7.gml");

        NecromancyTier1Skills.ApplyEventAndReplace(burstImpact, modFiles,
            "gml_Object_o_nm_corpse_burst_impact_Create_0.gml",
            "gml_Object_o_nm_corpse_burst_impact_Other_7.gml");

        NecromancyTier1Skills.ApplyEventAndReplace(sepulchreImpact, modFiles,
            "gml_Object_o_nm_black_sepulchre_impact_Create_0.gml",
            "gml_Object_o_nm_black_sepulchre_impact_Other_7.gml");

        NecromancyTier1Skills.ApplyEventAndReplace(unbind, modFiles,
            "gml_Object_o_b_nm_unbind_Create_0.gml",
            "gml_Object_o_b_nm_unbind_Other_10.gml",
            "gml_Object_o_b_nm_unbind_Mouse_53.gml");
    }
}
