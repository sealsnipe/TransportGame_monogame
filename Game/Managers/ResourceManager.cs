using Microsoft.Xna.Framework;
using TransportGame.Game.Constants;

namespace TransportGame.Game.Managers;

/// <summary>
/// Manages all game resources (wheat, iron, food, steel, money).
/// Ported from Godot ResourceManager.gd singleton.
/// </summary>
public class ResourceManager : IDisposable
{
    private readonly EventBus _eventBus;
    private readonly ErrorHandler _errorHandler;
    
    // Resource storage
    private readonly Dictionary<string, int> _resources = new();
    private readonly Dictionary<string, int> _resourceCapacities = new();
    
    // Resource tracking
    private readonly Dictionary<string, int> _totalProduced = new();
    private readonly Dictionary<string, int> _totalConsumed = new();

    public ResourceManager(EventBus eventBus)
    {
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _errorHandler = new ErrorHandler();
        
        InitializeResources();
        SubscribeToEvents();
        
        _errorHandler.LogInfo("ResourceManager initialized");
    }

    private void InitializeResources()
    {
        // Initialize all resource types with starting amounts
        _resources[GameConstants.RESOURCE_WHEAT] = 0;
        _resources[GameConstants.RESOURCE_IRON] = 0;
        _resources[GameConstants.RESOURCE_FOOD] = 0;
        _resources[GameConstants.RESOURCE_STEEL] = 0;
        _resources[GameConstants.RESOURCE_MONEY] = 10000; // Starting money
        
        // Set default capacities (can be increased by buildings)
        _resourceCapacities[GameConstants.RESOURCE_WHEAT] = 1000;
        _resourceCapacities[GameConstants.RESOURCE_IRON] = 1000;
        _resourceCapacities[GameConstants.RESOURCE_FOOD] = 1000;
        _resourceCapacities[GameConstants.RESOURCE_STEEL] = 1000;
        _resourceCapacities[GameConstants.RESOURCE_MONEY] = int.MaxValue; // No money limit
        
        // Initialize tracking
        foreach (var resourceType in _resources.Keys)
        {
            _totalProduced[resourceType] = 0;
            _totalConsumed[resourceType] = 0;
        }
    }

    private void SubscribeToEvents()
    {
        _eventBus.ResourceProduced += OnResourceProduced;
        _eventBus.ResourceConsumed += OnResourceConsumed;
    }

    /// <summary>
    /// Updates the resource manager each frame.
    /// </summary>
    public void Update(GameTime gameTime)
    {
        // Resource manager doesn't need per-frame updates currently
        // Production happens via events from buildings
    }

    /// <summary>
    /// Gets the current amount of a specific resource.
    /// </summary>
    public int GetResourceAmount(string resourceType)
    {
        return _resources.TryGetValue(resourceType, out var amount) ? amount : 0;
    }

    /// <summary>
    /// Gets the maximum capacity for a specific resource.
    /// </summary>
    public int GetResourceCapacity(string resourceType)
    {
        return _resourceCapacities.TryGetValue(resourceType, out var capacity) ? capacity : 0;
    }

    /// <summary>
    /// Checks if the player has at least the specified amount of a resource.
    /// </summary>
    public bool HasResource(string resourceType, int amount)
    {
        return GetResourceAmount(resourceType) >= amount;
    }

    /// <summary>
    /// Attempts to consume the specified amount of a resource.
    /// </summary>
    public bool ConsumeResource(string resourceType, int amount, string reason = "")
    {
        if (!HasResource(resourceType, amount))
        {
            _errorHandler.HandleWarning($"Insufficient {resourceType}: need {amount}, have {GetResourceAmount(resourceType)}");
            return false;
        }
        
        _resources[resourceType] -= amount;
        _totalConsumed[resourceType] += amount;
        
        var newAmount = _resources[resourceType];
        _eventBus.EmitResourceChanged(resourceType, newAmount);
        _eventBus.EmitResourceConsumed(resourceType, amount, newAmount);
        
        if (!string.IsNullOrEmpty(reason))
        {
            _errorHandler.LogInfo($"Consumed {amount} {resourceType} for: {reason}. Remaining: {newAmount}");
        }
        
        return true;
    }

    /// <summary>
    /// Adds the specified amount of a resource, respecting capacity limits.
    /// </summary>
    public int AddResource(string resourceType, int amount, string reason = "")
    {
        if (!_resources.ContainsKey(resourceType))
        {
            _errorHandler.HandleWarning($"Unknown resource type: {resourceType}");
            return 0;
        }
        
        var currentAmount = _resources[resourceType];
        var capacity = _resourceCapacities[resourceType];
        var availableSpace = capacity - currentAmount;
        var actualAmount = Math.Min(amount, availableSpace);
        
        if (actualAmount <= 0)
        {
            _errorHandler.HandleWarning($"No space for {resourceType}: capacity {capacity}, current {currentAmount}");
            return 0;
        }
        
        _resources[resourceType] += actualAmount;
        _totalProduced[resourceType] += actualAmount;
        
        var newAmount = _resources[resourceType];
        _eventBus.EmitResourceChanged(resourceType, newAmount);
        _eventBus.EmitResourceProduced(resourceType, actualAmount, newAmount);
        
        if (!string.IsNullOrEmpty(reason))
        {
            _errorHandler.LogInfo($"Added {actualAmount} {resourceType} from: {reason}. Total: {newAmount}");
        }
        
        if (actualAmount < amount)
        {
            _errorHandler.HandleWarning($"Could only add {actualAmount}/{amount} {resourceType} due to capacity limit");
        }
        
        return actualAmount;
    }

    /// <summary>
    /// Increases the storage capacity for a specific resource.
    /// </summary>
    public void IncreaseCapacity(string resourceType, int additionalCapacity)
    {
        if (!_resourceCapacities.ContainsKey(resourceType))
        {
            _errorHandler.HandleWarning($"Unknown resource type: {resourceType}");
            return;
        }
        
        _resourceCapacities[resourceType] += additionalCapacity;
        _errorHandler.LogInfo($"Increased {resourceType} capacity by {additionalCapacity}. New capacity: {_resourceCapacities[resourceType]}");
    }

    /// <summary>
    /// Gets production statistics for a resource.
    /// </summary>
    public (int produced, int consumed, int net) GetResourceStats(string resourceType)
    {
        var produced = _totalProduced.TryGetValue(resourceType, out var p) ? p : 0;
        var consumed = _totalConsumed.TryGetValue(resourceType, out var c) ? c : 0;
        return (produced, consumed, produced - consumed);
    }

    /// <summary>
    /// Gets all current resource amounts.
    /// </summary>
    public Dictionary<string, int> GetAllResources()
    {
        return new Dictionary<string, int>(_resources);
    }

    /// <summary>
    /// Gets all resource capacities.
    /// </summary>
    public Dictionary<string, int> GetAllCapacities()
    {
        return new Dictionary<string, int>(_resourceCapacities);
    }

    /// <summary>
    /// Processes resource conversion (e.g., wheat to food).
    /// </summary>
    public bool ProcessConversion(string inputResource, int inputAmount, string outputResource, int outputAmount, string reason = "")
    {
        // Check if we have enough input resource
        if (!HasResource(inputResource, inputAmount))
        {
            _errorHandler.HandleWarning($"Cannot convert: insufficient {inputResource} ({GetResourceAmount(inputResource)}/{inputAmount})");
            return false;
        }
        
        // Check if we have space for output resource
        var currentOutput = GetResourceAmount(outputResource);
        var outputCapacity = GetResourceCapacity(outputResource);
        if (currentOutput + outputAmount > outputCapacity)
        {
            _errorHandler.HandleWarning($"Cannot convert: insufficient {outputResource} storage space");
            return false;
        }
        
        // Perform the conversion
        if (ConsumeResource(inputResource, inputAmount, reason) && 
            AddResource(outputResource, outputAmount, reason) == outputAmount)
        {
            _errorHandler.LogInfo($"Converted {inputAmount} {inputResource} to {outputAmount} {outputResource}");
            return true;
        }
        
        _errorHandler.HandleError($"Conversion failed: {inputAmount} {inputResource} to {outputAmount} {outputResource}");
        return false;
    }

    #region Event Handlers

    private void OnResourceProduced(string resourceType, int amount, int totalAmount)
    {
        // This event is emitted by AddResource, so we don't need to do anything here
        // But we could add additional logic like achievements, notifications, etc.
    }

    private void OnResourceConsumed(string resourceType, int amount, int remainingAmount)
    {
        // This event is emitted by ConsumeResource, so we don't need to do anything here
        // But we could add additional logic like low resource warnings, etc.
        
        if (remainingAmount < 10 && resourceType != GameConstants.RESOURCE_MONEY)
        {
            _eventBus.EmitUINotification($"Low {resourceType}: only {remainingAmount} remaining!");
        }
    }

    #endregion

    public void Dispose()
    {
        // Unsubscribe from events
        _eventBus.ResourceProduced -= OnResourceProduced;
        _eventBus.ResourceConsumed -= OnResourceConsumed;
        
        _errorHandler.LogInfo("ResourceManager disposed");
    }
}
