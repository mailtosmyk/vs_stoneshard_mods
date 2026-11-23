using ModShardLauncher;

namespace Summoner;

/// <summary>
/// Credit player XP when a Summoner beast (sm_summon + player owner) lands the kill, and
/// feed the species companion XP track via scr_sm_on_enemy_kill_xp.
/// Mirrors Necromancy's <c>o_enemy_Destroy</c> assembly patch; coexist with Necromancy load order.
/// </summary>
public static class SummonerKillExperience
{
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
        const string failLabel = ":[9002]";
        var output = new List<string>(lines.Count + 24);
        var inserted = false;
        foreach (var item in lines)
        {
            if (!inserted && item.Trim() == failLabel)
            {
                output.Add(SmSummonOwnerIsPlayerAsm);
                inserted = true;
            }

            output.Add(item);
        }

        if (!inserted)
        {
            throw new InvalidOperationException(
                "Summoner: expected Necromancy summon XP label :[9002] in gml_Object_o_enemy_Destroy_0");
        }

        return output;
    }

    const string SmSummonOwnerIsPlayerAsm = @"
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