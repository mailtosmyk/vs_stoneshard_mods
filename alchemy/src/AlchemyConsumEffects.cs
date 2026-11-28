using System.Globalization;
using System.Text.Json;
using ModShardLauncher;

namespace Alchemy;

/// <summary>
/// Writes structured elixir effects from ingredients.json into table_items_stats
/// columns (max_hp_res, Pain, Duration, …). MSL InjectTableItemStats only sets
/// price / category / tags, so without this step custom drinks have no gameplay effect.
/// </summary>
internal static class AlchemyConsumEffects
{
    // Column indices in current Stoneshard 0.9.4 table_items_stats (107 cols).
    const int ColDuration = 11;
    const int ColThirsty = 19;
    const int ColThirstChange = 20;
    const int ColIntoxication = 25;
    const int ColToxicityChange = 26;
    const int ColPain = 29;
    const int ColPainChange = 30;
    const int ColPainResistance = 31;
    const int ColPainLimit = 32;
    const int ColMoraleSituational = 34;
    const int ColMoraleChange = 35;
    const int ColMoraleTemporary = 36;
    const int ColSanitySituational = 38;
    const int ColSanityChange = 39;
    const int ColMaxHpRes = 43;
    const int ColHpTurn = 44;
    const int ColHealthRestoration = 45;
    const int ColMaxMpRes = 49;
    const int ColMpRestoration = 50;
    const int ColMpTurn = 51;
    const int ColFatigueChange = 54;
    const int ColFatigueGain = 55;
    const int ColWeaponDamage = 59;
    const int ColMagicPower = 60;
    const int ColBleedResistance = 67;
    const int ColPhysicalResistance = 71;
    const int ColNatureResistance = 72;
    const int ColMagicResistance = 73;
    const int ColFireResistance = 78;
    const int ColFrostResistance = 82;
    const int ColSlingAmmo = 92;
    const int ColIsLiquid = 94;
    const int ColBottle = 99;

    static readonly Dictionary<string, int> InstantKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        ["add_thirst"] = ColThirsty,
        ["add_intoxication"] = ColIntoxication,
        ["add_pain"] = ColPain,
        ["add_morale"] = ColMoraleSituational,
        ["add_temporary_morale"] = ColMoraleTemporary,
        ["add_sanity"] = ColSanitySituational,
        ["add_health_replenishment"] = ColMaxHpRes,
        ["add_max_health_replenish_pct"] = ColMaxHpRes,
        ["add_energy_replenishment"] = ColMaxMpRes,
        ["add_max_energy_replenish_pct"] = ColMaxMpRes,
    };

    static readonly Dictionary<string, int> TimedKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        ["add_thirst_change"] = ColThirstChange,
        ["add_intoxication_change"] = ColToxicityChange,
        ["add_pain_change"] = ColPainChange,
        ["add_pain_resistance"] = ColPainResistance,
        ["add_pain_limit"] = ColPainLimit,
        ["add_morale_change"] = ColMoraleChange,
        ["add_sanity_change"] = ColSanityChange,
        ["add_health_restoration"] = ColHealthRestoration,
        ["add_health_per_turn"] = ColHpTurn,
        ["add_energy_restoration"] = ColMpRestoration,
        ["add_mp_turn"] = ColMpTurn,
        ["add_fatigue_change"] = ColFatigueChange,
        ["add_fatigue_gain"] = ColFatigueGain,
        ["add_weapon_damage"] = ColWeaponDamage,
        ["add_magic_power"] = ColMagicPower,
        ["add_bleed_resistance"] = ColBleedResistance,
        ["add_physical_resistance"] = ColPhysicalResistance,
        ["add_nature_resistance"] = ColNatureResistance,
        ["add_magic_resistance"] = ColMagicResistance,
        ["add_fire_resistance"] = ColFireResistance,
        ["add_frost_resistance"] = ColFrostResistance,
    };

    public static void Patch()
    {
        var patches = BuildPatches();
        if (patches.Count == 0)
            return;

        Msl.LoadGML("gml_GlobalScript_table_items_stats")
            .Apply(lines => lines.Select(line => PatchLine(line, patches)))
            .Save();
    }

    static Dictionary<string, Dictionary<int, string>> BuildPatches()
    {
        var result = new Dictionary<string, Dictionary<int, string>>(StringComparer.OrdinalIgnoreCase);
        foreach (var dto in AlchemyCatalog.Root.Elixirs)
        {
            if (string.IsNullOrWhiteSpace(dto.Id) || string.IsNullOrWhiteSpace(dto.Icon))
                continue;
            if (!dto.Icon.Contains("alch", StringComparison.OrdinalIgnoreCase))
                continue;
            if (dto.Effects is null || dto.Effects.Count == 0)
                continue;

            var cols = MapEffects(dto.Effects);
            // Drinkable glassware flags matching vanilla aqua_vitae / essence.
            cols.TryAdd(ColSlingAmmo, "1");
            cols.TryAdd(ColIsLiquid, "1");
            cols.TryAdd(ColBottle, "1");
            result[dto.Id] = cols;
        }

        return result;
    }

    static Dictionary<int, string> MapEffects(List<Dictionary<string, JsonElement>> effects)
    {
        var cols = new Dictionary<int, string>();
        var maxTurns = 0;

        foreach (var effect in effects)
        {
            var turns = 0;
            if (effect.TryGetValue("turns", out var turnsEl) && TryNumber(turnsEl, out var t))
                turns = (int)Math.Round(t);

            foreach (var (key, el) in effect)
            {
                if (string.Equals(key, "turns", StringComparison.OrdinalIgnoreCase))
                    continue;
                if (!TryNumber(el, out var value))
                    continue;

                if (turns > 0 && TimedKeys.TryGetValue(key, out var timedCol))
                {
                    cols[timedCol] = Format(value);
                    maxTurns = Math.Max(maxTurns, turns);
                }
                else if (InstantKeys.TryGetValue(key, out var instantCol))
                {
                    cols[instantCol] = Format(value);
                }
                else if (TimedKeys.TryGetValue(key, out var alsoTimed))
                {
                    // Timed key without turns — still write the column.
                    cols[alsoTimed] = Format(value);
                }
            }
        }

        if (maxTurns > 0)
            cols[ColDuration] = maxTurns.ToString(CultureInfo.InvariantCulture);

        return cols;
    }

    static bool TryNumber(JsonElement el, out double value)
    {
        switch (el.ValueKind)
        {
            case JsonValueKind.Number:
                value = el.GetDouble();
                return true;
            case JsonValueKind.String:
                return double.TryParse(el.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out value);
            case JsonValueKind.True:
                value = 1;
                return true;
            case JsonValueKind.False:
                value = 0;
                return true;
            default:
                value = 0;
                return false;
        }
    }

    static string Format(double value)
    {
        if (Math.Abs(value - Math.Round(value)) < 0.0001)
            return ((long)Math.Round(value)).ToString(CultureInfo.InvariantCulture);
        return value.ToString("0.##", CultureInfo.InvariantCulture);
    }

    static string PatchLine(string line, Dictionary<string, Dictionary<int, string>> patches)
    {
        var sb = new System.Text.StringBuilder(line.Length + 64);
        var i = 0;
        while (i < line.Length)
        {
            if (line[i] != '"')
            {
                sb.Append(line[i]);
                i++;
                continue;
            }

            var close = FindClosingQuote(line, i);
            if (close < 0)
            {
                sb.Append(line.AsSpan(i));
                break;
            }

            var inner = line.Substring(i + 1, close - (i + 1));
            if (inner.Contains(';'))
            {
                var idEnd = inner.IndexOf(';');
                if (idEnd > 0)
                {
                    var id = inner[..idEnd];
                    if (patches.TryGetValue(id, out var cols))
                        inner = ApplyColumns(inner, cols);
                }
            }

            sb.Append('"').Append(inner).Append('"');
            i = close + 1;
        }

        return sb.ToString();
    }

    static string ApplyColumns(string inner, Dictionary<int, string> cols)
    {
        var parts = inner.Split(';');
        if (parts.Length < 106)
            return inner;

        foreach (var (index, value) in cols)
        {
            if (index >= 0 && index < parts.Length)
                parts[index] = value;
        }

        return string.Join(";", parts);
    }

    static int FindClosingQuote(string line, int openQuote)
    {
        for (var i = openQuote + 1; i < line.Length; i++)
        {
            if (line[i] == '\\' && i + 1 < line.Length)
            {
                i++;
                continue;
            }

            if (line[i] == '"')
                return i;
        }

        return -1;
    }
}
