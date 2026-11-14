using ModShardLauncher;
using ModShardLauncher.Mods;
using Summoner.Skills;
using UndertaleModLib.Models;

namespace Summoner;

/// <summary>
/// Creates GameMaker objects + skills_stats rows for all Summoner skills.
/// </summary>
public static class SummonerSkillsPatch
{
    public static void Patch(ModFile modFiles)
    {
        CreateShared(modFiles);
        foreach (var skill in SummonerSkills.All)
            CreateSkillObjects(skill, modFiles);

        var actives = SummonerSkills.All.Where(s => s.Type == SkillType.Active && s.Combat is not null).ToArray();
        if (actives.Length > 0)
            InjectSkillsStatsRows(actives);
    }

    static void CreateShared(ModFile modFiles)
    {
        var servant = Msl.AddObject(
            name: "o_b_sm_servant",
            spriteName: "s_summon_wolf",
            parentName: "o_invisible_buff",
            isVisible: false,
            isPersistent: false,
            isAwake: true,
            collisionShapeFlags: CollisionShapeFlags.Circle);
        var invisibleBuff = Msl.GetObject("o_invisible_buff");
        if (invisibleBuff != null)
            servant.ParentId = invisibleBuff;

        ApplyEventAndReplace(servant, modFiles,
            "gml_Object_o_b_sm_servant_Create_0.gml",
            "gml_Object_o_b_sm_servant_Other_10.gml",
            "gml_Object_o_b_sm_servant_Destroy_0.gml");

        // Temporary Morale penalty when a summon dies in combat (600t, does not stack).
        var summonDeath = Msl.AddObject(
            name: "o_b_sm_summon_death",
            spriteName: "s_kindred_spirit",
            parentName: "o_physical_buff",
            isVisible: true,
            isPersistent: false,
            isAwake: true,
            collisionShapeFlags: CollisionShapeFlags.Circle);
        ApplyEventAndReplace(summonDeath, modFiles,
            "gml_Object_o_b_sm_summon_death_Create_0.gml",
            "gml_Object_o_b_sm_summon_death_Other_10.gml",
            "gml_Object_o_b_sm_summon_death_Alarm_2.gml");
    }

    static void CreateSkillObjects(SummonerSkill skill, ModFile modFiles)
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
            ApplyEventAndReplace(passiveIco, modFiles,
                $"gml_Object_{skill.IconObjectName}_Create_0.gml",
                $"gml_Object_{skill.IconObjectName}_Step_2.gml",
                $"gml_Object_{skill.IconObjectName}_Other_17.gml");
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

        if (skill.Combat?.Kind == SkillCombatKind.SelfBuff)
        {
            var buff = Msl.AddObject(
                name: skill.BuffObjectName,
                spriteName: skill.SpriteName,
                parentName: "o_physical_buff",
                isVisible: true,
                isPersistent: false,
                isAwake: true,
                collisionShapeFlags: CollisionShapeFlags.Circle);
            ApplyEventAndReplace(buff, modFiles,
                $"gml_Object_{skill.BuffObjectName}_Create_0.gml",
                $"gml_Object_{skill.BuffObjectName}_Other_10.gml",
                $"gml_Object_{skill.BuffObjectName}_Alarm_2.gml");
        }

        ApplyEventAndReplace(ico, modFiles,
            $"gml_Object_{skill.IconObjectName}_Create_0.gml",
            $"gml_Object_{skill.IconObjectName}_Step_2.gml");

        ApplyEventAndReplace(skillObj, modFiles,
            $"gml_Object_{skill.SkillObjectName}_Create_0.gml",
            $"gml_Object_{skill.SkillObjectName}_Other_17.gml");

        ApplyEventAndReplace(birth, modFiles,
            $"gml_Object_{skill.BirthObjectName}_Create_0.gml",
            $"gml_Object_{skill.BirthObjectName}_Other_10.gml");
    }

    internal static void ApplyEventAndReplace(
        UndertaleGameObject gameObject, ModFile modFiles, params string[] codeFiles)
    {
        var events = codeFiles.Select(ParseMslEvent).ToArray();
        try { gameObject.ApplyEvent(modFiles, events); }
        catch (ArgumentException) { }

        foreach (var codeFile in codeFiles)
            ForceReplaceCode(modFiles, codeFile);
    }

    internal static MslEvent ParseMslEvent(string codeFile)
    {
        var baseName = codeFile.EndsWith(".gml", StringComparison.OrdinalIgnoreCase)
            ? codeFile[..^4] : codeFile;
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
            _ => throw new InvalidOperationException($"Unknown event '{eventToken}' in '{codeFile}'.")
        };
        return new MslEvent(codeFile, eventType, subtype);
    }

    internal static void ForceReplaceCode(ModFile modFiles, string codeFile)
    {
        var gmlName = codeFile.EndsWith(".gml", StringComparison.OrdinalIgnoreCase)
            ? codeFile[..^4] : codeFile;
        var raw = modFiles.GetCode(codeFile);
        if (string.IsNullOrEmpty(raw))
            throw new InvalidOperationException($"Missing code in mod package: {codeFile}");
        var lines = raw.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
        Msl.LoadGML(gmlName).Apply(_ => lines).Save();
    }

    internal static void InjectSkillsStatsRows(IReadOnlyList<SummonerSkill> skills)
    {
        Msl.LoadGML("gml_GlobalScript_table_skills_stats")
            .Apply(lines => InsertAfterUndeadMarker(lines, skills))
            .Save();
    }

    static IEnumerable<string> InsertAfterUndeadMarker(
        IEnumerable<string> lines, IReadOnlyList<SummonerSkill> skills)
    {
        const string markerPrefix = "\"// UNDEAD;";
        var rows = string.Concat(skills.Select(FormatSkillsStatsRow));
        var inserted = false;
        foreach (var line in lines)
        {
            if (!inserted && line.Contains(markerPrefix, StringComparison.Ordinal))
            {
                if (line.Contains("summon_wolf;", StringComparison.Ordinal))
                {
                    var updated = line;
                    foreach (var skill in skills)
                        updated = UpsertSkillStatsRow(updated, skill);
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
                yield return line;
        }

        if (!inserted)
            throw new InvalidOperationException("Could not find // UNDEAD marker in table_skills_stats.");
    }

    static string UpsertSkillStatsRow(string line, SummonerSkill skill)
    {
        var newRow = FormatSkillsStatsRow(skill);
        var needle = "\"" + skill.Slug + ";";
        var start = line.IndexOf(needle, StringComparison.Ordinal);
        if (start < 0)
        {
            const string anchor = "\"// UNDEAD;";
            var anchorStart = line.IndexOf(anchor, StringComparison.Ordinal);
            if (anchorStart < 0) return line;
            var anchorEnd = line.IndexOf("\",", anchorStart, StringComparison.Ordinal);
            if (anchorEnd < 0) return line;
            return line.Substring(0, anchorEnd + 2) + newRow + line.Substring(anchorEnd + 2);
        }

        var end = line.IndexOf("\",", start, StringComparison.Ordinal);
        if (end < 0) return line;
        return line.Substring(0, start) + newRow + line.Substring(end + 2);
    }

    static string FormatSkillsStatsRow(SummonerSkill skill)
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
        var tags = combat.Kind is SkillCombatKind.Debuff or SkillCombatKind.Missile ? "offensive" : string.Empty;
        return
            $"\"{skill.Slug};{skill.StatsObjectName};{target};{combat.Range};{combat.Cooldown};{combat.Energy};" +
            $"{combat.Reserve};{combat.Duration};{combat.AoeLength};{combat.AoeWidth};{(combat.IsMovement ? "1" : "0")};" +
            $"{pattern};;{cls};{(combat.BonusRange ? "1" : "0")};{starcast};summoner;0;1;utility;" +
            $"{combat.Fumble};{combat.ArmorPenetration};;;;;1;{tags};\",";
    }
}
