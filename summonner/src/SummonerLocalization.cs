using System.Text;
using ModShardLauncher;
using ModShardLauncher.Mods;
using Summoner.Skills;

namespace Summoner;

public static class SummonerLocalization
{
    static readonly List<(string Id, string Name, string Desc)> Pending = new();
    static readonly List<(string Id, string Name, string Desc)> PendingBuffs = new();

    public static void PatchAll()
    {
        Pending.Clear();
        foreach (var skill in SummonerSkills.All)
            Pending.Add((skill.Slug, skill.Name, skill.Description));

        PendingBuffs.Clear();
        PendingBuffs.Add((
            "o_b_sm_summon_death",
            "Companion's Death",
            "~r~Morale~/~ penalty from a companion dying in combat.##Lasts ~w~600~/~ turns.##Does ~lg~not~/~ stack."));

        PatchSchool();

        Msl.LoadGML("gml_GlobalScript_table_skills")
            .Apply(InjectTableSkillsRows)
            .Save();

        Msl.LoadGML("gml_GlobalScript_table_effects")
            .Apply(InjectTableEffectsRows)
            .Save();
    }

    static void PatchSchool()
    {
        // o_skillmenu Create does:
        //   name = ds_map_find_value(global.tier_name, text)
        //   scr_stringGetHeightExt(name, ...)
        // global.tier_name comes from table_text Tier_name — missing key => undefined crash.
        Msl.LoadGML("gml_GlobalScript_table_text")
            .Apply(InjectSummonerTableTextRows)
            .Save();
    }

    /// <summary>
    /// CSV columns: id;RU;EN;ZH;DE;ES;FR;IT;PT;PL;TR;JA;KO
    /// Id must match category Create `text` / SchoolName ("Summoner").
    /// </summary>
    static IEnumerable<string> InjectSummonerTableTextRows(IEnumerable<string> lines)
    {
        var school = SummonerSkills.SchoolName;
        const string tierMarker = "\";Tier_name_end;";
        const string hoverMarker = "\";skilltree_hover_end;";

        // RU;EN;ZH;DE;ES;FR;IT;PT;PL;TR;JA;KO
        var tierRow =
            "\"" + school + ";Призыватель;" + school + ";召唤师;Beschwörer;Invocador;Invocateur;Evocatore;Invocador;Przywoływacz;Çağırıcı;召喚師;소환사;\", ";

        const string hoverEn =
            "Bind living beasts as companions that keep their own experience and grow with you.##~y~Main focus:~/~#~w~Minion Management~/~, ~w~Support~/~, ~w~Pack Synergy~/~, ~w~Sustain~/~";
        const string hoverRu =
            "Приручайте живых зверей — компаньоны сохраняют опыт и растут вместе с вами.##~y~Особенности:~/~#~w~Управление призывами~/~, ~w~Поддержка~/~, ~w~Синергия стаи~/~, ~w~Устойчивость~/~";
        const string hoverZh =
            "束缚活体野兽作为同伴，它们保留自身经验并与你一同成长。##~y~主要方向:~/~#~w~召唤物管理~/~, ~w~支援~/~, ~w~群体协同~/~, ~w~续航~/~";
        const string hoverDe =
            "Binde lebende Bestien als Gefährten, die eigene Erfahrung behalten und mit dir wachsen.##~y~Fokus:~/~#~w~Dienerverwaltung~/~, ~w~Unterstützung~/~, ~w~Rudel-Synergie~/~, ~w~Durchhaltevermögen~/~";
        const string hoverJa =
            "生きた獣を仲間として束ね、経験を保持したまま共に成長する。##~y~主な特徴:~/~#~w~召喚管理~/~, ~w~支援~/~, ~w~群れの相乗~/~, ~w~持続~/~";

        var hoverRow =
            "\"" + school + ";" + hoverRu + ";" + hoverEn + ";" + hoverZh + ";" + hoverDe + ";" +
            hoverEn + ";" + hoverEn + ";" + hoverEn + ";" + hoverEn + ";" + hoverEn + ";" + hoverEn + ";" +
            hoverJa + ";" + hoverEn + ";\", ";

        var tierGuard = "\"" + school + ";Призыватель;";
        var hoverGuard = "\"" + school + ";" + hoverRu;

        foreach (var line in lines)
        {
            var result = line;
            if (result.Contains(tierMarker, StringComparison.Ordinal) &&
                !result.Contains(tierGuard, StringComparison.Ordinal))
                result = result.Replace(tierMarker, tierRow + tierMarker, StringComparison.Ordinal);

            if (result.Contains(hoverMarker, StringComparison.Ordinal) &&
                !result.Contains(hoverGuard, StringComparison.Ordinal))
                result = result.Replace(hoverMarker, hoverRow + hoverMarker, StringComparison.Ordinal);

            yield return result;
        }
    }

    static string Escape(string value) =>
        (value ?? string.Empty)
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace(";", ".");

    static string CsvRow(string id, string text)
    {
        var t = Escape(text);
        return "\"" + id + ";" + t + ";" + t + ";" + t + ";" + t + ";" +
               t + ";" + t + ";" + t + ";" + t + ";" + t + ";" + t + ";" +
               t + ";" + t + ";\", ";
    }

    static IEnumerable<string> InjectTableSkillsRows(IEnumerable<string> lines)
    {
        const string nameMarker = "\";skill_name_end;";
        const string descMarker = "\";skill_desc_end;";
        // Desc section must not share a slug-only guard with names — both live on the same
        // giant return line, so a shared guard would skip every description after name insert.
        var descGuard = "\"" + Pending[0].Id + ";" + Escape(Pending[0].Desc);

        var names = new StringBuilder();
        var descs = new StringBuilder();
        foreach (var e in Pending)
        {
            names.Append(CsvRow(e.Id, e.Name));
            descs.Append(CsvRow(e.Id, e.Desc));
        }

        var nameText = names.ToString();
        var descText = descs.ToString();

        foreach (var line in lines)
        {
            var result = line;
            if (result.Contains(nameMarker, StringComparison.Ordinal))
            {
                if (SectionHasId(result, Pending[0].Id, "\";skill_name;", nameMarker))
                {
                    foreach (var e in Pending)
                        result = UpsertCsvRow(result, e.Id, e.Name, nameMarker, "\";skill_name;");
                }
                else
                {
                    result = result.Replace(nameMarker, nameText + nameMarker, StringComparison.Ordinal);
                }
            }

            if (result.Contains(descMarker, StringComparison.Ordinal))
            {
                if (SectionHasId(result, Pending[0].Id, "\";skill_desc;", descMarker) ||
                    result.Contains(descGuard, StringComparison.Ordinal))
                {
                    foreach (var e in Pending)
                        result = UpsertCsvRow(result, e.Id, e.Desc, descMarker, "\";skill_desc;");
                }
                else
                {
                    result = result.Replace(descMarker, descText + descMarker, StringComparison.Ordinal);
                }
            }

            yield return result;
        }
    }

    static IEnumerable<string> InjectTableEffectsRows(IEnumerable<string> lines)
    {
        const string nameMarker = "\";buff_name_end;";
        const string descMarker = "\";buff_desc_end;";
        var descGuard = "\"" + PendingBuffs[0].Id + ";" + Escape(PendingBuffs[0].Desc);

        var names = new StringBuilder();
        var descs = new StringBuilder();
        foreach (var e in PendingBuffs)
        {
            names.Append(CsvRow(e.Id, e.Name));
            descs.Append(CsvRow(e.Id, e.Desc));
        }

        var nameText = names.ToString();
        var descText = descs.ToString();

        foreach (var line in lines)
        {
            var result = line;
            if (result.Contains(nameMarker, StringComparison.Ordinal))
            {
                if (SectionHasId(result, PendingBuffs[0].Id, "\";buff_name;", nameMarker))
                {
                    foreach (var e in PendingBuffs)
                        result = UpsertCsvRow(result, e.Id, e.Name, nameMarker, "\";buff_name;");
                }
                else
                {
                    result = result.Replace(nameMarker, nameText + nameMarker, StringComparison.Ordinal);
                }
            }

            if (result.Contains(descMarker, StringComparison.Ordinal))
            {
                if (SectionHasId(result, PendingBuffs[0].Id, "\";buff_desc;", descMarker) ||
                    result.Contains(descGuard, StringComparison.Ordinal))
                {
                    foreach (var e in PendingBuffs)
                        result = UpsertCsvRow(result, e.Id, e.Desc, descMarker, "\";buff_desc;");
                }
                else
                {
                    result = result.Replace(descMarker, descText + descMarker, StringComparison.Ordinal);
                }
            }

            yield return result;
        }
    }

    static bool SectionHasId(
        string line, string id, string sectionStartMarker, string sectionEndMarker)
    {
        var sectionEnd = line.IndexOf(sectionEndMarker, StringComparison.Ordinal);
        if (sectionEnd < 0)
            return false;
        var sectionStart = line.LastIndexOf(sectionStartMarker, sectionEnd, StringComparison.Ordinal);
        if (sectionStart < 0)
            sectionStart = 0;

        var needle = "\"" + id + ";";
        var start = line.IndexOf(needle, sectionStart, StringComparison.Ordinal);
        return start >= 0 && start < sectionEnd;
    }

    static string UpsertCsvRow(
        string line, string id, string text, string sectionEndMarker, string sectionStartMarker)
    {
        var newRow = CsvRow(id, text);
        var sectionEnd = line.IndexOf(sectionEndMarker, StringComparison.Ordinal);
        if (sectionEnd < 0)
            return line;

        var sectionStart = line.LastIndexOf(sectionStartMarker, sectionEnd, StringComparison.Ordinal);
        if (sectionStart < 0)
            sectionStart = 0;

        var needle = "\"" + id + ";";
        var start = line.IndexOf(needle, sectionStart, StringComparison.Ordinal);
        if (start >= 0 && start < sectionEnd)
        {
            var end = line.IndexOf("\",", start, StringComparison.Ordinal);
            if (end < 0 || end > sectionEnd)
                return line;
            return line[..start] + newRow + line[(end + 2)..];
        }

        return line[..sectionEnd] + newRow + line[sectionEnd..];
    }
}
