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
/// Note: JSON <c>reserve</c> maps to CSV <c>is_moving_zone</c> (not energy reservation).
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
    public string BuffObjectName => $"o_b_{Slug}";
    public string SpriteName => string.IsNullOrWhiteSpace(Image) ? $"s_{Slug}" : Image;

    /// <summary>
    /// skills_stats Object column. No Target self-buffs must point at the buff
    /// (guiInteractive) — birth objects make scr_guiCreateInteractive GUI ERROR.
    /// </summary>
    public string StatsObjectName =>
        Combat?.Kind == SkillCombatKind.SelfBuff ? BuffObjectName : BirthObjectName;
}

/// <summary>
/// Data-driven item from necromancy.json <c>items</c> (books, enchants, …).
/// </summary>
public sealed class NecromancyItem
{
    public NecromancyItem(
        string id,
        string name,
        string slug,
        string type,
        int tier,
        bool enabled,
        string image,
        string lootImage,
        string description,
        string stat = "",
        string slot = "all",
        int rarity = 2,
        int value = 0)
    {
        Id = id;
        Name = name;
        Slug = slug;
        Type = type;
        Tier = tier;
        Enabled = enabled;
        Image = image;
        LootImage = lootImage;
        Description = description;
        Stat = stat;
        Slot = string.IsNullOrWhiteSpace(slot) ? "all" : slot.Trim();
        Rarity = rarity;
        Value = value;
    }

    public string Id { get; }
    public string Name { get; }
    public string Slug { get; }
    public string Type { get; }
    public int Tier { get; }
    public bool Enabled { get; }
    public string Image { get; }
    public string LootImage { get; }
    public string Description { get; }
    /// <summary>Gear enchant attribute key written to item data / CharN (enchants only).</summary>
    public string Stat { get; }
    /// <summary>Weapon / Armor / all — which Metatype can roll this enchant.</summary>
    public string Slot { get; }
    /// <summary>1 = common pool, 2 = rare pool (vanilla prefix convention).</summary>
    public int Rarity { get; }
    /// <summary>Base enchant magnitude before quality/strength rolls.</summary>
    public int Value { get; }

    public string InvObjectName => $"o_inv_{Slug}";
    public string LootObjectName => $"o_loot_{Slug}";
    public string LocalizationId => Slug;

    public string InvSpriteName =>
        string.IsNullOrWhiteSpace(Image) ? $"s_inv_{Slug}" : Image;

    public string LootSpriteName =>
        string.IsNullOrWhiteSpace(LootImage) ? $"s_loot_{Slug}" : LootImage;

    public bool IsEnchant =>
        string.Equals(Type, "enchant", StringComparison.OrdinalIgnoreCase);
}
