using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TransportGame.Game.Models;

/// <summary>
/// Defines the properties and behavior of a building type.
/// Used for JSON-based building definitions similar to TileDefinition.
/// </summary>
public class BuildingDefinition
{
    [Required]
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [Required]
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [Required]
    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty; // "production", "transport", "city", "resource"

    [Required]
    [JsonPropertyName("size")]
    public BuildingSize Size { get; set; } = new();

    [Required]
    [JsonPropertyName("cost")]
    public int Cost { get; set; }

    [JsonPropertyName("construction_time")]
    public float ConstructionTime { get; set; } = 5.0f; // seconds

    [JsonPropertyName("placement_rules")]
    public PlacementRules PlacementRules { get; set; } = new();

    [JsonPropertyName("production")]
    public ProductionInfo? Production { get; set; }

    [JsonPropertyName("storage")]
    public StorageInfo? Storage { get; set; }

    [JsonPropertyName("visual")]
    public VisualInfo Visual { get; set; } = new();

    [JsonPropertyName("rotation_allowed")]
    public bool RotationAllowed { get; set; } = true;

    /// <summary>
    /// Validates the building definition.
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Id) &&
               !string.IsNullOrWhiteSpace(Name) &&
               !string.IsNullOrWhiteSpace(Description) &&
               !string.IsNullOrWhiteSpace(Category) &&
               Size.IsValid() &&
               Cost > 0 &&
               ConstructionTime > 0;
    }
}

/// <summary>
/// Defines the size and shape of a building.
/// </summary>
public class BuildingSize
{
    [Required]
    [JsonPropertyName("width")]
    public int Width { get; set; } = 1;

    [Required]
    [JsonPropertyName("height")]
    public int Height { get; set; } = 1;

    [JsonPropertyName("shape")]
    public string Shape { get; set; } = "rectangle"; // "rectangle", "cross", "l_shape", etc.

    [JsonPropertyName("occupied_tiles")]
    public List<TileOffset>? OccupiedTiles { get; set; } // For custom shapes

    public bool IsValid()
    {
        return Width > 0 && Width <= 10 &&
               Height > 0 && Height <= 10;
    }

    public int GetTileCount()
    {
        if (OccupiedTiles != null)
            return OccupiedTiles.Count;
        
        return Width * Height;
    }
}

/// <summary>
/// Represents a tile offset for custom building shapes.
/// </summary>
public class TileOffset
{
    [JsonPropertyName("x")]
    public int X { get; set; }

    [JsonPropertyName("y")]
    public int Y { get; set; }
}

/// <summary>
/// Defines placement rules for buildings.
/// </summary>
public class PlacementRules
{
    [JsonPropertyName("allowed_terrain")]
    public List<string> AllowedTerrain { get; set; } = new() { "Grass" };

    [JsonPropertyName("forbidden_terrain")]
    public List<string> ForbiddenTerrain { get; set; } = new() { "Water", "Mountain" };

    [JsonPropertyName("requires_road_access")]
    public bool RequiresRoadAccess { get; set; } = false;

    [JsonPropertyName("min_distance_to_water")]
    public int MinDistanceToWater { get; set; } = 0;

    [JsonPropertyName("max_distance_to_road")]
    public int MaxDistanceToRoad { get; set; } = 5;

    [JsonPropertyName("buildable")]
    public bool Buildable { get; set; } = true;
}

/// <summary>
/// Defines production capabilities of a building.
/// </summary>
public class ProductionInfo
{
    [JsonPropertyName("input_resources")]
    public List<ResourceRequirement> InputResources { get; set; } = new();

    [JsonPropertyName("output_resources")]
    public List<ResourceProduction> OutputResources { get; set; } = new();

    [JsonPropertyName("production_rate")]
    public float ProductionRate { get; set; } = 1.0f; // items per second

    [JsonPropertyName("efficiency")]
    public float Efficiency { get; set; } = 1.0f; // 0.0 to 1.0

    [JsonPropertyName("power_required")]
    public int PowerRequired { get; set; } = 0;
}

/// <summary>
/// Defines resource requirements for production.
/// </summary>
public class ResourceRequirement
{
    [JsonPropertyName("resource_type")]
    public string ResourceType { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public int Amount { get; set; } = 1;
}

/// <summary>
/// Defines resource production output.
/// </summary>
public class ResourceProduction
{
    [JsonPropertyName("resource_type")]
    public string ResourceType { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public int Amount { get; set; } = 1;
}

/// <summary>
/// Defines storage capabilities of a building.
/// </summary>
public class StorageInfo
{
    [JsonPropertyName("input_capacity")]
    public int InputCapacity { get; set; } = 50;

    [JsonPropertyName("output_capacity")]
    public int OutputCapacity { get; set; } = 50;

    [JsonPropertyName("accepted_resources")]
    public List<string> AcceptedResources { get; set; } = new();

    [JsonPropertyName("stored_resources")]
    public List<string> StoredResources { get; set; } = new();
}

/// <summary>
/// Defines visual properties of a building.
/// </summary>
public class VisualInfo
{
    [JsonPropertyName("color")]
    public string Color { get; set; } = "#8B4513"; // Brown default

    [JsonPropertyName("texture")]
    public string? Texture { get; set; }

    [JsonPropertyName("animation")]
    public string? Animation { get; set; }

    [JsonPropertyName("smoke_effect")]
    public bool SmokeEffect { get; set; } = false;
}
