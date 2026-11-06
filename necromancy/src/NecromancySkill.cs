namespace Necromancy.Skills;

public enum SkillType
{
    Active,
    Passive
}

public enum SkillLogic
{
    None,
    And,
    Or
}

public enum SkillTargetKind
{
    None,
    Object,
    Point,
    Area,
    Ally
}

public enum SkillCombatKind
{
    None,
    Missile,
    Debuff,
    SelfBuff,
    Summon
}

public sealed record AttributeRequirement(
    string[] Stats,
    SkillLogic Logic,
    int Min);

public sealed record SkillRequirements(
    AttributeRequirement[] Attributes,
    string[] SkillIds,
    SkillLogic SkillLogic);

public sealed record SkillTreeTier(
    int Tier,
    string[] SkillIds);

public sealed record SkillTreeEdge(
    string From,
    string To);

public sealed record SkillTree(
    SkillTreeTier[] Tiers,
    SkillTreeEdge[] Edges);

/// <summary>
/// Combat / table_skills_stats fields loaded from necromancy.json.
/// </summary>
public sealed class SkillCombat
{
    public SkillCombat(
        SkillCombatKind kind,
        SkillTargetKind target,
        string range,
        ushort cooldown,
        ushort energy,
        ushort reserve,
        ushort duration,
        byte aoeLength,
        byte aoeWidth,
        bool isMovement,
        string pattern,
        string @class,
        bool bonusRange,
        string starcast,
        short fumble,
        string armorPenetration,
        bool isMissile,
        bool selfCast,
        IReadOnlyDictionary<string, double> numbers)
    {
        Kind = kind;
        Target = target;
        Range = range;
        Cooldown = cooldown;
        Energy = energy;
        Reserve = reserve;
        Duration = duration;
        AoeLength = aoeLength;
        AoeWidth = aoeWidth;
        IsMovement = isMovement;
        Pattern = pattern;
        Class = @class;
        BonusRange = bonusRange;
        Starcast = starcast;
        Fumble = fumble;
        ArmorPenetration = armorPenetration;
        IsMissile = isMissile;
        SelfCast = selfCast;
        Numbers = numbers;
    }

    public SkillCombatKind Kind { get; }
    public SkillTargetKind Target { get; }
    public string Range { get; }
    public ushort Cooldown { get; }
    public ushort Energy { get; }
    public ushort Reserve { get; }
    public ushort Duration { get; }
    public byte AoeLength { get; }
    public byte AoeWidth { get; }
    public bool IsMovement { get; }
    public string Pattern { get; }
    public string Class { get; }
    public bool BonusRange { get; }
    public string Starcast { get; }
    public short Fumble { get; }
    public string ArmorPenetration { get; }
    public bool IsMissile { get; }
    public bool SelfCast { get; }
    public IReadOnlyDictionary<string, double> Numbers { get; }

    public double Number(string key, double fallback = 0) =>
        Numbers.TryGetValue(key, out var value) ? value : fallback;
}

/// <summary>
/// Data-driven necromancy skill loaded from necromancy.json.
/// </summary>
public sealed class NecromancySkill
{
    public NecromancySkill(
        string id,
        string name,
        string slug,
        SkillType type,
        int tier,
        string image,
        string description,
        string magicPhrase,
        SkillRequirements requirements,
        SkillCombat? combat)
    {
        Id = id;
        Name = name;
        Slug = slug;
        Type = type;
        Tier = tier;
        Image = image;
        Description = description;
        MagicPhrase = magicPhrase;
        Requirements = requirements;
        Combat = combat;
    }

    public string Id { get; }
    public string Name { get; }
    public string Slug { get; }
    public SkillType Type { get; }
    public int Tier { get; }
    public string Image { get; }
    public string Description { get; }
    public string MagicPhrase { get; }
    public SkillRequirements Requirements { get; }
    public SkillCombat? Combat { get; }

    public string SkillObjectName => $"o_nm_{Slug}";
    public string IconObjectName => $"o_nm_{Slug}_ico";
    public string BirthObjectName => $"o_nm_{Slug}_birth";
    public string SpriteName => string.IsNullOrWhiteSpace(Image) ? $"s_{Slug}" : Image;
}
