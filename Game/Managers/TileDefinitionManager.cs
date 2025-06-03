using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using TransportGame.Game.Constants;

namespace TransportGame.Game.Managers;

/// <summary>
/// Manages tile definitions loaded from JSON files.
/// Provides tile properties, rendering info, and gameplay data.
/// </summary>
public class TileDefinitionManager : IDisposable
{
    private readonly ErrorHandler _errorHandler;
    private readonly Dictionary<TileType, TileDefinition> _tileDefinitions;
    private readonly string _definitionsPath;
    private bool _isInitialized = false;

    public TileDefinitionManager(ErrorHandler errorHandler)
    {
        _errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
        _tileDefinitions = new Dictionary<TileType, TileDefinition>();
        _definitionsPath = Path.Combine("Game", "Data", "Tiles", "Definitions");
        
        Console.WriteLine("[TILE-DEF] TileDefinitionManager: Constructor called");
        _errorHandler.LogInfo("TileDefinitionManager initialized");
    }

    /// <summary>
    /// Loads all tile definitions from JSON files.
    /// </summary>
    public void LoadDefinitions()
    {
        try
        {
            Console.WriteLine($"[TILE-DEF] TileDefinitionManager: Loading definitions from {_definitionsPath}");
            
            if (!Directory.Exists(_definitionsPath))
            {
                _errorHandler.HandleError($"Definitions directory not found: {_definitionsPath}", "TileDefinitionManager.LoadDefinitions");
                return;
            }

            var jsonFiles = Directory.GetFiles(_definitionsPath, "*.json");
            Console.WriteLine($"[TILE-DEF] TileDefinitionManager: Found {jsonFiles.Length} JSON files");

            foreach (var filePath in jsonFiles)
            {
                LoadDefinitionFile(filePath);
            }

            _isInitialized = true;
            Console.WriteLine($"[TILE-DEF] TileDefinitionManager: Loaded {_tileDefinitions.Count} tile definitions");
            _errorHandler.LogInfo($"Loaded {_tileDefinitions.Count} tile definitions successfully");
        }
        catch (Exception ex)
        {
            _errorHandler.HandleError($"Failed to load tile definitions: {ex.Message}", "TileDefinitionManager.LoadDefinitions");
            Console.WriteLine($"[TILE-DEF] TileDefinitionManager: ERROR loading definitions: {ex.Message}");
        }
    }

    /// <summary>
    /// Loads a single tile definition file.
    /// </summary>
    private void LoadDefinitionFile(string filePath)
    {
        try
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            Console.WriteLine($"[TILE-DEF] TileDefinitionManager: Loading file {fileName}");

            var jsonContent = File.ReadAllText(filePath);
            var definition = JsonSerializer.Deserialize<TileDefinition>(jsonContent, GetJsonOptions());

            if (definition == null)
            {
                _errorHandler.HandleError($"Failed to deserialize tile definition: {fileName}", "TileDefinitionManager.LoadDefinitionFile");
                return;
            }

            // Parse TileType from definition
            if (!Enum.TryParse<TileType>(definition.TileType, true, out var tileType))
            {
                _errorHandler.HandleError($"Invalid tile type '{definition.TileType}' in file {fileName}", "TileDefinitionManager.LoadDefinitionFile");
                return;
            }

            // Validate definition
            if (!ValidateDefinition(definition, fileName))
            {
                return;
            }

            _tileDefinitions[tileType] = definition;
            Console.WriteLine($"[TILE-DEF] TileDefinitionManager: Successfully loaded {tileType} from {fileName}");
        }
        catch (Exception ex)
        {
            _errorHandler.HandleError($"Error loading definition file {filePath}: {ex.Message}", "TileDefinitionManager.LoadDefinitionFile");
            Console.WriteLine($"[TILE-DEF] TileDefinitionManager: ERROR loading file {filePath}: {ex.Message}");
        }
    }

    /// <summary>
    /// Validates a tile definition for required fields and consistency.
    /// </summary>
    private bool ValidateDefinition(TileDefinition definition, string fileName)
    {
        var errors = new List<string>();

        if (string.IsNullOrEmpty(definition.Id))
            errors.Add("Missing 'id' field");

        if (string.IsNullOrEmpty(definition.TileType))
            errors.Add("Missing 'tileType' field");

        if (definition.Properties == null)
            errors.Add("Missing 'properties' section");

        if (definition.Rendering == null)
            errors.Add("Missing 'rendering' section");

        if (definition.Gameplay == null)
            errors.Add("Missing 'gameplay' section");

        if (errors.Count > 0)
        {
            var errorMessage = $"Validation failed for {fileName}: {string.Join(", ", errors)}";
            _errorHandler.HandleError(errorMessage, "TileDefinitionManager.ValidateDefinition");
            Console.WriteLine($"[TILE-DEF] TileDefinitionManager: VALIDATION ERROR: {errorMessage}");
            return false;
        }

        Console.WriteLine($"[TILE-DEF] TileDefinitionManager: Validation passed for {fileName}");
        return true;
    }

    /// <summary>
    /// Gets JSON serialization options.
    /// </summary>
    private JsonSerializerOptions GetJsonOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };
    }

    /// <summary>
    /// Gets a tile definition by tile type.
    /// </summary>
    public TileDefinition? GetDefinition(TileType tileType)
    {
        if (!_isInitialized)
        {
            Console.WriteLine("[TILE-DEF] TileDefinitionManager: GetDefinition called but not initialized");
            return null;
        }

        return _tileDefinitions.TryGetValue(tileType, out var definition) ? definition : null;
    }

    /// <summary>
    /// Checks if a tile type has a definition loaded.
    /// </summary>
    public bool HasDefinition(TileType tileType)
    {
        return _isInitialized && _tileDefinitions.ContainsKey(tileType);
    }

    /// <summary>
    /// Gets all loaded tile definitions.
    /// </summary>
    public IReadOnlyDictionary<TileType, TileDefinition> GetAllDefinitions()
    {
        return _tileDefinitions;
    }

    /// <summary>
    /// Gets debug information about loaded definitions.
    /// </summary>
    public string GetDebugInfo()
    {
        return $"TileDefinitions: {_tileDefinitions.Count} loaded, Initialized: {_isInitialized}";
    }

    public void Dispose()
    {
        _tileDefinitions.Clear();
        _isInitialized = false;
        Console.WriteLine("[TILE-DEF] TileDefinitionManager: Disposed");
        _errorHandler.LogInfo("TileDefinitionManager disposed");
    }
}

/// <summary>
/// Represents a complete tile definition loaded from JSON.
/// </summary>
public class TileDefinition
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("tileType")]
    public string TileType { get; set; } = "";

    [JsonPropertyName("properties")]
    public TileProperties? Properties { get; set; }

    [JsonPropertyName("rendering")]
    public TileRendering? Rendering { get; set; }

    [JsonPropertyName("gameplay")]
    public TileGameplay? Gameplay { get; set; }
}

/// <summary>
/// Tile properties section from JSON.
/// </summary>
public class TileProperties
{
    [JsonPropertyName("buildable")]
    public bool Buildable { get; set; }

    [JsonPropertyName("movementCost")]
    public float MovementCost { get; set; } = 1.0f;

    [JsonPropertyName("fertility")]
    public float Fertility { get; set; } = 0.0f;

    [JsonPropertyName("resourceProduction")]
    public List<string> ResourceProduction { get; set; } = new();

    [JsonPropertyName("allowedBuildings")]
    public List<string> AllowedBuildings { get; set; } = new();

    [JsonPropertyName("terrain")]
    public string Terrain { get; set; } = "";

    [JsonPropertyName("passable")]
    public bool Passable { get; set; } = true;
}

/// <summary>
/// Tile rendering section from JSON.
/// </summary>
public class TileRendering
{
    [JsonPropertyName("color")]
    public string Color { get; set; } = "#FFFFFF";

    [JsonPropertyName("texture")]
    public string Texture { get; set; } = "";
}

/// <summary>
/// Tile gameplay section from JSON.
/// </summary>
public class TileGameplay
{
    [JsonPropertyName("constructionCost")]
    public int ConstructionCost { get; set; } = 0;

    [JsonPropertyName("maintenanceCost")]
    public int MaintenanceCost { get; set; } = 0;

    [JsonPropertyName("maxBuildings")]
    public int MaxBuildings { get; set; } = 1;
}
