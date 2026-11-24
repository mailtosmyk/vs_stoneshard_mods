using System.Text.RegularExpressions;
using ModShardLauncher;

namespace Summoner;

/// <summary>
/// Credit player XP when a Summoner beast (sm_summon + player owner) lands the kill, and
/// feed the species companion XP track via scr_sm_on_enemy_kill_xp.
/// Assembly-only: o_enemy Destroy does not round-trip through LoadGML / ReplaceGML.
/// </summary>
public static class SummonerKillExperience
{
    static readonly Regex BranchFail = new(@"^\s*bf\s+\[(\d+)\]\s*$", RegexOptions.Compiled);
    static readonly Regex BranchAlways = new(@"^\s*b\s+\[(\d+)\]\s*$", RegexOptions.Compiled);

    const string CompanionXpCall = "call.i gml_Script_scr_sm_on_enemy_kill_xp(argc=0)";

    public static void Patch()
    {
        Msl.LoadAssemblyAsString("gml_Object_o_enemy_Destroy_0")
            .Apply(SummonKillExperienceAsm)
            .Save();
    }

    /// <summary>
    /// Vanilla credits XP when last_attacker is o_summoned_entity owned by the player.
    /// Extend that path for nm_summon / sm_summon, then hook companion XP after event_inherited.
    /// </summary>
    static IEnumerable<string> SummonKillExperienceAsm(IEnumerable<string> input)
    {
        var lines = input as IList<string> ?? input.ToList();

        if (lines.Any(l => l.Contains("nm_summon", StringComparison.Ordinal)) &&
            !lines.Any(l => l.Contains("sm_summon", StringComparison.Ordinal)))
        {
            lines = InsertSmSummonBeforeFailPush(lines).ToList();
        }
        else if (!lines.Any(l => l.Contains("sm_summon", StringComparison.Ordinal)))
        {
            lines = PatchVanillaSummonCreditSite(lines).ToList();
        }

        return InsertCompanionKillXpAsm(lines);
    }

    static IEnumerable<string> InsertCompanionKillXpAsm(IList<string> lines)
    {
        if (lines.Any(l => l.Contains("scr_sm_on_enemy_kill_xp", StringComparison.Ordinal)))
            return lines;

        var idx = -1;
        for (var i = 0; i < lines.Count - 1; i++)
        {
            if (lines[i].Contains("call.i event_inherited(argc=0)", StringComparison.Ordinal)
                && lines[i + 1].Trim() == "popz.v")
            {
                idx = i + 1;
                break;
            }
        }

        if (idx < 0)
        {
            throw new InvalidOperationException(
                "Summoner: event_inherited site not found in gml_Object_o_enemy_Destroy_0");
        }

        var output = new List<string>(lines.Count + 3);
        output.AddRange(lines.Take(idx + 1));
        output.Add(CompanionXpCall);
        output.Add("popz.v");
        output.AddRange(lines.Skip(idx + 1));
        return output;
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
