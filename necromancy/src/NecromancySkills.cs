using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Necromancy.Skills;

/// <summary>
/// Registry of necromancy skills, built dynamically from necromancy.json.
/// </summary>
public static class NecromancySkills
{
    static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public static string SchoolId { get; }
    public static string SchoolName { get; }
    public static SkillTree Tree { get; }
    public static NecromancySkill[] All { get; }

    static NecromancySkills()
    {
        var root = LoadRoot();
        SchoolId = root.Id ?? string.Empty;
        SchoolName = root.Name ?? "Necromancy";
        Tree = MapTree(root.Tree);
        All = (root.Skills ?? Array.Empty<SkillDto>())
            .Select(MapSkill)
            .ToArray();
    }

    static NecromancyJsonRoot LoadRoot()
    {
        using var stream = OpenDataStream();
        var root = JsonSerializer.Deserialize<NecromancyJsonRoot>(stream, JsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize necromancy.json.");
        return root;
    }

    static Stream OpenDataStream()
    {
        var asm = Assembly.GetExecutingAssembly();
        var dir = Path.GetDirectoryName(asm.Location);

        if (!string.IsNullOrEmpty(dir))
        {
            var besideAssembly = Path.Combine(dir, "necromancy.json");
            if (File.Exists(besideAssembly))
                return File.OpenRead(besideAssembly);

            // bin/Release/net6.0 -> project root
            var projectJson = Path.GetFullPath(Path.Combine(dir, "..", "..", "..", "necromancy.json"));
            if (File.Exists(projectJson))
                return File.OpenRead(projectJson);
        }

        // Injected by build-sml (msp compiles .cs only — no EmbeddedResource).
        if (NecromancyJsonResource.TryOpen(out var generated))
            return generated;

        var resourceName = asm.GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith("necromancy.json", StringComparison.OrdinalIgnoreCase));
        if (resourceName is not null)
        {
            var embedded = asm.GetManifestResourceStream(resourceName);
            if (embedded is not null)
                return embedded;
        }

        throw new FileNotFoundException(
            "necromancy.json not found beside the assembly, as NecromancyJsonResource, or as an embedded resource. Run build-sml.sh / build-sml.bat.");
    }

    static NecromancySkill MapSkill(SkillDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Id))
            throw new InvalidOperationException("Skill entry is missing id.");
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new InvalidOperationException($"Skill '{dto.Id}' is missing name.");
        if (string.IsNullOrWhiteSpace(dto.Slug))
            throw new InvalidOperationException($"Skill '{dto.Id}' is missing slug.");

        var requirements = MapRequirements(dto.Requirements);
        return new NecromancySkill(
            dto.Id,
            dto.Name,
            dto.Slug,
            dto.Type,
            dto.Tier,
            dto.Image ?? string.Empty,
            dto.Description ?? string.Empty,
            dto.MagicPhrase ?? string.Empty,
            requirements);
    }

    static SkillRequirements MapRequirements(RequirementsDto? dto)
    {
        if (dto is null)
        {
            return new SkillRequirements(
                Array.Empty<AttributeRequirement>(),
                Array.Empty<string>(),
                SkillLogic.None);
        }

        var attributes = (dto.Attributes ?? Array.Empty<AttributeRequirementDto>())
            .Select(a => new AttributeRequirement(
                a.Stats ?? Array.Empty<string>(),
                a.Logic,
                a.Min))
            .ToArray();

        return new SkillRequirements(
            attributes,
            dto.Skills ?? Array.Empty<string>(),
            dto.SkillLogic);
    }

    static SkillTree MapTree(TreeDto? dto)
    {
        if (dto is null)
            return new SkillTree(Array.Empty<SkillTreeTier>(), Array.Empty<SkillTreeEdge>());

        var tiers = (dto.Tiers ?? Array.Empty<TreeTierDto>())
            .Select(t => new SkillTreeTier(t.Tier, t.Skills ?? Array.Empty<string>()))
            .ToArray();

        var edges = (dto.Edges ?? Array.Empty<TreeEdgeDto>())
            .Select(e => new SkillTreeEdge(e.From ?? string.Empty, e.To ?? string.Empty))
            .ToArray();

        return new SkillTree(tiers, edges);
    }

    sealed class NecromancyJsonRoot
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public SkillDto[]? Skills { get; set; }
        public TreeDto? Tree { get; set; }
    }

    sealed class SkillDto
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Slug { get; set; }
        public SkillType Type { get; set; }
        public int Tier { get; set; }
        public string? Image { get; set; }
        public string? Description { get; set; }
        public string? MagicPhrase { get; set; }
        public RequirementsDto? Requirements { get; set; }
    }

    sealed class RequirementsDto
    {
        public AttributeRequirementDto[]? Attributes { get; set; }
        public string[]? Skills { get; set; }
        public SkillLogic SkillLogic { get; set; }
    }

    sealed class AttributeRequirementDto
    {
        public string[]? Stats { get; set; }
        public SkillLogic Logic { get; set; }
        public int Min { get; set; }
    }

    sealed class TreeDto
    {
        public TreeTierDto[]? Tiers { get; set; }
        public TreeEdgeDto[]? Edges { get; set; }
    }

    sealed class TreeTierDto
    {
        public int Tier { get; set; }
        public string[]? Skills { get; set; }
    }

    sealed class TreeEdgeDto
    {
        public string? From { get; set; }
        public string? To { get; set; }
    }
}
