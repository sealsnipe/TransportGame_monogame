using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using TransportGame.Game.Managers;
using TransportGame.Game.Systems;
using FontStashSharp;

namespace TransportGame.Game.States
{
    /// <summary>
    /// Main menu state with Weiter/Optionen/Beenden buttons
    /// Implements F10 key handling and menu navigation
    /// </summary>
    public class MenuState : IGameState
    {
        private readonly ErrorHandler _errorHandler;
        private readonly StateManager _stateManager;
        private readonly GraphicsDevice _graphicsDevice;
        private readonly InputSystem _inputSystem;
        private OptionsState? _optionsState;

        // Menu system
        private List<MenuButton> _menuButtons = new();
        private int _selectedButtonIndex;
        private bool _isInitialized;

        // Input handling with debouncing
        private KeyboardState _previousKeyboardState;
        private MouseState _previousMouseState;
        private float _keyRepeatTimer;
        private const float KEY_REPEAT_DELAY = 0.2f;

        // Font system
        private FontSystem? _fontSystem;
        private DynamicSpriteFont? _font;

        // Rendering
        private Texture2D? _pixelTexture;

        public bool IsActive { get; set; }
        public bool UpdateBelow => false; // Pause game when menu is open
        public bool DrawBelow => false;   // Don't draw when another menu is on top

        public MenuState(ErrorHandler errorHandler, StateManager stateManager, GraphicsDevice graphicsDevice, InputSystem inputSystem)
        {
            _errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
            _stateManager = stateManager ?? throw new ArgumentNullException(nameof(stateManager));
            _graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
            _inputSystem = inputSystem ?? throw new ArgumentNullException(nameof(inputSystem));
        }

        public void SetOptionsState(OptionsState optionsState)
        {
            _optionsState = optionsState;
        }

        public void Initialize()
        {
            try
            {
                // Initialize font system
                InitializeFontSystem();

                // Create pixel texture for backgrounds
                _pixelTexture = new Texture2D(_graphicsDevice, 1, 1);
                _pixelTexture.SetData(new[] { Color.White });

                // Initialize menu buttons
                InitializeMenuButtons();

                _selectedButtonIndex = 0;

                // Initialize input states to current state to prevent false positives
                _previousKeyboardState = Keyboard.GetState();
                _previousMouseState = Mouse.GetState();

                _isInitialized = true;
                _errorHandler.LogInfo("MenuState initialized with F10 key support");
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError($"Error initializing MenuState: {ex.Message}", "MenuState.Initialize");
            }
        }

        private void InitializeFontSystem()
        {
            try
            {
                _fontSystem = new FontSystem();

                // Load Arial font from Windows
                var arialPath = @"C:\Windows\Fonts\arial.ttf";
                if (System.IO.File.Exists(arialPath))
                {
                    var fontData = System.IO.File.ReadAllBytes(arialPath);
                    _fontSystem.AddFont(fontData);
                    _font = _fontSystem.GetFont(32); // Larger font for menu
                    _errorHandler.LogInfo("MenuState: Arial font loaded successfully");
                }
                else
                {
                    _errorHandler.LogInfo("MenuState: Arial font not found, using fallback");
                }
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError($"Error initializing font system: {ex.Message}", "MenuState.InitializeFontSystem");
            }
        }

        private void InitializeMenuButtons()
        {
            _menuButtons = new List<MenuButton>
            {
                new MenuButton("Weiter", MenuAction.Resume),
                new MenuButton("Optionen", MenuAction.Options),
                new MenuButton("Beenden", MenuAction.Exit)
            };
        }

        public void Update(GameTime gameTime)
        {
            try
            {
                if (!_isInitialized || !IsActive) return;

                // Removed excessive debug logging - only log meaningful events

                var currentKeyboardState = Keyboard.GetState();
                var currentMouseState = Mouse.GetState();
                _keyRepeatTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;

                // Only log when keys are actually pressed (reduce spam)
                if (_inputSystem.IsKeyPressed(Keys.Up) || _inputSystem.IsKeyPressed(Keys.Down) ||
                    _inputSystem.IsKeyPressed(Keys.Enter) || _inputSystem.IsKeyPressed(Keys.Escape))
                {
                    _errorHandler.LogInfo($"MenuState: Key pressed - Up={_inputSystem.IsKeyPressed(Keys.Up)} Down={_inputSystem.IsKeyPressed(Keys.Down)} Enter={_inputSystem.IsKeyPressed(Keys.Enter)} ESC={_inputSystem.IsKeyPressed(Keys.Escape)}");
                }

                // Handle ESC and F10 to close menu (TAB only opens, doesn't close)
                // DIRECT keyboard check with debouncing - bypass InputSystem
                if (currentKeyboardState.IsKeyDown(Keys.Escape) &&
                    !_previousKeyboardState.IsKeyDown(Keys.Escape) &&
                    _keyRepeatTimer <= 0f)
                {
                    _errorHandler.LogInfo("MENU: ESC pressed (direct) - closing menu");
                    _stateManager.PopState(); // Close menu, return to game
                    _keyRepeatTimer = KEY_REPEAT_DELAY; // Set debounce timer
                    return;
                }

                if (currentKeyboardState.IsKeyDown(Keys.F10) &&
                    !_previousKeyboardState.IsKeyDown(Keys.F10) &&
                    _keyRepeatTimer <= 0f)
                {
                    _errorHandler.LogInfo("MENU: F10 pressed (direct) - closing menu");
                    _stateManager.PopState(); // Close menu, return to game
                    _keyRepeatTimer = KEY_REPEAT_DELAY; // Set debounce timer
                    return;
                }

                // Handle menu navigation
                if (_inputSystem.IsKeyPressed(Keys.Up) || _inputSystem.IsKeyPressed(Keys.W))
                {
                    _selectedButtonIndex = (_selectedButtonIndex - 1 + _menuButtons.Count) % _menuButtons.Count;
                    _errorHandler.LogInfo($"MENU: UP key - selected '{_menuButtons[_selectedButtonIndex].Text}'");
                }
                else if (_inputSystem.IsKeyPressed(Keys.Down) || _inputSystem.IsKeyPressed(Keys.S))
                {
                    _selectedButtonIndex = (_selectedButtonIndex + 1) % _menuButtons.Count;
                    _errorHandler.LogInfo($"MENU: DOWN key - selected '{_menuButtons[_selectedButtonIndex].Text}'");
                }

                // Handle button activation
                if (_inputSystem.IsKeyPressed(Keys.Enter) || _inputSystem.IsKeyPressed(Keys.Space))
                {
                    _errorHandler.LogInfo($"MENU: ENTER/SPACE pressed - executing '{_menuButtons[_selectedButtonIndex].Text}'");
                    ExecuteMenuAction(_menuButtons[_selectedButtonIndex].Action);
                }

                // Handle mouse input (before updating previous state)
                HandleMouseInput(currentMouseState);

                // Update previous states AFTER handling input
                _previousKeyboardState = currentKeyboardState;
                _previousMouseState = currentMouseState;
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError($"Error in MenuState Update: {ex.Message}", "MenuState.Update");
            }
        }

        private bool IsKeyPressed(KeyboardState currentState, Keys key)
        {
            return currentState.IsKeyDown(key) &&
                   !_previousKeyboardState.IsKeyDown(key) &&
                   _keyRepeatTimer <= 0f;
        }

        private void HandleMouseInput(MouseState currentMouseState)
        {
            // Check for mouse click (left button pressed and released)
            if (currentMouseState.LeftButton == ButtonState.Released &&
                _previousMouseState.LeftButton == ButtonState.Pressed)
            {
                var mousePosition = new Vector2(currentMouseState.X, currentMouseState.Y);

                // Check if click is on any button
                for (int i = 0; i < _menuButtons.Count; i++)
                {
                    var button = _menuButtons[i];
                    if (IsPointInButton(mousePosition, button))
                    {
                        _errorHandler.LogInfo($"MENU: '{button.Text}' button clicked");
                        _selectedButtonIndex = i;
                        ExecuteMenuAction(button.Action);
                        return;
                    }
                }
            }

            // Check for mouse hover to update selection (no logging for hover to reduce spam)
            var currentMousePosition = new Vector2(currentMouseState.X, currentMouseState.Y);
            for (int i = 0; i < _menuButtons.Count; i++)
            {
                var button = _menuButtons[i];
                if (IsPointInButton(currentMousePosition, button))
                {
                    if (_selectedButtonIndex != i)
                    {
                        _selectedButtonIndex = i;
                        // No logging for hover to reduce spam
                    }
                    break;
                }
            }
        }

        private bool IsPointInButton(Vector2 point, MenuButton button)
        {
            // Calculate button bounds using the SAME coordinates as DrawMenuButtons()
            var screenWidth = _graphicsDevice.Viewport.Width;
            var screenHeight = _graphicsDevice.Viewport.Height;

            // Menu background position (same as Draw method)
            var menuWidth = 400;
            var menuHeight = 300;
            var menuX = (screenWidth - menuWidth) / 2;
            var menuY = (screenHeight - menuHeight) / 2;

            // Button position (same as DrawMenuButtons method)
            var startX = menuX;
            var startY = menuY + 80;
            var buttonIndex = _menuButtons.IndexOf(button);
            var buttonY = startY + buttonIndex * 60;

            // Button bounds (same as DrawMenuButtons background rect)
            var buttonX = startX + 10;
            var buttonWidth = 360;
            var buttonHeight = 50;

            return point.X >= buttonX && point.X <= buttonX + buttonWidth &&
                   point.Y >= buttonY - 5 && point.Y <= buttonY - 5 + buttonHeight;
        }

        private void ExecuteMenuAction(MenuAction action)
        {
            try
            {
                switch (action)
                {
                    case MenuAction.Resume:
                        _errorHandler.LogInfo("MENU: Resuming game - closing menu");
                        _stateManager.PopState(); // Return to game
                        break;

                    case MenuAction.Options:
                        _errorHandler.LogInfo("MENU: Options selected - opening options menu");
                        OpenOptionsMenu();
                        break;

                    case MenuAction.Exit:
                        _errorHandler.LogInfo("MENU: Exit game selected (not implemented yet)");
                        // TODO: Implement game exit
                        break;
                }

                _keyRepeatTimer = KEY_REPEAT_DELAY;
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError($"Error executing menu action: {ex.Message}", "MenuState.ExecuteMenuAction");
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            try
            {
                if (!_isInitialized) return;

                var screenWidth = _graphicsDevice.Viewport.Width;
                var screenHeight = _graphicsDevice.Viewport.Height;

                // Draw semi-transparent overlay
                var overlayRect = new Rectangle(0, 0, screenWidth, screenHeight);
                spriteBatch.Draw(_pixelTexture, overlayRect, Color.Black * 0.7f);

                // Draw menu background
                var menuWidth = 400;
                var menuHeight = 300;
                var menuX = (screenWidth - menuWidth) / 2;
                var menuY = (screenHeight - menuHeight) / 2;
                var menuRect = new Rectangle(menuX, menuY, menuWidth, menuHeight);
                
                spriteBatch.Draw(_pixelTexture, menuRect, Color.DarkSlateGray);
                spriteBatch.Draw(_pixelTexture, new Rectangle(menuX, menuY, menuWidth, 3), Color.White);
                spriteBatch.Draw(_pixelTexture, new Rectangle(menuX, menuY + menuHeight - 3, menuWidth, 3), Color.White);
                spriteBatch.Draw(_pixelTexture, new Rectangle(menuX, menuY, 3, menuHeight), Color.White);
                spriteBatch.Draw(_pixelTexture, new Rectangle(menuX + menuWidth - 3, menuY, 3, menuHeight), Color.White);

                // Draw menu title
                if (_font != null)
                {
                    var titleText = "Hauptmenü";
                    var titlePosition = new Vector2(menuX + 20, menuY + 20);
                    _font.DrawText(spriteBatch, titleText, titlePosition, Color.Yellow, 
                        rotation: 0f, origin: Vector2.Zero, scale: new Vector2(1.0f, 1.0f), layerDepth: 0f);
                }

                // Draw menu buttons
                DrawMenuButtons(spriteBatch, menuX, menuY + 80);
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError($"Error in MenuState Draw: {ex.Message}", "MenuState.Draw");
            }
        }

        private void DrawMenuButtons(SpriteBatch spriteBatch, int startX, int startY)
        {
            try
            {
                if (_font == null) return;

                for (int i = 0; i < _menuButtons.Count; i++)
                {
                    var button = _menuButtons[i];
                    var isSelected = i == _selectedButtonIndex;
                    var buttonY = startY + i * 60;

                    // Draw button background if selected
                    if (isSelected)
                    {
                        var buttonRect = new Rectangle(startX + 10, buttonY - 5, 360, 50);
                        spriteBatch.Draw(_pixelTexture, buttonRect, Color.DarkBlue * 0.5f);
                    }

                    // Draw button text
                    var textColor = isSelected ? Color.White : Color.LightGray;
                    var textPosition = new Vector2(startX + 20, buttonY);
                    
                    _font.DrawText(spriteBatch, button.Text, textPosition, textColor,
                        rotation: 0f, origin: Vector2.Zero, scale: new Vector2(0.8f, 0.8f), layerDepth: 0f);
                }
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError($"Error drawing menu buttons: {ex.Message}", "MenuState.DrawMenuButtons");
            }
        }

        private void OpenOptionsMenu()
        {
            try
            {
                if (_optionsState == null)
                {
                    _errorHandler.HandleError("OptionsState not set in MenuState", "MenuState.OpenOptionsMenu");
                    return;
                }

                _errorHandler.LogInfo("MENU: Pushing OptionsState onto stack");
                _stateManager.PushState(_optionsState);
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError($"Error opening options menu: {ex.Message}", "MenuState.OpenOptionsMenu");
            }
        }

        public void OpenOptionsWithSection(string sectionName)
        {
            try
            {
                if (_optionsState == null)
                {
                    _errorHandler.HandleError("OptionsState not set in MenuState", "MenuState.OpenOptionsWithSection");
                    return;
                }

                _errorHandler.LogInfo($"MENU: Opening Options menu with section: {sectionName}");

                // Set the section before pushing the state
                _optionsState.SetInitialSection(sectionName);
                _stateManager.PushState(_optionsState);
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError($"Error opening options menu with section: {ex.Message}", "MenuState.OpenOptionsWithSection");
            }
        }

        public void OnEnter()
        {
            try
            {
                _errorHandler.LogInfo("MENU: Menu opened - game paused");
                _keyRepeatTimer = KEY_REPEAT_DELAY; // Prevent immediate key repeat
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError($"Error in MenuState OnEnter: {ex.Message}", "MenuState.OnEnter");
            }
        }

        public void OnExit()
        {
            try
            {
                _errorHandler.LogInfo("MENU: Menu closed - returning to game");
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError($"Error in MenuState OnExit: {ex.Message}", "MenuState.OnExit");
            }
        }

        public void Dispose()
        {
            try
            {
                _pixelTexture?.Dispose();
                _fontSystem?.Dispose();
                _isInitialized = false;
                _errorHandler.LogInfo("MenuState disposed");
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError($"Error disposing MenuState: {ex.Message}", "MenuState.Dispose");
            }
        }
    }

    /// <summary>
    /// Menu button data structure
    /// </summary>
    public class MenuButton
    {
        public string Text { get; }
        public MenuAction Action { get; }

        public MenuButton(string text, MenuAction action)
        {
            Text = text;
            Action = action;
        }
    }

    /// <summary>
    /// Available menu actions
    /// </summary>
    public enum MenuAction
    {
        Resume,
        Options,
        Exit
    }
}
