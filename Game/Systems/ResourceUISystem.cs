using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FontStashSharp;
using TransportGame.Game.Managers;
using TransportGame.Game.Utils;

namespace TransportGame.Game.Systems;

/// <summary>
/// Handles UI display for resource information and production statistics.
/// Shows resource storage, production rates, and building status.
/// </summary>
public class ResourceUISystem : IDisposable
{
    private readonly ErrorHandler _errorHandler;
    private readonly ProductionSystem _productionSystem;
    private readonly ResourceDefinitionManager _resourceDefinitionManager;
    private readonly BuildingPlacementSystem _buildingPlacementSystem;

    // UI State
    private bool _showResourcePanel = false;
    private string? _selectedBuildingId = null;
    private Vector2 _panelPosition = new Vector2(10, 100);
    private const int PANEL_WIDTH = 300;
    private const int PANEL_HEIGHT = 400;

    // Graphics
    private Texture2D? _pixelTexture;
    private SpriteFontBase? _font;

    public bool ShowResourcePanel => _showResourcePanel;

    public ResourceUISystem(
        ProductionSystem productionSystem,
        ResourceDefinitionManager resourceDefinitionManager,
        BuildingPlacementSystem buildingPlacementSystem)
    {
        _productionSystem = productionSystem ?? throw new ArgumentNullException(nameof(productionSystem));
        _resourceDefinitionManager = resourceDefinitionManager ?? throw new ArgumentNullException(nameof(resourceDefinitionManager));
        _buildingPlacementSystem = buildingPlacementSystem ?? throw new ArgumentNullException(nameof(buildingPlacementSystem));
        _errorHandler = new ErrorHandler();

        _errorHandler.LogInfo("ResourceUISystem initialized");
    }

    /// <summary>
    /// Loads graphics resources.
    /// </summary>
    public void LoadContent(GraphicsDevice graphicsDevice, SpriteFontBase font)
    {
        try
        {
            _font = font;
            
            // Create pixel texture for UI backgrounds
            _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });

            _errorHandler.LogInfo("ResourceUISystem content loaded");
        }
        catch (Exception ex)
        {
            _errorHandler.HandleError($"Failed to load ResourceUISystem content: {ex.Message}", "ResourceUISystem.LoadContent");
        }
    }

    /// <summary>
    /// Toggles the resource panel visibility.
    /// </summary>
    public void ToggleResourcePanel()
    {
        _showResourcePanel = !_showResourcePanel;
        _errorHandler.LogInfo($"Resource panel toggled: {_showResourcePanel}");
    }

    /// <summary>
    /// Shows resource information for a specific building.
    /// </summary>
    public void ShowBuildingResources(string buildingId)
    {
        _selectedBuildingId = buildingId;
        _showResourcePanel = true;
        _errorHandler.LogInfo($"Showing resources for building: {buildingId}");
    }

    /// <summary>
    /// Draws the resource UI.
    /// </summary>
    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        if (!_showResourcePanel || _pixelTexture == null || _font == null)
            return;

        try
        {
            if (!string.IsNullOrEmpty(_selectedBuildingId))
            {
                DrawBuildingResourcePanel(spriteBatch);
            }
            else
            {
                DrawGlobalResourcePanel(spriteBatch);
            }
        }
        catch (Exception ex)
        {
            _errorHandler.HandleError($"Error drawing resource UI: {ex.Message}", "ResourceUISystem.Draw");
        }
    }

    /// <summary>
    /// Draws resource information for a specific building.
    /// </summary>
    private void DrawBuildingResourcePanel(SpriteBatch spriteBatch)
    {
        if (string.IsNullOrEmpty(_selectedBuildingId)) return;

        var production = _productionSystem.GetBuildingProduction(_selectedBuildingId);
        if (production == null) return;

        var building = production.Building;
        var panelRect = new Rectangle((int)_panelPosition.X, (int)_panelPosition.Y, PANEL_WIDTH, PANEL_HEIGHT);

        // Draw panel background
        spriteBatch.Draw(_pixelTexture, panelRect, Color.Black * 0.8f);
        spriteBatch.Draw(_pixelTexture, new Rectangle(panelRect.X, panelRect.Y, panelRect.Width, 2), Color.White);
        spriteBatch.Draw(_pixelTexture, new Rectangle(panelRect.X, panelRect.Bottom - 2, panelRect.Width, 2), Color.White);
        spriteBatch.Draw(_pixelTexture, new Rectangle(panelRect.X, panelRect.Y, 2, panelRect.Height), Color.White);
        spriteBatch.Draw(_pixelTexture, new Rectangle(panelRect.Right - 2, panelRect.Y, 2, panelRect.Height), Color.White);

        var y = panelRect.Y + 10;
        var lineHeight = 20;

        // Title
        var title = $"{building.Definition.Name} Resources";
        spriteBatch.DrawString(_font, title, new Vector2(panelRect.X + 10, y), Color.White);
        y += lineHeight * 2;

        // Building status
        var status = building.IsOperational ? "Operational" : $"Construction: {building.ConstructionProgress:P0}";
        var statusColor = building.IsOperational ? Color.Green : Color.Yellow;
        spriteBatch.DrawString(_font, $"Status: {status}", new Vector2(panelRect.X + 10, y), statusColor);
        y += lineHeight;

        // Production info
        if (building.Definition.Production != null)
        {
            spriteBatch.DrawString(_font, $"Production Rate: {production.GetProductionRate():F1}/s", new Vector2(panelRect.X + 10, y), Color.White);
            y += lineHeight;
            spriteBatch.DrawString(_font, $"Efficiency: {production.GetEfficiency():P0}", new Vector2(panelRect.X + 10, y), Color.White);
            y += lineHeight;
            spriteBatch.DrawString(_font, $"Cycles: {production.TotalProductionCycles}", new Vector2(panelRect.X + 10, y), Color.White);
            y += lineHeight * 2;
        }

        // Input storage
        spriteBatch.DrawString(_font, "Input Storage:", new Vector2(panelRect.X + 10, y), Color.Cyan);
        y += lineHeight;
        DrawStorageInfo(spriteBatch, production.InputStorage, panelRect.X + 20, ref y, lineHeight);

        y += lineHeight;

        // Output storage
        spriteBatch.DrawString(_font, "Output Storage:", new Vector2(panelRect.X + 10, y), Color.Orange);
        y += lineHeight;
        DrawStorageInfo(spriteBatch, production.OutputStorage, panelRect.X + 20, ref y, lineHeight);

        // Close instruction
        spriteBatch.DrawString(_font, "Press R to close", new Vector2(panelRect.X + 10, panelRect.Bottom - 25), Color.Gray);
    }

    /// <summary>
    /// Draws global resource overview.
    /// </summary>
    private void DrawGlobalResourcePanel(SpriteBatch spriteBatch)
    {
        var panelRect = new Rectangle((int)_panelPosition.X, (int)_panelPosition.Y, PANEL_WIDTH, PANEL_HEIGHT);

        // Draw panel background
        spriteBatch.Draw(_pixelTexture, panelRect, Color.Black * 0.8f);
        spriteBatch.Draw(_pixelTexture, new Rectangle(panelRect.X, panelRect.Y, panelRect.Width, 2), Color.White);
        spriteBatch.Draw(_pixelTexture, new Rectangle(panelRect.X, panelRect.Bottom - 2, panelRect.Width, 2), Color.White);
        spriteBatch.Draw(_pixelTexture, new Rectangle(panelRect.X, panelRect.Y, 2, panelRect.Height), Color.White);
        spriteBatch.Draw(_pixelTexture, new Rectangle(panelRect.Right - 2, panelRect.Y, 2, panelRect.Height), Color.White);

        var y = panelRect.Y + 10;
        var lineHeight = 20;

        // Title
        spriteBatch.DrawString(_font, "Global Resources", new Vector2(panelRect.X + 10, y), Color.White);
        y += lineHeight * 2;

        // Building count
        var productions = _productionSystem.GetAllProductions();
        spriteBatch.DrawString(_font, $"Active Buildings: {productions.Count}", new Vector2(panelRect.X + 10, y), Color.White);
        y += lineHeight * 2;

        // Resource totals
        var resourceTotals = CalculateResourceTotals();
        spriteBatch.DrawString(_font, "Total Resources:", new Vector2(panelRect.X + 10, y), Color.Cyan);
        y += lineHeight;

        foreach (var kvp in resourceTotals)
        {
            var resourceDef = _resourceDefinitionManager.GetDefinition(kvp.Key);
            var displayName = resourceDef?.Name ?? kvp.Key;
            spriteBatch.DrawString(_font, $"  {displayName}: {kvp.Value}", new Vector2(panelRect.X + 20, y), Color.White);
            y += lineHeight;
        }

        // Close instruction
        spriteBatch.DrawString(_font, "Press R to close", new Vector2(panelRect.X + 10, panelRect.Bottom - 25), Color.Gray);
    }

    /// <summary>
    /// Draws storage information.
    /// </summary>
    private void DrawStorageInfo(SpriteBatch spriteBatch, Models.ResourceStorage storage, int x, ref int y, int lineHeight)
    {
        spriteBatch.DrawString(_font, $"Capacity: {storage.UsedCapacity}/{storage.MaxCapacity} ({storage.GetUtilization():P0})", 
            new Vector2(x, y), Color.White);
        y += lineHeight;

        foreach (var kvp in storage.Stacks)
        {
            var resourceDef = _resourceDefinitionManager.GetDefinition(kvp.Key);
            var displayName = resourceDef?.Name ?? kvp.Key;
            var color = ParseColor(resourceDef?.Visual.Color ?? "#FFFFFF");
            spriteBatch.DrawString(_font, $"  {displayName}: {kvp.Value.Amount}", new Vector2(x, y), color);
            y += lineHeight;
        }

        if (storage.Stacks.Count == 0)
        {
            spriteBatch.DrawString(_font, "  (Empty)", new Vector2(x, y), Color.Gray);
            y += lineHeight;
        }
    }

    /// <summary>
    /// Calculates total resources across all buildings.
    /// </summary>
    private Dictionary<string, int> CalculateResourceTotals()
    {
        var totals = new Dictionary<string, int>();
        var productions = _productionSystem.GetAllProductions();

        foreach (var production in productions.Values)
        {
            // Add input storage
            foreach (var stack in production.InputStorage.Stacks)
            {
                totals[stack.Key] = totals.GetValueOrDefault(stack.Key, 0) + stack.Value.Amount;
            }

            // Add output storage
            foreach (var stack in production.OutputStorage.Stacks)
            {
                totals[stack.Key] = totals.GetValueOrDefault(stack.Key, 0) + stack.Value.Amount;
            }
        }

        return totals;
    }

    /// <summary>
    /// Parses a color string to XNA Color.
    /// </summary>
    private Color ParseColor(string colorString)
    {
        try
        {
            if (string.IsNullOrEmpty(colorString) || !colorString.StartsWith("#"))
                return Color.White;

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
            _errorHandler.HandleWarning($"Failed to parse color '{colorString}': {ex.Message}", "ResourceUISystem.ParseColor");
        }

        return Color.White;
    }

    /// <summary>
    /// Handles key input for resource UI.
    /// </summary>
    public void HandleKeyInput(string key)
    {
        switch (key)
        {
            case "R":
            case "r":
                if (_showResourcePanel)
                {
                    _showResourcePanel = false;
                    _selectedBuildingId = null;
                }
                break;
            case "resource_panel":
                ToggleResourcePanel();
                break;
        }
    }

    public void Dispose()
    {
        _pixelTexture?.Dispose();
        _errorHandler.LogInfo("ResourceUISystem disposed");
    }
}
