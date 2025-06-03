using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TransportGame.Game.Models;

/// <summary>
/// Represents a resource type in the game (grain, iron ore, steel, etc.).
/// Used for production chains and building requirements.
/// </summary>
public class Resource
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
    public string Category { get; set; } = string.Empty; // "raw", "processed", "finished"

    [JsonPropertyName("base_value")]
    public int BaseValue { get; set; } = 10; // Economic value per unit

    [JsonPropertyName("stack_size")]
    public int StackSize { get; set; } = 100; // Max units per stack

    [JsonPropertyName("visual")]
    public ResourceVisual Visual { get; set; } = new();

    [JsonPropertyName("transport")]
    public ResourceTransport Transport { get; set; } = new();

    /// <summary>
    /// Validates the resource definition.
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Id) &&
               !string.IsNullOrWhiteSpace(Name) &&
               !string.IsNullOrWhiteSpace(Description) &&
               !string.IsNullOrWhiteSpace(Category) &&
               BaseValue > 0 &&
               StackSize > 0;
    }
}

/// <summary>
/// Visual properties for resource representation.
/// </summary>
public class ResourceVisual
{
    [JsonPropertyName("color")]
    public string Color { get; set; } = "#8B4513"; // Brown default

    [JsonPropertyName("icon")]
    public string? Icon { get; set; }

    [JsonPropertyName("texture")]
    public string? Texture { get; set; }
}

/// <summary>
/// Transport properties for resources.
/// </summary>
public class ResourceTransport
{
    [JsonPropertyName("weight")]
    public float Weight { get; set; } = 1.0f; // Weight per unit

    [JsonPropertyName("volume")]
    public float Volume { get; set; } = 1.0f; // Volume per unit

    [JsonPropertyName("hazardous")]
    public bool Hazardous { get; set; } = false; // Special transport requirements
}

/// <summary>
/// Represents a quantity of a specific resource.
/// </summary>
public class ResourceStack
{
    public string ResourceId { get; set; } = string.Empty;
    public int Amount { get; set; } = 0;
    public DateTime LastUpdated { get; set; } = DateTime.Now;

    public ResourceStack() { }

    public ResourceStack(string resourceId, int amount)
    {
        ResourceId = resourceId;
        Amount = amount;
        LastUpdated = DateTime.Now;
    }

    /// <summary>
    /// Adds resources to this stack.
    /// </summary>
    public int Add(int amount, int maxStackSize = 100)
    {
        var canAdd = Math.Min(amount, maxStackSize - Amount);
        Amount += canAdd;
        LastUpdated = DateTime.Now;
        return amount - canAdd; // Return overflow
    }

    /// <summary>
    /// Removes resources from this stack.
    /// </summary>
    public int Remove(int amount)
    {
        var canRemove = Math.Min(amount, Amount);
        Amount -= canRemove;
        LastUpdated = DateTime.Now;
        return canRemove;
    }

    /// <summary>
    /// Checks if this stack can accept more resources.
    /// </summary>
    public bool CanAccept(int amount, int maxStackSize = 100)
    {
        return Amount + amount <= maxStackSize;
    }

    /// <summary>
    /// Checks if this stack has enough resources.
    /// </summary>
    public bool HasEnough(int amount)
    {
        return Amount >= amount;
    }

    public bool IsEmpty => Amount <= 0;
    public bool IsFull(int maxStackSize = 100) => Amount >= maxStackSize;
}

/// <summary>
/// Manages a collection of resource stacks for a building or storage.
/// </summary>
public class ResourceStorage
{
    private readonly Dictionary<string, ResourceStack> _stacks;
    private readonly int _maxCapacity;

    public int MaxCapacity => _maxCapacity;
    public int UsedCapacity => _stacks.Values.Sum(s => s.Amount);
    public int FreeCapacity => _maxCapacity - UsedCapacity;
    public IReadOnlyDictionary<string, ResourceStack> Stacks => _stacks.AsReadOnly();

    public ResourceStorage(int maxCapacity = 100)
    {
        _maxCapacity = maxCapacity;
        _stacks = new Dictionary<string, ResourceStack>();
    }

    /// <summary>
    /// Adds resources to storage.
    /// </summary>
    public int AddResource(string resourceId, int amount)
    {
        if (amount <= 0) return 0;

        // Check if we have space
        if (UsedCapacity >= _maxCapacity) return 0;

        // Get or create stack
        if (!_stacks.ContainsKey(resourceId))
        {
            _stacks[resourceId] = new ResourceStack(resourceId, 0);
        }

        var stack = _stacks[resourceId];
        var canAdd = Math.Min(amount, _maxCapacity - UsedCapacity);
        
        return stack.Add(canAdd, _maxCapacity);
    }

    /// <summary>
    /// Removes resources from storage.
    /// </summary>
    public int RemoveResource(string resourceId, int amount)
    {
        if (amount <= 0 || !_stacks.ContainsKey(resourceId)) return 0;

        var stack = _stacks[resourceId];
        var removed = stack.Remove(amount);

        // Clean up empty stacks
        if (stack.IsEmpty)
        {
            _stacks.Remove(resourceId);
        }

        return removed;
    }

    /// <summary>
    /// Checks if storage has enough of a resource.
    /// </summary>
    public bool HasResource(string resourceId, int amount)
    {
        return _stacks.ContainsKey(resourceId) && _stacks[resourceId].HasEnough(amount);
    }

    /// <summary>
    /// Gets the amount of a specific resource.
    /// </summary>
    public int GetResourceAmount(string resourceId)
    {
        return _stacks.ContainsKey(resourceId) ? _stacks[resourceId].Amount : 0;
    }

    /// <summary>
    /// Checks if storage can accept more resources.
    /// </summary>
    public bool CanAccept(string resourceId, int amount)
    {
        return UsedCapacity + amount <= _maxCapacity;
    }

    /// <summary>
    /// Gets all resource types in storage.
    /// </summary>
    public List<string> GetResourceTypes()
    {
        return _stacks.Keys.ToList();
    }

    /// <summary>
    /// Clears all resources from storage.
    /// </summary>
    public void Clear()
    {
        _stacks.Clear();
    }

    /// <summary>
    /// Gets storage utilization as percentage.
    /// </summary>
    public float GetUtilization()
    {
        return _maxCapacity > 0 ? (float)UsedCapacity / _maxCapacity : 0f;
    }
}
