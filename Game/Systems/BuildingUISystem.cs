using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FontStashSharp;
using TransportGame.Game.Managers;
using TransportGame.Game.Utils;
using System.IO;

namespace TransportGame.Game.Systems;

/// <summary>
/// Handles the building selection UI panel.
/// Shows available buildings that players can construct.
/// </summary>
public class BuildingUISystem : IDisposable
{
    private readonly ErrorHandler _errorHandler;
    private readonly BuildingDefinitionManager _buildingDefinitionManager;
    private readonly BuildingPlacementSystem _buildingPlacementSystem;

    // UI State
    private bool _showBuildingPanel = true; // Show by default
    private Vector2 _panelPosition = new Vector2(0, 0); // Will be calculated based on screen size
    private int _screenWidth = 1280;
    private int _screenHeight = 720;

    // Horizontal layout constants
    private const int BUTTON_WIDTH = 120;
    private const int BUTTON_HEIGHT = 80;
    private const int BUTTON_SPACING = 10;
    private const int PANEL_PADDING = 15;

    // Graphics
    private Texture2D? _pixelTexture;
    private FontSystem? _fontSystem;
    private DynamicSpriteFont? _font;

    // Buildable buildings (farms and mines are now map-generated, so we start with 1)
    private readonly string[] _buildableBuildings = { "steel_works", "food_factory", "train_depot", "station" };
    private readonly string[] _buildingKeys = { "1", "2", "3", "4" };
    private readonly string[] _buildingNames = { "Steel Works", "Food Factory", "Train Depot", "Station" };

    public bool ShowBuildingPanel => _showBuildingPanel;

    public BuildingUISystem(
        BuildingDefinitionManager buildingDefinitionManager,
        BuildingPlacementSystem buildingPlacementSystem)
    {
        _buildingDefinitionManager = buildingDefinitionManager ?? throw new ArgumentNullException(nameof(buildingDefinitionManager));
        _buildingPlacementSystem = buildingPlacementSystem ?? throw new ArgumentNullException(nameof(buildingPlacementSystem));
        _errorHandler = new ErrorHandler();

        _errorHandler.LogInfo("BuildingUISystem initialized");
    }

    /// <summary>
    /// Loads graphics resources.
    /// </summary>
    public void LoadContent(GraphicsDevice graphicsDevice)
    {
        try
        {
            // Initialize FontStashSharp
            InitializeFontSystem();

            // Create pixel texture for UI backgrounds
            _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });

            _errorHandler.LogInfo("BuildingUISystem content loaded");
        }
        catch (Exception ex)
        {
            _errorHandler.HandleError($"Failed to load BuildingUISystem content: {ex.Message}", "BuildingUISystem.LoadContent");
        }
    }

    private void InitializeFontSystem()
    {
        try
        {
            _fontSystem = new FontSystem();

            // Try to load Arial font from Windows
            var arialPath = @"C:\Windows\Fonts\arial.ttf";
            if (File.Exists(arialPath))
            {
                var fontData = File.ReadAllBytes(arialPath);
                _fontSystem.AddFont(fontData);
                _font = _fontSystem.GetFont(18); // Medium size for UI
                _errorHandler.LogInfo("BuildingUISystem: Arial font loaded successfully");
            }
            else
            {
                _errorHandler.LogInfo("BuildingUISystem: Arial font not found, using fallback");
            }
        }
        catch (Exception ex)
        {
            _errorHandler.HandleError($"FontStashSharp initialization failed: {ex.Message}", "BuildingUISystem.InitializeFontSystem");
        }
    }

    /// <summary>
    /// Toggles the building panel visibility.
    /// </summary>
    public void ToggleBuildingPanel()
    {
        _showBuildingPanel = !_showBuildingPanel;
        _errorHandler.LogInfo($"Building panel toggled: {_showBuildingPanel}");
    }

    /// <summary>
    /// Draws the building selection UI.
    /// </summary>
    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        if (!_showBuildingPanel || _pixelTexture == null || _font == null)
            return;

        try
        {
            DrawBuildingPanel(spriteBatch);
        }
        catch (Exception ex)
        {
            _errorHandler.HandleError($"Error drawing building UI: {ex.Message}", "BuildingUISystem.Draw");
        }
    }

    /// <summary>
    /// Draws the main building selection panel (horizontal layout at bottom center).
    /// </summary>
    private void DrawBuildingPanel(SpriteBatch spriteBatch)
    {
        // Calculate panel dimensions
        var totalButtonWidth = _buildableBuildings.Length * BUTTON_WIDTH;
        var totalSpacing = (_buildableBuildings.Length - 1) * BUTTON_SPACING;
        var panelWidth = totalButtonWidth + totalSpacing + (PANEL_PADDING * 2);
        var panelHeight = BUTTON_HEIGHT + (PANEL_PADDING * 2) + 30; // Extra space for title

        // Center horizontally, position at bottom
        var panelX = (_screenWidth - panelWidth) / 2;
        var panelY = _screenHeight - panelHeight - 20; // 20px from bottom
        var panelRect = new Rectangle(panelX, panelY, panelWidth, panelHeight);

        // Draw panel background with rounded appearance
        spriteBatch.Draw(_pixelTexture, panelRect, Color.Black * 0.85f);

        // Draw panel border
        DrawBorder(spriteBatch, panelRect, Color.Gray, 2);

        // Title centered above buttons
        var title = "Buildings";
        if (_font != null)
        {
            var titleSize = _font.MeasureString(title);
            var titleX = panelRect.X + (panelRect.Width - titleSize.X) / 2;
            var titleY = panelRect.Y + 8;
            _font.DrawText(spriteBatch, title, new Vector2(titleX, titleY), Color.White, scale: new Vector2(0.8f));
        }

        // Building buttons in horizontal row
        var buttonY = panelRect.Y + 30;
        var currentX = panelRect.X + PANEL_PADDING;

        for (int i = 0; i < _buildableBuildings.Length; i++)
        {
            var buttonRect = new Rectangle(currentX, buttonY, BUTTON_WIDTH, BUTTON_HEIGHT);
            DrawBuildingButton(spriteBatch, _buildableBuildings[i], _buildingKeys[i], _buildingNames[i], buttonRect);
            currentX += BUTTON_WIDTH + BUTTON_SPACING;
        }
    }

    /// <summary>
    /// Draws a single building button (compact horizontal layout).
    /// </summary>
    private void DrawBuildingButton(SpriteBatch spriteBatch, string buildingId, string key, string name, Rectangle buttonRect)
    {
        try
        {
            var definition = _buildingDefinitionManager.GetDefinition(buildingId);
            if (definition == null) return;

            // Check if this building is currently selected
            var isSelected = _buildingPlacementSystem.IsPlacementMode &&
                           _buildingPlacementSystem.SelectedBuildingId == buildingId;

            // Button background with hover effect
            var bgColor = isSelected ? Color.Orange * 0.8f : Color.DarkGray * 0.7f;
            spriteBatch.Draw(_pixelTexture, buttonRect, bgColor);

            // Button border
            var borderColor = isSelected ? Color.Orange : Color.Gray;
            DrawBorder(spriteBatch, buttonRect, borderColor, isSelected ? 3 : 1);

            // Key indicator (top-left corner)
            var keySize = 20;
            var keyRect = new Rectangle(buttonRect.X + 3, buttonRect.Y + 3, keySize, keySize);
            spriteBatch.Draw(_pixelTexture, keyRect, Color.Navy);
            if (_font != null)
            {
                _font.DrawText(spriteBatch, key, new Vector2(keyRect.X + 6, keyRect.Y + 3), Color.White, scale: new Vector2(0.6f));
            }

            // Building name (centered, top part)
            if (_font != null)
            {
                var nameSize = _font.MeasureString(name, scale: new Vector2(0.7f));
                var nameX = buttonRect.X + (buttonRect.Width - nameSize.X) / 2;
                var nameY = buttonRect.Y + 8;
                _font.DrawText(spriteBatch, name, new Vector2(nameX, nameY), Color.White, scale: new Vector2(0.7f));
            }

            // Building info (centered, bottom part)
            var infoText = $"{definition.Size.Width}x{definition.Size.Height}";
            var costText = $"${definition.Cost}";
            if (_font != null)
            {
                // Size info
                var infoSize = _font.MeasureString(infoText, scale: new Vector2(0.5f));
                var infoX = buttonRect.X + (buttonRect.Width - infoSize.X) / 2;
                var infoY = buttonRect.Y + 35;
                _font.DrawText(spriteBatch, infoText, new Vector2(infoX, infoY), Color.LightGray, scale: new Vector2(0.5f));

                // Cost info
                var costSize = _font.MeasureString(costText, scale: new Vector2(0.5f));
                var costX = buttonRect.X + (buttonRect.Width - costSize.X) / 2;
                var costY = buttonRect.Y + 50;
                _font.DrawText(spriteBatch, costText, new Vector2(costX, costY), Color.Yellow, scale: new Vector2(0.5f));
            }

            // Selection indicator (glowing border effect)
            if (isSelected)
            {
                // Draw additional glow border
                var glowRect = new Rectangle(buttonRect.X - 2, buttonRect.Y - 2, buttonRect.Width + 4, buttonRect.Height + 4);
                DrawBorder(spriteBatch, glowRect, Color.Yellow * 0.6f, 2);
            }
        }
        catch (Exception ex)
        {
            _errorHandler.HandleError($"Error drawing building button: {ex.Message}", "BuildingUISystem.DrawBuildingButton");
        }
    }

    /// <summary>
    /// Draws a border around a rectangle.
    /// </summary>
    private void DrawBorder(SpriteBatch spriteBatch, Rectangle rect, Color color, int thickness)
    {
        // Top
        spriteBatch.Draw(_pixelTexture, new Rectangle(rect.X, rect.Y, rect.Width, thickness), color);
        // Bottom
        spriteBatch.Draw(_pixelTexture, new Rectangle(rect.X, rect.Bottom - thickness, rect.Width, thickness), color);
        // Left
        spriteBatch.Draw(_pixelTexture, new Rectangle(rect.X, rect.Y, thickness, rect.Height), color);
        // Right
        spriteBatch.Draw(_pixelTexture, new Rectangle(rect.Right - thickness, rect.Y, thickness, rect.Height), color);
    }

    /// <summary>
    /// Handles mouse clicks on the building panel.
    /// </summary>
    public bool HandleMouseClick(Vector2 mousePosition)
    {
        if (!_showBuildingPanel) return false;

        try
        {
            // Calculate panel dimensions (same as in DrawBuildingPanel)
            var totalButtonWidth = _buildableBuildings.Length * BUTTON_WIDTH;
            var totalSpacing = (_buildableBuildings.Length - 1) * BUTTON_SPACING;
            var panelWidth = totalButtonWidth + totalSpacing + (PANEL_PADDING * 2);
            var panelHeight = BUTTON_HEIGHT + (PANEL_PADDING * 2) + 30;

            // Center horizontally, position at bottom
            var panelX = (_screenWidth - panelWidth) / 2;
            var panelY = _screenHeight - panelHeight - 20;
            var panelRect = new Rectangle(panelX, panelY, panelWidth, panelHeight);

            // Check if click is within panel
            if (!panelRect.Contains(mousePosition)) return false;

            // Check building buttons (horizontal layout)
            var buttonY = panelRect.Y + 30;
            var currentX = panelRect.X + PANEL_PADDING;

            for (int i = 0; i < _buildableBuildings.Length; i++)
            {
                var buttonRect = new Rectangle(currentX, buttonY, BUTTON_WIDTH, BUTTON_HEIGHT);
                if (buttonRect.Contains(mousePosition))
                {
                    // Start building placement
                    _buildingPlacementSystem.StartPlacement(_buildableBuildings[i]);
                    _errorHandler.LogInfo($"Building selected via UI: {_buildableBuildings[i]}");
                    return true;
                }
                currentX += BUTTON_WIDTH + BUTTON_SPACING;
            }

            return true; // Click was within panel, consume it
        }
        catch (Exception ex)
        {
            _errorHandler.HandleError($"Error handling mouse click: {ex.Message}", "BuildingUISystem.HandleMouseClick");
            return false;
        }
    }

    /// <summary>
    /// Handles key input for building UI.
    /// </summary>
    public void HandleKeyInput(string key)
    {
        switch (key)
        {
            case "B":
            case "b":
                ToggleBuildingPanel();
                break;
        }
    }

    /// <summary>
    /// Updates the screen size for proper panel positioning.
    /// </summary>
    public void UpdateScreenSize(int width, int height)
    {
        _screenWidth = width;
        _screenHeight = height;
        _errorHandler.LogInfo($"BuildingUISystem: Screen size updated to {width}x{height}");
    }

    public void Dispose()
    {
        _pixelTexture?.Dispose();
        _font = null;
        _fontSystem?.Dispose();
        _fontSystem = null;
        _errorHandler.LogInfo("BuildingUISystem disposed");
    }
}
