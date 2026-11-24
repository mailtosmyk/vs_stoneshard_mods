using System.Text.RegularExpressions;
using ModShardLauncher;

namespace Summoner;

/// <summary>
/// Credit player XP when a Summoner beast (sm_summon + player owner) lands the kill, and
/// feed the species companion XP track via scr_sm_on_enemy_kill_xp.
/// Mirrors Necromancy's <c>o_enemy_Destroy</c> assembly patch; coexist with Necromancy load order.
/// </summary>
public static class SummonerKillExperience
{
    static readonly Regex LabelDef = new(@"^\s*:\[(\d+)\]\s*$", RegexOptions.Compiled);
    static readonly Regex BranchFail = new(@"^\s*bf\s+\[(\d+)\]\s*$", RegexOptions.Compiled);
    static readonly Regex BranchAlways = new(@"^\s*b\s+\[(\d+)\]\s*$", RegexOptions.Compiled);

    public static void Patch()
    {
        Msl.LoadAssemblyAsString("gml_Object_o_enemy_Destroy_0")
            .Apply(SummonKillExperienceAsm)
            .Save();

        Msl.LoadGML("gml_Object_o_enemy_Destroy_0")
            .Apply(InsertCompanionKillXpHook)
            .Save();
    }

    static IEnumerable<string> InsertCompanionKillXpHook(IEnumerable<string> lines)
    {
        var list = lines as IList<string> ?? lines.ToList();
        if (list.Any(l => l.Contains("scr_sm_on_enemy_kill_xp", StringComparison.Ordinal)))
            return list;

        var hook = "scr_sm_on_enemy_kill_xp()\n";
        var output = new List<string>(list.Count + 2);
        foreach (var line in list)
        {
            output.Add(line);
            if (line.Contains("event_inherited()", StringComparison.Ordinal))
                output.Add(hook);
        }

        if (!output.Any(l => l.Contains("scr_sm_on_enemy_kill_xp", StringComparison.Ordinal)))
        {
            throw new InvalidOperationException(
                "Summoner: could not insert scr_sm_on_enemy_kill_xp in gml_Object_o_enemy_Destroy_0");
        }

        return output;
    }

    /// <summary>
    /// Vanilla credits XP when last_attacker is o_summoned_entity owned by the player.
    /// When that instance_is check fails it pushes false ([156] push.e 0). Extend that path
    /// for nm_summon and sm_summon thralls owned by the player (Necromancy + Summoner).
    /// </summary>
    static IEnumerable<string> SummonKillExperienceAsm(IEnumerable<string> input)
    {
        var lines = input as IList<string> ?? input.ToList();

        // Necromancy already extended this block with nm_summon — add sm_summon only.
        // After Necromancy's patch is saved/reloaded, UTMT renumbers :[9002]/:[9003],
        // so discover the fail/success labels from the live nm_summon block.
        if (lines.Any(l => l.Contains("nm_summon", StringComparison.Ordinal)) &&
            !lines.Any(l => l.Contains("sm_summon", StringComparison.Ordinal)))
        {
            return InsertSmSummonBeforeFailPush(lines);
        }

        if (lines.Any(l => l.Contains("sm_summon", StringComparison.Ordinal)))
            return lines;

        return PatchVanillaSummonCreditSite(lines);
    }

    static IEnumerable<string> InsertSmSummonBeforeFailPush(IList<string> lines)
    {
        var nmIdx = -1;
        for (var i = 0; i < lines.Count; i++)
        {
            if (lines[i].Contains("nm_summon", StringComparison.Ordinal))
                nmIdx = i;
        }

        if (nmIdx < 0)
        {
            throw new InvalidOperationException(
                "Summoner: nm_summon present but index lost in gml_Object_o_enemy_Destroy_0");
        }

        // After the last nm_summon reference: collect bf targets and the following b target,
        // then find :[fail] / push.e 0 / :[success].
        int? failLabel = null;
        int? successLabel = null;
        for (var i = nmIdx; i < lines.Count && i < nmIdx + 40; i++)
        {
            var bf = BranchFail.Match(lines[i]);
            if (bf.Success)
                failLabel = int.Parse(bf.Groups[1].Value);

            var b = BranchAlways.Match(lines[i]);
            if (b.Success)
            {
                successLabel = int.Parse(b.Groups[1].Value);
                break;
            }
        }

        if (failLabel is null || successLabel is null)
        {
            throw new InvalidOperationException(
                "Summoner: could not resolve Necromancy summon XP fail/success labels in gml_Object_o_enemy_Destroy_0");
        }

        var failDef = $":[{failLabel.Value}]";
        var failDefIdx = -1;
        for (var i = nmIdx; i < lines.Count; i++)
        {
            if (lines[i].Trim() != failDef)
                continue;
            if (i + 1 < lines.Count && lines[i + 1].Trim() == "push.e 0")
            {
                failDefIdx = i;
                break;
            }
        }

        if (failDefIdx < 0)
        {
            throw new InvalidOperationException(
                $"Summoner: expected Necromancy summon XP fail label {failDef} before push.e 0 in gml_Object_o_enemy_Destroy_0");
        }

        var block = BuildSummonOwnerIsPlayerAsm("sm_summon", failLabel.Value, successLabel.Value);
        var output = new List<string>(lines.Count + 24);
        output.AddRange(lines.Take(failDefIdx));
        foreach (var line in block.Split('\n'))
            output.Add(line);
        output.AddRange(lines.Skip(failDefIdx));
        return output;
    }

    static string BuildSummonOwnerIsPlayerAsm(string flag, int failLabel, int successLabel) => $@"
push.v self.last_attacker
call.i instance_exists(argc=1)
conv.v.b
bf [{failLabel}]

push.s ""{flag}""
conv.s.v
push.v self.last_attacker
call.i variable_instance_exists(argc=2)
conv.v.b
bf [{failLabel}]

push.s ""{flag}""
conv.s.v
push.v self.last_attacker
call.i variable_instance_get(argc=2)
conv.v.b
bf [{failLabel}]

push.v self.last_attacker
pushi.e -9
push.v [stacktop]self.owner
call.i gml_Script_is_player(argc=1)
conv.v.b
b [{successLabel}]
";

    static IEnumerable<string> PatchVanillaSummonCreditSite(IList<string> lines)
    {
        var status = 0;
        var output = new List<string>(lines.Count + 64);
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

push.v self.last_attacker
call.i instance_exists(argc=1)
conv.v.b
bf [9002]

push.s ""sm_summon""
conv.s.v
push.v self.last_attacker
call.i variable_instance_exists(argc=2)
conv.v.b
bf [9002]

push.s ""sm_summon""
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
            throw new InvalidOperationException(
                "Summoner: summon XP assembly patch site not found in gml_Object_o_enemy_Destroy_0");
        }

        return output;
    }
}
