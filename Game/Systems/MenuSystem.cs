using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using TransportGame.Game.Managers;
using TransportGame.Game.Utils;

namespace TransportGame.Game.Systems;

/// <summary>
/// Handles the F10 menu system with main menu and options.
/// </summary>
public class MenuSystem : IDisposable
{
    private readonly EventBus _eventBus;
    private readonly ErrorHandler _errorHandler;
    
    // Menu state
    private bool _isMenuVisible = false;
    private bool _isOptionsVisible = false;
    private MenuState _currentState = MenuState.Hidden;
    
    // Menu layout
    private const int MENU_WIDTH = 400;
    private const int MENU_HEIGHT = 300;
    private const int OPTIONS_WIDTH = 600;
    private const int OPTIONS_HEIGHT = 400;
    private const int BUTTON_HEIGHT = 40;
    private const int BUTTON_SPACING = 10;
    private const int MENU_PADDING = 20;
    
    // Button tracking
    private List<MenuButton> _mainMenuButtons = new();
    private List<MenuButton> _optionsButtons = new();
    private List<MenuButton> _optionsNavButtons = new();
    private string _selectedOptionsTab = "Controls";
    
    // Mouse state
    private MouseState _previousMouseState;
    private MouseState _currentMouseState;
    
    public MenuSystem(EventBus eventBus, ErrorHandler errorHandler)
    {
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
        
        // Subscribe to events
        _eventBus.KeyPressed += OnKeyPressed;
        
        InitializeButtons();
        
        Console.WriteLine("MenuSystem initialized");
    }
    
    private void InitializeButtons()
    {
        // Main menu buttons
        _mainMenuButtons.Clear();
        _mainMenuButtons.Add(new MenuButton("Weiter", "continue", 0));
        _mainMenuButtons.Add(new MenuButton("Optionen", "options", 1));
        _mainMenuButtons.Add(new MenuButton("Beenden", "exit", 2));
        
        // Options navigation buttons
        _optionsNavButtons.Clear();
        _optionsNavButtons.Add(new MenuButton("Display", "nav_display", 0));
        _optionsNavButtons.Add(new MenuButton("Controls", "nav_controls", 1));
        _optionsNavButtons.Add(new MenuButton("Audio", "nav_audio", 2));
        
        // Options action buttons
        _optionsButtons.Clear();
        _optionsButtons.Add(new MenuButton("Zurück", "back", 0));
    }
    
    public void Update(GameTime gameTime)
    {
        if (!_isMenuVisible && !_isOptionsVisible)
            return;
            
        // Update mouse state
        _previousMouseState = _currentMouseState;
        _currentMouseState = Mouse.GetState();
        
        // Handle mouse clicks
        if (_currentMouseState.LeftButton == ButtonState.Pressed && 
            _previousMouseState.LeftButton == ButtonState.Released)
        {
            HandleMouseClick();
        }
    }
    
    private void HandleMouseClick()
    {
        var mousePos = new System.Numerics.Vector2(_currentMouseState.X, _currentMouseState.Y);
        
        if (_isOptionsVisible)
        {
            // Check options navigation buttons
            foreach (var button in _optionsNavButtons)
            {
                if (IsPointInButton(mousePos, button))
                {
                    HandleOptionsNavClick(button.Action);
                    return;
                }
            }
            
            // Check options action buttons
            foreach (var button in _optionsButtons)
            {
                if (IsPointInButton(mousePos, button))
                {
                    HandleOptionsActionClick(button.Action);
                    return;
                }
            }
        }
        else if (_isMenuVisible)
        {
            // Check main menu buttons
            foreach (var button in _mainMenuButtons)
            {
                if (IsPointInButton(mousePos, button))
                {
                    HandleMainMenuClick(button.Action);
                    return;
                }
            }
        }
    }
    
    private bool IsPointInButton(System.Numerics.Vector2 point, MenuButton button)
    {
        var rect = GetButtonRect(button);
        return point.X >= rect.X && point.X <= rect.X + rect.Width &&
               point.Y >= rect.Y && point.Y <= rect.Y + rect.Height;
    }
    
    private Rectangle GetButtonRect(MenuButton button)
    {
        if (_isOptionsVisible)
        {
            if (_optionsNavButtons.Contains(button))
            {
                // Navigation buttons on the left
                var screenWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                var screenHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
                var menuX = (screenWidth - OPTIONS_WIDTH) / 2;
                var menuY = (screenHeight - OPTIONS_HEIGHT) / 2;
                
                return new Rectangle(
                    menuX + MENU_PADDING,
                    menuY + MENU_PADDING + button.Index * (BUTTON_HEIGHT + BUTTON_SPACING),
                    120,
                    BUTTON_HEIGHT
                );
            }
            else
            {
                // Action buttons at bottom
                var screenWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                var screenHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
                var menuX = (screenWidth - OPTIONS_WIDTH) / 2;
                var menuY = (screenHeight - OPTIONS_HEIGHT) / 2;
                
                return new Rectangle(
                    menuX + OPTIONS_WIDTH - 120 - MENU_PADDING,
                    menuY + OPTIONS_HEIGHT - BUTTON_HEIGHT - MENU_PADDING,
                    120,
                    BUTTON_HEIGHT
                );
            }
        }
        else
        {
            // Main menu buttons
            var screenWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            var screenHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            var menuX = (screenWidth - MENU_WIDTH) / 2;
            var menuY = (screenHeight - MENU_HEIGHT) / 2;
            
            return new Rectangle(
                menuX + MENU_PADDING,
                menuY + MENU_PADDING + button.Index * (BUTTON_HEIGHT + BUTTON_SPACING),
                MENU_WIDTH - 2 * MENU_PADDING,
                BUTTON_HEIGHT
            );
        }
    }
    
    private void HandleMainMenuClick(string action)
    {
        Console.WriteLine($"MenuSystem: Main menu action: {action}");
        
        switch (action)
        {
            case "continue":
                HideMenu();
                break;
                
            case "options":
                ShowOptions();
                break;
                
            case "exit":
                _eventBus.EmitKeyPressed("game_exit");
                break;
        }
    }
    
    private void HandleOptionsNavClick(string action)
    {
        Console.WriteLine($"MenuSystem: Options nav action: {action}");
        
        switch (action)
        {
            case "nav_display":
                _selectedOptionsTab = "Display";
                break;
                
            case "nav_controls":
                _selectedOptionsTab = "Controls";
                break;
                
            case "nav_audio":
                _selectedOptionsTab = "Audio";
                break;
        }
    }
    
    private void HandleOptionsActionClick(string action)
    {
        Console.WriteLine($"MenuSystem: Options action: {action}");
        
        switch (action)
        {
            case "back":
                ShowMainMenu();
                break;
        }
    }
    
    public void Draw(SpriteBatch spriteBatch, Texture2D pixelTexture)
    {
        if (!_isMenuVisible && !_isOptionsVisible)
            return;
            
        if (_isOptionsVisible)
        {
            DrawOptionsMenu(spriteBatch, pixelTexture);
        }
        else if (_isMenuVisible)
        {
            DrawMainMenu(spriteBatch, pixelTexture);
        }
    }
    
    private void DrawMainMenu(SpriteBatch spriteBatch, Texture2D pixelTexture)
    {
        // Removed log spam - this gets called every frame!

        // Use Viewport instead of DisplayMode (ChatGPT fix)
        var viewport = spriteBatch.GraphicsDevice.Viewport;
        var screenWidth = viewport.Width;
        var screenHeight = viewport.Height;

        // Screen size logging removed - was spamming console every frame

        // Draw semi-transparent overlay
        var overlayRect = new Rectangle(0, 0, screenWidth, screenHeight);
        spriteBatch.Draw(pixelTexture, overlayRect, Color.Black * 0.7f);
        // Overlay logging removed - was spamming console every frame

        // Draw menu background
        var menuX = (screenWidth - MENU_WIDTH) / 2;
        var menuY = (screenHeight - MENU_HEIGHT) / 2;
        var menuRect = new Rectangle(menuX, menuY, MENU_WIDTH, MENU_HEIGHT);
        spriteBatch.Draw(pixelTexture, menuRect, Color.DarkGray);

        // Draw menu border
        DrawBorder(spriteBatch, pixelTexture, menuRect, Color.White, 2);

        // Draw buttons
        foreach (var button in _mainMenuButtons)
        {
            // Button drawing logging removed - was spamming console every frame
            DrawButton(spriteBatch, pixelTexture, button);
        }

        // DrawMainMenu logging removed - was spamming console every frame
    }
    
    private void DrawOptionsMenu(SpriteBatch spriteBatch, Texture2D pixelTexture)
    {
        // Use Viewport instead of DisplayMode (ChatGPT fix)
        var viewport = spriteBatch.GraphicsDevice.Viewport;
        var screenWidth = viewport.Width;
        var screenHeight = viewport.Height;
        
        // Draw semi-transparent overlay
        var overlayRect = new Rectangle(0, 0, screenWidth, screenHeight);
        spriteBatch.Draw(pixelTexture, overlayRect, Color.Black * 0.7f);
        
        // Draw options background
        var menuX = (screenWidth - OPTIONS_WIDTH) / 2;
        var menuY = (screenHeight - OPTIONS_HEIGHT) / 2;
        var menuRect = new Rectangle(menuX, menuY, OPTIONS_WIDTH, OPTIONS_HEIGHT);
        spriteBatch.Draw(pixelTexture, menuRect, Color.DarkGray);
        
        // Draw menu border
        DrawBorder(spriteBatch, pixelTexture, menuRect, Color.White, 2);
        
        // Draw navigation buttons
        foreach (var button in _optionsNavButtons)
        {
            var isSelected = (_selectedOptionsTab == button.Text);
            DrawButton(spriteBatch, pixelTexture, button, isSelected);
        }
        
        // Draw content area
        DrawOptionsContent(spriteBatch, pixelTexture, menuX, menuY);
        
        // Draw action buttons
        foreach (var button in _optionsButtons)
        {
            DrawButton(spriteBatch, pixelTexture, button);
        }
    }
    
    private void DrawOptionsContent(SpriteBatch spriteBatch, Texture2D pixelTexture, int menuX, int menuY)
    {
        var contentX = menuX + 150; // After navigation area
        var contentY = menuY + MENU_PADDING;
        var contentWidth = OPTIONS_WIDTH - 170;
        var contentHeight = OPTIONS_HEIGHT - 80;
        
        // Draw content background
        var contentRect = new Rectangle(contentX, contentY, contentWidth, contentHeight);
        spriteBatch.Draw(pixelTexture, contentRect, Color.Gray);
        
        // Draw content based on selected tab
        switch (_selectedOptionsTab)
        {
            case "Display":
                DrawDisplayOptions(spriteBatch, pixelTexture, contentRect);
                break;
                
            case "Controls":
                DrawControlsOptions(spriteBatch, pixelTexture, contentRect);
                break;
                
            case "Audio":
                DrawAudioOptions(spriteBatch, pixelTexture, contentRect);
                break;
        }
    }
    
    private void DrawDisplayOptions(SpriteBatch spriteBatch, Texture2D pixelTexture, Rectangle contentRect)
    {
        // Placeholder for display options
        var textRect = new Rectangle(contentRect.X + 10, contentRect.Y + 10, 200, 20);
        spriteBatch.Draw(pixelTexture, textRect, Color.Yellow);
    }
    
    private void DrawControlsOptions(SpriteBatch spriteBatch, Texture2D pixelTexture, Rectangle contentRect)
    {
        // Draw key bindings list
        var y = contentRect.Y + 10;
        var lineHeight = 25;
        
        var keyBindings = new[]
        {
            "WASD / Pfeiltasten: Kamera bewegen",
            "Mausrad: Zoom",
            "Rechtsklick + Drag: Kamera bewegen",
            "Linksklick: Tooltip anzeigen",
            "ESC: Tooltip ausblenden",
            "F1: Debug Info ein/aus",
            "F2: Grid ein/aus",
            "M: Menü öffnen",
            "Home: Kamera zentrieren",
            "F: Zug verfolgen"
        };
        
        for (int i = 0; i < keyBindings.Length && i < 12; i++)
        {
            var lineRect = new Rectangle(contentRect.X + 10, y + i * lineHeight, 300, 15);
            spriteBatch.Draw(pixelTexture, lineRect, Color.LightGray);
        }
    }
    
    private void DrawAudioOptions(SpriteBatch spriteBatch, Texture2D pixelTexture, Rectangle contentRect)
    {
        // Placeholder for audio options
        var textRect = new Rectangle(contentRect.X + 10, contentRect.Y + 10, 200, 20);
        spriteBatch.Draw(pixelTexture, textRect, Color.Green);
    }

    private void DrawButton(SpriteBatch spriteBatch, Texture2D pixelTexture, MenuButton button, bool isSelected = false)
    {
        var rect = GetButtonRect(button);
        var buttonColor = isSelected ? Color.DarkBlue : Color.Gray;
        var borderColor = isSelected ? Color.Yellow : Color.White;

        // Draw button background
        spriteBatch.Draw(pixelTexture, rect, buttonColor);

        // Draw button border
        DrawBorder(spriteBatch, pixelTexture, rect, borderColor, 1);

        // Draw button text placeholder (colored rectangle)
        var textRect = new Rectangle(rect.X + 5, rect.Y + 5, rect.Width - 10, rect.Height - 10);
        spriteBatch.Draw(pixelTexture, textRect, Color.White);
    }

    private void DrawBorder(SpriteBatch spriteBatch, Texture2D pixelTexture, Rectangle rect, Color color, int thickness)
    {
        // Top
        spriteBatch.Draw(pixelTexture, new Rectangle(rect.X, rect.Y, rect.Width, thickness), color);
        // Bottom
        spriteBatch.Draw(pixelTexture, new Rectangle(rect.X, rect.Y + rect.Height - thickness, rect.Width, thickness), color);
        // Left
        spriteBatch.Draw(pixelTexture, new Rectangle(rect.X, rect.Y, thickness, rect.Height), color);
        // Right
        spriteBatch.Draw(pixelTexture, new Rectangle(rect.X + rect.Width - thickness, rect.Y, thickness, rect.Height), color);
    }

    private void OnKeyPressed(string key)
    {
        Console.WriteLine($"*** MenuSystem: Received key event: '{key}' ***");

        switch (key)
        {
            case "menu_toggle":
            case "M":
            case "F10":
                // TAB removed - now handled by new MenuState system
                ToggleMenu();
                break;

            case "Escape":
                if (_isOptionsVisible)
                {
                    ShowMainMenu();
                }
                else if (_isMenuVisible)
                {
                    HideMenu();
                }
                break;
        }
    }

    public void ToggleMenu()
    {
        Console.WriteLine($"*** MenuSystem: ToggleMenu called! Current state - Menu: {_isMenuVisible}, Options: {_isOptionsVisible} ***");

        if (_isMenuVisible || _isOptionsVisible)
        {
            Console.WriteLine("*** MenuSystem: Hiding menu ***");
            HideMenu();
        }
        else
        {
            Console.WriteLine("*** MenuSystem: Showing main menu ***");
            ShowMainMenu();
        }

        Console.WriteLine($"*** MenuSystem: After toggle - Menu: {_isMenuVisible}, Options: {_isOptionsVisible}, IsMenuVisible: {IsMenuVisible} ***");
    }

    public void ShowMainMenu()
    {
        _isMenuVisible = true;
        _isOptionsVisible = false;
        _currentState = MenuState.MainMenu;
        Console.WriteLine("*** MENU OPENED ***");
        FileLogger.Log("MENU OPENED - ShowMainMenu() called");
    }

    public void ShowOptions()
    {
        _isMenuVisible = false;
        _isOptionsVisible = true;
        _currentState = MenuState.Options;
        Console.WriteLine("MenuSystem: Options menu shown");
    }

    public void HideMenu()
    {
        _isMenuVisible = false;
        _isOptionsVisible = false;
        _currentState = MenuState.Hidden;
        Console.WriteLine("*** MENU CLOSED ***");
        FileLogger.Log("MENU CLOSED - HideMenu() called");
    }

    public bool IsMenuVisible => _isMenuVisible || _isOptionsVisible;

    public void Dispose()
    {
        _eventBus.KeyPressed -= OnKeyPressed;
    }
}

/// <summary>
/// Represents a menu button.
/// </summary>
public class MenuButton
{
    public string Text { get; }
    public string Action { get; }
    public int Index { get; }

    public MenuButton(string text, string action, int index)
    {
        Text = text;
        Action = action;
        Index = index;
    }
}

/// <summary>
/// Menu state enumeration.
/// </summary>
public enum MenuState
{
    Hidden,
    MainMenu,
    Options
}
