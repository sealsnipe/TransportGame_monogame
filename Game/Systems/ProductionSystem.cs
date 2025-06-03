using Microsoft.Xna.Framework;
using TransportGame.Game.Entities;
using TransportGame.Game.Managers;
using TransportGame.Game.Models;
using TransportGame.Game.Utils;

namespace TransportGame.Game.Systems;

/// <summary>
/// Handles resource production and consumption for buildings.
/// Manages production chains and resource flow between buildings.
/// </summary>
public class ProductionSystem : IDisposable
{
    private readonly EventBus _eventBus;
    private readonly ErrorHandler _errorHandler;
    private readonly BuildingPlacementSystem _buildingPlacementSystem;
    private readonly ResourceDefinitionManager _resourceDefinitionManager;

    // Production tracking
    private readonly Dictionary<string, BuildingProduction> _buildingProductions;
    private float _productionTimer = 0f;
    private const float PRODUCTION_INTERVAL = 1.0f; // 1 second production cycles

    public ProductionSystem(
        EventBus eventBus,
        BuildingPlacementSystem buildingPlacementSystem,
        ResourceDefinitionManager resourceDefinitionManager)
    {
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _buildingPlacementSystem = buildingPlacementSystem ?? throw new ArgumentNullException(nameof(buildingPlacementSystem));
        _resourceDefinitionManager = resourceDefinitionManager ?? throw new ArgumentNullException(nameof(resourceDefinitionManager));
        _errorHandler = new ErrorHandler();

        _buildingProductions = new Dictionary<string, BuildingProduction>();

        // Subscribe to building events
        _eventBus.BuildingPlaced += OnBuildingPlaced;

        _errorHandler.LogInfo("ProductionSystem initialized");
    }

    /// <summary>
    /// Updates production for all buildings.
    /// </summary>
    public void Update(GameTime gameTime)
    {
        try
        {
            _productionTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_productionTimer >= PRODUCTION_INTERVAL)
            {
                ProcessProduction();
                _productionTimer = 0f;
            }
        }
        catch (Exception ex)
        {
            _errorHandler.HandleError($"Error in ProductionSystem update: {ex.Message}", "ProductionSystem.Update");
        }
    }

    /// <summary>
    /// Processes production for all active buildings.
    /// </summary>
    private void ProcessProduction()
    {
        var placedBuildings = _buildingPlacementSystem.GetPlacedBuildings();

        foreach (var kvp in placedBuildings)
        {
            var building = kvp.Value;
            ProcessBuildingProduction(building);
        }
    }

    /// <summary>
    /// Processes production for a single building.
    /// </summary>
    private void ProcessBuildingProduction(PlacedBuilding building)
    {
        try
        {
            // Skip if building is not operational
            if (!building.IsOperational)
            {
                // Check if construction is complete
                if (building.ConstructionProgress >= 1.0f)
                {
                    building.IsOperational = true;
                    _errorHandler.LogInfo($"Building {building.BuildingId} at {building.GridPosition} is now operational");
                }
                else
                {
                    // Update construction progress
                    var constructionRate = 1.0f / building.Definition.ConstructionTime; // Progress per second
                    building.ConstructionProgress += constructionRate * PRODUCTION_INTERVAL;
                    building.ConstructionProgress = Math.Min(building.ConstructionProgress, 1.0f);
                }
                return;
            }

            var productionInfo = building.Definition.Production;
            if (productionInfo == null) return;

            // Get or create building production state
            if (!_buildingProductions.ContainsKey(building.Id))
            {
                _buildingProductions[building.Id] = new BuildingProduction(building);
            }

            var production = _buildingProductions[building.Id];

            // Check if we can produce (have inputs and space for outputs)
            if (!CanProduce(building, production))
                return;

            // Consume input resources
            ConsumeInputs(building, production);

            // Produce output resources
            ProduceOutputs(building, production);

            // Update production statistics
            production.TotalProductionCycles++;
            production.LastProductionTime = DateTime.Now;

            Console.WriteLine($"[PRODUCTION] {building.BuildingId} at {building.GridPosition} produced resources");
        }
        catch (Exception ex)
        {
            _errorHandler.HandleError($"Error processing production for building {building.Id}: {ex.Message}", "ProductionSystem.ProcessBuildingProduction");
        }
    }

    /// <summary>
    /// Checks if a building can produce (has inputs, space for outputs).
    /// </summary>
    private bool CanProduce(PlacedBuilding building, BuildingProduction production)
    {
        var productionInfo = building.Definition.Production;
        if (productionInfo == null) return false;

        // Check input requirements
        foreach (var input in productionInfo.InputResources)
        {
            if (!production.InputStorage.HasResource(input.ResourceType, input.Amount))
                return false;
        }

        // Check output space
        foreach (var output in productionInfo.OutputResources)
        {
            if (!production.OutputStorage.CanAccept(output.ResourceType, output.Amount))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Consumes input resources for production.
    /// </summary>
    private void ConsumeInputs(PlacedBuilding building, BuildingProduction production)
    {
        var productionInfo = building.Definition.Production;
        if (productionInfo == null) return;

        foreach (var input in productionInfo.InputResources)
        {
            var consumed = production.InputStorage.RemoveResource(input.ResourceType, input.Amount);
            if (consumed < input.Amount)
            {
                _errorHandler.HandleWarning($"Building {building.Id} could not consume enough {input.ResourceType}", "ProductionSystem.ConsumeInputs");
            }
        }
    }

    /// <summary>
    /// Produces output resources.
    /// </summary>
    private void ProduceOutputs(PlacedBuilding building, BuildingProduction production)
    {
        var productionInfo = building.Definition.Production;
        if (productionInfo == null) return;

        foreach (var output in productionInfo.OutputResources)
        {
            var actualAmount = (int)(output.Amount * productionInfo.ProductionRate * productionInfo.Efficiency);
            var overflow = production.OutputStorage.AddResource(output.ResourceType, actualAmount);
            
            if (overflow > 0)
            {
                _errorHandler.HandleWarning($"Building {building.Id} output storage full, lost {overflow} {output.ResourceType}", "ProductionSystem.ProduceOutputs");
            }
        }
    }

    /// <summary>
    /// Gets production information for a building.
    /// </summary>
    public BuildingProduction? GetBuildingProduction(string buildingId)
    {
        return _buildingProductions.TryGetValue(buildingId, out var production) ? production : null;
    }

    /// <summary>
    /// Gets all building productions.
    /// </summary>
    public IReadOnlyDictionary<string, BuildingProduction> GetAllProductions()
    {
        return _buildingProductions.AsReadOnly();
    }

    /// <summary>
    /// Manually adds resources to a building's input storage (for testing).
    /// </summary>
    public bool AddResourceToBuilding(string buildingId, string resourceId, int amount)
    {
        if (!_buildingProductions.ContainsKey(buildingId))
            return false;

        var production = _buildingProductions[buildingId];
        var overflow = production.InputStorage.AddResource(resourceId, amount);
        
        _errorHandler.LogInfo($"Added {amount - overflow} {resourceId} to building {buildingId}");
        return overflow == 0;
    }

    /// <summary>
    /// Manually removes resources from a building's output storage.
    /// </summary>
    public int RemoveResourceFromBuilding(string buildingId, string resourceId, int amount)
    {
        if (!_buildingProductions.ContainsKey(buildingId))
            return 0;

        var production = _buildingProductions[buildingId];
        var removed = production.OutputStorage.RemoveResource(resourceId, amount);
        
        _errorHandler.LogInfo($"Removed {removed} {resourceId} from building {buildingId}");
        return removed;
    }

    private void OnBuildingPlaced(Building building, System.Numerics.Vector2 position)
    {
        // This will be called when buildings are placed
        // For now, we handle this in ProcessBuildingProduction when we detect new buildings
        Console.WriteLine($"[PRODUCTION] Building placed event received: {building}");
    }

    public void Dispose()
    {
        _eventBus.BuildingPlaced -= OnBuildingPlaced;
        _buildingProductions.Clear();
        _errorHandler.LogInfo("ProductionSystem disposed");
    }
}

/// <summary>
/// Represents the production state of a building.
/// </summary>
public class BuildingProduction
{
    public string BuildingId { get; }
    public PlacedBuilding Building { get; }
    public ResourceStorage InputStorage { get; }
    public ResourceStorage OutputStorage { get; }
    public DateTime CreatedAt { get; }
    public DateTime LastProductionTime { get; set; }
    public int TotalProductionCycles { get; set; }

    public BuildingProduction(PlacedBuilding building)
    {
        Building = building ?? throw new ArgumentNullException(nameof(building));
        BuildingId = building.Id;
        
        // Initialize storage based on building definition
        var storageInfo = building.Definition.Storage;
        InputStorage = new ResourceStorage(storageInfo?.InputCapacity ?? 50);
        OutputStorage = new ResourceStorage(storageInfo?.OutputCapacity ?? 50);
        
        CreatedAt = DateTime.Now;
        LastProductionTime = DateTime.MinValue;
        TotalProductionCycles = 0;
    }

    /// <summary>
    /// Gets the production efficiency (0.0 to 1.0).
    /// </summary>
    public float GetEfficiency()
    {
        return Building.Definition.Production?.Efficiency ?? 1.0f;
    }

    /// <summary>
    /// Gets the production rate (items per second).
    /// </summary>
    public float GetProductionRate()
    {
        return Building.Definition.Production?.ProductionRate ?? 1.0f;
    }

    /// <summary>
    /// Checks if the building is currently producing.
    /// </summary>
    public bool IsProducing()
    {
        return Building.IsOperational && 
               DateTime.Now - LastProductionTime < TimeSpan.FromSeconds(2);
    }

    /// <summary>
    /// Gets input storage utilization percentage.
    /// </summary>
    public float GetInputUtilization()
    {
        return InputStorage.GetUtilization();
    }

    /// <summary>
    /// Gets output storage utilization percentage.
    /// </summary>
    public float GetOutputUtilization()
    {
        return OutputStorage.GetUtilization();
    }
}
