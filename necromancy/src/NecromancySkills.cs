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
    /// <summary>Skill-menu meta column from JSON root <c>target</c> (e.g. Sorcery).</summary>
    public static string SchoolTarget { get; }
    public static SkillTree Tree { get; }
    public static NecromancySkill[] All { get; }

    static NecromancySkills()
    {
        var root = LoadRoot();
        SchoolId = root.Id ?? string.Empty;
        SchoolName = root.Name ?? "Necromancy";
        SchoolTarget = string.IsNullOrWhiteSpace(root.Target) ? "Sorcery" : root.Target.Trim();
        Tree = MapTree(root.Tree);
        All = (root.Skills ?? Array.Empty<SkillDto>())
            .Select(MapSkill)
            .ToArray();
    }

    public static IEnumerable<NecromancySkill> ByTier(int tier) =>
        All.Where(s => s.Tier == tier);

    public static NecromancySkill? FindBySlug(string slug) =>
        All.FirstOrDefault(s => string.Equals(s.Slug, slug, StringComparison.OrdinalIgnoreCase));

    public static NecromancySkill? FindById(string id) =>
        All.FirstOrDefault(s => string.Equals(s.Id, id, StringComparison.OrdinalIgnoreCase));

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

            // bin/Release/net6.0 -> src/necromancy.json
            var projectJson = Path.GetFullPath(Path.Combine(dir, "..", "..", "..", "src", "necromancy.json"));
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

        return new NecromancySkill(
            dto.Id,
            dto.Name,
            dto.Slug,
            dto.Type,
            dto.Tier,
            dto.Image ?? string.Empty,
            dto.Description ?? string.Empty,
            dto.MagicPhrase ?? string.Empty,
            MapRequirements(dto.Requirements),
            MapCombat(dto.Combat));
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

    static SkillCombat? MapCombat(CombatDto? dto)
    {
        if (dto is null)
            return null;

        var numbers = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        void Add(string key, double? value)
        {
            if (value.HasValue)
                numbers[key] = value.Value;
        }

        Add("baseDamage", dto.BaseDamage);
        Add("energyBurnRatio", dto.EnergyBurnRatio);
        Add("buffTurnsMin", dto.BuffTurnsMin);
        Add("buffTurnsMax", dto.BuffTurnsMax);
        Add("buffTurns", dto.BuffTurns);
        Add("physRes", dto.PhysRes);
        Add("bleedRes", dto.BleedRes);
        Add("magicRes", dto.MagicRes);
        Add("moveRes", dto.MoveRes);
        Add("shardUnholy", dto.ShardUnholy);
        Add("shardPiercing", dto.ShardPiercing);
        Add("staggerChance", dto.StaggerChance);
        Add("baseTurns", dto.BaseTurns);
        Add("baseHpPct", dto.BaseHpPct);
        Add("baseMpPct", dto.BaseMpPct);
        Add("sanityFloor", dto.SanityFloor);
        Add("fortitude", dto.Fortitude);
        Add("maxHpPct", dto.MaxHpPct);
        Add("damageTaken", dto.DamageTaken);
        Add("bodypartWorsen", dto.BodypartWorsen);
        Add("maxStage", dto.MaxStage);

        return new SkillCombat(
            ParseCombatKind(dto.Kind),
            ParseTarget(dto.Target),
            dto.Range ?? "0",
            dto.Cooldown,
            dto.Energy,
            dto.Reserve,
            dto.Duration,
            dto.AoeLength,
            dto.AoeWidth,
            dto.IsMovement,
            dto.Pattern ?? "normal",
            dto.Class ?? "spell",
            dto.BonusRange,
            dto.Starcast ?? string.Empty,
            dto.Fumble,
            dto.ArmorPenetration ?? "x",
            dto.IsMissile,
            dto.SelfCast,
            numbers);
    }

    static SkillCombatKind ParseCombatKind(string? kind) => kind?.Trim().ToLowerInvariant() switch
    {
        "missile" => SkillCombatKind.Missile,
        "debuff" => SkillCombatKind.Debuff,
        "self_buff" or "selfbuff" => SkillCombatKind.SelfBuff,
        "summon" => SkillCombatKind.Summon,
        _ => SkillCombatKind.None
    };

    static SkillTargetKind ParseTarget(string? target) => target?.Trim().ToLowerInvariant() switch
    {
        "object" => SkillTargetKind.Object,
        "point" => SkillTargetKind.Point,
        "area" => SkillTargetKind.Area,
        "ally" => SkillTargetKind.Ally,
        "none" or "no" or "notarget" => SkillTargetKind.None,
        _ => SkillTargetKind.None
    };

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
        public string? Target { get; set; }
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
        public CombatDto? Combat { get; set; }
    }

    sealed class CombatDto
    {
        public string? Kind { get; set; }
        public string? Target { get; set; }
        public string? Range { get; set; }
        public ushort Cooldown { get; set; }
        public ushort Energy { get; set; }
        public ushort Reserve { get; set; }
        public ushort Duration { get; set; }
        public byte AoeLength { get; set; }
        public byte AoeWidth { get; set; }
        public bool IsMovement { get; set; }
        public string? Pattern { get; set; }
        public string? Class { get; set; }
        public bool BonusRange { get; set; }
        public string? Starcast { get; set; }
        public short Fumble { get; set; }
        public string? ArmorPenetration { get; set; }
        public bool IsMissile { get; set; }
        public bool SelfCast { get; set; }

        public double? BaseDamage { get; set; }
        public double? EnergyBurnRatio { get; set; }
        public double? BuffTurnsMin { get; set; }
        public double? BuffTurnsMax { get; set; }
        public double? BuffTurns { get; set; }
        public double? PhysRes { get; set; }
        public double? BleedRes { get; set; }
        public double? MagicRes { get; set; }
        public double? MoveRes { get; set; }
        public double? ShardUnholy { get; set; }
        public double? ShardPiercing { get; set; }
        public double? StaggerChance { get; set; }
        public double? BaseTurns { get; set; }
        public double? BaseHpPct { get; set; }
        public double? BaseMpPct { get; set; }
        public double? SanityFloor { get; set; }
        public double? Fortitude { get; set; }
        public double? MaxHpPct { get; set; }
        public double? DamageTaken { get; set; }
        public double? BodypartWorsen { get; set; }
        public double? MaxStage { get; set; }
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
