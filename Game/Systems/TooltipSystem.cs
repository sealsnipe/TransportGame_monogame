using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using TransportGame.Game.Managers;
using TransportGame.Game.Utils;
using TransportGame.Game.Constants;
using FontStashSharp;

namespace TransportGame.Game.Systems
{
    /// <summary>
    /// Clean TooltipSystem without debug spam - handles tooltip display for tiles.
    /// </summary>
    public class TooltipSystem : IDisposable
    {
        // Constants
        private const int TOOLTIP_WIDTH = 300;
        private const int TOOLTIP_MIN_HEIGHT = 80;
        private const int TOOLTIP_PADDING = 10;
        private const int TOOLTIP_MARGIN_RIGHT = 20;
        private const int TOOLTIP_MARGIN_BOTTOM = 20;
        private const float LOG_INTERVAL = 5.0f; // Log every 5 seconds

        // Core dependencies
        private readonly EventBus _eventBus;
        private readonly ErrorHandler _errorHandler;

        // JSON data managers
        private TileDefinitionManager? _tileDefinitionManager;
        private LocalizationManager? _localizationManager;
        private SettingsManager? _settingsManager;

        // FontStashSharp
        private FontSystem? _fontSystem;
        private DynamicSpriteFont? _font;

        // State
        private bool _isInitialized = false;
        private bool _isTooltipVisible = false;
        private string _tooltipText = "";
        private System.Numerics.Vector2 _tooltipPosition;

        // Current tile info
        private int _currentGridX = -1;
        private int _currentGridY = -1;
        private TileType _currentTileType = TileType.Grass;

        // Screen dimensions for positioning - will be updated dynamically
        private int _screenWidth = 1280;
        private int _screenHeight = 720;

        // Debug timer
        private float _logTimer = 0f;

        // Fallback German tile names
        private readonly Dictionary<TileType, string> _germanTileNames = new()
        {
            { TileType.Grass, "Grasland" },
            { TileType.Forest, "Wald" },
            { TileType.Mountain, "Berg" },
            { TileType.Water, "Wasser" },
            { TileType.Desert, "Wüste" },
            { TileType.Farmland, "Ackerland" },
            { TileType.Hills, "Hügel" },
            { TileType.DeepWater, "Tiefes Wasser" },
            { TileType.Road, "Straße" },
            { TileType.Rail, "Eisenbahn" },
            { TileType.Dirt, "Erde" },
            { TileType.Beach, "Strand" }
        };

        public TooltipSystem(EventBus eventBus, ErrorHandler errorHandler)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));

            // Initialize FontStashSharp
            InitializeFontSystem();

            // Subscribe to events
            _eventBus.MouseClicked += OnMouseClicked;
            _eventBus.KeyPressed += OnKeyPressed;

            // Calculate fixed tooltip position (bottom right)
            CalculateFixedTooltipPosition();

            _isInitialized = true;
            _errorHandler.LogInfo("TooltipSystem initialized (clean version)");
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
                    _font = _fontSystem.GetFont(24); // Larger base size to avoid scaling issues
                    _errorHandler.LogInfo("FontStashSharp: Arial font loaded successfully");
                }
                else
                {
                    _errorHandler.LogInfo("FontStashSharp: Arial font not found, using fallback");
                }
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError($"FontStashSharp initialization failed: {ex.Message}", "TooltipSystem.InitializeFontSystem");
            }
        }

        public void SetDataManagers(TileDefinitionManager tileDefinitionManager, LocalizationManager localizationManager)
        {
            _tileDefinitionManager = tileDefinitionManager;
            _localizationManager = localizationManager;
            _errorHandler.LogInfo("TooltipSystem: JSON data managers connected");
        }

        public void SetSettingsManager(SettingsManager settingsManager)
        {
            _settingsManager = settingsManager;
            _errorHandler.LogInfo("TooltipSystem: SettingsManager connected");
        }

        public void UpdateScreenSize(int width, int height)
        {
            _screenWidth = width;
            _screenHeight = height;
            Console.WriteLine($"[TOOLTIP-DRAW] Screen size updated to {width}x{height}");
        }

        public void Update(GameTime gameTime)
        {
            if (!_isInitialized) return;

            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _logTimer += deltaTime;

            // Reduced logging frequency - only log tooltip changes, not periodic status
            // if (_logTimer >= LOG_INTERVAL && _isTooltipVisible)
            // {
            //     _errorHandler.LogInfo($"Tooltip visible: {_tooltipText.Substring(0, Math.Min(50, _tooltipText.Length))}...");
            //     _logTimer = 0f;
            // }
        }

        private void CalculateFixedTooltipPosition()
        {
            _tooltipPosition = new System.Numerics.Vector2(
                _screenWidth - TOOLTIP_WIDTH - TOOLTIP_MARGIN_RIGHT,
                _screenHeight - TOOLTIP_MIN_HEIGHT - TOOLTIP_MARGIN_BOTTOM
            );
        }

        private void OnMouseClicked(System.Numerics.Vector2 screenPosition)
        {
            // This will be handled by MouseInteractionSystem calling OnTileClicked
        }

        public void OnTileClicked(System.Numerics.Vector2 screenPosition, int gridX, int gridY, TileType tileType)
        {
            try
            {
                // Update tooltip state
                _currentGridX = gridX;
                _currentGridY = gridY;
                _currentTileType = tileType;

                // Create tooltip text
                _tooltipText = CreateTooltipText(tileType, gridX, gridY);

                // Show tooltip at fixed position
                _isTooltipVisible = true;

                _errorHandler.LogInfo($"Tooltip shown for {tileType} at ({gridX},{gridY})");
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError($"Error in OnTileClicked: {ex.Message}", "TooltipSystem.OnTileClicked");
            }
        }

        /// <summary>
        /// Handles building click events and displays building tooltips.
        /// </summary>
        public void OnBuildingClicked(System.Numerics.Vector2 screenPosition, int gridX, int gridY, PlacedBuilding building)
        {
            try
            {
                // Update tooltip state
                _currentGridX = gridX;
                _currentGridY = gridY;
                _currentTileType = TileType.Grass; // Default for buildings

                // Create building tooltip text
                _tooltipText = CreateBuildingTooltipText(building, gridX, gridY);

                // Show tooltip at fixed position
                _isTooltipVisible = true;

                _errorHandler.LogInfo($"Building tooltip shown for {building.BuildingId} at ({gridX},{gridY})");
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError($"Error in OnBuildingClicked: {ex.Message}", "TooltipSystem.OnBuildingClicked");
            }
        }

        private string CreateTooltipText(TileType tileType, int gridX, int gridY)
        {
            try
            {
                // Get localized name and description
                string tileName = tileType.ToString();
                string tileDescription = "";

                if (_localizationManager != null && _localizationManager.IsInitialized)
                {
                    tileName = _localizationManager.GetTileName(tileType);
                    tileDescription = _localizationManager.GetTileDescription(tileType);
                }
                else
                {
                    // Fallback to German names
                    tileName = _germanTileNames.TryGetValue(tileType, out var name) ? name : tileType.ToString();
                }

                // Get properties from JSON
                string propertiesText = "";
                if (_tileDefinitionManager != null && _tileDefinitionManager.HasDefinition(tileType))
                {
                    var definition = _tileDefinitionManager.GetDefinition(tileType);
                    if (definition?.Properties != null)
                    {
                        propertiesText = CreatePropertiesText(definition.Properties);
                    }
                }

                // Combine information
                var result = $"{tileName} ({gridX},{gridY})";
                if (!string.IsNullOrEmpty(tileDescription))
                {
                    result += $"\n{tileDescription}";
                }
                if (!string.IsNullOrEmpty(propertiesText))
                {
                    result += $"\n{propertiesText}";
                }

                return result;
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError($"Error creating tooltip: {ex.Message}", "TooltipSystem.CreateTooltipText");
                var fallbackName = _germanTileNames.TryGetValue(tileType, out var name) ? name : tileType.ToString();
                return $"{fallbackName} ({gridX},{gridY})";
            }
        }

        /// <summary>
        /// Creates tooltip text for buildings with production and resource information.
        /// </summary>
        private string CreateBuildingTooltipText(PlacedBuilding building, int gridX, int gridY)
        {
            try
            {
                var lines = new List<string>();

                // Building name and position
                lines.Add($"{building.Definition.Name} ({gridX},{gridY})");

                // Building description
                if (!string.IsNullOrEmpty(building.Definition.Description))
                {
                    lines.Add(building.Definition.Description);
                }

                // Building status
                var status = building.IsOperational ? "Operational" : $"Construction: {building.ConstructionProgress:P0}";
                var statusIcon = building.IsOperational ? "✓" : "🔨";
                lines.Add($"{statusIcon} Status: {status}");

                // Building size and cost
                lines.Add($"📏 Size: {building.Definition.Size.Width}x{building.Definition.Size.Height}");
                lines.Add($"💰 Cost: {building.Definition.Cost}");

                // Production information
                if (building.Definition.Production != null)
                {
                    var production = building.Definition.Production;
                    lines.Add("");
                    lines.Add("🏭 Production:");
                    lines.Add($"  Rate: {production.ProductionRate:F1}/s");
                    lines.Add($"  Efficiency: {production.Efficiency:P0}");

                    // Input resources
                    if (production.InputResources.Any())
                    {
                        lines.Add("  Inputs:");
                        foreach (var input in production.InputResources)
                        {
                            lines.Add($"    • {input.ResourceType}: {input.Amount}");
                        }
                    }

                    // Output resources
                    if (production.OutputResources.Any())
                    {
                        lines.Add("  Outputs:");
                        foreach (var output in production.OutputResources)
                        {
                            lines.Add($"    • {output.ResourceType}: {output.Amount}");
                        }
                    }
                }

                // Storage information
                if (building.Definition.Storage != null)
                {
                    var storage = building.Definition.Storage;
                    lines.Add("");
                    lines.Add("📦 Storage:");
                    lines.Add($"  Input: {storage.InputCapacity}");
                    lines.Add($"  Output: {storage.OutputCapacity}");
                }

                // Placement requirements
                if (building.Definition.PlacementRules?.AllowedTerrain?.Any() == true)
                {
                    lines.Add("");
                    lines.Add("🌍 Terrain Requirements:");
                    foreach (var terrain in building.Definition.PlacementRules.AllowedTerrain)
                    {
                        lines.Add($"  • {terrain}");
                    }
                }

                return string.Join("\n", lines);
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError($"Error creating building tooltip: {ex.Message}", "TooltipSystem.CreateBuildingTooltipText");
                return $"{building.Definition.Name} ({gridX},{gridY})\nError loading details";
            }
        }

        private string CreatePropertiesText(TransportGame.Game.Managers.TileProperties properties)
        {
            var lines = new List<string>();

            if (properties.Buildable)
                lines.Add("• Bebaubar");

            if (properties.MovementCost != 1.0f)
                lines.Add($"• Bewegungskosten: {properties.MovementCost:F1}");

            if (properties.Fertility > 0)
                lines.Add($"• Fruchtbarkeit: {properties.Fertility:F1}");

            if (!properties.Passable)
                lines.Add("• Nicht passierbar");

            return string.Join("\n", lines);
        }

        private void OnKeyPressed(string key)
        {
            try
            {
                if (key == "Escape" && _isTooltipVisible)
                {
                    HideTooltip();
                    _errorHandler.LogInfo("Tooltip hidden by ESC key");
                }
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError($"Error in OnKeyPressed: {ex.Message}", "TooltipSystem.OnKeyPressed");
            }
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D pixelTexture, SpriteFont? font = null)
        {
            if (!_isTooltipVisible || string.IsNullOrEmpty(_tooltipText) || pixelTexture == null)
            {
                return;
            }

            try
            {
                // Calculate dynamic height based on FontStashSharp or fallback
                var tooltipWidth = TOOLTIP_WIDTH;
                var lineCount = _tooltipText.Split('\n').Length;
                var scale = GetTooltipScale();

                int lineHeight;
                if (_font != null)
                {
                    lineHeight = (int)(_font.LineHeight * scale);
                }
                else
                {
                    lineHeight = (int)(18 * scale); // Fallback line height
                }

                var tooltipHeight = Math.Max(TOOLTIP_MIN_HEIGHT, lineCount * lineHeight + TOOLTIP_PADDING * 2);

                // Create background rectangle
                var backgroundRect = new Rectangle(
                    (int)_tooltipPosition.X,
                    (int)_tooltipPosition.Y,
                    tooltipWidth,
                    tooltipHeight
                );
                
                // Draw background
                spriteBatch.Draw(pixelTexture, backgroundRect, Color.Black * 0.8f);
                
                // Draw border
                var borderThickness = 1;
                spriteBatch.Draw(pixelTexture, new Rectangle(backgroundRect.X, backgroundRect.Y, backgroundRect.Width, borderThickness), Color.White);
                spriteBatch.Draw(pixelTexture, new Rectangle(backgroundRect.X, backgroundRect.Y + backgroundRect.Height - borderThickness, backgroundRect.Width, borderThickness), Color.White);
                spriteBatch.Draw(pixelTexture, new Rectangle(backgroundRect.X, backgroundRect.Y, borderThickness, backgroundRect.Height), Color.White);
                spriteBatch.Draw(pixelTexture, new Rectangle(backgroundRect.X + backgroundRect.Width - borderThickness, backgroundRect.Y, borderThickness, backgroundRect.Height), Color.White);
                
                // Draw text using FontStashSharp or fallback
                if (_font != null)
                {
                    DrawFontStashText(spriteBatch, backgroundRect);
                }
                else
                {
                    DrawFallbackText(spriteBatch, pixelTexture, backgroundRect);
                }
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError($"Error in Draw: {ex.Message}", "TooltipSystem.Draw");
            }
        }

        private void DrawFontStashText(SpriteBatch spriteBatch, Rectangle backgroundRect)
        {
            if (_font == null) return;

            var lines = _tooltipText.Split('\n');
            var currentY = backgroundRect.Y + TOOLTIP_PADDING;
            var scale = GetTooltipScale() * 0.8f; // Apply settings scale with 20% reduction for better fit

            for (int lineIndex = 0; lineIndex < lines.Length && lineIndex < 10; lineIndex++)
            {
                var line = lines[lineIndex].Trim();
                if (string.IsNullOrEmpty(line))
                {
                    currentY += (int)(_font.LineHeight * scale * 0.5f);
                    continue;
                }

                var currentX = backgroundRect.X + TOOLTIP_PADDING;
                var textColor = lineIndex == 0 ? Color.Yellow :
                               line.StartsWith("•") ? Color.LightGray : Color.White;

                // Draw text with FontStashSharp - fix diagonal orientation issue
                var position = new Vector2(currentX, currentY);

                // Try the simplest approach first - maybe the issue is elsewhere
                _font.DrawText(spriteBatch, line, position, textColor,
                    rotation: 0f,                    // Explicitly no rotation
                    origin: Vector2.Zero,            // Top-left origin
                    scale: new Vector2(scale, scale), // Vector2 scale
                    layerDepth: 0f);

                currentY += (int)(_font.LineHeight * scale);
            }
        }

        private void DrawFallbackText(SpriteBatch spriteBatch, Texture2D pixelTexture, Rectangle backgroundRect)
        {
            var lines = _tooltipText.Split('\n');
            var currentY = backgroundRect.Y + TOOLTIP_PADDING;
            var scale = GetTooltipScale();
            var charWidth = (int)(6 * scale);
            var lineHeight = (int)(12 * scale);

            for (int lineIndex = 0; lineIndex < lines.Length && lineIndex < 10; lineIndex++)
            {
                var line = lines[lineIndex].Trim();
                if (string.IsNullOrEmpty(line))
                {
                    currentY += lineHeight / 2;
                    continue;
                }

                var currentX = backgroundRect.X + TOOLTIP_PADDING;
                var textColor = lineIndex == 0 ? Color.Yellow : 
                               line.StartsWith("•") ? Color.LightGray : Color.White;

                // Draw simplified text (just rectangles for now)
                for (int charIndex = 0; charIndex < line.Length && charIndex < 40; charIndex++)
                {
                    if (line[charIndex] != ' ')
                    {
                        var charRect = new Rectangle(currentX, currentY, (int)(4 * scale), (int)(6 * scale));
                        spriteBatch.Draw(pixelTexture, charRect, textColor);
                    }
                    currentX += charWidth;
                }

                currentY += lineHeight;
            }
        }

        /// <summary>
        /// Gets the tooltip scale from settings, with fallback to default.
        /// </summary>
        private float GetTooltipScale()
        {
            if (_settingsManager?.IsInitialized == true)
            {
                return _settingsManager.GetControlSettings().TooltipScale;
            }

            // Fallback to default if settings not available
            return 1.5f;
        }

        public void HideTooltip()
        {
            _isTooltipVisible = false;
            _tooltipText = "";
        }

        public bool IsTooltipVisible => _isTooltipVisible;

        public void Dispose()
        {
            if (_eventBus != null)
            {
                _eventBus.MouseClicked -= OnMouseClicked;
                _eventBus.KeyPressed -= OnKeyPressed;
            }

            // Dispose FontStashSharp resources
            _font = null;
            _fontSystem?.Dispose();
            _fontSystem = null;

            _isInitialized = false;
            _errorHandler.LogInfo("TooltipSystem disposed");
        }
    }
}
