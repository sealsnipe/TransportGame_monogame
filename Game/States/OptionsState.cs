using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TransportGame.Game.Managers;
using TransportGame.Game.Systems;
using TransportGame.Game.Models;
using FontStashSharp;
using System.Collections.Generic;

namespace TransportGame.Game.States
{
    /// <summary>
    /// Options menu state with Display/Audio/Controls sections
    /// Integrates with SettingsManager for persistent settings
    /// </summary>
    public class OptionsState : IGameState
    {
        private readonly ErrorHandler _errorHandler;
        private readonly StateManager _stateManager;
        private readonly GraphicsDevice _graphicsDevice;
        private readonly InputSystem _inputSystem;
        private readonly SettingsManager _settingsManager;

        // State properties
        public bool IsActive { get; set; }
        public bool UpdateBelow => false;
        public bool DrawBelow => false;  // Don't draw anything below

        private Texture2D? _pixelTexture;
        private FontSystem? _fontSystem;
        private DynamicSpriteFont? _font;
        private bool _isInitialized;

        // Menu structure
        private enum OptionsSection { Display, Audio, Controls }
        private OptionsSection _currentSection = OptionsSection.Display;
        private List<string> _sectionNames = new() { "Display", "Audio", "Controls" };
        private int _selectedSectionIndex = 0;

        // ESC key debouncing
        private KeyboardState _previousKeyboardState;
        private MouseState _previousMouseState;
        private const float ESC_DEBOUNCE_DELAY = 0.2f; // 200ms debounce
        private float _escDebounceTimer = 0f;

        // Current settings (working copy)
        private GameSettings _workingSettings;

        // Interactive controls state
        private bool _isDraggingSlider = false;
        private string _draggingSliderType = "";
        private Rectangle _lastSliderRect;

        // Control rectangles for hit detection
        private Dictionary<string, Rectangle> _controlRects = new Dictionary<string, Rectangle>();

        // Resolution dropdown
        private bool _showResolutionDropdown = false;
        private readonly string[] _resolutionOptions = { "1280x720", "1920x1080", "2560x1440", "3840x2160" };

        // UI Control dimensions
        private const int BUTTON_WIDTH = 80;
        private const int BUTTON_HEIGHT = 30;
        private const int SLIDER_WIDTH = 200;
        private const int SLIDER_HEIGHT = 20;
        private const int CHECKBOX_SIZE = 20;

        // Scrolling support
        private float _scrollOffset = 0f;
        private const float SCROLL_SPEED = 30f;
        private float _maxScrollOffset = 0f;

        public OptionsState(ErrorHandler errorHandler, StateManager stateManager, GraphicsDevice graphicsDevice,
                           InputSystem inputSystem, SettingsManager settingsManager)
        {
            _errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
            _stateManager = stateManager ?? throw new ArgumentNullException(nameof(stateManager));
            _graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
            _inputSystem = inputSystem ?? throw new ArgumentNullException(nameof(inputSystem));
            _settingsManager = settingsManager ?? throw new ArgumentNullException(nameof(settingsManager));

            // Initialize working settings
            _workingSettings = new GameSettings();
        }

        public void Initialize()
        {
            try
            {
                // Create pixel texture for UI elements
                _pixelTexture = new Texture2D(_graphicsDevice, 1, 1);
                _pixelTexture.SetData(new[] { Color.White });

                // Load font (reuse from other states)
                LoadFont();

                // Load current settings from SettingsManager
                _workingSettings = _settingsManager.GetSettings();

                _isInitialized = true;
                _errorHandler.LogInfo("OptionsState initialized with settings integration");
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError($"Error initializing OptionsState: {ex.Message}", "OptionsState.Initialize");
            }
        }

        private void LoadFont()
        {
            try
            {
                _fontSystem = new FontSystem();

                // Load Arial font from Windows (same as MenuState)
                var arialPath = @"C:\Windows\Fonts\arial.ttf";
                if (File.Exists(arialPath))
                {
                    var fontData = File.ReadAllBytes(arialPath);
                    _fontSystem.AddFont(fontData);
                    _font = _fontSystem.GetFont(24); // Medium size for options
                    _errorHandler.LogInfo("OptionsState: Arial font loaded successfully");
                }
                else
                {
                    _errorHandler.LogInfo("OptionsState: Arial font not found, using fallback");
                }
            }
            catch (Exception ex)
            {
                _errorHandler.HandleWarning($"Could not load font in OptionsState: {ex.Message}", "OptionsState.LoadFont");
                _font = null;
            }
        }

        public void Update(GameTime gameTime)
        {
            try
            {
                if (!_isInitialized || !IsActive) return;

                // Update debounce timer
                _escDebounceTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;

                var currentKeyboard = Keyboard.GetState();
                var currentMouse = Mouse.GetState();

                // ESC - Go back to main menu
                if (currentKeyboard.IsKeyDown(Keys.Escape) &&
                    !_previousKeyboardState.IsKeyDown(Keys.Escape) &&
                    _escDebounceTimer <= 0f)
                {
                    _errorHandler.LogInfo("OPTIONS: ESC pressed - going back");
                    _stateManager.PopState();
                    _escDebounceTimer = ESC_DEBOUNCE_DELAY;
                }

                // Left/Right Arrow - Navigate between sections
                if (currentKeyboard.IsKeyDown(Keys.Left) && !_previousKeyboardState.IsKeyDown(Keys.Left))
                {
                    _selectedSectionIndex = (_selectedSectionIndex - 1 + _sectionNames.Count) % _sectionNames.Count;
                    _currentSection = (OptionsSection)_selectedSectionIndex;
                    _errorHandler.LogInfo($"OPTIONS: Switched to {_currentSection} section");
                }
                else if (currentKeyboard.IsKeyDown(Keys.Right) && !_previousKeyboardState.IsKeyDown(Keys.Right))
                {
                    _selectedSectionIndex = (_selectedSectionIndex + 1) % _sectionNames.Count;
                    _currentSection = (OptionsSection)_selectedSectionIndex;
                    _errorHandler.LogInfo($"OPTIONS: Switched to {_currentSection} section");
                }

                // F-Key shortcuts for quick section access or exit if already in section
                if (currentKeyboard.IsKeyDown(Keys.F1) && !_previousKeyboardState.IsKeyDown(Keys.F1))
                {
                    if (_currentSection == OptionsSection.Display)
                    {
                        // Already in Display - close menu and return to game
                        _errorHandler.LogInfo("OPTIONS: F1 pressed in Display section - closing menu");
                        _stateManager.PopState(); // Close OptionsState
                        _stateManager.PopState(); // Close MenuState - return to game
                    }
                    else
                    {
                        _selectedSectionIndex = 0;
                        _currentSection = OptionsSection.Display;
                        _errorHandler.LogInfo("OPTIONS: F1 pressed - switched to Display");
                    }
                }
                else if (currentKeyboard.IsKeyDown(Keys.F2) && !_previousKeyboardState.IsKeyDown(Keys.F2))
                {
                    if (_currentSection == OptionsSection.Audio)
                    {
                        // Already in Audio - close menu and return to game
                        _errorHandler.LogInfo("OPTIONS: F2 pressed in Audio section - closing menu");
                        _stateManager.PopState(); // Close OptionsState
                        _stateManager.PopState(); // Close MenuState - return to game
                    }
                    else
                    {
                        _selectedSectionIndex = 1;
                        _currentSection = OptionsSection.Audio;
                        _errorHandler.LogInfo("OPTIONS: F2 pressed - switched to Audio");
                    }
                }
                else if (currentKeyboard.IsKeyDown(Keys.F3) && !_previousKeyboardState.IsKeyDown(Keys.F3))
                {
                    if (_currentSection == OptionsSection.Controls)
                    {
                        // Already in Controls - close menu and return to game
                        _errorHandler.LogInfo("OPTIONS: F3 pressed in Controls section - closing menu");
                        _stateManager.PopState(); // Close OptionsState
                        _stateManager.PopState(); // Close MenuState - return to game
                    }
                    else
                    {
                        _selectedSectionIndex = 2;
                        _currentSection = OptionsSection.Controls;
                        _errorHandler.LogInfo("OPTIONS: F3 pressed - switched to Controls");
                    }
                }



                // Mouse click on section tabs
                if (currentMouse.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released)
                {
                    HandleSectionTabClick(currentMouse.X, currentMouse.Y);
                }

                // Mouse wheel scrolling
                var scrollDelta = currentMouse.ScrollWheelValue - _previousMouseState.ScrollWheelValue;
                if (scrollDelta != 0)
                {
                    _scrollOffset -= scrollDelta / 120f * SCROLL_SPEED; // 120 is standard wheel delta
                    _scrollOffset = Math.Max(0, Math.Min(_scrollOffset, _maxScrollOffset));
                }

                // Handle interactive controls
                HandleInteractiveControls(currentMouse);

                // Update previous states
                _previousKeyboardState = currentKeyboard;
                _previousMouseState = currentMouse;
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError($"CRITICAL ERROR in OptionsState Update: {ex.Message}\nStackTrace: {ex.StackTrace}", "OptionsState.Update");
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            try
            {
                if (!_isInitialized || _pixelTexture == null) return;

                // Clear control rectangles for this frame
                _controlRects.Clear();

                var screenWidth = _graphicsDevice.Viewport.Width;
                var screenHeight = _graphicsDevice.Viewport.Height;

                // Draw semi-transparent background
                var overlayRect = new Rectangle(0, 0, screenWidth, screenHeight);
                spriteBatch.Draw(_pixelTexture, overlayRect, Color.Black * 0.7f);

                // Draw main options panel
                var panelWidth = 800;
                var panelHeight = 600;
                var panelX = (screenWidth - panelWidth) / 2;
                var panelY = (screenHeight - panelHeight) / 2;
                var panelRect = new Rectangle(panelX, panelY, panelWidth, panelHeight);
                spriteBatch.Draw(_pixelTexture, panelRect, Color.DarkSlateGray);

                // Draw panel border
                DrawBorder(spriteBatch, panelRect, Color.White, 2);

                // Draw title
                if (_font != null)
                {
                    var titleText = "Optionen";
                    var titlePos = new Vector2(panelX + 20, panelY + 20);
                    _font.DrawText(spriteBatch, titleText, titlePos, Color.White,
                        rotation: 0f, origin: Vector2.Zero, scale: new Vector2(1.2f, 1.2f), layerDepth: 0f);
                }

                // Draw section navigation (sticky at top)
                DrawSectionNavigation(spriteBatch, panelX, panelY + 70, panelWidth);

                // Create scrollable content area
                var contentY = panelY + 120;
                var contentHeight = panelHeight - 160;
                var scrollableRect = new Rectangle(panelX + 20, contentY, panelWidth - 40, contentHeight);

                // Calculate content height and max scroll
                var totalContentHeight = CalculateContentHeight();
                _maxScrollOffset = Math.Max(0, totalContentHeight - contentHeight);

                // Draw scrollable content with clipping
                DrawScrollableContent(spriteBatch, scrollableRect);

                // Draw instructions
                if (_font != null)
                {
                    var instructionText = "← → Navigation | ESC Zurück";
                    var instructionPos = new Vector2(panelX + 20, panelY + panelHeight - 40);
                    _font.DrawText(spriteBatch, instructionText, instructionPos, Color.LightGray,
                        rotation: 0f, origin: Vector2.Zero, scale: new Vector2(0.8f, 0.8f), layerDepth: 0f);
                }
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError($"Error in OptionsState Draw: {ex.Message}", "OptionsState.Draw");
            }
        }

        private void DrawBorder(SpriteBatch spriteBatch, Rectangle rect, Color color, int thickness)
        {
            if (_pixelTexture == null) return;

            // Top
            spriteBatch.Draw(_pixelTexture, new Rectangle(rect.X, rect.Y, rect.Width, thickness), color);
            // Bottom
            spriteBatch.Draw(_pixelTexture, new Rectangle(rect.X, rect.Bottom - thickness, rect.Width, thickness), color);
            // Left
            spriteBatch.Draw(_pixelTexture, new Rectangle(rect.X, rect.Y, thickness, rect.Height), color);
            // Right
            spriteBatch.Draw(_pixelTexture, new Rectangle(rect.Right - thickness, rect.Y, thickness, rect.Height), color);
        }

        private void DrawSectionNavigation(SpriteBatch spriteBatch, int panelX, int navY, int panelWidth)
        {
            if (_font == null || _pixelTexture == null) return;

            var sectionWidth = panelWidth / _sectionNames.Count;

            for (int i = 0; i < _sectionNames.Count; i++)
            {
                var sectionX = panelX + i * sectionWidth;
                var isSelected = i == _selectedSectionIndex;

                // Draw section background
                var sectionRect = new Rectangle(sectionX, navY, sectionWidth, 40);
                var bgColor = isSelected ? Color.DarkBlue : Color.DarkGray;
                spriteBatch.Draw(_pixelTexture, sectionRect, bgColor);

                // Draw section border
                DrawBorder(spriteBatch, sectionRect, Color.White, 1);

                // Draw section text
                var textColor = isSelected ? Color.White : Color.LightGray;
                var textPos = new Vector2(sectionX + 10, navY + 10);
                _font.DrawText(spriteBatch, _sectionNames[i], textPos, textColor,
                    rotation: 0f, origin: Vector2.Zero, scale: new Vector2(0.9f, 0.9f), layerDepth: 0f);
            }
        }

        private void DrawCurrentSection(SpriteBatch spriteBatch, int contentX, int contentY, int contentWidth, int contentHeight)
        {
            if (_font == null) return;

            switch (_currentSection)
            {
                case OptionsSection.Display:
                    DrawDisplaySettings(spriteBatch, contentX, contentY, contentWidth, contentHeight);
                    break;
                case OptionsSection.Audio:
                    DrawAudioSettings(spriteBatch, contentX, contentY, contentWidth, contentHeight);
                    break;
                case OptionsSection.Controls:
                    DrawControlSettings(spriteBatch, contentX, contentY, contentWidth, contentHeight);
                    break;
            }
        }

        private void HandleSectionTabClick(int mouseX, int mouseY)
        {
            try
            {
                var screenWidth = _graphicsDevice.Viewport.Width;
                var screenHeight = _graphicsDevice.Viewport.Height;
                var panelWidth = 800;
                var panelX = (screenWidth - panelWidth) / 2;
                var navY = (screenHeight - 600) / 2 + 70; // Same as in Draw method

                var sectionWidth = panelWidth / _sectionNames.Count;

                for (int i = 0; i < _sectionNames.Count; i++)
                {
                    var sectionX = panelX + i * sectionWidth;
                    var sectionRect = new Rectangle(sectionX, navY, sectionWidth, 40);

                    if (sectionRect.Contains(mouseX, mouseY))
                    {
                        _selectedSectionIndex = i;
                        _currentSection = (OptionsSection)i;
                        _errorHandler.LogInfo($"OPTIONS: Clicked on {_currentSection} section");
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError($"Error handling section tab click: {ex.Message}", "OptionsState.HandleSectionTabClick");
            }
        }

        private float CalculateContentHeight()
        {
            // Calculate the total height needed for the current section
            switch (_currentSection)
            {
                case OptionsSection.Display:
                    return 200f; // 4 items * 40px + some padding
                case OptionsSection.Audio:
                    return 200f; // 4 items * 40px + some padding
                case OptionsSection.Controls:
                    return 600f; // Many items with keyboard symbols
                default:
                    return 200f;
            }
        }

        private void DrawScrollableContent(SpriteBatch spriteBatch, Rectangle contentRect)
        {
            // Save current scissor rectangle
            var originalScissor = spriteBatch.GraphicsDevice.ScissorRectangle;
            var originalRasterizerState = spriteBatch.GraphicsDevice.RasterizerState;

            try
            {
                // End current spriteBatch to change rasterizer state
                spriteBatch.End();

                // Set up scissor test for clipping
                var rasterizerState = new RasterizerState { ScissorTestEnable = true };
                spriteBatch.GraphicsDevice.RasterizerState = rasterizerState;
                spriteBatch.GraphicsDevice.ScissorRectangle = contentRect;

                // Restart spriteBatch with scissor test
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                                DepthStencilState.None, rasterizerState);

                // Draw content with scroll offset
                var scrolledY = contentRect.Y - (int)_scrollOffset;
                DrawCurrentSection(spriteBatch, contentRect.X, scrolledY, contentRect.Width, contentRect.Height + (int)_scrollOffset);

                // End spriteBatch to restore state
                spriteBatch.End();
            }
            finally
            {
                // Restore original state
                spriteBatch.GraphicsDevice.RasterizerState = originalRasterizerState;
                spriteBatch.GraphicsDevice.ScissorRectangle = originalScissor;

                // Restart spriteBatch with original state
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                                DepthStencilState.None, originalRasterizerState);
            }
        }

        public void SetInitialSection(string sectionName)
        {
            try
            {
                switch (sectionName.ToLower())
                {
                    case "display":
                        _selectedSectionIndex = 0;
                        _currentSection = OptionsSection.Display;
                        break;
                    case "audio":
                        _selectedSectionIndex = 1;
                        _currentSection = OptionsSection.Audio;
                        break;
                    case "controls":
                        _selectedSectionIndex = 2;
                        _currentSection = OptionsSection.Controls;
                        break;
                    default:
                        _errorHandler.HandleWarning($"Unknown section name: {sectionName}", "OptionsState.SetInitialSection");
                        break;
                }
                _errorHandler.LogInfo($"OPTIONS: Initial section set to {_currentSection}");
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError($"Error setting initial section: {ex.Message}", "OptionsState.SetInitialSection");
            }
        }

        public void OnEnter()
        {
            _errorHandler.LogInfo("OPTIONS: Entered options state with settings integration");
            // Set debounce timer when entering to prevent immediate ESC
            _escDebounceTimer = ESC_DEBOUNCE_DELAY;
            _previousKeyboardState = Keyboard.GetState();
            _previousMouseState = Mouse.GetState();

            // Reload current settings
            _workingSettings = _settingsManager.GetSettings();
        }

        public void OnExit()
        {
            _errorHandler.LogInfo("OPTIONS: Exited options state");
        }

        private void DrawDisplaySettings(SpriteBatch spriteBatch, int x, int y, int width, int height)
        {
            if (_font == null || _pixelTexture == null) return;

            var lineHeight = 60;
            var currentY = y;

            // Resolution with dropdown
            _font.DrawText(spriteBatch, "Auflösung:", new Vector2(x, currentY), Color.White,
                rotation: 0f, origin: Vector2.Zero, scale: new Vector2(0.9f, 0.9f), layerDepth: 0f);

            var resolutionText = $"{_workingSettings.Display.ResolutionWidth}x{_workingSettings.Display.ResolutionHeight}";
            DrawDropdown(spriteBatch, x + 150, currentY - 5, 150, BUTTON_HEIGHT, resolutionText, "resolution_dropdown", _resolutionOptions, _showResolutionDropdown);
            currentY += lineHeight;

            // Add extra space if dropdown is open
            if (_showResolutionDropdown)
            {
                currentY += _resolutionOptions.Length * BUTTON_HEIGHT;
            }

            // Fullscreen with checkbox
            _font.DrawText(spriteBatch, "Vollbild:", new Vector2(x, currentY), Color.White,
                rotation: 0f, origin: Vector2.Zero, scale: new Vector2(0.9f, 0.9f), layerDepth: 0f);
            DrawCheckbox(spriteBatch, x + 150, currentY - 5, _workingSettings.Display.Fullscreen, "fullscreen");
            currentY += lineHeight;

            // VSync with checkbox
            _font.DrawText(spriteBatch, "VSync:", new Vector2(x, currentY), Color.White,
                rotation: 0f, origin: Vector2.Zero, scale: new Vector2(0.9f, 0.9f), layerDepth: 0f);
            DrawCheckbox(spriteBatch, x + 150, currentY - 5, _workingSettings.Display.VSync, "vsync");
            currentY += lineHeight;

            // UI Scale with slider
            _font.DrawText(spriteBatch, "UI-Skalierung:", new Vector2(x, currentY), Color.White,
                rotation: 0f, origin: Vector2.Zero, scale: new Vector2(0.9f, 0.9f), layerDepth: 0f);
            var scaleText = $"{_workingSettings.Display.UIScale:F1}x";
            _font.DrawText(spriteBatch, scaleText, new Vector2(x + 150, currentY), Color.Yellow,
                rotation: 0f, origin: Vector2.Zero, scale: new Vector2(0.9f, 0.9f), layerDepth: 0f);
            DrawSlider(spriteBatch, x + 250, currentY + 5, SLIDER_WIDTH, SLIDER_HEIGHT,
                _workingSettings.Display.UIScale, 0.5f, 3.0f, "ui_scale");
            currentY += lineHeight;

            // Reset button
            DrawButton(spriteBatch, x, currentY, 120, BUTTON_HEIGHT, "Zurücksetzen", "reset_display");
        }

        private void DrawAudioSettings(SpriteBatch spriteBatch, int x, int y, int width, int height)
        {
            if (_font == null || _pixelTexture == null) return;

            var lineHeight = 60;
            var currentY = y;

            // Master Volume with slider
            _font.DrawText(spriteBatch, "Master-Lautstärke:", new Vector2(x, currentY), Color.White,
                rotation: 0f, origin: Vector2.Zero, scale: new Vector2(0.9f, 0.9f), layerDepth: 0f);
            var masterText = $"{(_workingSettings.Audio.MasterVolume * 100):F0}%";
            _font.DrawText(spriteBatch, masterText, new Vector2(x + 200, currentY), Color.Yellow,
                rotation: 0f, origin: Vector2.Zero, scale: new Vector2(0.9f, 0.9f), layerDepth: 0f);
            DrawSlider(spriteBatch, x + 280, currentY + 5, SLIDER_WIDTH, SLIDER_HEIGHT,
                _workingSettings.Audio.MasterVolume, 0.0f, 1.0f, "master_volume");
            currentY += lineHeight;

            // SFX Volume with slider
            _font.DrawText(spriteBatch, "Effekt-Lautstärke:", new Vector2(x, currentY), Color.White,
                rotation: 0f, origin: Vector2.Zero, scale: new Vector2(0.9f, 0.9f), layerDepth: 0f);
            var sfxText = $"{(_workingSettings.Audio.SfxVolume * 100):F0}%";
            _font.DrawText(spriteBatch, sfxText, new Vector2(x + 200, currentY), Color.Yellow,
                rotation: 0f, origin: Vector2.Zero, scale: new Vector2(0.9f, 0.9f), layerDepth: 0f);
            DrawSlider(spriteBatch, x + 280, currentY + 5, SLIDER_WIDTH, SLIDER_HEIGHT,
                _workingSettings.Audio.SfxVolume, 0.0f, 1.0f, "sfx_volume");
            currentY += lineHeight;

            // Music Volume with slider
            _font.DrawText(spriteBatch, "Musik-Lautstärke:", new Vector2(x, currentY), Color.White,
                rotation: 0f, origin: Vector2.Zero, scale: new Vector2(0.9f, 0.9f), layerDepth: 0f);
            var musicText = $"{(_workingSettings.Audio.MusicVolume * 100):F0}%";
            _font.DrawText(spriteBatch, musicText, new Vector2(x + 200, currentY), Color.Yellow,
                rotation: 0f, origin: Vector2.Zero, scale: new Vector2(0.9f, 0.9f), layerDepth: 0f);
            DrawSlider(spriteBatch, x + 280, currentY + 5, SLIDER_WIDTH, SLIDER_HEIGHT,
                _workingSettings.Audio.MusicVolume, 0.0f, 1.0f, "music_volume");
            currentY += lineHeight;

            // Muted with checkbox
            _font.DrawText(spriteBatch, "Stumm:", new Vector2(x, currentY), Color.White,
                rotation: 0f, origin: Vector2.Zero, scale: new Vector2(0.9f, 0.9f), layerDepth: 0f);
            DrawCheckbox(spriteBatch, x + 200, currentY - 5, _workingSettings.Audio.Muted, "muted");
            currentY += lineHeight;

            // Reset button
            DrawButton(spriteBatch, x, currentY, 120, BUTTON_HEIGHT, "Zurücksetzen", "reset_audio");
        }

        private void DrawControlSettings(SpriteBatch spriteBatch, int x, int y, int width, int height)
        {
            if (_font == null || _pixelTexture == null) return;

            var lineHeight = 50;
            var currentY = y;

            // Title
            _font.DrawText(spriteBatch, "Tastenbelegung:", new Vector2(x, currentY), Color.Yellow,
                rotation: 0f, origin: Vector2.Zero, scale: new Vector2(1.0f, 1.0f), layerDepth: 0f);
            currentY += lineHeight;

            // Menu Controls
            DrawKeyBinding(spriteBatch, x, currentY, "TAB", "Hauptmenü öffnen/schließen");
            currentY += lineHeight;

            DrawKeyBinding(spriteBatch, x, currentY, "ESC", "Menü schließen / Zurück");
            currentY += lineHeight;

            // Game Controls
            currentY += 20; // Extra spacing
            _font.DrawText(spriteBatch, "Spiel-Steuerung:", new Vector2(x, currentY), Color.Yellow,
                rotation: 0f, origin: Vector2.Zero, scale: new Vector2(1.0f, 1.0f), layerDepth: 0f);
            currentY += lineHeight;

            DrawKeyBinding(spriteBatch, x, currentY, "G", "Grid ein/ausblenden");
            currentY += lineHeight;

            DrawMultiKeyBinding(spriteBatch, x, currentY, new[] { "W", "A", "S", "D" }, "Kamera bewegen");
            currentY += lineHeight;

            DrawKeyBinding(spriteBatch, x, currentY, "Mausrad", "Zoom");
            currentY += lineHeight;

            DrawKeyBinding(spriteBatch, x, currentY, "Linksklick", "Tooltip anzeigen");
            currentY += lineHeight;

            // Quick Access
            currentY += 20; // Extra spacing
            _font.DrawText(spriteBatch, "Schnellzugriff:", new Vector2(x, currentY), Color.Yellow,
                rotation: 0f, origin: Vector2.Zero, scale: new Vector2(1.0f, 1.0f), layerDepth: 0f);
            currentY += lineHeight;

            DrawKeyBinding(spriteBatch, x, currentY, "F1", "→ Display-Einstellungen");
            currentY += lineHeight;

            DrawKeyBinding(spriteBatch, x, currentY, "F2", "→ Audio-Einstellungen");
            currentY += lineHeight;

            DrawKeyBinding(spriteBatch, x, currentY, "F3", "→ Controls (diese Seite)");
        }

        private void DrawKeyBinding(SpriteBatch spriteBatch, int x, int y, string key, string description)
        {
            if (_font == null || _pixelTexture == null) return;

            var keyWidth = key == "Mausrad" || key == "Linksklick" ? 120 : Math.Max(40, key.Length * 15);
            var keyHeight = 30;

            // Draw key background (like a keyboard key)
            var keyRect = new Rectangle(x, y, keyWidth, keyHeight);
            spriteBatch.Draw(_pixelTexture, keyRect, Color.DarkGray);

            // Draw key border (3D effect)
            DrawBorder(spriteBatch, keyRect, Color.LightGray, 2);

            // Draw inner shadow for 3D effect
            var innerRect = new Rectangle(x + 2, y + 2, keyWidth - 4, keyHeight - 4);
            DrawBorder(spriteBatch, innerRect, Color.Gray, 1);

            // Draw key text
            var keyTextPos = new Vector2(x + keyWidth / 2 - key.Length * 6, y + 6);
            _font.DrawText(spriteBatch, key, keyTextPos, Color.White,
                rotation: 0f, origin: Vector2.Zero, scale: new Vector2(0.8f, 0.8f), layerDepth: 0f);

            // Draw description
            var descriptionPos = new Vector2(x + keyWidth + 20, y + 6);
            _font.DrawText(spriteBatch, description, descriptionPos, Color.LightGray,
                rotation: 0f, origin: Vector2.Zero, scale: new Vector2(0.8f, 0.8f), layerDepth: 0f);
        }

        private void DrawMultiKeyBinding(SpriteBatch spriteBatch, int x, int y, string[] keys, string description)
        {
            if (_font == null || _pixelTexture == null) return;

            var currentX = x;
            var keySize = 30;
            var keySpacing = 5;

            // Draw each key
            foreach (var key in keys)
            {
                var keyRect = new Rectangle(currentX, y, keySize, keySize);
                spriteBatch.Draw(_pixelTexture, keyRect, Color.DarkGray);
                DrawBorder(spriteBatch, keyRect, Color.LightGray, 2);

                var innerRect = new Rectangle(currentX + 2, y + 2, keySize - 4, keySize - 4);
                DrawBorder(spriteBatch, innerRect, Color.Gray, 1);

                var keyTextPos = new Vector2(currentX + keySize / 2 - 6, y + 6);
                _font.DrawText(spriteBatch, key, keyTextPos, Color.White,
                    rotation: 0f, origin: Vector2.Zero, scale: new Vector2(0.8f, 0.8f), layerDepth: 0f);

                currentX += keySize + keySpacing;
            }

            // Draw description
            var descriptionPos = new Vector2(currentX + 15, y + 6);
            _font.DrawText(spriteBatch, description, descriptionPos, Color.LightGray,
                rotation: 0f, origin: Vector2.Zero, scale: new Vector2(0.8f, 0.8f), layerDepth: 0f);
        }

        private void HandleInteractiveControls(MouseState currentMouse)
        {
            // Handle slider dragging
            if (_isDraggingSlider && currentMouse.LeftButton == ButtonState.Pressed)
            {
                HandleSliderDrag(currentMouse);
            }
            else if (_isDraggingSlider && currentMouse.LeftButton == ButtonState.Released)
            {
                _isDraggingSlider = false;
                _draggingSliderType = "";
                SaveCurrentSettings();
            }

            // Handle clicks only on button press (not release)
            if (currentMouse.LeftButton == ButtonState.Pressed &&
                _previousMouseState.LeftButton == ButtonState.Released)
            {
                HandleControlClicks(currentMouse);
            }
        }

        private void HandleControlClicks(MouseState mouse)
        {
            var mousePoint = new Point(mouse.X, mouse.Y);

            foreach (var control in _controlRects)
            {
                if (control.Value.Contains(mousePoint))
                {
                    HandleControlClick(control.Key, mouse);
                    break; // Only handle one click per frame
                }
            }
        }

        private void HandleControlClick(string controlId, MouseState mouse)
        {
            _errorHandler.LogInfo($"OPTIONS: Control clicked: {controlId}");

            switch (controlId)
            {
                // Resolution dropdown
                case "resolution_dropdown":
                    _showResolutionDropdown = !_showResolutionDropdown;
                    break;

                // Resolution options
                case "resolution_720p":
                    _workingSettings.Display.ResolutionWidth = 1280;
                    _workingSettings.Display.ResolutionHeight = 720;
                    _showResolutionDropdown = false;
                    SaveCurrentSettings();
                    break;
                case "resolution_1080p":
                    _workingSettings.Display.ResolutionWidth = 1920;
                    _workingSettings.Display.ResolutionHeight = 1080;
                    _showResolutionDropdown = false;
                    SaveCurrentSettings();
                    break;
                case "resolution_1440p":
                    _workingSettings.Display.ResolutionWidth = 2560;
                    _workingSettings.Display.ResolutionHeight = 1440;
                    _showResolutionDropdown = false;
                    SaveCurrentSettings();
                    break;
                case "resolution_4k":
                    _workingSettings.Display.ResolutionWidth = 3840;
                    _workingSettings.Display.ResolutionHeight = 2160;
                    _showResolutionDropdown = false;
                    SaveCurrentSettings();
                    break;

                // Checkboxes
                case "fullscreen":
                    _workingSettings.Display.Fullscreen = !_workingSettings.Display.Fullscreen;
                    SaveCurrentSettings();
                    break;
                case "vsync":
                    _workingSettings.Display.VSync = !_workingSettings.Display.VSync;
                    SaveCurrentSettings();
                    break;
                case "muted":
                    _workingSettings.Audio.Muted = !_workingSettings.Audio.Muted;
                    SaveCurrentSettings();
                    break;

                // Reset buttons
                case "reset_display":
                    ResetDisplaySettings();
                    break;
                case "reset_audio":
                    ResetAudioSettings();
                    break;

                // Sliders (start dragging)
                case "ui_scale":
                case "master_volume":
                case "sfx_volume":
                case "music_volume":
                    _isDraggingSlider = true;
                    _draggingSliderType = controlId;
                    HandleSliderDrag(mouse);
                    break;
            }
        }

        private void ResetDisplaySettings()
        {
            _workingSettings.Display.ResolutionWidth = 1280;
            _workingSettings.Display.ResolutionHeight = 720;
            _workingSettings.Display.Fullscreen = false;
            _workingSettings.Display.VSync = true;
            _workingSettings.Display.UIScale = 1.0f;
            SaveCurrentSettings();
            _errorHandler.LogInfo("OPTIONS: Display settings reset to defaults");
        }

        private void ResetAudioSettings()
        {
            _workingSettings.Audio.MasterVolume = 0.8f;
            _workingSettings.Audio.SfxVolume = 0.6f;
            _workingSettings.Audio.MusicVolume = 0.4f;
            _workingSettings.Audio.Muted = false;
            SaveCurrentSettings();
            _errorHandler.LogInfo("OPTIONS: Audio settings reset to defaults");
        }

        private void HandleSliderDrag(MouseState mouse)
        {
            if (_lastSliderRect.Width == 0) return;

            var relativeX = mouse.X - _lastSliderRect.X;
            var percentage = Math.Max(0, Math.Min(1, (float)relativeX / _lastSliderRect.Width));

            switch (_draggingSliderType)
            {
                case "ui_scale":
                    _workingSettings.Display.UIScale = 0.5f + percentage * (3.0f - 0.5f);
                    break;
                case "master_volume":
                    _workingSettings.Audio.MasterVolume = percentage;
                    break;
                case "sfx_volume":
                    _workingSettings.Audio.SfxVolume = percentage;
                    break;
                case "music_volume":
                    _workingSettings.Audio.MusicVolume = percentage;
                    break;
            }
        }

        private void SaveCurrentSettings()
        {
            try
            {
                _settingsManager.UpdateSettings(_workingSettings);
                _errorHandler.LogInfo("OPTIONS: Settings saved successfully");
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError($"Failed to save settings: {ex.Message}", "OptionsState.SaveCurrentSettings");
            }
        }

        private void DrawButton(SpriteBatch spriteBatch, int x, int y, int width, int height, string text, string id)
        {
            if (_font == null || _pixelTexture == null) return;

            var buttonRect = new Rectangle(x, y, width, height);

            // Register for hit detection
            _controlRects[id] = buttonRect;

            // Button background
            spriteBatch.Draw(_pixelTexture, buttonRect, Color.DarkGray);
            DrawBorder(spriteBatch, buttonRect, Color.LightGray, 2);

            // Button text (centered)
            var textSize = _font.MeasureString(text);
            var textPos = new Vector2(
                x + (width - textSize.X) / 2,
                y + (height - textSize.Y) / 2
            );
            _font.DrawText(spriteBatch, text, textPos, Color.White,
                rotation: 0f, origin: Vector2.Zero, scale: new Vector2(0.7f, 0.7f), layerDepth: 0f);
        }

        private void DrawCheckbox(SpriteBatch spriteBatch, int x, int y, bool isChecked, string id)
        {
            if (_pixelTexture == null) return;

            var checkboxRect = new Rectangle(x, y, CHECKBOX_SIZE, CHECKBOX_SIZE);

            // Register for hit detection
            _controlRects[id] = checkboxRect;

            // Checkbox background
            spriteBatch.Draw(_pixelTexture, checkboxRect, Color.White);
            DrawBorder(spriteBatch, checkboxRect, Color.Black, 2);

            // Checkmark if checked
            if (isChecked)
            {
                var innerRect = new Rectangle(x + 4, y + 4, CHECKBOX_SIZE - 8, CHECKBOX_SIZE - 8);
                spriteBatch.Draw(_pixelTexture, innerRect, Color.Green);
            }
        }

        private void DrawSlider(SpriteBatch spriteBatch, int x, int y, int width, int height,
            float value, float min, float max, string id)
        {
            if (_pixelTexture == null) return;

            var sliderRect = new Rectangle(x, y, width, height);
            _lastSliderRect = sliderRect; // Store for drag detection

            // Register for hit detection
            _controlRects[id] = sliderRect;

            // Slider track
            spriteBatch.Draw(_pixelTexture, sliderRect, Color.DarkGray);
            DrawBorder(spriteBatch, sliderRect, Color.Gray, 1);

            // Slider handle
            var percentage = (value - min) / (max - min);
            var handleX = x + (int)(percentage * (width - 10));
            var handleRect = new Rectangle(handleX, y - 2, 10, height + 4);
            spriteBatch.Draw(_pixelTexture, handleRect, Color.LightGray);
            DrawBorder(spriteBatch, handleRect, Color.White, 1);
        }

        private void DrawDropdown(SpriteBatch spriteBatch, int x, int y, int width, int height,
            string selectedText, string id, string[] options, bool isOpen)
        {
            if (_font == null || _pixelTexture == null) return;

            var dropdownRect = new Rectangle(x, y, width, height);

            // Register main dropdown for hit detection
            _controlRects[id] = dropdownRect;

            // Main dropdown button
            spriteBatch.Draw(_pixelTexture, dropdownRect, Color.DarkGray);
            DrawBorder(spriteBatch, dropdownRect, Color.LightGray, 2);

            // Selected text
            var textPos = new Vector2(x + 5, y + 5);
            _font.DrawText(spriteBatch, selectedText, textPos, Color.White,
                rotation: 0f, origin: Vector2.Zero, scale: new Vector2(0.7f, 0.7f), layerDepth: 0f);

            // Dropdown arrow
            var arrowText = isOpen ? "▲" : "▼";
            var arrowPos = new Vector2(x + width - 20, y + 5);
            _font.DrawText(spriteBatch, arrowText, arrowPos, Color.White,
                rotation: 0f, origin: Vector2.Zero, scale: new Vector2(0.7f, 0.7f), layerDepth: 0f);

            // Dropdown options (if open) - open DOWNWARDS with layout adjustment
            if (isOpen)
            {
                for (int i = 0; i < options.Length; i++)
                {
                    var optionY = y + height + (i * height); // Open downwards
                    var optionRect = new Rectangle(x, optionY, width, height);

                    // Register each option for hit detection
                    var optionId = GetResolutionOptionId(options[i]);
                    _controlRects[optionId] = optionRect;

                    // Option background
                    spriteBatch.Draw(_pixelTexture, optionRect, Color.Gray);
                    DrawBorder(spriteBatch, optionRect, Color.LightGray, 1);

                    // Option text
                    var optionTextPos = new Vector2(x + 5, optionY + 5);
                    _font.DrawText(spriteBatch, options[i], optionTextPos, Color.White,
                        rotation: 0f, origin: Vector2.Zero, scale: new Vector2(0.7f, 0.7f), layerDepth: 0f);
                }
            }
        }

        private string GetResolutionOptionId(string resolution)
        {
            return resolution switch
            {
                "1280x720" => "resolution_720p",
                "1920x1080" => "resolution_1080p",
                "2560x1440" => "resolution_1440p",
                "3840x2160" => "resolution_4k",
                _ => "resolution_720p"
            };
        }

        public void Dispose()
        {
            try
            {
                _pixelTexture?.Dispose();
                _fontSystem?.Dispose();
                _isInitialized = false;
                _errorHandler.LogInfo("OptionsState disposed with settings integration");
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError($"Error disposing OptionsState: {ex.Message}", "OptionsState.Dispose");
            }
        }
    }
}