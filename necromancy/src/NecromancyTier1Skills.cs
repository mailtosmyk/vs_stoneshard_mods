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

        // Vanilla only credits summon kills when last_attacker is o_summoned_entity.
        // Our skeleton/thralls are regular enemies tagged nm_summon — extend that check.
        PatchSummonKillExperience();
    }

    /// <summary>
    /// Credit player XP when a necromancy summon (nm_summon + player owner) lands the kill.
    /// Mirrors vanilla o_summoned_entity handling in o_enemy Destroy.
    /// </summary>
    static void PatchSummonKillExperience()
    {
        // o_enemy Destroy does not round-trip through LoadGML (UTMT decompile emits
        // forms ReplaceGML cannot parse). Patch the summon-credit bytecode instead,
        // same approach as BW-s-Necromancy EnemyDestroyIterator.
        Msl.LoadAssemblyAsString("gml_Object_o_enemy_Destroy_0")
            .Apply(SummonKillExperienceAsm)
            .Save();
    }

    /// <summary>
    /// Vanilla credits XP when last_attacker is o_summoned_entity owned by the player.
    /// When that instance_is check fails it pushes false ([156] push.e 0). Replace that
    /// false with an nm_summon + owner-is-player check so skeleton/thrall kills count.
    /// </summary>
    static IEnumerable<string> SummonKillExperienceAsm(IEnumerable<string> input)
    {
        var lines = input as IList<string> ?? input.ToList();
        if (lines.Any(l => l.Contains("nm_summon", StringComparison.Ordinal)))
            return lines;

        var status = 0;
        var output = new List<string>(lines.Count + 32);
        foreach (var item in lines)
        {
            if (status == 0 && item.Contains("gml_Script_instance_is(argc=2)", StringComparison.Ordinal))
            {
                status = 1;
                output.Add(item);
            }
            else if (status == 1 && item.Trim() == ":[156]")
            {
                status = 2;
                output.Add(item);
            }
            else if (status == 2 && item.Trim() == "push.e 0")
            {
                status = 3;
                // When not o_summoned_entity, credit nm_summon thralls owned by the player.
                output.Add(@"
push.v self.last_attacker
call.i instance_exists(argc=1)
conv.v.b
bf [9002]

push.s ""nm_summon""
conv.s.v
push.v self.last_attacker
call.i variable_instance_exists(argc=2)
conv.v.b
bf [9002]

push.s ""nm_summon""
conv.s.v
push.v self.last_attacker
call.i variable_instance_get(argc=2)
conv.v.b
bf [9002]

push.v self.last_attacker
pushi.e -9
push.v [stacktop]self.owner
call.i gml_Script_is_player(argc=1)
conv.v.b
b [9003]

:[9002]
push.e 0

:[9003]
");
            }
            else
            {
                output.Add(item);
            }
        }

        if (status != 3)
        {
            throw new Exception(
                "Necromancy: summon XP assembly patch site not found in gml_Object_o_enemy_Destroy_0");
        }

        return output;
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

        // Force-replace birth bodies on re-patch (ApplyEvent skips existing subtypes).
        ApplyEventAndReplace(birth, modFiles,
            $"gml_Object_{skill.BirthObjectName}_Create_0.gml",
            $"gml_Object_{skill.BirthObjectName}_Other_10.gml");

        if (skill.Combat.IsMissile)
        {
            ApplyEventAndReplace(birth, modFiles,
                $"gml_Object_{skill.BirthObjectName}_Other_17.gml");
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

        // Buff-only icon (not a skill-tree sprite), so EnsureSpriteOrigins never sees it.
        // Without 12,12 the enemy Effects grid draws it between rows.
        var witheringCurseIcon = Msl.GetSprite("s_withering_curse");
        witheringCurseIcon.OriginX = 12;
        witheringCurseIcon.OriginY = 12;

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

        // Concrete ally skeleton: inherits militiaman Create/scr_param, overrides Other_10
        // with Syndicate-style retinue AI (event_inherited combat / scr_turn follow).
        var skeletonAlly = Msl.AddObject(
            name: "o_nm_skeleton_ally",
            spriteName: "s_skeleton_militiaman",
            parentName: "o_skeleton_militiaman",
            isVisible: true,
            isPersistent: false,
            isAwake: true,
            collisionShapeFlags: CollisionShapeFlags.Circle);
        var militiaman = Msl.GetObject("o_skeleton_militiaman");
        if (militiaman != null)
            skeletonAlly.ParentId = militiaman;

        var skeletonTimer = Msl.AddObject(
            name: "o_b_nm_skeleton_timer",
            spriteName: "s_summon_skeleton",
            parentName: "o_magical_buff",
            isVisible: true,
            isPersistent: false,
            isAwake: true,
            collisionShapeFlags: CollisionShapeFlags.Circle);

        // Invisible marker + sticky ally flags on the summoned skeleton.
        // Parent o_invisible_buff like vanilla o_b_summoned_entity so Effect_Immunity
        // and summoned-entity effect blocks cannot strip the bind buff.
        var skeletonServant = Msl.AddObject(
            name: "o_b_nm_servant",
            spriteName: "s_summon_skeleton",
            parentName: "o_invisible_buff",
            isVisible: false,
            isPersistent: false,
            isAwake: true,
            collisionShapeFlags: CollisionShapeFlags.Circle);
        // AddObject may leave an older parent on re-patch — force invisible_buff.
        var invisibleBuff = Msl.GetObject("o_invisible_buff");
        if (invisibleBuff != null)
            skeletonServant.ParentId = invisibleBuff;

        var desecrationBuff = Msl.AddObject(
            name: "o_b_nm_desecration",
            spriteName: "s_desecration",
            parentName: "o_magical_buff",
            isVisible: true,
            isPersistent: false,
            isAwake: true,
            collisionShapeFlags: CollisionShapeFlags.Circle);

        ApplyEventAndReplace(bolt, modFiles,
            "gml_Object_o_nm_desecration_bolt_Create_0.gml",
            "gml_Object_o_nm_desecration_bolt_Other_10.gml",
            "gml_Object_o_nm_desecration_bolt_Other_11.gml",
            "gml_Object_o_nm_desecration_bolt_Destroy_0.gml");

        ApplyEventAndReplace(curseImpact, modFiles,
            "gml_Object_o_nm_withering_curse_impact_Create_0.gml",
            "gml_Object_o_nm_withering_curse_impact_Other_7.gml",
            "gml_Object_o_nm_withering_curse_impact_Draw_0.gml");

        ApplyEventAndReplace(curseBuff, modFiles,
            "gml_Object_o_db_withering_curse_Create_0.gml",
            "gml_Object_o_db_withering_curse_Other_10.gml",
            "gml_Object_o_db_withering_curse_Other_15.gml",
            "gml_Object_o_db_withering_curse_Alarm_2.gml",
            "gml_Object_o_db_withering_curse_Destroy_0.gml",
            "gml_Object_o_db_withering_curse_Step_0.gml");

        wardImpact.ApplyEvent(modFiles,
            new MslEvent("gml_Object_o_nm_bone_ward_impact_Create_0.gml", EventType.Create, 0),
            new MslEvent("gml_Object_o_nm_bone_ward_impact_Alarm_0.gml", EventType.Alarm, 0),
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

        // ApplyEvent/AddNewEvent skips when the event already exists, so re-patches
        // leave stale GML in data.win. Force-replace after add so fixes actually apply.
        ApplyEventAndReplace(skeletonAlly, modFiles,
            "gml_Object_o_nm_skeleton_ally_Other_10.gml");

        ApplyEventAndReplace(skeletonTimer, modFiles,
            "gml_Object_o_b_nm_skeleton_timer_Create_0.gml",
            "gml_Object_o_b_nm_skeleton_timer_Other_10.gml",
            "gml_Object_o_b_nm_skeleton_timer_Alarm_2.gml",
            "gml_Object_o_b_nm_skeleton_timer_Destroy_0.gml");

        ApplyEventAndReplace(skeletonServant, modFiles,
            "gml_Object_o_b_nm_servant_Create_0.gml",
            "gml_Object_o_b_nm_servant_Other_10.gml",
            "gml_Object_o_b_nm_servant_Alarm_2.gml");

        desecrationBuff.ApplyEvent(modFiles,
            new MslEvent("gml_Object_o_b_nm_desecration_Create_0.gml", EventType.Create, 0),
            new MslEvent("gml_Object_o_b_nm_desecration_Other_10.gml", EventType.Other, 10),
            new MslEvent("gml_Object_o_b_nm_desecration_Alarm_2.gml", EventType.Alarm, 2));
    }

    /// <summary>
    /// Add events on first install, then always overwrite code bodies from ModFiles.
    /// Needed because Msl.AddNewEvent refuses to replace an existing event subtype.
    /// </summary>
    internal static void ApplyEventAndReplace(
        UndertaleGameObject gameObject,
        ModFile modFiles,
        params string[] codeFiles)
    {
        var events = codeFiles.Select(ParseMslEvent).ToArray();
        try
        {
            gameObject.ApplyEvent(modFiles, events);
        }
        catch (ArgumentException)
        {
            // Event already present from a previous patch — ForceReplaceCode below refreshes it.
        }

        foreach (var codeFile in codeFiles)
            ForceReplaceCode(modFiles, codeFile);
    }

    internal static MslEvent ParseMslEvent(string codeFile)
    {
        // gml_Object_<name>_<Event>_<subtype>.gml  (Other_10, Alarm_2, Create_0, Destroy_0, ...)
        var baseName = codeFile.EndsWith(".gml", StringComparison.OrdinalIgnoreCase)
            ? codeFile[..^4]
            : codeFile;
        var parts = baseName.Split('_');
        if (parts.Length < 2 || !uint.TryParse(parts[^1], out var subtype))
            throw new InvalidOperationException($"Cannot parse event subtype from '{codeFile}'.");

        var eventToken = parts[^2];
        var eventType = eventToken switch
        {
            "Create" => EventType.Create,
            "Destroy" => EventType.Destroy,
            "Alarm" => EventType.Alarm,
            "Step" => EventType.Step,
            "Other" => EventType.Other,
            "Draw" => EventType.Draw,
            "Mouse" => EventType.Mouse,
            "PreCreate" => EventType.PreCreate,
            _ => throw new InvalidOperationException($"Unknown event type '{eventToken}' in '{codeFile}'.")
        };

        return new MslEvent(codeFile, eventType, subtype);
    }

    internal static void ForceReplaceCode(ModFile modFiles, string codeFile)
    {
        var gmlName = codeFile.EndsWith(".gml", StringComparison.OrdinalIgnoreCase)
            ? codeFile[..^4]
            : codeFile;
        var raw = modFiles.GetCode(codeFile);
        if (string.IsNullOrEmpty(raw))
            throw new InvalidOperationException($"Missing code in mod package: {codeFile}");

        var lines = raw.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
        Msl.LoadGML(gmlName)
            .Apply(_ => lines)
            .Save();
    }

    /// <summary>
    /// Column layout matches current table_skills_stats header:
    /// id;Object;Target;Range;KD;MP;is_moving_zone;Duration;AOE_*;is_movement;Pattern;Validators;Class;...
    /// JSON <c>reserve</c> maps to <c>is_moving_zone</c> (not energy reserve).
    /// </summary>
    internal static void InjectSkillsStatsRows(IReadOnlyList<NecromancySkill> skills)
    {
        Msl.LoadGML("gml_GlobalScript_table_skills_stats")
            .Apply(lines => InsertAfterUndeadMarker(lines, skills))
            .Save();
    }

    internal static IEnumerable<string> InsertAfterUndeadMarker(
        IEnumerable<string> lines,
        IReadOnlyList<NecromancySkill> skills)
    {
        const string markerPrefix = "\"// UNDEAD;";
        var rows = string.Concat(skills.Select(FormatSkillsStatsRow));
        var inserted = false;
        foreach (var line in lines)
        {
            if (!inserted && line.Contains(markerPrefix, StringComparison.Ordinal))
            {
                // Already patched: refresh each necromancy row so Object/column fixes apply.
                if (line.Contains("desecration;", StringComparison.Ordinal))
                {
                    var updated = line;
                    foreach (var skill in skills)
                        updated = UpsertSkillStatsRow(updated, skill);
                    // Belt-and-suspenders: No Target self-buffs must never keep a spellbirth Object.
                    updated = updated.Replace(
                        "bone_ward;o_nm_bone_ward_birth;",
                        "bone_ward;o_b_bone_ward;",
                        StringComparison.Ordinal);
                    yield return updated;
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

    internal static string UpsertSkillStatsRow(string line, NecromancySkill skill)
    {
        var newRow = FormatSkillsStatsRow(skill);
        var needle = "\"" + skill.Slug + ";";
        var start = line.IndexOf(needle, StringComparison.Ordinal);
        if (start < 0)
        {
            // New skill: insert after an existing necromancy row (desecration is always first).
            const string anchor = "\"desecration;";
            var anchorStart = line.IndexOf(anchor, StringComparison.Ordinal);
            if (anchorStart < 0)
                return line;

            var anchorEnd = line.IndexOf("\",", anchorStart, StringComparison.Ordinal);
            if (anchorEnd < 0)
                return line;

            return line.Substring(0, anchorEnd + 2) + newRow + line.Substring(anchorEnd + 2);
        }

        var end = line.IndexOf("\",", start, StringComparison.Ordinal);
        if (end < 0)
            return line;

        // FormatSkillsStatsRow already includes trailing comma after the CSV quote.
        return line.Substring(0, start) + newRow + line.Substring(end + 2);
    }

    internal static string FormatSkillsStatsRow(NecromancySkill skill)
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
        // Self-buffs use the buff object (Stone_Armor → o_b_stone_armor), not spellbirth.
        return
            $"\"{skill.Slug};{skill.StatsObjectName};{target};{combat.Range};{combat.Cooldown};{combat.Energy};" +
            $"{combat.Reserve};{combat.Duration};{combat.AoeLength};{combat.AoeWidth};{(combat.IsMovement ? "1" : "0")};" +
            $"{pattern};;{cls};{(combat.BonusRange ? "1" : "0")};{starcast};necromancy;0;1;sorcery;" +
            $"{combat.Fumble};{combat.ArmorPenetration};;;;;1;{tags};\",";
    }
}
