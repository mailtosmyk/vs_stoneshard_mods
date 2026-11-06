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
        var tier1 = NecromancySkills.ByTier(1).ToArray();

        // Register bolt/impact/buff objects first so birth Create GML can resolve
        // `spell = o_nm_*` as asset refs (otherwise they compile as unset variables).
        CreateSharedCombatObjects(modFiles);

        foreach (var skill in tier1)
        {
            if (skill.Combat is null)
                throw new InvalidOperationException($"Tier-1 skill '{skill.Slug}' is missing combat config in necromancy.json.");

            CreateSkillObjects(skill, modFiles);
        }

        // Msl.InjectTableSkillsStats fails to rewrite the multi-line skills_stats array on
        // current Stoneshard builds (rows never appear in table_skills_stats). Splice like
        // BW-s-Necromancy Bw_Bolt so Target Point / Object actually resolve at cast time.
        InjectSkillsStatsRows(tier1);
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
        // Withering Curse impact pillar (sprite shipped from BW-s-Necromancy-main).
        var tealCurse = Msl.GetSprite("s_teal_curse");
        tealCurse.OriginX = 20;
        tealCurse.OriginY = 61;
        tealCurse.IsSpecialType = true;
        tealCurse.GMS2PlaybackSpeedType = AnimSpeedType.FramesPerGameFrame;
        tealCurse.GMS2PlaybackSpeed = 0.4f;

        // Add all objects before ApplyEvent so cross-refs (bolt→curse, impact→buff, ward→shard)
        // compile as asset indices rather than unset instance variables.
        var bolt = Msl.AddObject(
            name: "o_nm_desecration_bolt",
            spriteName: "s_spell_darlball",
            parentName: "o_shell_damage",
            isVisible: true,
            isPersistent: false,
            isAwake: true,
            collisionShapeFlags: CollisionShapeFlags.Circle);

        var curseImpact = Msl.AddObject(
            name: "o_nm_withering_curse_impact",
            spriteName: "s_teal_curse",
            parentName: "o_magic_pillar",
            isVisible: true,
            isPersistent: false,
            isAwake: true,
            collisionShapeFlags: CollisionShapeFlags.Circle);

        var curseBuff = Msl.AddObject(
            name: "o_db_withering_curse",
            spriteName: "s_withering_curse",
            parentName: "o_magical_debuff",
            isVisible: true,
            isPersistent: false,
            isAwake: true,
            collisionShapeFlags: CollisionShapeFlags.Circle);

        var wardImpact = Msl.AddObject(
            name: "o_nm_bone_ward_impact",
            spriteName: "s_bone_ward",
            parentName: "o_magic_pillar",
            isVisible: true,
            isPersistent: false,
            isAwake: true,
            collisionShapeFlags: CollisionShapeFlags.Circle);

        var wardBuff = Msl.AddObject(
            name: "o_b_bone_ward",
            spriteName: "s_bone_ward",
            parentName: "o_physical_buff",
            isVisible: true,
            isPersistent: false,
            isAwake: true,
            collisionShapeFlags: CollisionShapeFlags.Circle);

        var shardHit = Msl.AddObject(
            name: "o_nm_bone_shard_hit",
            spriteName: "",
            parentName: "",
            isVisible: false,
            isPersistent: false,
            isAwake: true,
            collisionShapeFlags: CollisionShapeFlags.Circle);

        var skeletonTimer = Msl.AddObject(
            name: "o_b_nm_skeleton_timer",
            spriteName: "s_summon_skeleton",
            parentName: "o_buff",
            isVisible: true,
            isPersistent: false,
            isAwake: true,
            collisionShapeFlags: CollisionShapeFlags.Circle);

        bolt.ApplyEvent(modFiles,
            new MslEvent("gml_Object_o_nm_desecration_bolt_Create_0.gml", EventType.Create, 0),
            new MslEvent("gml_Object_o_nm_desecration_bolt_Other_10.gml", EventType.Other, 10),
            new MslEvent("gml_Object_o_nm_desecration_bolt_Other_11.gml", EventType.Other, 11),
            new MslEvent("gml_Object_o_nm_desecration_bolt_Destroy_0.gml", EventType.Destroy, 0));

        curseImpact.ApplyEvent(modFiles,
            new MslEvent("gml_Object_o_nm_withering_curse_impact_Create_0.gml", EventType.Create, 0),
            new MslEvent("gml_Object_o_nm_withering_curse_impact_Other_7.gml", EventType.Other, 7),
            new MslEvent("gml_Object_o_nm_withering_curse_impact_Draw_0.gml", EventType.Draw, 0));

        curseBuff.ApplyEvent(modFiles,
            new MslEvent("gml_Object_o_db_withering_curse_Create_0.gml", EventType.Create, 0),
            new MslEvent("gml_Object_o_db_withering_curse_Other_10.gml", EventType.Other, 10),
            new MslEvent("gml_Object_o_db_withering_curse_Other_15.gml", EventType.Other, 15),
            new MslEvent("gml_Object_o_db_withering_curse_Alarm_2.gml", EventType.Alarm, 2),
            new MslEvent("gml_Object_o_db_withering_curse_Destroy_0.gml", EventType.Destroy, 0),
            new MslEvent("gml_Object_o_db_withering_curse_Step_0.gml", EventType.Step, 0));

        wardImpact.ApplyEvent(modFiles,
            new MslEvent("gml_Object_o_nm_bone_ward_impact_Create_0.gml", EventType.Create, 0),
            new MslEvent("gml_Object_o_nm_bone_ward_impact_Other_7.gml", EventType.Other, 7),
            new MslEvent("gml_Object_o_nm_bone_ward_impact_Draw_0.gml", EventType.Draw, 0));

        wardBuff.ApplyEvent(modFiles,
            new MslEvent("gml_Object_o_b_bone_ward_Create_0.gml", EventType.Create, 0),
            new MslEvent("gml_Object_o_b_bone_ward_Other_10.gml", EventType.Other, 10),
            new MslEvent("gml_Object_o_b_bone_ward_Alarm_2.gml", EventType.Alarm, 2),
            new MslEvent("gml_Object_o_b_bone_ward_Destroy_0.gml", EventType.Destroy, 0));

        shardHit.ApplyEvent(modFiles,
            new MslEvent("gml_Object_o_nm_bone_shard_hit_Create_0.gml", EventType.Create, 0),
            new MslEvent("gml_Object_o_nm_bone_shard_hit_Alarm_1.gml", EventType.Alarm, 1));

        skeletonTimer.ApplyEvent(modFiles,
            new MslEvent("gml_Object_o_b_nm_skeleton_timer_Create_0.gml", EventType.Create, 0),
            new MslEvent("gml_Object_o_b_nm_skeleton_timer_Destroy_0.gml", EventType.Destroy, 0));
    }

    /// <summary>
    /// Column layout matches current table_skills_stats header:
    /// id;Object;Target;Range;KD;MP;is_moving_zone;Duration;AOE_*;is_movement;Pattern;Validators;Class;...
    /// JSON <c>reserve</c> maps to <c>is_moving_zone</c> (not energy reserve).
    /// </summary>
    static void InjectSkillsStatsRows(IReadOnlyList<NecromancySkill> skills)
    {
        var rows = string.Concat(skills.Select(FormatSkillsStatsRow));
        Msl.LoadGML("gml_GlobalScript_table_skills_stats")
            .Apply(lines => InsertAfterUndeadMarker(lines, rows))
            .Save();
    }

    static IEnumerable<string> InsertAfterUndeadMarker(IEnumerable<string> lines, string rows)
    {
        const string markerPrefix = "\"// UNDEAD;";
        var inserted = false;
        foreach (var line in lines)
        {
            if (!inserted && line.Contains(markerPrefix, StringComparison.Ordinal))
            {
                // Idempotent across re-patches of an already-modded data.win.
                if (line.Contains("desecration;", StringComparison.Ordinal))
                {
                    yield return line;
                    inserted = true;
                    continue;
                }

                var markerStart = line.IndexOf(markerPrefix, StringComparison.Ordinal);
                var markerEnd = line.IndexOf("\",", markerStart, StringComparison.Ordinal);
                if (markerEnd < 0)
                    throw new InvalidOperationException("Could not find end of // UNDEAD skills_stats marker.");

                yield return line.Insert(markerEnd + 2, rows);
                inserted = true;
            }
            else
            {
                yield return line;
            }
        }

        if (!inserted)
            throw new InvalidOperationException("Could not find // UNDEAD marker in table_skills_stats.");
    }

    static string FormatSkillsStatsRow(NecromancySkill skill)
    {
        var combat = skill.Combat!;
        var target = combat.Target switch
        {
            SkillTargetKind.Object => "Target Object",
            SkillTargetKind.Point => "Target Point",
            SkillTargetKind.Area => "Target Area",
            SkillTargetKind.Ally => "Target Ally",
            _ => "No Target"
        };

        var pattern = string.IsNullOrWhiteSpace(combat.Pattern) ? "normal" : combat.Pattern.ToLowerInvariant();
        var cls = string.IsNullOrWhiteSpace(combat.Class) ? "spell" : combat.Class.ToLowerInvariant();
        var starcast = combat.Starcast ?? string.Empty;
        // Player sorcery spells use metacategory "sorcery"; offensive tags match Fire_Barrage-style missiles.
        var tags = combat.IsMissile || combat.Kind is SkillCombatKind.Missile or SkillCombatKind.Debuff
            ? "offensive"
            : string.Empty;

        // Keep validators empty (;; between Pattern and Class) — required by current game CSV.
        return
            $"\"{skill.Slug};{skill.BirthObjectName};{target};{combat.Range};{combat.Cooldown};{combat.Energy};" +
            $"{combat.Reserve};{combat.Duration};{combat.AoeLength};{combat.AoeWidth};{(combat.IsMovement ? "1" : "0")};" +
            $"{pattern};;{cls};{(combat.BonusRange ? "1" : "0")};{starcast};necromancy;0;1;sorcery;" +
            $"{combat.Fumble};{combat.ArmorPenetration};;;;;1;{tags};\",";
    }
}
