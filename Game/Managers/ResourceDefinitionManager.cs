using System.Text.Json;
using TransportGame.Game.Models;
using TransportGame.Game.Utils;

namespace TransportGame.Game.Managers;

/// <summary>
/// Manages resource definitions loaded from JSON files.
/// Handles all resource types available in the game.
/// </summary>
public class ResourceDefinitionManager : IDisposable
{
    private readonly ErrorHandler _errorHandler;
    private readonly Dictionary<string, Resource> _resourceDefinitions;
    private readonly string _definitionsPath;
    private bool _isInitialized = false;

    public ResourceDefinitionManager()
    {
        _errorHandler = new ErrorHandler();
        _resourceDefinitions = new Dictionary<string, Resource>();
        _definitionsPath = Path.Combine("Game", "Data", "Resources", "Definitions");
        
        Console.WriteLine("[RESOURCE-DEF] ResourceDefinitionManager: Constructor called");
        _errorHandler.LogInfo("ResourceDefinitionManager initialized");
    }

    /// <summary>
    /// Loads all resource definitions from JSON files.
    /// </summary>
    public void LoadDefinitions()
    {
        try
        {
            Console.WriteLine($"[RESOURCE-DEF] ResourceDefinitionManager: Loading definitions from {_definitionsPath}");
            
            if (!Directory.Exists(_definitionsPath))
            {
                Directory.CreateDirectory(_definitionsPath);
                _errorHandler.HandleWarning($"Created missing directory: {_definitionsPath}", "ResourceDefinitionManager.LoadDefinitions");
            }

            var jsonFiles = Directory.GetFiles(_definitionsPath, "*.json");
            Console.WriteLine($"[RESOURCE-DEF] ResourceDefinitionManager: Found {jsonFiles.Length} JSON files");

            foreach (var filePath in jsonFiles)
            {
                LoadDefinitionFromFile(filePath);
            }

            Console.WriteLine($"[RESOURCE-DEF] ResourceDefinitionManager: Loaded {_resourceDefinitions.Count} resource definitions");
            _errorHandler.LogInfo($"Loaded {_resourceDefinitions.Count} resource definitions successfully");
            _isInitialized = true;
        }
        catch (Exception ex)
        {
            _errorHandler.HandleError($"Failed to load resource definitions: {ex.Message}", "ResourceDefinitionManager.LoadDefinitions");
        }
    }

    private void LoadDefinitionFromFile(string filePath)
    {
        try
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            Console.WriteLine($"[RESOURCE-DEF] ResourceDefinitionManager: Loading file {fileName}");

            var jsonContent = File.ReadAllText(filePath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip
            };

            var definition = JsonSerializer.Deserialize<Resource>(jsonContent, options);
            
            if (definition == null)
            {
                _errorHandler.HandleWarning($"Failed to deserialize resource definition from {fileName}", "ResourceDefinitionManager.LoadDefinitionFromFile");
                return;
            }

            // Validate definition
            if (!definition.IsValid())
            {
                _errorHandler.HandleWarning($"Invalid resource definition in {fileName}", "ResourceDefinitionManager.LoadDefinitionFromFile");
                return;
            }

            Console.WriteLine($"[RESOURCE-DEF] ResourceDefinitionManager: Validation passed for {definition.Id}");

            // Store definition
            _resourceDefinitions[definition.Id] = definition;
            Console.WriteLine($"[RESOURCE-DEF] ResourceDefinitionManager: Successfully loaded {definition.Id} from {fileName}");
        }
        catch (Exception ex)
        {
            _errorHandler.HandleError($"Error loading resource definition from {filePath}: {ex.Message}", "ResourceDefinitionManager.LoadDefinitionFromFile");
        }
    }

    /// <summary>
    /// Gets a resource definition by ID.
    /// </summary>
    public Resource? GetDefinition(string resourceId)
    {
        if (!_isInitialized)
        {
            _errorHandler.HandleWarning("ResourceDefinitionManager not initialized", "ResourceDefinitionManager.GetDefinition");
            return null;
        }

        return _resourceDefinitions.TryGetValue(resourceId, out var definition) ? definition : null;
    }

    /// <summary>
    /// Gets all resource definitions.
    /// </summary>
    public IReadOnlyDictionary<string, Resource> GetAllDefinitions()
    {
        return _resourceDefinitions.AsReadOnly();
    }

    /// <summary>
    /// Gets resource definitions by category.
    /// </summary>
    public List<Resource> GetDefinitionsByCategory(string category)
    {
        return _resourceDefinitions.Values
            .Where(def => def.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    /// <summary>
    /// Checks if a resource definition exists.
    /// </summary>
    public bool HasDefinition(string resourceId)
    {
        return _resourceDefinitions.ContainsKey(resourceId);
    }

    /// <summary>
    /// Gets all available resource categories.
    /// </summary>
    public List<string> GetCategories()
    {
        return _resourceDefinitions.Values
            .Select(def => def.Category)
            .Distinct()
            .OrderBy(cat => cat)
            .ToList();
    }

    /// <summary>
    /// Gets raw materials (category: "raw").
    /// </summary>
    public List<Resource> GetRawMaterials()
    {
        return GetDefinitionsByCategory("raw");
    }

    /// <summary>
    /// Gets processed materials (category: "processed").
    /// </summary>
    public List<Resource> GetProcessedMaterials()
    {
        return GetDefinitionsByCategory("processed");
    }

    /// <summary>
    /// Gets finished goods (category: "finished").
    /// </summary>
    public List<Resource> GetFinishedGoods()
    {
        return GetDefinitionsByCategory("finished");
    }

    /// <summary>
    /// Validates a resource stack against definition limits.
    /// </summary>
    public bool ValidateResourceStack(string resourceId, int amount)
    {
        var definition = GetDefinition(resourceId);
        if (definition == null) return false;

        return amount >= 0 && amount <= definition.StackSize;
    }

    /// <summary>
    /// Gets the maximum stack size for a resource.
    /// </summary>
    public int GetMaxStackSize(string resourceId)
    {
        var definition = GetDefinition(resourceId);
        return definition?.StackSize ?? 100; // Default stack size
    }

    /// <summary>
    /// Gets the base value for a resource.
    /// </summary>
    public int GetBaseValue(string resourceId)
    {
        var definition = GetDefinition(resourceId);
        return definition?.BaseValue ?? 10; // Default value
    }

    /// <summary>
    /// Gets the display name for a resource.
    /// </summary>
    public string GetDisplayName(string resourceId)
    {
        var definition = GetDefinition(resourceId);
        return definition?.Name ?? resourceId;
    }

    /// <summary>
    /// Gets the description for a resource.
    /// </summary>
    public string GetDescription(string resourceId)
    {
        var definition = GetDefinition(resourceId);
        return definition?.Description ?? "Unknown resource";
    }

    public void Dispose()
    {
        _resourceDefinitions.Clear();
        Console.WriteLine("[RESOURCE-DEF] ResourceDefinitionManager: Disposed");
        _errorHandler.LogInfo("ResourceDefinitionManager disposed");
    }
}
