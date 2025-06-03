using System.Numerics;
using TransportGame.Game.Constants;
using TransportGame.Game.Managers;
using TransportGame.Game.Models;
using TransportGame.Game.Utils;

namespace TransportGame.Game.Systems;

/// <summary>
/// Handles multi-tile building placement, validation, and preview.
/// Supports rotation, collision detection, and terrain validation.
/// </summary>
public class BuildingPlacementSystem : IDisposable
{
    private readonly EventBus _eventBus;
    private readonly ErrorHandler _errorHandler;
    private readonly TilemapManager _tilemapManager;
    private readonly BuildingDefinitionManager _buildingDefinitionManager;

    // Placement state
    private bool _isPlacementMode = false;
    private string? _selectedBuildingId = null;
    private BuildingDefinition? _selectedBuilding = null;
    private int _currentRotation = 0; // 0, 90, 180, 270 degrees
    private Vector2 _previewPosition = Vector2.Zero;
    private bool _isValidPlacement = false;
    private bool _lastValidPlacement = false;

    // Building tracking
    private readonly Dictionary<Vector2, PlacedBuilding> _placedBuildings;
    private readonly HashSet<Vector2> _occupiedTiles;

    public bool IsPlacementMode => _isPlacementMode;
    public string? SelectedBuildingId => _selectedBuildingId;
    public Vector2 PreviewPosition => _previewPosition;
    public bool IsValidPlacement => _isValidPlacement;

    public BuildingPlacementSystem(
        EventBus eventBus,
        TilemapManager tilemapManager,
        BuildingDefinitionManager buildingDefinitionManager)
    {
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _tilemapManager = tilemapManager ?? throw new ArgumentNullException(nameof(tilemapManager));
        _buildingDefinitionManager = buildingDefinitionManager ?? throw new ArgumentNullException(nameof(buildingDefinitionManager));
        _errorHandler = new ErrorHandler();

        _placedBuildings = new Dictionary<Vector2, PlacedBuilding>();
        _occupiedTiles = new HashSet<Vector2>();

        // Subscribe to events
        _eventBus.KeyPressed += OnKeyPressed;
        _eventBus.MouseClicked += OnMouseClicked;

        _errorHandler.LogInfo("BuildingPlacementSystem initialized");
    }

    /// <summary>
    /// Enters building placement mode for the specified building type.
    /// </summary>
    public bool StartPlacement(string buildingId)
    {
        var definition = _buildingDefinitionManager.GetDefinition(buildingId);
        if (definition == null)
        {
            _errorHandler.HandleWarning($"Building definition not found: {buildingId}", "BuildingPlacementSystem.StartPlacement");
            return false;
        }

        if (!definition.PlacementRules.Buildable)
        {
            _errorHandler.HandleWarning($"Building is not buildable: {buildingId}", "BuildingPlacementSystem.StartPlacement");
            return false;
        }

        _isPlacementMode = true;
        _selectedBuildingId = buildingId;
        _selectedBuilding = definition;
        _currentRotation = 0;
        _previewPosition = Vector2.Zero;
        _isValidPlacement = false;

        _errorHandler.LogInfo($"Started building placement for: {buildingId}");
        return true;
    }

    /// <summary>
    /// Exits building placement mode.
    /// </summary>
    public void StopPlacement()
    {
        _isPlacementMode = false;
        _selectedBuildingId = null;
        _selectedBuilding = null;
        _currentRotation = 0;
        _previewPosition = Vector2.Zero;
        _isValidPlacement = false;

        _errorHandler.LogInfo("Stopped building placement");
    }

    /// <summary>
    /// Updates the preview position and validates placement.
    /// </summary>
    public void UpdatePreview(Vector2 worldPosition)
    {
        if (!_isPlacementMode || _selectedBuilding == null)
            return;

        // Convert world position to grid position
        var gridPosition = new Vector2(
            (float)Math.Floor(worldPosition.X / GameConstants.TILE_SIZE),
            (float)Math.Floor(worldPosition.Y / GameConstants.TILE_SIZE)
        );

        _previewPosition = gridPosition;
        _isValidPlacement = ValidatePlacement(gridPosition, _selectedBuilding, _currentRotation);

        // Debug output (only when placement validity changes to avoid spam)
        if (_isValidPlacement != _lastValidPlacement)
        {
            var tileType = _tilemapManager.GetTileType((int)gridPosition.X, (int)gridPosition.Y);
            Console.WriteLine($"[BUILDING-PLACEMENT] Placement validity changed: {_selectedBuilding.Id} at Grid({gridPosition.X},{gridPosition.Y}) TileType:{tileType} Valid:{_isValidPlacement}");
            _lastValidPlacement = _isValidPlacement;
        }
    }

    /// <summary>
    /// Attempts to place the selected building at the current preview position.
    /// </summary>
    public bool PlaceBuilding()
    {
        if (!_isPlacementMode || _selectedBuilding == null || !_isValidPlacement)
            return false;

        try
        {
            var placedBuilding = new PlacedBuilding
            {
                Id = Guid.NewGuid().ToString(),
                BuildingId = _selectedBuildingId!,
                Definition = _selectedBuilding,
                GridPosition = _previewPosition,
                Rotation = _currentRotation,
                ConstructionProgress = 0.0f,
                IsOperational = false
            };

            // Mark tiles as occupied
            var occupiedTiles = GetOccupiedTiles(_previewPosition, _selectedBuilding, _currentRotation);
            foreach (var tile in occupiedTiles)
            {
                _occupiedTiles.Add(tile);
            }

            // Store the building
            _placedBuildings[_previewPosition] = placedBuilding;

            _errorHandler.LogInfo($"Placed building {_selectedBuildingId} at {_previewPosition}");

            // Emit building placed event
            _eventBus.EmitBuildingPlaced(placedBuilding, _previewPosition);

            return true;
        }
        catch (Exception ex)
        {
            _errorHandler.HandleError($"Failed to place building: {ex.Message}", "BuildingPlacementSystem.PlaceBuilding");
            return false;
        }
    }

    /// <summary>
    /// Rotates the selected building by 90 degrees.
    /// </summary>
    public void RotateBuilding()
    {
        if (!_isPlacementMode || _selectedBuilding == null || !_selectedBuilding.RotationAllowed)
            return;

        _currentRotation = (_currentRotation + 90) % 360;
        _isValidPlacement = ValidatePlacement(_previewPosition, _selectedBuilding, _currentRotation);

        _errorHandler.LogInfo($"Rotated building to {_currentRotation} degrees");
    }

    /// <summary>
    /// Validates if a building can be placed at the specified position.
    /// </summary>
    private bool ValidatePlacement(Vector2 gridPosition, BuildingDefinition building, int rotation)
    {
        try
        {
            var occupiedTiles = GetOccupiedTiles(gridPosition, building, rotation);

            foreach (var tile in occupiedTiles)
            {
                // Check if tile is within world bounds
                if (!_tilemapManager.IsValidGridPosition((int)tile.X, (int)tile.Y))
                    return false;

                // Check if tile is already occupied
                if (_occupiedTiles.Contains(tile))
                    return false;

                // Check terrain requirements
                var tileType = _tilemapManager.GetTileType((int)tile.X, (int)tile.Y);
                if (!IsTerrainAllowed(tileType.ToString(), building.PlacementRules))
                    return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _errorHandler.HandleError($"Error validating placement: {ex.Message}", "BuildingPlacementSystem.ValidatePlacement");
            return false;
        }
    }

    /// <summary>
    /// Gets all tiles occupied by a building at the specified position and rotation.
    /// </summary>
    private List<Vector2> GetOccupiedTiles(Vector2 gridPosition, BuildingDefinition building, int rotation)
    {
        var tiles = new List<Vector2>();
        var size = building.Size;

        // For now, only support rectangular buildings
        // TODO: Add support for custom shapes
        if (size.Shape == "rectangle")
        {
            var width = size.Width;
            var height = size.Height;

            // Apply rotation (swap width/height for 90/270 degree rotations)
            if (rotation == 90 || rotation == 270)
            {
                (width, height) = (height, width);
            }

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    tiles.Add(new Vector2(gridPosition.X + x, gridPosition.Y + y));
                }
            }
        }

        return tiles;
    }

    /// <summary>
    /// Checks if the terrain type is allowed for the building.
    /// </summary>
    private bool IsTerrainAllowed(string terrainType, PlacementRules rules)
    {
        // Check forbidden terrain
        if (rules.ForbiddenTerrain.Contains(terrainType))
            return false;

        // Check allowed terrain (if specified)
        if (rules.AllowedTerrain.Count > 0 && !rules.AllowedTerrain.Contains(terrainType))
            return false;

        return true;
    }

    /// <summary>
    /// Gets all placed buildings.
    /// </summary>
    public IReadOnlyDictionary<Vector2, PlacedBuilding> GetPlacedBuildings()
    {
        return _placedBuildings.AsReadOnly();
    }

    /// <summary>
    /// Gets the building at the specified grid position.
    /// </summary>
    public PlacedBuilding? GetBuildingAt(Vector2 gridPosition)
    {
        return _placedBuildings.TryGetValue(gridPosition, out var building) ? building : null;
    }

    private void OnKeyPressed(string key)
    {
        switch (key)
        {
            case "R":
            case "r":
                RotateBuilding();
                break;
            case "Escape":
                if (_isPlacementMode)
                    StopPlacement();
                break;
            // Player-buildable buildings only (farms and mines are map-generated)
            case "1":
            case "D1":
                StartPlacement("steel_works");
                break;
            case "2":
            case "D2":
                StartPlacement("food_factory");
                break;
            case "3":
            case "D3":
                StartPlacement("train_depot");
                break;
            case "4":
            case "D4":
                StartPlacement("station");
                break;
            // Resource testing shortcuts
            case "7":
            case "D7":
                AddTestResources();
                break;
            case "8":
            case "D8":
                ShowResourceInfo();
                break;
        }
    }

    private void OnMouseClicked(Vector2 position)
    {
        if (_isPlacementMode)
        {
            PlaceBuilding();
        }
    }

    /// <summary>
    /// Adds test resources to all placed buildings (for testing production chains).
    /// </summary>
    private void AddTestResources()
    {
        try
        {
            var placedBuildings = GetPlacedBuildings();
            var productionSystem = GetProductionSystem();

            if (productionSystem == null)
            {
                _errorHandler.HandleWarning("ProductionSystem not available for resource testing", "BuildingPlacementSystem.AddTestResources");
                return;
            }

            foreach (var building in placedBuildings.Values)
            {
                // Add appropriate input resources based on building type
                switch (building.BuildingId)
                {
                    case "steel_works":
                        productionSystem.AddResourceToBuilding(building.Id, "iron_ore", 50);
                        _errorHandler.LogInfo($"Added 50 iron ore to steel works at {building.GridPosition}");
                        break;
                    case "food_factory":
                        productionSystem.AddResourceToBuilding(building.Id, "grain", 50);
                        _errorHandler.LogInfo($"Added 50 grain to food factory at {building.GridPosition}");
                        break;
                }
            }

            _errorHandler.LogInfo($"Added test resources to {placedBuildings.Count} buildings");
        }
        catch (Exception ex)
        {
            _errorHandler.HandleError($"Error adding test resources: {ex.Message}", "BuildingPlacementSystem.AddTestResources");
        }
    }

    /// <summary>
    /// Shows resource information for all buildings.
    /// </summary>
    private void ShowResourceInfo()
    {
        try
        {
            var placedBuildings = GetPlacedBuildings();
            var productionSystem = GetProductionSystem();

            if (productionSystem == null)
            {
                _errorHandler.HandleWarning("ProductionSystem not available", "BuildingPlacementSystem.ShowResourceInfo");
                return;
            }

            Console.WriteLine("[RESOURCE-INFO] === Building Resource Status ===");

            foreach (var building in placedBuildings.Values)
            {
                var production = productionSystem.GetBuildingProduction(building.Id);
                if (production != null)
                {
                    Console.WriteLine($"[RESOURCE-INFO] {building.BuildingId} at {building.GridPosition}:");
                    Console.WriteLine($"[RESOURCE-INFO]   Status: {(building.IsOperational ? "Operational" : $"Construction {building.ConstructionProgress:P0}")}");
                    Console.WriteLine($"[RESOURCE-INFO]   Input Storage: {production.InputStorage.UsedCapacity}/{production.InputStorage.MaxCapacity}");
                    Console.WriteLine($"[RESOURCE-INFO]   Output Storage: {production.OutputStorage.UsedCapacity}/{production.OutputStorage.MaxCapacity}");
                    Console.WriteLine($"[RESOURCE-INFO]   Production Cycles: {production.TotalProductionCycles}");
                }
            }

            Console.WriteLine("[RESOURCE-INFO] === End Resource Status ===");
        }
        catch (Exception ex)
        {
            _errorHandler.HandleError($"Error showing resource info: {ex.Message}", "BuildingPlacementSystem.ShowResourceInfo");
        }
    }

    /// <summary>
    /// Gets the ProductionSystem instance (helper method).
    /// </summary>
    private ProductionSystem? GetProductionSystem()
    {
        // This is a bit of a hack - in a real implementation, we'd inject this dependency
        // For now, we'll try to access it through reflection or a static reference
        // TODO: Refactor to proper dependency injection
        return null; // Will be implemented when we integrate with main game
    }

    public void Dispose()
    {
        _eventBus.KeyPressed -= OnKeyPressed;
        _eventBus.MouseClicked -= OnMouseClicked;
        _errorHandler.LogInfo("BuildingPlacementSystem disposed");
    }
}

/// <summary>
/// Represents a building that has been placed on the map.
/// </summary>
public class PlacedBuilding
{
    public string Id { get; set; } = string.Empty;
    public string BuildingId { get; set; } = string.Empty;
    public BuildingDefinition Definition { get; set; } = null!;
    public Vector2 GridPosition { get; set; }
    public int Rotation { get; set; }
    public float ConstructionProgress { get; set; }
    public bool IsOperational { get; set; }
    public DateTime PlacedAt { get; set; } = DateTime.Now;
}
