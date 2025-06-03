using Microsoft.Xna.Framework;
using TransportGame.Game.Entities;
using TransportGame.Game.Managers;
using TransportGame.Game.Models;
using TransportGame.Game.Utils;
using TransportGame.Game.Constants;

namespace TransportGame.Game.Systems;

/// <summary>
/// Generates natural industries (farms, mines) on the map during world generation.
/// These are pre-placed resource producers that the player cannot build manually.
/// </summary>
public class IndustryGenerationSystem : IDisposable
{
    private readonly TilemapManager _tilemapManager;
    private readonly BuildingDefinitionManager _buildingDefinitionManager;
    private readonly ErrorHandler _errorHandler;
    private readonly Random _random;

    // Generated industries
    private readonly Dictionary<Vector2, PlacedBuilding> _generatedIndustries;

    public IReadOnlyDictionary<Vector2, PlacedBuilding> GeneratedIndustries => _generatedIndustries.AsReadOnly();

    public IndustryGenerationSystem(TilemapManager tilemapManager, BuildingDefinitionManager buildingDefinitionManager)
    {
        _tilemapManager = tilemapManager ?? throw new ArgumentNullException(nameof(tilemapManager));
        _buildingDefinitionManager = buildingDefinitionManager ?? throw new ArgumentNullException(nameof(buildingDefinitionManager));
        _errorHandler = new ErrorHandler();
        _random = new Random();
        _generatedIndustries = new Dictionary<Vector2, PlacedBuilding>();

        _errorHandler.LogInfo("IndustryGenerationSystem initialized");
    }

    /// <summary>
    /// Generates natural industries across the map based on terrain.
    /// </summary>
    public void GenerateIndustries()
    {
        try
        {
            Console.WriteLine("[INDUSTRY-GEN] Starting industry generation...");
            
            // Clear existing industries
            _generatedIndustries.Clear();

            // Generate farms on farmland
            GenerateFarms();

            // Generate mines on mountains
            GenerateMines();

            Console.WriteLine($"[INDUSTRY-GEN] Generated {_generatedIndustries.Count} industries total");
            _errorHandler.LogInfo($"Generated {_generatedIndustries.Count} natural industries");
        }
        catch (Exception ex)
        {
            _errorHandler.HandleError($"Error generating industries: {ex.Message}", "IndustryGenerationSystem.GenerateIndustries");
        }
    }

    /// <summary>
    /// Generates farms on suitable farmland areas.
    /// </summary>
    private void GenerateFarms()
    {
        try
        {
            var farmDefinition = _buildingDefinitionManager.GetDefinition("farm");
            if (farmDefinition == null)
            {
                _errorHandler.HandleWarning("Farm definition not found", "IndustryGenerationSystem.GenerateFarms");
                return;
            }

            var farmCount = 0;
            var attempts = 0;
            var maxAttempts = 1000;
            var targetFarms = 15; // Target number of farms

            while (farmCount < targetFarms && attempts < maxAttempts)
            {
                attempts++;

                // Random position on map
                var x = _random.Next(0, _tilemapManager.WorldWidth - farmDefinition.Size.Width);
                var y = _random.Next(0, _tilemapManager.WorldHeight - farmDefinition.Size.Height);
                var position = new Vector2(x, y);

                // Check if this area is suitable for a farm
                if (IsSuitableForFarm(position, farmDefinition))
                {
                    var farm = CreateIndustry(farmDefinition, position, $"farm_{farmCount}");
                    if (farm != null)
                    {
                        _generatedIndustries[position] = farm;
                        farmCount++;
                        Console.WriteLine($"[INDUSTRY-GEN] Generated farm {farmCount} at ({x},{y})");
                    }
                }
            }

            Console.WriteLine($"[INDUSTRY-GEN] Generated {farmCount} farms in {attempts} attempts");
        }
        catch (Exception ex)
        {
            _errorHandler.HandleError($"Error generating farms: {ex.Message}", "IndustryGenerationSystem.GenerateFarms");
        }
    }

    /// <summary>
    /// Generates mines on suitable mountain areas.
    /// </summary>
    private void GenerateMines()
    {
        try
        {
            var mineDefinition = _buildingDefinitionManager.GetDefinition("mine");
            if (mineDefinition == null)
            {
                _errorHandler.HandleWarning("Mine definition not found", "IndustryGenerationSystem.GenerateMines");
                return;
            }

            var mineCount = 0;
            var attempts = 0;
            var maxAttempts = 1000;
            var targetMines = 8; // Target number of mines

            while (mineCount < targetMines && attempts < maxAttempts)
            {
                attempts++;

                // Random position on map
                var x = _random.Next(0, _tilemapManager.WorldWidth - mineDefinition.Size.Width);
                var y = _random.Next(0, _tilemapManager.WorldHeight - mineDefinition.Size.Height);
                var position = new Vector2(x, y);

                // Check if this area is suitable for a mine
                if (IsSuitableForMine(position, mineDefinition))
                {
                    var mine = CreateIndustry(mineDefinition, position, $"mine_{mineCount}");
                    if (mine != null)
                    {
                        _generatedIndustries[position] = mine;
                        mineCount++;
                        Console.WriteLine($"[INDUSTRY-GEN] Generated mine {mineCount} at ({x},{y})");
                    }
                }
            }

            Console.WriteLine($"[INDUSTRY-GEN] Generated {mineCount} mines in {attempts} attempts");
        }
        catch (Exception ex)
        {
            _errorHandler.HandleError($"Error generating mines: {ex.Message}", "IndustryGenerationSystem.GenerateMines");
        }
    }

    /// <summary>
    /// Checks if a position is suitable for a farm.
    /// </summary>
    private bool IsSuitableForFarm(Vector2 position, BuildingDefinition farmDefinition)
    {
        try
        {
            // Check if all tiles in the farm area are farmland or grass
            for (int x = 0; x < farmDefinition.Size.Width; x++)
            {
                for (int y = 0; y < farmDefinition.Size.Height; y++)
                {
                    var checkX = (int)position.X + x;
                    var checkY = (int)position.Y + y;

                    if (!_tilemapManager.IsValidGridPosition(checkX, checkY))
                        return false;

                    var tileType = _tilemapManager.GetTileType(checkX, checkY);
                    if (tileType != TileType.Farmland && tileType != TileType.Grass)
                        return false;
                }
            }

            // Check for minimum distance from other industries
            return !IsNearOtherIndustry(position, 5);
        }
        catch (Exception ex)
        {
            _errorHandler.HandleError($"Error checking farm suitability: {ex.Message}", "IndustryGenerationSystem.IsSuitableForFarm");
            return false;
        }
    }

    /// <summary>
    /// Checks if a position is suitable for a mine.
    /// </summary>
    private bool IsSuitableForMine(Vector2 position, BuildingDefinition mineDefinition)
    {
        try
        {
            // Check if all tiles in the mine area are mountains or hills
            for (int x = 0; x < mineDefinition.Size.Width; x++)
            {
                for (int y = 0; y < mineDefinition.Size.Height; y++)
                {
                    var checkX = (int)position.X + x;
                    var checkY = (int)position.Y + y;

                    if (!_tilemapManager.IsValidGridPosition(checkX, checkY))
                        return false;

                    var tileType = _tilemapManager.GetTileType(checkX, checkY);
                    if (tileType != TileType.Mountain && tileType != TileType.Hills)
                        return false;
                }
            }

            // Check for minimum distance from other industries
            return !IsNearOtherIndustry(position, 4);
        }
        catch (Exception ex)
        {
            _errorHandler.HandleError($"Error checking mine suitability: {ex.Message}", "IndustryGenerationSystem.IsSuitableForMine");
            return false;
        }
    }

    /// <summary>
    /// Checks if there's another industry nearby.
    /// </summary>
    private bool IsNearOtherIndustry(Vector2 position, int minDistance)
    {
        foreach (var industry in _generatedIndustries.Values)
        {
            var distance = Vector2.Distance(position, industry.GridPosition);
            if (distance < minDistance)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Creates an industry building instance.
    /// </summary>
    private PlacedBuilding? CreateIndustry(BuildingDefinition definition, Vector2 position, string id)
    {
        try
        {
            var industry = new PlacedBuilding
            {
                Id = id,
                BuildingId = definition.Id,
                Definition = definition,
                GridPosition = new System.Numerics.Vector2(position.X, position.Y),
                Rotation = 0,
                IsOperational = true, // Industries start operational
                ConstructionProgress = 1.0f, // Already built
                PlacedAt = DateTime.Now
            };

            return industry;
        }
        catch (Exception ex)
        {
            _errorHandler.HandleError($"Error creating industry: {ex.Message}", "IndustryGenerationSystem.CreateIndustry");
            return null;
        }
    }

    /// <summary>
    /// Gets an industry at a specific position.
    /// </summary>
    public PlacedBuilding? GetIndustryAt(Vector2 position)
    {
        return _generatedIndustries.TryGetValue(position, out var industry) ? industry : null;
    }

    /// <summary>
    /// Gets an industry at a specific grid position (checks all tiles of multi-tile industries).
    /// </summary>
    public PlacedBuilding? GetIndustryAtGridPosition(int gridX, int gridY)
    {
        foreach (var industry in _generatedIndustries.Values)
        {
            var industryX = (int)industry.GridPosition.X;
            var industryY = (int)industry.GridPosition.Y;
            var width = industry.Definition.Size.Width;
            var height = industry.Definition.Size.Height;

            if (gridX >= industryX && gridX < industryX + width &&
                gridY >= industryY && gridY < industryY + height)
            {
                return industry;
            }
        }
        return null;
    }

    public void Dispose()
    {
        _generatedIndustries.Clear();
        _errorHandler.LogInfo("IndustryGenerationSystem disposed");
    }
}
