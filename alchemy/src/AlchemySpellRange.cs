using ModShardLauncher;
using ModShardLauncher.Mods;
using UndertaleModLib.Models;

namespace Alchemy;

/// <summary>
/// Horizon Philter (+1) and Farreach Elixir (+2) spell-range buffs for 60 turns,
/// with visible buff-bar icons and duration counters.
/// </summary>
internal static class AlchemySpellRange
{
    const int BuffTurns = 60;

    static readonly SpellRangeElixir[] Elixirs =
    {
        new("horizon_philter", "o_b_alch_horizon", "s_alch_bf_horizon", 1,
            "Horizon Philter",
            "~lg~+1~/~ Bonus Range for ~w~60~/~ turns."),
        new("farreach_elixir", "o_b_alch_farreach", "s_alch_bf_farreach", 2,
            "Farreach Elixir",
            "~lg~+2~/~ Bonus Range for ~w~60~/~ turns."),
    };

    public static void Patch()
    {
        foreach (var elixir in Elixirs)
            RegisterBuff(elixir);

        InjectBuffLocalization();

        foreach (var elixir in Elixirs)
            HookElixirUse(elixir);
    }

    static void RegisterBuff(SpellRangeElixir elixir)
    {
        var buff = Msl.AddObject(
            name: elixir.BuffObject,
            spriteName: elixir.BuffSprite,
            parentName: "o_magical_buff",
            isVisible: true,
            isPersistent: false,
            isAwake: true,
            collisionShapeFlags: CollisionShapeFlags.Circle);

        var magicalBuff = Msl.GetObject("o_magical_buff");
        if (magicalBuff is not null)
            buff.ParentId = magicalBuff;

        SetEvent(buff, EventType.Create, 0, string.Join("\n",
            "event_inherited()",
            "scr_buff_atr()",
            "stack = 1",
            "buff_snd = -4",
            "have_duration = 1",
            "draw_duration = true",
            $"max_duration = {BuffTurns}"));

        SetEvent(buff, EventType.Other, 10, string.Join("\n",
            "event_inherited()",
            "duration = min(duration, max_duration)"));

        SetEvent(buff, EventType.Alarm, 2, string.Join("\n",
            "event_inherited()",
            "with (target)",
            "{",
            "    ds_map_clear(other.data)",
            $"    ds_map_add(other.data, \"Bonus_Range\", {elixir.BonusRange})",
            "    stats_is_change = true",
            "    scr_atr_calc(id)",
            "}"));
    }

    static void HookElixirUse(SpellRangeElixir elixir)
    {
        var inv = Msl.GetObject($"o_inv_{elixir.Id}");
        if (inv is null)
            return;

        // Apply after the vanilla drink path so the item is consumed normally.
        var gml = string.Join("\n",
            "event_inherited()",
            "with (o_player)",
            "{",
            "    scr_delete_buff_type(o_b_alch_horizon)",
            "    scr_delete_buff_type(o_b_alch_farreach)",
            "}",
            $"scr_effect_create({elixir.BuffObject}, {BuffTurns}, o_player, o_player)");

        try
        {
            Msl.AddNewEvent(inv, gml, EventType.Other, 24);
        }
        catch (ArgumentException)
        {
            // Re-patch: refresh body below.
        }

        var codeName = Msl.EventName(inv.Name.Content, EventType.Other, 24);
        var lines = gml.Split('\n');
        Msl.LoadGML(codeName)
            .Apply(_ => lines)
            .Save();
    }

    static void InjectBuffLocalization()
    {
        Msl.LoadGML("gml_GlobalScript_table_effects")
            .Apply(InjectEffectsRows)
            .Save();
    }

    static IEnumerable<string> InjectEffectsRows(IEnumerable<string> lines)
    {
        const string nameMarker = "\";buff_name_end;";
        const string descMarker = "\";buff_desc_end;";
        const string nameGuard = "\"o_b_alch_horizon;";

        var nameInsert = new System.Text.StringBuilder();
        var descInsert = new System.Text.StringBuilder();
        foreach (var elixir in Elixirs)
        {
            nameInsert.Append(EffectsCsvRow(elixir.BuffObject, elixir.BuffName));
            descInsert.Append(EffectsCsvRow(elixir.BuffObject, elixir.BuffDesc));
        }

        var nameText = nameInsert.ToString();
        var descText = descInsert.ToString();

        foreach (var line in lines)
        {
            var result = line;
            var alreadyPatched = result.Contains(nameGuard, StringComparison.Ordinal);

            if (result.Contains(nameMarker, StringComparison.Ordinal))
            {
                if (alreadyPatched)
                {
                    foreach (var elixir in Elixirs)
                        result = UpsertEffectsCsvRow(result, elixir.BuffObject, elixir.BuffName, nameMarker);
                }
                else
                {
                    result = result.Replace(nameMarker, nameText + nameMarker, StringComparison.Ordinal);
                }
            }

            if (result.Contains(descMarker, StringComparison.Ordinal))
            {
                if (alreadyPatched || result.Contains(nameGuard, StringComparison.Ordinal))
                {
                    foreach (var elixir in Elixirs)
                        result = UpsertEffectsCsvRow(result, elixir.BuffObject, elixir.BuffDesc, descMarker);
                }
                else
                {
                    result = result.Replace(descMarker, descText + descMarker, StringComparison.Ordinal);
                }
            }

            yield return result;
        }
    }

    static string UpsertEffectsCsvRow(string line, string id, string text, string sectionEndMarker)
    {
        var newRow = EffectsCsvRow(id, text);
        var sectionEnd = line.IndexOf(sectionEndMarker, StringComparison.Ordinal);
        if (sectionEnd < 0)
            return line;

        var sectionStartMarker = sectionEndMarker.Contains("buff_name", StringComparison.Ordinal)
            ? "\";buff_name;"
            : "\";buff_desc;";
        var sectionStart = line.LastIndexOf(sectionStartMarker, sectionEnd, StringComparison.Ordinal);
        if (sectionStart < 0)
            sectionStart = 0;

        var needle = "\"" + id + ";";
        var start = line.IndexOf(needle, sectionStart, StringComparison.Ordinal);
        if (start >= 0 && start < sectionEnd)
        {
            var end = line.IndexOf("\",", start, StringComparison.Ordinal);
            if (end < 0 || end > sectionEnd)
                end = sectionEnd;
            else
                end += 2;
            return line[..start] + newRow + line[end..];
        }

        return line.Replace(sectionEndMarker, newRow + sectionEndMarker, StringComparison.Ordinal);
    }

    static string EffectsCsvRow(string id, string text)
    {
        var escaped = text.Replace("\\", "\\\\").Replace("\"", "\\\"");
        // id;RU;EN;ZH;DE;ES;FR;IT;PT;PL;TR;JA;KO
        return "\"" + id + ";" + escaped + ";" + escaped + ";" + escaped + ";" + escaped + ";" +
               escaped + ";" + escaped + ";" + escaped + ";" + escaped + ";" + escaped + ";" + escaped + ";" +
               escaped + ";" + escaped + ";\", ";
    }

    static void SetEvent(UndertaleGameObject obj, EventType type, uint subtype, string gml)
    {
        try
        {
            Msl.AddNewEvent(obj, gml, type, subtype);
        }
        catch (ArgumentException)
        {
            // Already present from a previous patch.
        }

        var codeName = Msl.EventName(obj.Name.Content, type, subtype);
        var lines = gml.Split('\n');
        Msl.LoadGML(codeName)
            .Apply(_ => lines)
            .Save();
    }

    sealed record SpellRangeElixir(
        string Id,
        string BuffObject,
        string BuffSprite,
        int BonusRange,
        string BuffName,
        string BuffDesc);
}
