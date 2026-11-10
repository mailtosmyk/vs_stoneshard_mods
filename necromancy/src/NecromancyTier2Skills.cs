using ModShardLauncher;
using ModShardLauncher.Mods;
using Necromancy.Skills;
using UndertaleModLib.Models;

namespace Necromancy;

/// <summary>
/// Creates GameMaker objects + skills_stats rows for tier-2 skills from necromancy.json.
/// </summary>
public static class NecromancyTier2Skills
{
    public static void Patch(ModFile modFiles)
    {
        var tier2 = NecromancySkills.ByTier(2).ToArray();
        if (tier2.Length == 0)
            return;

        CreateSharedCombatObjects(modFiles);

        foreach (var skill in tier2)
        {
            if (skill.Combat is null)
                throw new InvalidOperationException($"Tier-2 skill '{skill.Slug}' is missing combat config in necromancy.json.");

            CreateSkillObjects(skill, modFiles);
        }

        NecromancyTier1Skills.InjectSkillsStatsRows(tier2);
        HookEssenceChargeSpellConsume();
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

        if (skill.Slug == "resurrection")
        {
            NecromancyTier1Skills.ApplyEventAndReplace(ico, modFiles,
                $"gml_Object_{skill.IconObjectName}_Other_13.gml");
        }

        var skillEvents = new List<string>
        {
            $"gml_Object_{skill.SkillObjectName}_Create_0.gml",
            $"gml_Object_{skill.SkillObjectName}_Other_17.gml",
        };
        if (skill.Slug == "essence_leech")
            skillEvents.Add($"gml_Object_{skill.SkillObjectName}_Other_19.gml");

        NecromancyTier1Skills.ApplyEventAndReplace(skillObj, modFiles, skillEvents.ToArray());

        NecromancyTier1Skills.ApplyEventAndReplace(birth, modFiles,
            $"gml_Object_{skill.BirthObjectName}_Create_0.gml",
            $"gml_Object_{skill.BirthObjectName}_Other_10.gml");
    }

    static void CreateSharedCombatObjects(ModFile modFiles)
    {
        var essenceImpact = Msl.AddObject(
            name: "o_nm_essence_leech_impact",
            spriteName: "s_essence_leech",
            parentName: "o_magic_pillar",
            isVisible: true,
            isPersistent: false,
            isAwake: true,
            collisionShapeFlags: CollisionShapeFlags.Circle);

        var essenceCharge = Msl.AddObject(
            name: "o_b_nm_essence_charge",
            spriteName: "s_essence_leech",
            parentName: "o_magical_buff",
            isVisible: true,
            isPersistent: false,
            isAwake: true,
            collisionShapeFlags: CollisionShapeFlags.Circle);

        var blessImpact = Msl.AddObject(
            name: "o_nm_deaths_blessing_impact",
            spriteName: "s_deaths_blessing",
            parentName: "o_magic_pillar",
            isVisible: true,
            isPersistent: false,
            isAwake: true,
            collisionShapeFlags: CollisionShapeFlags.Circle);

        var blessBuff = Msl.AddObject(
            name: "o_b_nm_deaths_blessing",
            spriteName: "s_deaths_blessing",
            parentName: "o_magical_buff",
            isVisible: true,
            isPersistent: false,
            isAwake: true,
            collisionShapeFlags: CollisionShapeFlags.Circle);

        var resCast = Msl.AddObject(
            name: "o_nm_resurrection_cast",
            spriteName: "s_resurrection",
            parentName: "o_magic_pillar",
            isVisible: true,
            isPersistent: false,
            isAwake: true,
            collisionShapeFlags: CollisionShapeFlags.Circle);

        var thrallTimer = Msl.AddObject(
            name: "o_b_nm_thrall_timer",
            spriteName: "s_resurrection",
            parentName: "o_magical_buff",
            isVisible: true,
            isPersistent: false,
            isAwake: true,
            collisionShapeFlags: CollisionShapeFlags.Circle);

        var blightImpact = Msl.AddObject(
            name: "o_nm_grave_blight_impact",
            spriteName: "s_grave_blight",
            parentName: "o_magic_pillar",
            isVisible: true,
            isPersistent: false,
            isAwake: true,
            collisionShapeFlags: CollisionShapeFlags.Circle);

        var blightBuff = Msl.AddObject(
            name: "o_b_nm_grave_blight",
            spriteName: "s_grave_blight",
            parentName: "o_magical_buff",
            isVisible: true,
            isPersistent: false,
            isAwake: true,
            collisionShapeFlags: CollisionShapeFlags.Circle);

        var blightZone = Msl.AddObject(
            name: "o_nm_grave_blight_zone",
            spriteName: "s_grave_blight",
            parentName: "",
            isVisible: true,
            isPersistent: false,
            isAwake: true,
            collisionShapeFlags: CollisionShapeFlags.Circle);

        var blightTick = Msl.AddObject(
            name: "o_nm_grave_blight_tick",
            spriteName: "",
            parentName: "",
            isVisible: false,
            isPersistent: false,
            isAwake: true,
            collisionShapeFlags: CollisionShapeFlags.Circle);

        var blightDebuff = Msl.AddObject(
            name: "o_db_nm_grave_blight",
            spriteName: "s_grave_blight",
            parentName: "o_magical_debuff",
            isVisible: true,
            isPersistent: false,
            isAwake: true,
            collisionShapeFlags: CollisionShapeFlags.Circle);

        NecromancyTier1Skills.ApplyEventAndReplace(essenceImpact, modFiles,
            "gml_Object_o_nm_essence_leech_impact_Create_0.gml",
            "gml_Object_o_nm_essence_leech_impact_Other_7.gml");

        NecromancyTier1Skills.ApplyEventAndReplace(essenceCharge, modFiles,
            "gml_Object_o_b_nm_essence_charge_Create_0.gml",
            "gml_Object_o_b_nm_essence_charge_Alarm_2.gml",
            "gml_Object_o_b_nm_essence_charge_Alarm_3.gml",
            "gml_Object_o_b_nm_essence_charge_Other_10.gml",
            "gml_Object_o_b_nm_essence_charge_Other_14.gml",
            "gml_Object_o_b_nm_essence_charge_Step_2.gml");

        NecromancyTier1Skills.ApplyEventAndReplace(blessImpact, modFiles,
            "gml_Object_o_nm_deaths_blessing_impact_Create_0.gml",
            "gml_Object_o_nm_deaths_blessing_impact_Other_7.gml");

        NecromancyTier1Skills.ApplyEventAndReplace(blessBuff, modFiles,
            "gml_Object_o_b_nm_deaths_blessing_Create_0.gml",
            "gml_Object_o_b_nm_deaths_blessing_Alarm_2.gml",
            "gml_Object_o_b_nm_deaths_blessing_Other_10.gml");

        NecromancyTier1Skills.ApplyEventAndReplace(resCast, modFiles,
            "gml_Object_o_nm_resurrection_cast_Create_0.gml",
            "gml_Object_o_nm_resurrection_cast_Alarm_0.gml");

        NecromancyTier1Skills.ApplyEventAndReplace(thrallTimer, modFiles,
            "gml_Object_o_b_nm_thrall_timer_Create_0.gml",
            "gml_Object_o_b_nm_thrall_timer_Other_10.gml",
            "gml_Object_o_b_nm_thrall_timer_Alarm_2.gml",
            "gml_Object_o_b_nm_thrall_timer_Destroy_0.gml");

        NecromancyTier1Skills.ApplyEventAndReplace(blightImpact, modFiles,
            "gml_Object_o_nm_grave_blight_impact_Create_0.gml",
            "gml_Object_o_nm_grave_blight_impact_Other_7.gml");

        NecromancyTier1Skills.ApplyEventAndReplace(blightBuff, modFiles,
            "gml_Object_o_b_nm_grave_blight_Create_0.gml",
            "gml_Object_o_b_nm_grave_blight_Alarm_2.gml",
            "gml_Object_o_b_nm_grave_blight_Other_10.gml",
            "gml_Object_o_b_nm_grave_blight_Destroy_0.gml");

        NecromancyTier1Skills.ApplyEventAndReplace(blightZone, modFiles,
            "gml_Object_o_nm_grave_blight_zone_Create_0.gml",
            "gml_Object_o_nm_grave_blight_zone_Step_0.gml");

        NecromancyTier1Skills.ApplyEventAndReplace(blightTick, modFiles,
            "gml_Object_o_nm_grave_blight_tick_Create_0.gml",
            "gml_Object_o_nm_grave_blight_tick_Alarm_1.gml");

        NecromancyTier1Skills.ApplyEventAndReplace(blightDebuff, modFiles,
            "gml_Object_o_db_nm_grave_blight_Create_0.gml",
            "gml_Object_o_db_nm_grave_blight_Alarm_2.gml",
            "gml_Object_o_db_nm_grave_blight_Alarm_3.gml",
            "gml_Object_o_db_nm_grave_blight_Other_10.gml",
            "gml_Object_o_db_nm_grave_blight_Step_2.gml");
    }

    /// <summary>
    /// Spells consume one Essence Charge stack (same hook BW uses on spirit_and_body).
    /// Wraith Summoning skips consume (birth removes all).
    /// </summary>
    static void HookEssenceChargeSpellConsume()
    {
        Msl.LoadGML("gml_Object_o_pass_skill_spirit_and_body_Other_13")
            .MatchFrom("event_inherited()")
            .InsertBelow(
                "if ((!is_enemy_skill) && (!is_attack_skill))\n" +
                "{\n" +
                "    if (skill != \"wraith_summoning\")\n" +
                "    {\n" +
                "        with (o_b_nm_essence_charge)\n" +
                "            event_user(4)\n" +
                "    }\n" +
                "}\n")
            .Save();
    }
}
