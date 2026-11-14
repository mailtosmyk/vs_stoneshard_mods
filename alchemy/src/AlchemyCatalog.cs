using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using ModShardLauncher;

namespace Alchemy;

/// <summary>
/// Loads <c>ingredients.json</c> and exposes enabled recipes / custom products.
/// </summary>
internal static class AlchemyCatalog
{
    static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    static AlchemyJsonRoot? _root;

    public static AlchemyJsonRoot Root => _root ??= LoadRoot();

    public static IReadOnlyList<AlchemyRecipe> EnabledRecipes =>
        Root.MappedRecipes.Where(r => r.Enabled && !string.IsNullOrWhiteSpace(r.Elixir)).ToArray();

    public static IReadOnlyDictionary<string, AlchemyProduct> CustomProducts => Root.CustomProducts;

    /// <summary>
    /// Elixir ids with <c>debug: true</c> — granted to the player inventory on create.
    /// </summary>
    public static IReadOnlyList<string> DebugElixirIds =>
        Root.Elixirs
            .Where(e => e.Debug && !string.IsNullOrWhiteSpace(e.Id))
            .Select(e => e.Id!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

    /// <summary>
    /// Recipe outcome ids with <c>debug: true</c> — grants <c>o_inv_recipe_{id}</c> on create.
    /// </summary>
    public static IReadOnlyList<string> DebugRecipeIds =>
        EnabledRecipes
            .Where(r => r.Debug)
            .Select(r => r.Elixir)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

    static AlchemyJsonRoot LoadRoot()
    {
        using var stream = OpenDataStream();
        var root = JsonSerializer.Deserialize<AlchemyJsonRoot>(stream, JsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize ingredients.json.");
        root.BuildProductIndex();
        return root;
    }

    static Stream OpenDataStream()
    {
        var asm = Assembly.GetExecutingAssembly();
        var dir = Path.GetDirectoryName(asm.Location);

        if (!string.IsNullOrEmpty(dir))
        {
            var besideAssembly = Path.Combine(dir, "ingredients.json");
            if (File.Exists(besideAssembly))
                return File.OpenRead(besideAssembly);

            var projectJson = Path.GetFullPath(Path.Combine(dir, "..", "..", "..", "src", "ingredients.json"));
            if (File.Exists(projectJson))
                return File.OpenRead(projectJson);
        }

        try
        {
            var modSources = Path.Combine(ModLoader.ModSourcesPath, "Alchemy", "src", "ingredients.json");
            if (File.Exists(modSources))
                return File.OpenRead(modSources);
        }
        catch
        {
            // ModLoader may not be ready in unit contexts.
        }

        if (AlchemyJsonResource.TryOpen(out var generated))
            return generated;

        var resourceName = asm.GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith("ingredients.json", StringComparison.OrdinalIgnoreCase));
        if (resourceName is not null)
        {
            var embedded = asm.GetManifestResourceStream(resourceName);
            if (embedded is not null)
                return embedded;
        }

        throw new FileNotFoundException(
            "ingredients.json not found beside the assembly, under ModSources, as AlchemyJsonResource, or as an embedded resource.");
    }
}

internal sealed class AlchemyJsonRoot
{
    [JsonPropertyName("elixirs")]
    public List<AlchemyProductDto> Elixirs { get; set; } = new();

    [JsonPropertyName("throwables")]
    public List<AlchemyProductDto> Throwables { get; set; } = new();

    [JsonPropertyName("coatings")]
    public List<AlchemyProductDto> Coatings { get; set; } = new();

    /// <summary>Vanilla ore / metal catalog; entries with icons are mod materials.</summary>
    [JsonPropertyName("ore")]
    public List<AlchemyProductDto> Ore { get; set; } = new();

    /// <summary>Scroll catalog; entries with icons are mod blank scrolls.</summary>
    [JsonPropertyName("scrolls")]
    public List<AlchemyProductDto> Scrolls { get; set; } = new();

    [JsonPropertyName("recipes")]
    public List<AlchemyRecipeDto> RecipeDtos { get; set; } = new();

    [JsonIgnore]
    public List<AlchemyRecipe> MappedRecipes { get; private set; } = new();

    [JsonIgnore]
    public Dictionary<string, AlchemyProduct> CustomProducts { get; private set; } = new(StringComparer.OrdinalIgnoreCase);

    public void BuildProductIndex()
    {
        CustomProducts = new Dictionary<string, AlchemyProduct>(StringComparer.OrdinalIgnoreCase);
        IndexSection(Elixirs, AlchemyProductKind.Elixir);
        IndexSection(Throwables, AlchemyProductKind.Throwable);
        IndexSection(Coatings, AlchemyProductKind.Coating);
        IndexOreMaterials(Ore);
        IndexScrollProducts(Scrolls);

        MappedRecipes = RecipeDtos
            .Select(dto => MapRecipe(dto, CustomProducts))
            .ToList();
    }

    void IndexSection(IEnumerable<AlchemyProductDto> items, AlchemyProductKind kind)
    {
        foreach (var dto in items)
        {
            if (string.IsNullOrWhiteSpace(dto.Id))
                continue;
            // Only register mod products that ship with an alchemy icon.
            if (string.IsNullOrWhiteSpace(dto.Icon) || !dto.Icon.Contains("alch", StringComparison.OrdinalIgnoreCase))
                continue;

            CustomProducts[dto.Id] = new AlchemyProduct(
                dto.Id,
                string.IsNullOrWhiteSpace(dto.Name) ? dto.Id : dto.Name,
                dto.Description ?? "",
                dto.Icon,
                dto.Price ?? 0,
                kind);
        }
    }

    void IndexOreMaterials(IEnumerable<AlchemyProductDto> items)
    {
        foreach (var dto in items)
        {
            if (string.IsNullOrWhiteSpace(dto.Id) || string.IsNullOrWhiteSpace(dto.Icon))
                continue;

            CustomProducts[dto.Id] = new AlchemyProduct(
                dto.Id,
                string.IsNullOrWhiteSpace(dto.Name) ? dto.Id : dto.Name,
                dto.Description ?? "",
                dto.Icon,
                dto.Price ?? 0,
                AlchemyProductKind.Material);
        }
    }

    void IndexScrollProducts(IEnumerable<AlchemyProductDto> items)
    {
        foreach (var dto in items)
        {
            // Only register mod blank scrolls (icon required). Vanilla ID/disenchant/enchant
            // already exist — never recreate them even if a page icon is present.
            if (string.IsNullOrWhiteSpace(dto.Id) || string.IsNullOrWhiteSpace(dto.Icon))
                continue;
            if (!string.Equals(dto.Id, "blank_scroll", StringComparison.OrdinalIgnoreCase)
                && !dto.Id.StartsWith("alch_", StringComparison.OrdinalIgnoreCase))
                continue;

            CustomProducts[dto.Id] = new AlchemyProduct(
                dto.Id,
                string.IsNullOrWhiteSpace(dto.Name) ? dto.Id : dto.Name,
                dto.Description ?? "",
                dto.Icon,
                dto.Price ?? 0,
                AlchemyProductKind.Scroll);
        }
    }

    static AlchemyRecipe MapRecipe(
        AlchemyRecipeDto dto,
        IReadOnlyDictionary<string, AlchemyProduct> products)
    {
        var ingredients = (dto.Ingredients ?? new List<string>())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s.Trim())
            .Take(4)
            .ToArray();

        var elixir = dto.Elixir?.Trim() ?? "";
        // Ore smelting + blank scroll + spirit: known-from-start inferred
        // (optional source/advanced overrides still honored).
        var materialOutcome =
            products.TryGetValue(elixir, out var product)
                && product.Kind == AlchemyProductKind.Material
            || string.Equals(elixir, "iron_ingot", StringComparison.OrdinalIgnoreCase);
        var blankScroll = string.Equals(elixir, "blank_scroll", StringComparison.OrdinalIgnoreCase);
        var scrollOutcome = blankScroll
            || elixir.StartsWith("scroll_", StringComparison.OrdinalIgnoreCase);
        var knownFromStart = materialOutcome
            || blankScroll
            || string.Equals(elixir, "spirit", StringComparison.OrdinalIgnoreCase);

        var source = string.Equals(dto.Source?.Trim(), "base", StringComparison.OrdinalIgnoreCase)
                || (dto.Source is null && knownFromStart)
            ? AlchemyCraftSource.BaseRecipe
            : AlchemyCraftSource.Trade;

        // Ingots and learned scrolls craft at the workbench / craftkit.
        // Blank Scroll is a simple known-from-start Base Recipe (basic craft).
        var advanced = dto.Advanced || materialOutcome
            || (scrollOutcome && !blankScroll);

        return new AlchemyRecipe(
            elixir,
            dto.Enabled,
            ingredients,
            dto.IngredientsCost,
            dto.RecipeCost,
            dto.ElixirCost,
            dto.Debug,
            source,
            advanced);
    }
}

internal enum AlchemyProductKind
{
    Elixir,
    Throwable,
    Coating,
    Material,
    Scroll,
}

internal enum AlchemyCraftSource
{
    Trade,
    BaseRecipe,
}

internal sealed record AlchemyProduct(
    string Id,
    string Name,
    string Description,
    string Icon,
    int Price,
    AlchemyProductKind Kind);

internal sealed record AlchemyRecipe(
    string Elixir,
    bool Enabled,
    IReadOnlyList<string> Ingredients,
    int IngredientsCost,
    int RecipeCost,
    int ElixirCost,
    bool Debug,
    AlchemyCraftSource Source,
    bool Advanced);

internal sealed class AlchemyProductDto
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("icon")]
    public string? Icon { get; set; }

    [JsonPropertyName("price")]
    public int? Price { get; set; }

    /// <summary>
    /// When true, grant this elixir to the player inventory on create. Default false.
    /// </summary>
    [JsonPropertyName("debug")]
    public bool Debug { get; set; }
}

internal sealed class AlchemyRecipeDto
{
    [JsonPropertyName("ingredients")]
    public List<string>? Ingredients { get; set; }

    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }

    [JsonPropertyName("elixir")]
    public string? Elixir { get; set; }

    [JsonPropertyName("ingredients_cost")]
    public int IngredientsCost { get; set; }

    [JsonPropertyName("recipe_cost")]
    public int RecipeCost { get; set; }

    [JsonPropertyName("elixir_cost")]
    public int ElixirCost { get; set; }

    /// <summary>
    /// When true, grant this recipe scroll to the player inventory on create. Default false.
    /// </summary>
    [JsonPropertyName("debug")]
    public bool Debug { get; set; }

    /// <summary>
    /// Craft discovery source: <c>trade</c> (default) or <c>base</c> (known from the start).
    /// </summary>
    [JsonPropertyName("source")]
    public string? Source { get; set; }

    /// <summary>
    /// When true, register as an advanced (craftkit / workbench) recipe.
    /// </summary>
    [JsonPropertyName("advanced")]
    public bool Advanced { get; set; }
}
