using System.Text.Json;
using TransportGame.Game.Models;
using TransportGame.Game.Utils;

namespace TransportGame.Game.Managers;

/// <summary>
/// Manages building definitions loaded from JSON files.
/// Similar to TileDefinitionManager but for buildings.
/// </summary>
public class BuildingDefinitionManager : IDisposable
{
    private readonly ErrorHandler _errorHandler;
    private readonly Dictionary<string, BuildingDefinition> _buildingDefinitions;
    private readonly string _definitionsPath;
    private bool _isInitialized = false;

    public BuildingDefinitionManager()
    {
        _errorHandler = new ErrorHandler();
        _buildingDefinitions = new Dictionary<string, BuildingDefinition>();
        _definitionsPath = Path.Combine("Game", "Data", "Buildings", "Definitions");
        
        Console.WriteLine("[BUILDING-DEF] BuildingDefinitionManager: Constructor called");
        _errorHandler.LogInfo("BuildingDefinitionManager initialized");
    }

    /// <summary>
    /// Loads all building definitions from JSON files.
    /// </summary>
    public void LoadDefinitions()
    {
        try
        {
            Console.WriteLine($"[BUILDING-DEF] BuildingDefinitionManager: Loading definitions from {_definitionsPath}");
            
            if (!Directory.Exists(_definitionsPath))
            {
                Directory.CreateDirectory(_definitionsPath);
                _errorHandler.HandleWarning($"Created missing directory: {_definitionsPath}", "BuildingDefinitionManager.LoadDefinitions");
            }

            var jsonFiles = Directory.GetFiles(_definitionsPath, "*.json");
            Console.WriteLine($"[BUILDING-DEF] BuildingDefinitionManager: Found {jsonFiles.Length} JSON files");

            foreach (var filePath in jsonFiles)
            {
                LoadDefinitionFromFile(filePath);
            }

            Console.WriteLine($"[BUILDING-DEF] BuildingDefinitionManager: Loaded {_buildingDefinitions.Count} building definitions");
            _errorHandler.LogInfo($"Loaded {_buildingDefinitions.Count} building definitions successfully");
            _isInitialized = true;
        }
        catch (Exception ex)
        {
            _errorHandler.HandleError($"Failed to load building definitions: {ex.Message}", "BuildingDefinitionManager.LoadDefinitions");
        }
    }

    private void LoadDefinitionFromFile(string filePath)
    {
        try
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            Console.WriteLine($"[BUILDING-DEF] BuildingDefinitionManager: Loading file {fileName}");

            var jsonContent = File.ReadAllText(filePath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip
            };

            var definition = JsonSerializer.Deserialize<BuildingDefinition>(jsonContent, options);
            
            if (definition == null)
            {
                _errorHandler.HandleWarning($"Failed to deserialize building definition from {fileName}", "BuildingDefinitionManager.LoadDefinitionFromFile");
                return;
            }

            // Validate definition
            if (!definition.IsValid())
            {
                _errorHandler.HandleWarning($"Invalid building definition in {fileName}", "BuildingDefinitionManager.LoadDefinitionFromFile");
                return;
            }

            Console.WriteLine($"[BUILDING-DEF] BuildingDefinitionManager: Validation passed for {definition.Id}");

            // Store definition
            _buildingDefinitions[definition.Id] = definition;
            Console.WriteLine($"[BUILDING-DEF] BuildingDefinitionManager: Successfully loaded {definition.Id} from {fileName}");
        }
        catch (Exception ex)
        {
            _errorHandler.HandleError($"Error loading building definition from {filePath}: {ex.Message}", "BuildingDefinitionManager.LoadDefinitionFromFile");
        }
    }

    /// <summary>
    /// Gets a building definition by ID.
    /// </summary>
    public BuildingDefinition? GetDefinition(string buildingId)
    {
        if (!_isInitialized)
        {
            _errorHandler.HandleWarning("BuildingDefinitionManager not initialized", "BuildingDefinitionManager.GetDefinition");
            return null;
        }

        return _buildingDefinitions.TryGetValue(buildingId, out var definition) ? definition : null;
    }

    /// <summary>
    /// Gets all building definitions.
    /// </summary>
    public IReadOnlyDictionary<string, BuildingDefinition> GetAllDefinitions()
    {
        return _buildingDefinitions.AsReadOnly();
    }

    /// <summary>
    /// Gets building definitions by category.
    /// </summary>
    public List<BuildingDefinition> GetDefinitionsByCategory(string category)
    {
        return _buildingDefinitions.Values
            .Where(def => def.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    /// <summary>
    /// Checks if a building definition exists.
    /// </summary>
    public bool HasDefinition(string buildingId)
    {
        return _buildingDefinitions.ContainsKey(buildingId);
    }

    /// <summary>
    /// Gets all available building categories.
    /// </summary>
    public List<string> GetCategories()
    {
        return _buildingDefinitions.Values
            .Select(def => def.Category)
            .Distinct()
            .OrderBy(cat => cat)
            .ToList();
    }

    /// <summary>
    /// Gets building definitions that can be built by the player.
    /// </summary>
    public List<BuildingDefinition> GetBuildableDefinitions()
    {
        return _buildingDefinitions.Values
            .Where(def => def.PlacementRules.Buildable)
            .OrderBy(def => def.Category)
            .ThenBy(def => def.Cost)
            .ToList();
    }

    public void Dispose()
    {
        _buildingDefinitions.Clear();
        Console.WriteLine("[BUILDING-DEF] BuildingDefinitionManager: Disposed");
        _errorHandler.LogInfo("BuildingDefinitionManager disposed");
    }
}
