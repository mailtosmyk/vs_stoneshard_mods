using ModShardLauncher;
using ModShardLauncher.Mods;
using Necromancy.Skills;
using UndertaleModLib.Models;

namespace Necromancy;

/// <summary>
/// Creates GameMaker objects + skills_stats rows for tier-4 skills from necromancy.json.
/// </summary>
public static class NecromancyTier4Skills
{
    public static void Patch(ModFile modFiles)
    {
        var tier4 = NecromancySkills.ByTier(4).ToArray();
        if (tier4.Length == 0)
            return;

        CreateSharedCombatObjects(modFiles);

        foreach (var skill in tier4)
        {
            if (skill.Type == SkillType.Active && skill.Combat is null)
                throw new InvalidOperationException($"Tier-4 skill '{skill.Slug}' is missing combat config in necromancy.json.");

            CreateSkillObjects(skill, modFiles);
        }

        var actives = tier4.Where(s => s.Type == SkillType.Active).ToArray();
        if (actives.Length > 0)
            NecromancyTier1Skills.InjectSkillsStatsRows(actives);

        HookImmortalityHungerThirst();
        HookBloomingMadnessTurnEnd();
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
            if (skill.Slug is "immortality" or "blooming_madness")
                passiveEvents.Add($"gml_Object_{skill.IconObjectName}_Other_13.gml");

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

        var birthSprite = skill.Slug == "wraith_summoning" ? "s_wraithsummon_cast" : "s_spell_darkball_birth";
        var birth = Msl.AddObject(
            name: skill.BirthObjectName,
            spriteName: birthSprite,
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
        if (skill.Slug == "corpse_burst")
            skillEvents.Add($"gml_Object_{skill.SkillObjectName}_Other_19.gml");

        NecromancyTier1Skills.ApplyEventAndReplace(skillObj, modFiles, skillEvents.ToArray());

        NecromancyTier1Skills.ApplyEventAndReplace(birth, modFiles,
            $"gml_Object_{skill.BirthObjectName}_Create_0.gml",
            $"gml_Object_{skill.BirthObjectName}_Other_10.gml");
    }

    static void CreateSharedCombatObjects(ModFile modFiles)
    {
        var mortalBlocker = Msl.AddObject(
            name: "o_nm_mortal_blocker",
            spriteName: "",
            parentName: "",
            isVisible: false,
            isPersistent: false,
            isAwake: true,
            collisionShapeFlags: CollisionShapeFlags.Circle);

        NecromancyTier1Skills.ApplyEventAndReplace(mortalBlocker, modFiles,
            "gml_Object_o_nm_mortal_blocker_Create_0.gml",
            "gml_Object_o_nm_mortal_blocker_Alarm_1.gml");
    }

    static void HookImmortalityHungerThirst()
    {
        Msl.LoadGML("gml_Object_o_db_hunger3_Other_10")
            .MatchFrom("if (turn_count == 4")
            .ReplaceBy("if (turn_count == 4 && (!(instance_exists(o_nm_immortality_ico) && o_nm_immortality_ico.is_open)))")
            .Save();

        Msl.LoadGML("gml_Object_o_db_thirst3_Other_10")
            .MatchFrom("if (turn_count =")
            .ReplaceBy("if (turn_count == 2 && (!(instance_exists(o_nm_immortality_ico) && o_nm_immortality_ico.is_open)))")
            .Save();
    }

    static void HookBloomingMadnessTurnEnd()
    {
        Msl.LoadGML("gml_GlobalScript_scr_global_turn_end")
            .MatchFrom("with (collision_circle(x, y, 10, o_cutscene_start_trigger, 0, 0))")
            .InsertAbove(
                "var _imm = asset_get_index(\"o_nm_immortality_ico\")\n" +
                "if (_imm != -1 && instance_exists(_imm))\n" +
                "    with (_imm) event_user(3)\n")
            .Save();
    }

}
