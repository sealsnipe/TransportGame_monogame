using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Numerics;
using TransportGame.Game.Constants;
using TransportGame.Game.Managers;
using TransportGame.Game.Utils;

namespace TransportGame.Game.Systems;

/// <summary>
/// Handles rendering of placed buildings and building placement preview.
/// Works with BuildingPlacementSystem to provide visual feedback.
/// </summary>
public class BuildingRenderSystem : IDisposable
{
    private readonly EventBus _eventBus;
    private readonly ErrorHandler _errorHandler;
    private readonly BuildingPlacementSystem _placementSystem;
    private readonly BuildingDefinitionManager _buildingDefinitionManager;
    private IndustryGenerationSystem? _industryGenerationSystem;

    // Graphics resources
    private Texture2D? _pixelTexture;
    private GraphicsDevice? _graphicsDevice;

    public BuildingRenderSystem(
        EventBus eventBus,
        BuildingPlacementSystem placementSystem,
        BuildingDefinitionManager buildingDefinitionManager)
    {
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _placementSystem = placementSystem ?? throw new ArgumentNullException(nameof(placementSystem));
        _buildingDefinitionManager = buildingDefinitionManager ?? throw new ArgumentNullException(nameof(buildingDefinitionManager));
        _errorHandler = new ErrorHandler();

        _errorHandler.LogInfo("BuildingRenderSystem initialized");
    }

    /// <summary>
    /// Sets the industry generation system for rendering natural industries.
    /// </summary>
    public void SetIndustryGenerationSystem(IndustryGenerationSystem industryGenerationSystem)
    {
        _industryGenerationSystem = industryGenerationSystem;
        _errorHandler.LogInfo("BuildingRenderSystem: IndustryGenerationSystem reference set");
    }

    /// <summary>
    /// Loads graphics resources.
    /// </summary>
    public void LoadContent(GraphicsDevice graphicsDevice)
    {
        try
        {
            _graphicsDevice = graphicsDevice;
            
            // Create a simple pixel texture for building rendering
            _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });

            _errorHandler.LogInfo("BuildingRenderSystem content loaded");
        }
        catch (Exception ex)
        {
            _errorHandler.HandleError($"Failed to load building render content: {ex.Message}", "BuildingRenderSystem.LoadContent");
        }
    }

    /// <summary>
    /// Draws all placed buildings and placement preview.
    /// </summary>
    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        if (_pixelTexture == null)
            return;

        try
        {
            // Draw all placed buildings (player-built)
            DrawPlacedBuildings(spriteBatch);

            // Draw natural industries (map-generated)
            DrawIndustries(spriteBatch);

            // Draw placement preview if in placement mode
            if (_placementSystem.IsPlacementMode)
            {
                Console.WriteLine($"[BUILDING-RENDER] Drawing placement preview for {_placementSystem.SelectedBuildingId} at {_placementSystem.PreviewPosition}");
                DrawPlacementPreview(spriteBatch);
            }
        }
        catch (Exception ex)
        {
            _errorHandler.HandleError($"Error drawing buildings: {ex.Message}", "BuildingRenderSystem.Draw");
        }
    }

    /// <summary>
    /// Draws all buildings that have been placed on the map.
    /// </summary>
    private void DrawPlacedBuildings(SpriteBatch spriteBatch)
    {
        var placedBuildings = _placementSystem.GetPlacedBuildings();

        foreach (var kvp in placedBuildings)
        {
            var building = kvp.Value;
            DrawBuilding(spriteBatch, building.GridPosition, building.Definition, building.Rotation, 1.0f);
        }
    }

    /// <summary>
    /// Draws all natural industries (farms, mines).
    /// </summary>
    private void DrawIndustries(SpriteBatch spriteBatch)
    {
        try
        {
            if (_industryGenerationSystem == null) return;

            var industries = _industryGenerationSystem.GeneratedIndustries;

            foreach (var industry in industries.Values)
            {
                // Draw industries with same method as player buildings
                DrawBuilding(spriteBatch, industry.GridPosition, industry.Definition, industry.Rotation, 1.0f);

                // Add industry indicator (small yellow dot in corner)
                var worldPos = new Microsoft.Xna.Framework.Vector2(
                    industry.GridPosition.X * GameConstants.TILE_SIZE,
                    industry.GridPosition.Y * GameConstants.TILE_SIZE
                );

                var indicatorRect = new Rectangle(
                    (int)worldPos.X + 2,
                    (int)worldPos.Y + 2,
                    4, 4
                );

                spriteBatch.Draw(_pixelTexture, indicatorRect, Color.Yellow);
            }
        }
        catch (Exception ex)
        {
            _errorHandler.HandleError($"Error drawing industries: {ex.Message}", "BuildingRenderSystem.DrawIndustries");
        }
    }

    /// <summary>
    /// Draws the building placement preview.
    /// </summary>
    private void DrawPlacementPreview(SpriteBatch spriteBatch)
    {
        if (_placementSystem.SelectedBuildingId == null)
            return;

        var definition = _buildingDefinitionManager.GetDefinition(_placementSystem.SelectedBuildingId);
        if (definition == null)
            return;

        // Draw preview with transparency and color coding
        var alpha = _placementSystem.IsValidPlacement ? 0.7f : 0.5f;
        var color = _placementSystem.IsValidPlacement ? Color.Green : Color.Red;

        DrawBuildingPreview(spriteBatch, _placementSystem.PreviewPosition, definition, 0, alpha, color);
    }

    /// <summary>
    /// Draws a single building at the specified position.
    /// </summary>
    private void DrawBuilding(SpriteBatch spriteBatch, System.Numerics.Vector2 gridPosition, Models.BuildingDefinition definition, int rotation, float alpha)
    {
        if (_pixelTexture == null)
            return;

        // Parse color from definition
        var buildingColor = ParseColor(definition.Visual.Color);
        
        // Get occupied tiles for this building
        var occupiedTiles = GetOccupiedTiles(gridPosition, definition, rotation);

        foreach (var tile in occupiedTiles)
        {
            var worldX = tile.X * GameConstants.TILE_SIZE;
            var worldY = tile.Y * GameConstants.TILE_SIZE;

            var rectangle = new Rectangle(
                (int)worldX,
                (int)worldY,
                GameConstants.TILE_SIZE,
                GameConstants.TILE_SIZE
            );

            // Draw building tile
            spriteBatch.Draw(_pixelTexture, rectangle, buildingColor * alpha);

            // Draw border for better visibility
            DrawBorder(spriteBatch, rectangle, Color.Black * (alpha * 0.8f));
        }

        // Draw building center marker (for identification)
        DrawCenterMarker(spriteBatch, gridPosition, definition, buildingColor * alpha);
    }

    /// <summary>
    /// Draws a building preview with custom color.
    /// </summary>
    private void DrawBuildingPreview(SpriteBatch spriteBatch, System.Numerics.Vector2 gridPosition, Models.BuildingDefinition definition, int rotation, float alpha, Color overrideColor)
    {
        if (_pixelTexture == null)
            return;

        var occupiedTiles = GetOccupiedTiles(gridPosition, definition, rotation);

        foreach (var tile in occupiedTiles)
        {
            var worldX = tile.X * GameConstants.TILE_SIZE;
            var worldY = tile.Y * GameConstants.TILE_SIZE;

            var rectangle = new Rectangle(
                (int)worldX,
                (int)worldY,
                GameConstants.TILE_SIZE,
                GameConstants.TILE_SIZE
            );

            // Draw preview tile
            spriteBatch.Draw(_pixelTexture, rectangle, overrideColor * alpha);

            // Draw border
            DrawBorder(spriteBatch, rectangle, Color.White * alpha);
        }
    }

    /// <summary>
    /// Draws a border around a rectangle.
    /// </summary>
    private void DrawBorder(SpriteBatch spriteBatch, Rectangle rectangle, Color color)
    {
        if (_pixelTexture == null)
            return;

        var thickness = 1;

        // Top
        spriteBatch.Draw(_pixelTexture, new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, thickness), color);
        // Bottom
        spriteBatch.Draw(_pixelTexture, new Rectangle(rectangle.X, rectangle.Bottom - thickness, rectangle.Width, thickness), color);
        // Left
        spriteBatch.Draw(_pixelTexture, new Rectangle(rectangle.X, rectangle.Y, thickness, rectangle.Height), color);
        // Right
        spriteBatch.Draw(_pixelTexture, new Rectangle(rectangle.Right - thickness, rectangle.Y, thickness, rectangle.Height), color);
    }

    /// <summary>
    /// Draws a center marker for building identification.
    /// </summary>
    private void DrawCenterMarker(SpriteBatch spriteBatch, System.Numerics.Vector2 gridPosition, Models.BuildingDefinition definition, Color color)
    {
        if (_pixelTexture == null)
            return;

        var centerX = gridPosition.X + (definition.Size.Width / 2f);
        var centerY = gridPosition.Y + (definition.Size.Height / 2f);

        var worldX = centerX * GameConstants.TILE_SIZE;
        var worldY = centerY * GameConstants.TILE_SIZE;

        var markerSize = Math.Max(2, GameConstants.TILE_SIZE / 3);
        var rectangle = new Rectangle(
            (int)(worldX - markerSize / 2),
            (int)(worldY - markerSize / 2),
            markerSize,
            markerSize
        );

        spriteBatch.Draw(_pixelTexture, rectangle, color);
    }

    /// <summary>
    /// Gets all tiles occupied by a building.
    /// </summary>
    private List<System.Numerics.Vector2> GetOccupiedTiles(System.Numerics.Vector2 gridPosition, Models.BuildingDefinition definition, int rotation)
    {
        var tiles = new List<System.Numerics.Vector2>();
        var size = definition.Size;

        // For now, only support rectangular buildings
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
                    tiles.Add(new System.Numerics.Vector2(gridPosition.X + x, gridPosition.Y + y));
                }
            }
        }

        return tiles;
    }

    /// <summary>
    /// Parses a color string (hex format) to XNA Color.
    /// </summary>
    private Color ParseColor(string colorString)
    {
        try
        {
            if (string.IsNullOrEmpty(colorString) || !colorString.StartsWith("#"))
                return Color.Brown; // Default building color

            var hex = colorString.Substring(1);
            if (hex.Length == 6)
            {
                var r = Convert.ToByte(hex.Substring(0, 2), 16);
                var g = Convert.ToByte(hex.Substring(2, 2), 16);
                var b = Convert.ToByte(hex.Substring(4, 2), 16);
                return new Color(r, g, b);
            }
        }
        catch (Exception ex)
        {
            _errorHandler.HandleWarning($"Failed to parse color '{colorString}': {ex.Message}", "BuildingRenderSystem.ParseColor");
        }

        return Color.Brown; // Fallback color
    }

    public void Dispose()
    {
        _pixelTexture?.Dispose();
        _errorHandler.LogInfo("BuildingRenderSystem disposed");
    }
}
