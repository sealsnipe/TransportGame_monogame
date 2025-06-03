using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Numerics;
using TransportGame.Game.Constants;

namespace TransportGame.Game.Managers;

/// <summary>
/// Manages the game world tilemap and terrain.
/// Ported from Godot TilemapManager.gd.
/// </summary>
public class TilemapManager : IDisposable
{
    private readonly EventBus _eventBus;
    private readonly ErrorHandler _errorHandler;
    
    // Tilemap data
    private TileType[,] _tiles;
    private readonly int _worldWidth;
    private readonly int _worldHeight;
    private readonly int _tileSize;
    
    // Graphics resources
    private Texture2D? _pixelTexture;
    private readonly Dictionary<TileType, Microsoft.Xna.Framework.Color> _tileColors;
    
    // Building placement tracking
    private readonly Dictionary<System.Numerics.Vector2, string> _buildingGrid;

    public int WorldWidth => _worldWidth;
    public int WorldHeight => _worldHeight;
    public int TileSize => _tileSize;

    public TilemapManager(EventBus eventBus)
    {
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _errorHandler = new ErrorHandler();
        
        _worldWidth = GameConstants.WORLD_WIDTH;
        _worldHeight = GameConstants.WORLD_HEIGHT;
        _tileSize = GameConstants.TILE_SIZE;
        
        // Initialize tilemap
        _tiles = new TileType[_worldWidth, _worldHeight];
        _buildingGrid = new Dictionary<System.Numerics.Vector2, string>();
        
        // Define tile colors for rendering - schönere Landschaftsfarben
        _tileColors = new Dictionary<TileType, Microsoft.Xna.Framework.Color>
        {
            [TileType.Grass] = new Microsoft.Xna.Framework.Color(34, 139, 34),        // ForestGreen
            [TileType.Water] = new Microsoft.Xna.Framework.Color(30, 144, 255),       // DodgerBlue
            [TileType.DeepWater] = new Microsoft.Xna.Framework.Color(0, 100, 200),    // Dunkleres Blau
            [TileType.Beach] = new Microsoft.Xna.Framework.Color(238, 203, 173),      // Sandfarbe
            [TileType.Mountain] = new Microsoft.Xna.Framework.Color(105, 105, 105),   // DimGray
            [TileType.Hills] = new Microsoft.Xna.Framework.Color(169, 169, 169),      // DarkGray
            [TileType.Forest] = new Microsoft.Xna.Framework.Color(0, 100, 0),         // DarkGreen
            [TileType.Farmland] = new Microsoft.Xna.Framework.Color(255, 215, 0),     // Gold/Gelb
            [TileType.Dirt] = new Microsoft.Xna.Framework.Color(139, 69, 19),         // SaddleBrown
            [TileType.Desert] = new Microsoft.Xna.Framework.Color(255, 218, 185),     // PeachPuff
            [TileType.Rail] = new Microsoft.Xna.Framework.Color(101, 67, 33),         // Dunkelbraun
            [TileType.Road] = new Microsoft.Xna.Framework.Color(64, 64, 64)           // DarkGray
        };
        
        // Subscribe to events
        SubscribeToEvents();
        
        // Generate initial world
        GenerateWorld();
        
        _errorHandler.LogInfo("TilemapManager initialized");
    }

    private void SubscribeToEvents()
    {
        _eventBus.BuildingPlaced += OnBuildingPlaced;
        _eventBus.BuildingRemoved += OnBuildingRemoved;
    }

    /// <summary>
    /// Loads content for the tilemap manager.
    /// </summary>
    public void LoadContent(ContentManager content)
    {
        try
        {
            // For now, we'll use a simple pixel texture for tiles
            // In a full implementation, you'd load actual tile textures here
            _errorHandler.LogInfo("TilemapManager content loaded");
        }
        catch (Exception ex)
        {
            _errorHandler.HandleError($"Failed to load tilemap content: {ex.Message}", "TilemapManager.LoadContent");
        }
    }

    /// <summary>
    /// Updates the tilemap manager.
    /// </summary>
    public void Update(GameTime gameTime)
    {
        // Tilemap doesn't need frequent updates currently
        // Could be used for animated tiles, weather effects, etc.
    }

    /// <summary>
    /// Draws the tilemap.
    /// </summary>
    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        if (_pixelTexture == null)
        {
            // Create a simple pixel texture if we don't have one
            CreatePixelTexture(spriteBatch.GraphicsDevice);
        }

        if (_pixelTexture == null)
            return;

        // For now, draw all tiles (we'll optimize culling later when camera system is integrated)
        // TODO: Implement proper camera-aware culling
        for (int x = 0; x < _worldWidth; x++)
        {
            for (int y = 0; y < _worldHeight; y++)
            {
                DrawTile(spriteBatch, x, y);
            }
        }
        
        // Draw building indicators
        DrawBuildingGrid(spriteBatch);
    }

    private void CreatePixelTexture(GraphicsDevice graphicsDevice)
    {
        _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
        _pixelTexture.SetData(new[] { Microsoft.Xna.Framework.Color.White });
    }

    private void DrawTile(SpriteBatch spriteBatch, int gridX, int gridY)
    {
        if (_pixelTexture == null || !IsValidGridPosition(gridX, gridY))
            return;
            
        var tileType = _tiles[gridX, gridY];
        var color = _tileColors.TryGetValue(tileType, out var tileColor) ? tileColor : Microsoft.Xna.Framework.Color.Magenta;

        var rectangle = new Microsoft.Xna.Framework.Rectangle(
            gridX * _tileSize,
            gridY * _tileSize,
            _tileSize,
            _tileSize
        );

        spriteBatch.Draw(_pixelTexture, rectangle, color * 0.8f);

        // Draw tile border (optional - this creates the dark grid!)
        // DrawTileBorder(spriteBatch, rectangle, Microsoft.Xna.Framework.Color.Black * 0.3f);
    }

    private void DrawTileBorder(SpriteBatch spriteBatch, Microsoft.Xna.Framework.Rectangle rectangle, Microsoft.Xna.Framework.Color color)
    {
        if (_pixelTexture == null)
            return;
            
        var thickness = 1;

        // Top
        spriteBatch.Draw(_pixelTexture, new Microsoft.Xna.Framework.Rectangle(rectangle.X, rectangle.Y, rectangle.Width, thickness), color);

        // Bottom
        spriteBatch.Draw(_pixelTexture, new Microsoft.Xna.Framework.Rectangle(rectangle.X, rectangle.Bottom - thickness, rectangle.Width, thickness), color);

        // Left
        spriteBatch.Draw(_pixelTexture, new Microsoft.Xna.Framework.Rectangle(rectangle.X, rectangle.Y, thickness, rectangle.Height), color);

        // Right
        spriteBatch.Draw(_pixelTexture, new Microsoft.Xna.Framework.Rectangle(rectangle.Right - thickness, rectangle.Y, thickness, rectangle.Height), color);
    }

    private void DrawBuildingGrid(SpriteBatch spriteBatch)
    {
        if (_pixelTexture == null)
            return;
            
        foreach (var kvp in _buildingGrid)
        {
            var gridPos = kvp.Key;
            var buildingType = kvp.Value;
            
            var rectangle = new Microsoft.Xna.Framework.Rectangle(
                (int)(gridPos.X * _tileSize),
                (int)(gridPos.Y * _tileSize),
                _tileSize,
                _tileSize
            );

            // Draw building indicator with different colors based on type
            var buildingColor = GetBuildingColor(buildingType);
            spriteBatch.Draw(_pixelTexture, rectangle, buildingColor * 0.7f);

            // Draw building border
            DrawTileBorder(spriteBatch, rectangle, Microsoft.Xna.Framework.Color.White);
        }
    }

    private Microsoft.Xna.Framework.Color GetBuildingColor(string buildingType)
    {
        return buildingType switch
        {
            GameConstants.BUILDING_FARM => Microsoft.Xna.Framework.Color.LightGreen,
            GameConstants.BUILDING_MINE => Microsoft.Xna.Framework.Color.Orange,
            GameConstants.BUILDING_DEPOT => Microsoft.Xna.Framework.Color.Purple,
            GameConstants.BUILDING_FACTORY => Microsoft.Xna.Framework.Color.Red,
            GameConstants.BUILDING_STATION => Microsoft.Xna.Framework.Color.Blue,
            GameConstants.BUILDING_CITY => Microsoft.Xna.Framework.Color.Yellow,
            _ => Microsoft.Xna.Framework.Color.White
        };
    }

    /// <summary>
    /// Generates an organized landscape with distinct regions.
    /// Grün (Grass) ist der Hintergrund, darauf befinden sich organisierte Gebiete.
    /// </summary>
    private void GenerateWorld()
    {
        var random = new Random(12345); // Fixed seed for consistent generation

        // Step 1: Fill everything with grass (green background)
        for (int x = 0; x < _worldWidth; x++)
        {
            for (int y = 0; y < _worldHeight; y++)
            {
                _tiles[x, y] = TileType.Grass;
            }
        }

        // Step 2: Add organized regions
        CreateOrganizedLandscape(random);

        _errorHandler.LogInfo($"Generated organized landscape: {_worldWidth}x{_worldHeight} tiles");
    }

    private void CreateOrganizedLandscape(Random random)
    {
        // 1. Create a mountain range (Gebirgskette) - scaled 2x for larger map
        CreateMountainRange(random.Next(100, 200), random.Next(60, 120), 160, 0.3); // Horizontal range
        CreateMountainRange(random.Next(500, 600), random.Next(80, 160), 120, 1.2); // Diagonal range

        // 2. Create several farmland areas (Farmländer) - scaled 2x for larger map
        CreateFarmlandRegion(160, 240, 80, 60);   // Large farmland area
        CreateFarmlandRegion(400, 160, 70, 50);   // Medium farmland area
        CreateFarmlandRegion(600, 280, 60, 40);   // Smaller farmland area
        CreateFarmlandRegion(300, 320, 50, 50);   // Square farmland area

        // 3. Create dirt/earth masses (Erdmassen) - scaled 2x for larger map
        CreateDirtRegion(240, 80, 100, 50);       // Large dirt area
        CreateDirtRegion(560, 200, 60, 70);       // Tall dirt area
        CreateDirtRegion(100, 360, 80, 40);       // Bottom dirt area

        // 4. Add some small water bodies (kleine Seen) - scaled 2x for larger map
        CreateWaterBody(320, 180, 30);            // Small lake
        CreateWaterBody(640, 320, 24);            // Another small lake

        // 5. Add some forest patches for variety - scaled 2x for larger map
        CreateForestPatch(60, 200, 50, 40);       // Left forest
        CreateForestPatch(700, 100, 40, 60);      // Right forest
    }

    private float GenerateNoise(int x, int y, Random random)
    {
        // Simple noise generation - in a real game you'd use Perlin noise or similar
        var seed = x * 1000 + y;
        random = new Random(seed);
        return (float)random.NextDouble();
    }

    private float GenerateHeightNoise(int x, int y)
    {
        // Simulate height-based terrain (mountains in center, water at edges)
        var centerX = _worldWidth / 2f;
        var centerY = _worldHeight / 2f;
        var distanceFromCenter = (float)Math.Sqrt((x - centerX) * (x - centerX) + (y - centerY) * (y - centerY));
        var maxDistance = (float)Math.Sqrt(centerX * centerX + centerY * centerY);

        // Invert so center is high, edges are low
        return 1f - (distanceFromCenter / maxDistance);
    }

    private float GenerateMoistureNoise(int x, int y)
    {
        // Simple moisture pattern - more moisture near water sources
        var moistureSeed = x * 73 + y * 37;
        var random = new Random(moistureSeed);
        var baseMoisture = (float)random.NextDouble();

        // Add some wave patterns for rivers/moisture
        var waveX = (float)Math.Sin(x * 0.1) * 0.3f;
        var waveY = (float)Math.Sin(y * 0.15) * 0.3f;

        return Math.Clamp(baseMoisture + waveX + waveY, 0f, 1f);
    }

    private void AddCoherentRegions(Random random)
    {
        // Add some larger coherent regions for more realistic terrain

        // Add a few large farmland areas
        for (int i = 0; i < 5; i++)
        {
            var centerX = random.Next(20, _worldWidth - 20);
            var centerY = random.Next(20, _worldHeight - 20);
            var radius = random.Next(8, 15);

            CreateCircularRegion(centerX, centerY, radius, TileType.Farmland);
        }

        // Add some forest patches
        for (int i = 0; i < 8; i++)
        {
            var centerX = random.Next(10, _worldWidth - 10);
            var centerY = random.Next(10, _worldHeight - 10);
            var radius = random.Next(5, 12);

            CreateCircularRegion(centerX, centerY, radius, TileType.Forest);
        }

        // Add some mountain ranges
        for (int i = 0; i < 3; i++)
        {
            var startX = random.Next(0, _worldWidth);
            var startY = random.Next(0, _worldHeight);
            var length = random.Next(15, 30);
            var direction = random.NextDouble() * Math.PI * 2;

            CreateMountainRange(startX, startY, length, direction);
        }
    }

    private void CreateCircularRegion(int centerX, int centerY, int radius, TileType tileType)
    {
        for (int x = centerX - radius; x <= centerX + radius; x++)
        {
            for (int y = centerY - radius; y <= centerY + radius; y++)
            {
                if (IsValidGridPosition(x, y))
                {
                    var distance = Math.Sqrt((x - centerX) * (x - centerX) + (y - centerY) * (y - centerY));
                    if (distance <= radius)
                    {
                        // Only place on suitable terrain
                        var currentTile = _tiles[x, y];
                        if (currentTile == TileType.Grass || currentTile == TileType.Dirt ||
                            (tileType == TileType.Forest && currentTile == TileType.Hills))
                        {
                            _tiles[x, y] = tileType;
                        }
                    }
                }
            }
        }
    }

    private void CreateMountainRange(int startX, int startY, int length, double direction)
    {
        for (int i = 0; i < length; i++)
        {
            var x = (int)(startX + Math.Cos(direction) * i);
            var y = (int)(startY + Math.Sin(direction) * i);

            if (IsValidGridPosition(x, y))
            {
                _tiles[x, y] = TileType.Mountain;

                // Add some hills around mountains (smaller for 5px tiles)
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        var nx = x + dx;
                        var ny = y + dy;
                        if (IsValidGridPosition(nx, ny) && _tiles[nx, ny] == TileType.Grass)
                        {
                            if (Math.Abs(dx) + Math.Abs(dy) == 1) // Only direct neighbors
                            {
                                _tiles[nx, ny] = TileType.Hills;
                            }
                        }
                    }
                }
            }
        }
    }

    private void CreateFarmlandRegion(int centerX, int centerY, int width, int height)
    {
        for (int x = centerX - width/2; x < centerX + width/2; x++)
        {
            for (int y = centerY - height/2; y < centerY + height/2; y++)
            {
                if (IsValidGridPosition(x, y) && _tiles[x, y] == TileType.Grass)
                {
                    _tiles[x, y] = TileType.Farmland;
                }
            }
        }
    }

    private void CreateDirtRegion(int centerX, int centerY, int width, int height)
    {
        for (int x = centerX - width/2; x < centerX + width/2; x++)
        {
            for (int y = centerY - height/2; y < centerY + height/2; y++)
            {
                if (IsValidGridPosition(x, y) && _tiles[x, y] == TileType.Grass)
                {
                    _tiles[x, y] = TileType.Dirt;
                }
            }
        }
    }

    private void CreateWaterBody(int centerX, int centerY, int radius)
    {
        for (int x = centerX - radius; x <= centerX + radius; x++)
        {
            for (int y = centerY - radius; y <= centerY + radius; y++)
            {
                if (IsValidGridPosition(x, y))
                {
                    var distance = Math.Sqrt((x - centerX) * (x - centerX) + (y - centerY) * (y - centerY));
                    if (distance <= radius)
                    {
                        _tiles[x, y] = TileType.Water;
                    }
                }
            }
        }
    }

    private void CreateForestPatch(int centerX, int centerY, int width, int height)
    {
        for (int x = centerX - width/2; x < centerX + width/2; x++)
        {
            for (int y = centerY - height/2; y < centerY + height/2; y++)
            {
                if (IsValidGridPosition(x, y) && _tiles[x, y] == TileType.Grass)
                {
                    _tiles[x, y] = TileType.Forest;
                }
            }
        }
    }

    /// <summary>
    /// Gets the tile type at the specified grid position.
    /// </summary>
    public TileType GetTileType(int gridX, int gridY)
    {
        if (!IsValidGridPosition(gridX, gridY))
            return TileType.Water; // Default for out-of-bounds
            
        return _tiles[gridX, gridY];
    }

    /// <summary>
    /// Sets the tile type at the specified grid position.
    /// </summary>
    public void SetTileType(int gridX, int gridY, TileType tileType)
    {
        if (!IsValidGridPosition(gridX, gridY))
            return;
            
        _tiles[gridX, gridY] = tileType;
    }

    /// <summary>
    /// Checks if a grid position is valid.
    /// </summary>
    public bool IsValidGridPosition(int gridX, int gridY)
    {
        return gridX >= 0 && gridX < _worldWidth && gridY >= 0 && gridY < _worldHeight;
    }

    /// <summary>
    /// Checks if a grid position is valid.
    /// </summary>
    public bool IsValidGridPosition(System.Numerics.Vector2 gridPosition)
    {
        return IsValidGridPosition((int)gridPosition.X, (int)gridPosition.Y);
    }

    /// <summary>
    /// Checks if a tile is buildable (not water or mountain).
    /// </summary>
    public bool IsTileBuildable(int gridX, int gridY)
    {
        if (!IsValidGridPosition(gridX, gridY))
            return false;
            
        var tileType = GetTileType(gridX, gridY);
        return tileType != TileType.Water && tileType != TileType.Mountain;
    }

    /// <summary>
    /// Checks if a tile is buildable.
    /// </summary>
    public bool IsTileBuildable(System.Numerics.Vector2 gridPosition)
    {
        return IsTileBuildable((int)gridPosition.X, (int)gridPosition.Y);
    }

    /// <summary>
    /// Converts world coordinates to grid coordinates.
    /// </summary>
    public System.Numerics.Vector2 WorldToGrid(System.Numerics.Vector2 worldPosition)
    {
        return new System.Numerics.Vector2(
            (float)Math.Floor(worldPosition.X / _tileSize),
            (float)Math.Floor(worldPosition.Y / _tileSize)
        );
    }

    /// <summary>
    /// Converts grid coordinates to world coordinates.
    /// </summary>
    public System.Numerics.Vector2 GridToWorld(System.Numerics.Vector2 gridPosition)
    {
        return new System.Numerics.Vector2(
            gridPosition.X * _tileSize,
            gridPosition.Y * _tileSize
        );
    }

    #region Event Handlers

    private void OnBuildingPlaced(Entities.Building building, System.Numerics.Vector2 position)
    {
        var gridPos = WorldToGrid(position);
        _buildingGrid[gridPos] = building.BuildingType;
        
        _errorHandler.LogInfo($"Building placed on tilemap: {building.BuildingType} at grid {gridPos}");
    }

    private void OnBuildingRemoved(Entities.Building building)
    {
        var gridPos = building.GridPosition;
        _buildingGrid.Remove(gridPos);
        
        _errorHandler.LogInfo($"Building removed from tilemap: {building.BuildingType} at grid {gridPos}");
    }

    #endregion

    public void Dispose()
    {
        // Unsubscribe from events
        _eventBus.BuildingPlaced -= OnBuildingPlaced;
        _eventBus.BuildingRemoved -= OnBuildingRemoved;
        
        // Dispose graphics resources
        _pixelTexture?.Dispose();
        
        _errorHandler.LogInfo("TilemapManager disposed");
    }
}
