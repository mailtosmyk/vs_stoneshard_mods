using System.Text;
using ModShardLauncher;
using ModShardLauncher.Mods;
using Summoner.Skills;

namespace Summoner;

public static class SummonerLocalization
{
    static readonly List<(string Id, string Name, string Desc)> Pending = new();

    public static void PatchAll()
    {
        Pending.Clear();
        foreach (var skill in SummonerSkills.All)
            Pending.Add((skill.Slug, skill.Name, skill.Description));

        Msl.LoadGML("gml_GlobalScript_table_skills")
            .Apply(InjectTableSkillsRows)
            .Save();
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
        var guard = "\"" + Pending[0].Id + ";";

        var names = new StringBuilder();
        var descs = new StringBuilder();
        foreach (var e in Pending)
        {
            names.Append(CsvRow(e.Id, e.Name));
            descs.Append(CsvRow(e.Id, e.Desc));
        }

        foreach (var line in lines)
        {
            var result = line;
            if (result.Contains(nameMarker, StringComparison.Ordinal) &&
                !result.Contains(guard, StringComparison.Ordinal))
                result = result.Replace(nameMarker, names + nameMarker, StringComparison.Ordinal);
            if (result.Contains(descMarker, StringComparison.Ordinal) &&
                !result.Contains(guard, StringComparison.Ordinal))
                result = result.Replace(descMarker, descs + descMarker, StringComparison.Ordinal);
            yield return result;
        }
    }
}
