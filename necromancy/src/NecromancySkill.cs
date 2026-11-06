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
        SkillRequirements requirements)
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
}
