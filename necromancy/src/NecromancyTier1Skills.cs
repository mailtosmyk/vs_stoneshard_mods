using ModShardLauncher;
using ModShardLauncher.Mods;
using Necromancy.Skills;
using UndertaleModLib.Models;

namespace Necromancy;

/// <summary>
/// Creates GameMaker objects + skills_stats rows for tier-1 skills from necromancy.json.
/// </summary>
public static class NecromancyTier1Skills
{
    public static void Patch(ModFile modFiles)
    {
        foreach (var skill in NecromancySkills.ByTier(1))
        {
            if (skill.Combat is null)
                throw new InvalidOperationException($"Tier-1 skill '{skill.Slug}' is missing combat config in necromancy.json.");

            CreateSkillObjects(skill, modFiles);
            InjectSkillsStats(skill);
        }

        CreateSharedCombatObjects(modFiles);
    }

    static void CreateSkillObjects(NecromancySkill skill, ModFile modFiles)
    {
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

        var birthSprite = skill.Combat!.Kind switch
        {
            SkillCombatKind.Missile => "s_spell_darkball_birth",
            SkillCombatKind.Debuff => "s_spell_darkball_birth",
            SkillCombatKind.SelfBuff => "s_spell_darkball_birth",
            SkillCombatKind.Summon => "s_spell_darkball_birth",
            _ => ""
        };

        var birth = Msl.AddObject(
            name: skill.BirthObjectName,
            spriteName: birthSprite,
            parentName: "o_spellbirth",
            isVisible: true,
            isPersistent: false,
            isAwake: true,
            collisionShapeFlags: CollisionShapeFlags.Circle);

        ico.ApplyEvent(modFiles,
            new MslEvent($"gml_Object_{skill.IconObjectName}_Create_0.gml", EventType.Create, 0));

        skillObj.ApplyEvent(modFiles,
            new MslEvent($"gml_Object_{skill.SkillObjectName}_Create_0.gml", EventType.Create, 0),
            new MslEvent($"gml_Object_{skill.SkillObjectName}_Other_17.gml", EventType.Other, 17));

        birth.ApplyEvent(modFiles,
            new MslEvent($"gml_Object_{skill.BirthObjectName}_Create_0.gml", EventType.Create, 0),
            new MslEvent($"gml_Object_{skill.BirthObjectName}_Other_10.gml", EventType.Other, 10));

        if (skill.Combat.IsMissile)
        {
            birth.ApplyEvent(modFiles,
                new MslEvent($"gml_Object_{skill.BirthObjectName}_Other_17.gml", EventType.Other, 17));
        }
    }

    static void CreateSharedCombatObjects(ModFile modFiles)
    {
        // Desecration projectile
        var bolt = Msl.AddObject(
            name: "o_nm_desecration_bolt",
            spriteName: "s_spell_darlball",
            parentName: "o_shell_damage",
            isVisible: true,
            isPersistent: false,
            isAwake: true,
            collisionShapeFlags: CollisionShapeFlags.Circle);
        bolt.ApplyEvent(modFiles,
            new MslEvent("gml_Object_o_nm_desecration_bolt_Create_0.gml", EventType.Create, 0),
            new MslEvent("gml_Object_o_nm_desecration_bolt_Other_10.gml", EventType.Other, 10),
            new MslEvent("gml_Object_o_nm_desecration_bolt_Other_11.gml", EventType.Other, 11),
            new MslEvent("gml_Object_o_nm_desecration_bolt_Destroy_0.gml", EventType.Destroy, 0));

        // Withering Curse impact pillar
        var curseImpact = Msl.AddObject(
            name: "o_nm_withering_curse_impact",
            spriteName: "s_teal_curse",
            parentName: "o_magic_pillar",
            isVisible: true,
            isPersistent: false,
            isAwake: true,
            collisionShapeFlags: CollisionShapeFlags.Circle);
        curseImpact.ApplyEvent(modFiles,
            new MslEvent("gml_Object_o_nm_withering_curse_impact_Create_0.gml", EventType.Create, 0),
            new MslEvent("gml_Object_o_nm_withering_curse_impact_Other_7.gml", EventType.Other, 7),
            new MslEvent("gml_Object_o_nm_withering_curse_impact_Draw_0.gml", EventType.Draw, 0));

        // Withering Curse debuff
        var curseBuff = Msl.AddObject(
            name: "o_db_withering_curse",
            spriteName: "s_withering_curse",
            parentName: "o_magical_debuff",
            isVisible: true,
            isPersistent: false,
            isAwake: true,
            collisionShapeFlags: CollisionShapeFlags.Circle);
        curseBuff.ApplyEvent(modFiles,
            new MslEvent("gml_Object_o_db_withering_curse_Create_0.gml", EventType.Create, 0),
            new MslEvent("gml_Object_o_db_withering_curse_Other_10.gml", EventType.Other, 10),
            new MslEvent("gml_Object_o_db_withering_curse_Other_15.gml", EventType.Other, 15),
            new MslEvent("gml_Object_o_db_withering_curse_Alarm_2.gml", EventType.Alarm, 2),
            new MslEvent("gml_Object_o_db_withering_curse_Destroy_0.gml", EventType.Destroy, 0),
            new MslEvent("gml_Object_o_db_withering_curse_Step_0.gml", EventType.Step, 0));

        // Bone Ward impact + buff
        var wardImpact = Msl.AddObject(
            name: "o_nm_bone_ward_impact",
            spriteName: "s_bone_ward",
            parentName: "o_magic_pillar",
            isVisible: true,
            isPersistent: false,
            isAwake: true,
            collisionShapeFlags: CollisionShapeFlags.Circle);
        wardImpact.ApplyEvent(modFiles,
            new MslEvent("gml_Object_o_nm_bone_ward_impact_Create_0.gml", EventType.Create, 0),
            new MslEvent("gml_Object_o_nm_bone_ward_impact_Other_7.gml", EventType.Other, 7),
            new MslEvent("gml_Object_o_nm_bone_ward_impact_Draw_0.gml", EventType.Draw, 0));

        var wardBuff = Msl.AddObject(
            name: "o_b_bone_ward",
            spriteName: "s_bone_ward",
            parentName: "o_physical_buff",
            isVisible: true,
            isPersistent: false,
            isAwake: true,
            collisionShapeFlags: CollisionShapeFlags.Circle);
        wardBuff.ApplyEvent(modFiles,
            new MslEvent("gml_Object_o_b_bone_ward_Create_0.gml", EventType.Create, 0),
            new MslEvent("gml_Object_o_b_bone_ward_Other_10.gml", EventType.Other, 10),
            new MslEvent("gml_Object_o_b_bone_ward_Alarm_2.gml", EventType.Alarm, 2),
            new MslEvent("gml_Object_o_b_bone_ward_Destroy_0.gml", EventType.Destroy, 0));

        var shardHit = Msl.AddObject(
            name: "o_nm_bone_shard_hit",
            spriteName: "",
            parentName: "",
            isVisible: false,
            isPersistent: false,
            isAwake: true,
            collisionShapeFlags: CollisionShapeFlags.Circle);
        shardHit.ApplyEvent(modFiles,
            new MslEvent("gml_Object_o_nm_bone_shard_hit_Create_0.gml", EventType.Create, 0),
            new MslEvent("gml_Object_o_nm_bone_shard_hit_Alarm_1.gml", EventType.Alarm, 1));

        var skeletonTimer = Msl.AddObject(
            name: "o_b_nm_skeleton_timer",
            spriteName: "s_summon_skeleton",
            parentName: "o_buff",
            isVisible: true,
            isPersistent: false,
            isAwake: true,
            collisionShapeFlags: CollisionShapeFlags.Circle);
        skeletonTimer.ApplyEvent(modFiles,
            new MslEvent("gml_Object_o_b_nm_skeleton_timer_Create_0.gml", EventType.Create, 0),
            new MslEvent("gml_Object_o_b_nm_skeleton_timer_Destroy_0.gml", EventType.Destroy, 0));
    }

    static void InjectSkillsStats(NecromancySkill skill)
    {
        var combat = skill.Combat!;
        var target = combat.Target switch
        {
            SkillTargetKind.Object => Msl.SkillsStatsTarget.TargetObject,
            SkillTargetKind.Point => Msl.SkillsStatsTarget.TargetPoint,
            SkillTargetKind.Area => Msl.SkillsStatsTarget.TargetArea,
            SkillTargetKind.Ally => Msl.SkillsStatsTarget.TargetAlly,
            _ => Msl.SkillsStatsTarget.NoTarget
        };

        var pattern = combat.Pattern.ToLowerInvariant() switch
        {
            "five" => Msl.SkillsStatsPattern.five,
            "line" => Msl.SkillsStatsPattern.line,
            "circle" => Msl.SkillsStatsPattern.circle,
            "pyramid" => Msl.SkillsStatsPattern.pyramid,
            "pyramid_shift" => Msl.SkillsStatsPattern.pyramid_shift,
            _ => Msl.SkillsStatsPattern.normal
        };

        var cls = combat.Class.ToLowerInvariant() switch
        {
            "skill" => Msl.SkillsStatsClass.skill,
            "attack" => Msl.SkillsStatsClass.attack,
            _ => Msl.SkillsStatsClass.spell
        };

        Msl.InjectTableSkillsStats(
            Msl.SkillsStatsHook.UNDEAD,
            skill.Slug,
            skill.BirthObjectName,
            target,
            combat.Range,
            combat.Cooldown,
            combat.Energy,
            combat.Reserve,
            combat.Duration,
            combat.AoeLength,
            combat.AoeWidth,
            combat.IsMovement,
            pattern,
            Msl.SkillsStatsValidator.none,
            cls,
            combat.BonusRange,
            combat.Starcast,
            Msl.SkillsStatsBranch.necromancy,
            is_knockback: false,
            Crime: true,
            Msl.SkillsStatsMetacategory.none,
            combat.Fumble,
            combat.ArmorPenetration,
            Attack: false,
            Stance: false,
            Charge: false,
            Maneuver: false,
            Spell: true);
    }
}
