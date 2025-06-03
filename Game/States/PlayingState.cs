using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using TransportGame.Game.Managers;
using TransportGame.Game.Systems;
using FontStashSharp;
using FontStashSharp.RichText;

namespace TransportGame.Game.States
{
    /// <summary>
    /// Main gameplay state - handles normal game input and rendering
    /// </summary>
    public class PlayingState : IGameState
    {
        private readonly ErrorHandler _errorHandler;
        private readonly RenderSystem _renderSystem;
        private readonly InputSystem _inputSystem;
        private readonly TooltipSystem _tooltipSystem;
        private readonly MouseInteractionSystem _mouseInteractionSystem;
        private readonly StateManager _stateManager;
        private readonly GraphicsDevice _graphicsDevice;
        private readonly CameraSystem _cameraSystem;
        private readonly SettingsManager _settingsManager;

        private bool _isInitialized;
        private MenuState? _menuState;

        // Key debouncing for menu keys
        private float _menuKeyDebounceTimer = 0f;
        private const float MENU_KEY_DEBOUNCE_DELAY = 0.2f; // 200ms debounce

        // HUD visibility and debug settings
        private bool _showHUD = true;
        private bool _showDebugPanel = false;
        private float _debugKeyDebounceTimer = 0f;
        private float _debugPanelKeyDebounceTimer = 0f;
        private float _debugClickDebounceTimer = 0f;
        private const float DEBUG_KEY_DEBOUNCE_DELAY = 0.3f;
        private const float DEBUG_CLICK_DEBOUNCE_DELAY = 0.2f;

        // Debug panel tree state
        private Dictionary<string, bool> _debugTreeExpanded = new Dictionary<string, bool>
        {
            { "Performance", false },
            { "GameWorld", false },
            { "GameState", false },
            { "Transport", false }
        };

        // HUD rendering
        private FontSystem? _fontSystem;
        private DynamicSpriteFont? _font;
        private Texture2D? _pixelTexture;

        public bool IsActive { get; set; }
        public bool UpdateBelow => false; // Don't update when menu is on top
        public bool DrawBelow => true;    // Allow drawing when menu is on top (for transparency)

        public PlayingState(
            ErrorHandler errorHandler,
            RenderSystem renderSystem,
            InputSystem inputSystem,
            TooltipSystem tooltipSystem,
            MouseInteractionSystem mouseInteractionSystem,
            StateManager stateManager,
            GraphicsDevice graphicsDevice,
            CameraSystem cameraSystem,
            SettingsManager settingsManager)
        {
            _errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
            _renderSystem = renderSystem ?? throw new ArgumentNullException(nameof(renderSystem));
            _inputSystem = inputSystem ?? throw new ArgumentNullException(nameof(inputSystem));
            _tooltipSystem = tooltipSystem ?? throw new ArgumentNullException(nameof(tooltipSystem));
            _mouseInteractionSystem = mouseInteractionSystem ?? throw new ArgumentNullException(nameof(mouseInteractionSystem));
            _stateManager = stateManager ?? throw new ArgumentNullException(nameof(stateManager));
            _graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
            _cameraSystem = cameraSystem ?? throw new ArgumentNullException(nameof(cameraSystem));
            _settingsManager = settingsManager ?? throw new ArgumentNullException(nameof(settingsManager));
        }

        public void SetMenuState(MenuState menuState)
        {
            _menuState = menuState;
        }

        public void Initialize()
        {
            try
            {
                // Load font for HUD
                LoadFont();

                // Create pixel texture for HUD rendering (like other states)
                _pixelTexture = new Texture2D(_graphicsDevice, 1, 1);
                _pixelTexture.SetData(new[] { Color.White });

                // Load UI settings from SettingsManager
                LoadUISettings();

                _isInitialized = true;
                _errorHandler.LogInfo("PlayingState initialized");
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError($"Error initializing PlayingState: {ex.Message}", "PlayingState.Initialize");
            }
        }

        private void LoadUISettings()
        {
            try
            {
                // Load UI settings from SettingsManager
                var settings = _settingsManager.GetSettings();

                // Load HUD visibility (default: true)
                _showHUD = settings.UI.ShowHUD;

                // Load Debug Panel visibility (default: false)
                _showDebugPanel = settings.UI.ShowDebugPanel;

                _errorHandler.LogInfo($"UI Settings loaded - HUD: {_showHUD}, Debug: {_showDebugPanel}");
            }
            catch (Exception ex)
            {
                _errorHandler.HandleWarning($"Could not load UI settings: {ex.Message}", "PlayingState.LoadUISettings");
                // Use defaults
                _showHUD = true;
                _showDebugPanel = false;
            }
        }

        private void SaveUISettings()
        {
            try
            {
                // Get current settings and update UI section
                var settings = _settingsManager.GetSettings();
                settings.UI.ShowHUD = _showHUD;
                settings.UI.ShowDebugPanel = _showDebugPanel;

                // Save updated settings
                _settingsManager.UpdateSettings(settings);

                _errorHandler.LogInfo($"UI Settings saved - HUD: {_showHUD}, Debug: {_showDebugPanel}");
            }
            catch (Exception ex)
            {
                _errorHandler.HandleWarning($"Could not save UI settings: {ex.Message}", "PlayingState.SaveUISettings");
            }
        }

        private void LoadFont()
        {
            try
            {
                _fontSystem = new FontSystem();

                // Load Arial font from Windows (same as other states)
                var arialPath = @"C:\Windows\Fonts\arial.ttf";
                if (File.Exists(arialPath))
                {
                    var fontData = File.ReadAllBytes(arialPath);
                    _fontSystem.AddFont(fontData);
                    _font = _fontSystem.GetFont(20); // Small size for HUD
                    _errorHandler.LogInfo("PlayingState: Arial font loaded successfully");
                }
                else
                {
                    _errorHandler.LogInfo("PlayingState: Arial font not found, using fallback");
                }
            }
            catch (Exception ex)
            {
                _errorHandler.HandleWarning($"Could not load font in PlayingState: {ex.Message}", "PlayingState.LoadFont");
                _font = null;
            }
        }

        public void Update(GameTime gameTime)
        {
            try
            {
                if (!_isInitialized || !IsActive) return;

                // Update debounce timers
                _menuKeyDebounceTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                _debugKeyDebounceTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                _debugPanelKeyDebounceTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                _debugClickDebounceTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;

                // Handle F9 UI toggle (HUD visibility only - F12 is for screenshots)
                if (_debugKeyDebounceTimer <= 0f && _inputSystem.IsKeyPressed(Keys.F9))
                {
                    _showHUD = !_showHUD;
                    SaveUISettings(); // Persist setting

                    _errorHandler.LogInfo($"F9 pressed - UI: {(_showHUD ? "ON" : "OFF")}");
                    _debugKeyDebounceTimer = DEBUG_KEY_DEBOUNCE_DELAY;
                }

                // Handle F11 Debug Panel toggle
                if (_debugPanelKeyDebounceTimer <= 0f && _inputSystem.IsKeyPressed(Keys.F11))
                {
                    _showDebugPanel = !_showDebugPanel;
                    SaveUISettings(); // Persist setting

                    _errorHandler.LogInfo($"F11 pressed - Debug Panel: {(_showDebugPanel ? "ON" : "OFF")}");
                    _debugPanelKeyDebounceTimer = DEBUG_KEY_DEBOUNCE_DELAY;
                }

                // Handle debug panel clicks (only when panel is visible)
                if (_showDebugPanel)
                {
                    HandleDebugPanelClicks();
                }

                // Handle menu keys (F10, M, TAB) for main menu - with debouncing
                if (_menuKeyDebounceTimer <= 0f)
                {
                    var keyboardState = Keyboard.GetState();
                    if (_inputSystem.IsKeyPressed(Keys.F10) ||
                        _inputSystem.IsKeyPressed(Keys.M) ||
                        _inputSystem.IsKeyPressed(Keys.Tab))
                    {
                        // Open main menu (use pre-configured MenuState)
                        if (_menuState == null)
                        {
                            _errorHandler.HandleError("MenuState not set in PlayingState", "PlayingState.Update");
                            return;
                        }

                        var keyPressed = _inputSystem.IsKeyPressed(Keys.F10) ? "F10" :
                                       _inputSystem.IsKeyPressed(Keys.M) ? "M" : "TAB";
                        _errorHandler.LogInfo($"{keyPressed} pressed - opening main menu");
                        _stateManager.PushState(_menuState);

                        // Set debounce timer to prevent immediate re-triggering
                        _menuKeyDebounceTimer = MENU_KEY_DEBOUNCE_DELAY;
                        return;
                    }
                    // Handle direct Options shortcuts (F1, F2, F3)
                    else if (_inputSystem.IsKeyPressed(Keys.F1) ||
                             _inputSystem.IsKeyPressed(Keys.F2) ||
                             _inputSystem.IsKeyPressed(Keys.F3))
                    {
                        // Open Options menu directly to specific section
                        if (_menuState == null)
                        {
                            _errorHandler.HandleError("MenuState not set in PlayingState", "PlayingState.OpenOptionsDirectly");
                            return;
                        }

                        var keyPressed = _inputSystem.IsKeyPressed(Keys.F1) ? "F1" :
                                       _inputSystem.IsKeyPressed(Keys.F2) ? "F2" : "F3";
                        var section = _inputSystem.IsKeyPressed(Keys.F1) ? "Display" :
                                    _inputSystem.IsKeyPressed(Keys.F2) ? "Audio" : "Controls";

                        _errorHandler.LogInfo($"{keyPressed} pressed - opening Options menu ({section})");

                        // Push MenuState first, then OptionsState with specific section
                        _stateManager.PushState(_menuState);
                        _menuState.OpenOptionsWithSection(section);

                        // Set debounce timer to prevent immediate re-triggering
                        _menuKeyDebounceTimer = MENU_KEY_DEBOUNCE_DELAY;
                        return;
                    }
                }

                // Update game systems only when playing state is active
                _inputSystem.Update(gameTime);
                _tooltipSystem.Update(gameTime);
                // Note: MouseInteractionSystem needs InputSystem and CameraSystem parameters
                // so it's still updated in TransportGameMain
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError($"Error in PlayingState Update: {ex.Message}", "PlayingState.Update");
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            try
            {
                if (!_isInitialized) return;

                // Draw tooltips when in playing state
                var pixelTexture = _renderSystem.GetPixelTexture();
                if (pixelTexture != null)
                {
                    _tooltipSystem.Draw(spriteBatch, pixelTexture, null);
                }

                // Draw key hints HUD (top-left) - only if enabled
                if (_showHUD)
                {
                    DrawKeyHintsHUD(spriteBatch);
                }

                // Draw debug panel (top-right) - only if enabled
                if (_showDebugPanel)
                {
                    DrawDebugPanel(spriteBatch);
                }
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError($"Error in PlayingState Draw: {ex.Message}", "PlayingState.Draw");
            }
        }

        private void DrawKeyHintsHUD(SpriteBatch spriteBatch)
        {
            try
            {
                // Check if resources are available
                if (_font == null || _pixelTexture == null) return;

                // Position in top-left corner as requested
                var startX = 20; // Left margin
                var startY = 20; // Top margin
                var currentY = startY;
                var lineHeight = 35;

                // Key hints data with dynamic status
                var gridStatus = _renderSystem?.IsGridVisible() == true ? "ON" : "OFF";
                var keyHints = new[]
                {
                    new { Key = "TAB", Description = "Hauptmenü" },
                    new { Key = "F1", Description = "Display" },
                    new { Key = "F2", Description = "Audio" },
                    new { Key = "F3", Description = "Controls" },
                    new { Key = "G", Description = $"Grid ({gridStatus})" },
                    new { Key = "F9", Description = "Toggle UI" },
                    new { Key = "F11", Description = "Debug Panel" },
                    new { Key = "F12", Description = "Screenshot" }
                };

                foreach (var hint in keyHints)
                {
                    DrawKeyHint(spriteBatch, startX, currentY, hint.Key, hint.Description);
                    currentY += lineHeight;
                }
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError($"Error drawing key hints HUD: {ex.Message}", "PlayingState.DrawKeyHintsHUD");
            }
        }

        private void DrawKeyHint(SpriteBatch spriteBatch, int x, int y, string key, string description)
        {
            try
            {
                if (_font == null || _pixelTexture == null) return;

                // Calculate key button size
                var keyWidth = Math.Max(40, key.Length * 15);
                var keyHeight = 25;

                // Draw key background (like a keyboard key)
                var keyRect = new Rectangle(x, y, keyWidth, keyHeight);
                spriteBatch.Draw(_pixelTexture, keyRect, Color.DarkGray * 0.9f);

                // Draw key border (3D effect)
                DrawBorder(spriteBatch, keyRect, Color.LightGray * 0.9f, 2);

                // Draw inner shadow for 3D effect
                var innerRect = new Rectangle(x + 2, y + 2, keyWidth - 4, keyHeight - 4);
                DrawBorder(spriteBatch, innerRect, Color.Gray * 0.8f, 1);

                // Draw key text (centered)
                var keyTextSize = _font.MeasureString(key);
                var keyTextPos = new Vector2(
                    x + (keyWidth - keyTextSize.X) / 2,
                    y + (keyHeight - keyTextSize.Y) / 2
                );
                _font.DrawText(spriteBatch, key, keyTextPos, Color.White * 0.95f,
                    rotation: 0f, origin: Vector2.Zero, scale: new Vector2(0.7f, 0.7f), layerDepth: 0f);

                // Draw description text
                var descriptionPos = new Vector2(x + keyWidth + 10, y + 3);
                _font.DrawText(spriteBatch, description, descriptionPos, Color.White * 0.9f,
                    rotation: 0f, origin: Vector2.Zero, scale: new Vector2(0.6f, 0.6f), layerDepth: 0f);
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError($"Error drawing key hint: {ex.Message}", "PlayingState.DrawKeyHint");
            }
        }

        private void DrawDebugPanel(SpriteBatch spriteBatch)
        {
            try
            {
                if (_font == null || _pixelTexture == null) return;

                // Position in top-right corner
                var viewport = spriteBatch.GraphicsDevice.Viewport;
                var panelWidth = 300;
                var startX = viewport.Width - panelWidth - 20; // Right margin
                var startY = 20; // Top margin
                var currentY = startY;
                var lineHeight = 25;

                // Draw semi-transparent background
                var panelHeight = 400; // Estimated height
                var backgroundRect = new Rectangle(startX - 10, startY - 10, panelWidth + 20, panelHeight);
                spriteBatch.Draw(_pixelTexture, backgroundRect, Color.Black * 0.8f);
                DrawBorder(spriteBatch, backgroundRect, Color.Gray, 2);

                // Title
                _font.DrawText(spriteBatch, "Debug Panel", new Vector2(startX, currentY), Color.Yellow,
                    rotation: 0f, origin: Vector2.Zero, scale: new Vector2(0.8f, 0.8f), layerDepth: 0f);
                currentY += lineHeight + 10;

                // Draw tree categories
                currentY = DrawDebugCategory(spriteBatch, startX, currentY, lineHeight, "Performance", "🎯");
                currentY = DrawDebugCategory(spriteBatch, startX, currentY, lineHeight, "GameWorld", "🗺️");
                currentY = DrawDebugCategory(spriteBatch, startX, currentY, lineHeight, "GameState", "🎮");
                currentY = DrawDebugCategory(spriteBatch, startX, currentY, lineHeight, "Transport", "🚂");
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError($"Error drawing debug panel: {ex.Message}", "PlayingState.DrawDebugPanel");
            }
        }

        private int DrawDebugCategory(SpriteBatch spriteBatch, int x, int y, int lineHeight, string category, string icon)
        {
            if (_font == null || _pixelTexture == null) return y;

            var isExpanded = _debugTreeExpanded.GetValueOrDefault(category, false);
            var expandIcon = isExpanded ? "▼" : "▶";

            // Draw category header (clickable)
            var headerText = $"{expandIcon} {icon} {category}";
            _font.DrawText(spriteBatch, headerText, new Vector2(x, y), Color.White,
                rotation: 0f, origin: Vector2.Zero, scale: new Vector2(0.7f, 0.7f), layerDepth: 0f);

            y += lineHeight;

            // Draw children if expanded
            if (isExpanded)
            {
                y = DrawDebugCategoryContent(spriteBatch, x + 20, y, lineHeight, category);
            }

            return y + 5; // Extra spacing between categories
        }

        private int DrawDebugCategoryContent(SpriteBatch spriteBatch, int x, int y, int lineHeight, string category)
        {
            if (_font == null) return y;

            switch (category)
            {
                case "Performance":
                    // Get real FPS (approximate - using 60 FPS target)
                    var targetFPS = 60;
                    y = DrawDebugLine(spriteBatch, x, y, lineHeight, $"FPS: ~{targetFPS}");
                    y = DrawDebugLine(spriteBatch, x, y, lineHeight, $"Frame: ~{1000.0/targetFPS:F1}ms");

                    // Get memory usage
                    var memoryMB = GC.GetTotalMemory(false) / (1024 * 1024);
                    y = DrawDebugLine(spriteBatch, x, y, lineHeight, $"Memory: {memoryMB}MB");
                    y = DrawDebugLine(spriteBatch, x, y, lineHeight, "Draw Calls: N/A");
                    break;

                case "GameWorld":
                    // Get real camera position
                    var cameraPos = _cameraSystem?.Position ?? System.Numerics.Vector2.Zero;
                    y = DrawDebugLine(spriteBatch, x, y, lineHeight, $"Camera: ({cameraPos.X:F0}, {cameraPos.Y:F0})");

                    // Get real mouse position
                    var mouseState = Mouse.GetState();
                    y = DrawDebugLine(spriteBatch, x, y, lineHeight, $"Mouse: Screen({mouseState.X},{mouseState.Y})");

                    // Convert to grid coordinates
                    var worldPos = _cameraSystem?.ScreenToWorld(new System.Numerics.Vector2(mouseState.X, mouseState.Y)) ?? System.Numerics.Vector2.Zero;
                    var gridX = (int)(worldPos.X / 5); // TILE_SIZE = 5
                    var gridY = (int)(worldPos.Y / 5);
                    y = DrawDebugLine(spriteBatch, x, y, lineHeight, $"Grid: ({gridX},{gridY})");

                    // Get tile under mouse (if valid)
                    var tileType = "Outside";
                    if (gridX >= 0 && gridX < 384 && gridY >= 0 && gridY < 216)
                    {
                        // This would need access to TilemapManager to get real tile type
                        tileType = "Unknown";
                    }
                    y = DrawDebugLine(spriteBatch, x, y, lineHeight, $"Tile: {tileType}");
                    y = DrawDebugLine(spriteBatch, x, y, lineHeight, "World: 384x216");
                    break;

                case "GameState":
                    y = DrawDebugLine(spriteBatch, x, y, lineHeight, "State: PlayingState");

                    // Get current input state
                    var keyboardState = Keyboard.GetState();
                    var inputKeys = new List<string>();
                    if (keyboardState.IsKeyDown(Keys.W)) inputKeys.Add("W");
                    if (keyboardState.IsKeyDown(Keys.A)) inputKeys.Add("A");
                    if (keyboardState.IsKeyDown(Keys.S)) inputKeys.Add("S");
                    if (keyboardState.IsKeyDown(Keys.D)) inputKeys.Add("D");
                    var inputText = inputKeys.Count > 0 ? string.Join(",", inputKeys) : "None";
                    y = DrawDebugLine(spriteBatch, x, y, lineHeight, $"Input: {inputText}");

                    y = DrawDebugLine(spriteBatch, x, y, lineHeight, "Tooltips: Available");
                    break;

                case "Transport":
                    y = DrawDebugLine(spriteBatch, x, y, lineHeight, "Trains: 1");
                    y = DrawDebugLine(spriteBatch, x, y, lineHeight, "Routes: 0");
                    y = DrawDebugLine(spriteBatch, x, y, lineHeight, "Buildings: 0");
                    break;
            }

            return y;
        }

        private int DrawDebugLine(SpriteBatch spriteBatch, int x, int y, int lineHeight, string text)
        {
            if (_font == null) return y;

            _font.DrawText(spriteBatch, text, new Vector2(x, y), Color.LightGray,
                rotation: 0f, origin: Vector2.Zero, scale: new Vector2(0.6f, 0.6f), layerDepth: 0f);

            return y + lineHeight;
        }

        private void HandleDebugPanelClicks()
        {
            try
            {
                // Only handle clicks if debounce timer has expired
                if (_debugClickDebounceTimer > 0f) return;

                var mouseState = Mouse.GetState();
                var previousMouseState = _inputSystem.GetPreviousMouseState();

                // Check for left mouse button click (pressed this frame, not last frame)
                if (mouseState.LeftButton == ButtonState.Pressed &&
                    previousMouseState.LeftButton == ButtonState.Released)
                {
                    var mousePos = new Point(mouseState.X, mouseState.Y);
                    if (CheckDebugCategoryClicks(mousePos))
                    {
                        // Set debounce timer to prevent rapid clicking
                        _debugClickDebounceTimer = DEBUG_CLICK_DEBOUNCE_DELAY;
                    }
                }
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError($"Error handling debug panel clicks: {ex.Message}", "PlayingState.HandleDebugPanelClicks");
            }
        }

        private bool CheckDebugCategoryClicks(Point mousePos)
        {
            // Calculate debug panel position (same as in DrawDebugPanel)
            var viewport = _graphicsDevice.Viewport;
            var panelWidth = 300;
            var startX = viewport.Width - panelWidth - 20;
            var startY = 20;
            var lineHeight = 25;

            // Skip title line
            var currentY = startY + lineHeight + 10;

            // Check each category
            var categories = new[] { "Performance", "GameWorld", "GameState", "Transport" };

            foreach (var category in categories)
            {
                var categoryRect = new Rectangle(startX, currentY, panelWidth, lineHeight);

                if (categoryRect.Contains(mousePos))
                {
                    // Toggle category expansion
                    _debugTreeExpanded[category] = !_debugTreeExpanded.GetValueOrDefault(category, false);
                    _errorHandler.LogInfo($"Debug category '{category}' {(_debugTreeExpanded[category] ? "expanded" : "collapsed")}");
                    return true; // Click was handled
                }

                // Move to next category position
                currentY += lineHeight;

                // Skip expanded content height if expanded
                if (_debugTreeExpanded.GetValueOrDefault(category, false))
                {
                    var contentLines = GetDebugCategoryContentLines(category);
                    currentY += contentLines * lineHeight;
                }

                currentY += 5; // Extra spacing
            }

            return false; // No click was handled
        }

        private int GetDebugCategoryContentLines(string category)
        {
            return category switch
            {
                "Performance" => 4, // FPS, Frame, Memory, Draw Calls
                "GameWorld" => 5,   // Camera, Mouse, Grid, Tile, World
                "GameState" => 3,   // State, Input, Tooltips
                "Transport" => 3,   // Trains, Routes, Buildings
                _ => 0
            };
        }

        private void DrawBorder(SpriteBatch spriteBatch, Rectangle rect, Color color, int thickness)
        {
            if (_pixelTexture == null) return;

            // Top
            spriteBatch.Draw(_pixelTexture, new Rectangle(rect.X, rect.Y, rect.Width, thickness), color);
            // Bottom
            spriteBatch.Draw(_pixelTexture, new Rectangle(rect.X, rect.Y + rect.Height - thickness, rect.Width, thickness), color);
            // Left
            spriteBatch.Draw(_pixelTexture, new Rectangle(rect.X, rect.Y, thickness, rect.Height), color);
            // Right
            spriteBatch.Draw(_pixelTexture, new Rectangle(rect.X + rect.Width - thickness, rect.Y, thickness, rect.Height), color);
        }

        public void OnEnter()
        {
            try
            {
                _errorHandler.LogInfo("PlayingState entered - game controls active");
                // Set debounce timer when entering to prevent immediate menu re-opening
                _menuKeyDebounceTimer = MENU_KEY_DEBOUNCE_DELAY;
                // Game controls are now active
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError($"Error in PlayingState OnEnter: {ex.Message}", "PlayingState.OnEnter");
            }
        }

        public void OnExit()
        {
            try
            {
                _errorHandler.LogInfo("PlayingState exited - game controls paused");
                // Game controls are now paused
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError($"Error in PlayingState OnExit: {ex.Message}", "PlayingState.OnExit");
            }
        }

        public void Dispose()
        {
            try
            {
                _fontSystem?.Dispose();
                _pixelTexture?.Dispose();
                _isInitialized = false;
                _errorHandler.LogInfo("PlayingState disposed");
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError($"Error disposing PlayingState: {ex.Message}", "PlayingState.Dispose");
            }
        }
    }
}
