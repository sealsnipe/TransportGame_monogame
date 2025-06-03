using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Numerics;
using TransportGame.Game.Managers;
using TransportGame.Game.Utils;

namespace TransportGame.Game.Systems;

/// <summary>
/// Handles all input processing and converts it to game events.
/// </summary>
public class InputSystem : IDisposable
{
    private readonly EventBus _eventBus;

    // Input state tracking
    private KeyboardState _previousKeyboardState;
    private KeyboardState _currentKeyboardState;

    // Menu system reference for direct access
    private MenuSystem? _menuSystem;
    private MouseState _previousMouseState;
    private MouseState _currentMouseState;
    
    // Mouse tracking
    private System.Numerics.Vector2 _mousePosition;
    private bool _isMouseDragging = false;
    private System.Numerics.Vector2 _dragStartPosition;

    // Debug tracking
    private float _debugTimer = 0f;

    public InputSystem(EventBus eventBus)
    {
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

        Console.WriteLine("InputSystem: Constructor called");

        // Initialize input states
        _currentKeyboardState = Keyboard.GetState();
        _previousKeyboardState = _currentKeyboardState;
        _currentMouseState = Mouse.GetState();
        _previousMouseState = _currentMouseState;

        Console.WriteLine("InputSystem: Initialized successfully");
    }

    /// <summary>
    /// Sets the menu system reference for direct menu control.
    /// </summary>
    public void SetMenuSystem(MenuSystem menuSystem)
    {
        _menuSystem = menuSystem;
        Console.WriteLine("InputSystem: MenuSystem reference set");
    }

    /// <summary>
    /// Updates input system and processes input events.
    /// </summary>
    public void Update(GameTime gameTime)
    {
        // Removed frame-by-frame logging - too much spam!

        // Update input states
        _previousKeyboardState = _currentKeyboardState;
        _previousMouseState = _currentMouseState;
        _currentKeyboardState = Keyboard.GetState();
        _currentMouseState = Mouse.GetState();

        // Update mouse position
        _mousePosition = new System.Numerics.Vector2(_currentMouseState.X, _currentMouseState.Y);

        // Process input
        ProcessKeyboardInput();
        ProcessMouseInput();

        // DIRECT MENU CONTROL (like WASD camera control)
        ProcessDirectMenuInput();
    }

    private void ProcessKeyboardInput()
    {
        // Check for key presses (key was up, now down)
        var pressedKeys = _currentKeyboardState.GetPressedKeys()
            .Where(key => !_previousKeyboardState.IsKeyDown(key));

        foreach (var key in pressedKeys)
        {
            HandleKeyPress(key);
        }
    }

    private void ProcessMouseInput()
    {
        // Left mouse button
        if (_currentMouseState.LeftButton == ButtonState.Pressed && 
            _previousMouseState.LeftButton == ButtonState.Released)
        {
            HandleLeftMousePress();
        }
        else if (_currentMouseState.LeftButton == ButtonState.Released && 
                 _previousMouseState.LeftButton == ButtonState.Pressed)
        {
            HandleLeftMouseRelease();
        }
        
        // Right mouse button
        if (_currentMouseState.RightButton == ButtonState.Pressed && 
            _previousMouseState.RightButton == ButtonState.Released)
        {
            HandleRightMousePress();
        }
        
        // Mouse wheel
        var scrollDelta = _currentMouseState.ScrollWheelValue - _previousMouseState.ScrollWheelValue;
        if (scrollDelta != 0)
        {
            HandleMouseScroll(scrollDelta);
        }
        
        // Mouse movement
        if (_mousePosition != new System.Numerics.Vector2(_previousMouseState.X, _previousMouseState.Y))
        {
            HandleMouseMove();
        }
    }

    private void HandleKeyPress(Microsoft.Xna.Framework.Input.Keys key)
    {
        var keyString = key.ToString();
        _eventBus.EmitKeyPressed(keyString);

        // Handle specific key actions - simplified for now
        var keyName = key.ToString();

        // Map common keys to actions
        switch (keyName)
        {
            case "Escape":
                // Handled in main game loop
                break;

            case "Space":
                _eventBus.EmitKeyPressed("pause_toggle");
                break;

            case "F12":
                Console.WriteLine("InputSystem: F12 pressed, emitting debug_toggle");
                _eventBus.EmitKeyPressed("debug_toggle");
                break;

            case "F2":
                Console.WriteLine("InputSystem: F2 pressed, emitting grid_toggle");
                _eventBus.EmitKeyPressed("grid_toggle");
                break;

            case "G":
                Console.WriteLine("InputSystem: G pressed, emitting grid_toggle");
                _eventBus.EmitKeyPressed("grid_toggle");
                break;

            // Remove D key debug toggle to avoid WASD conflict
            // case "D1":
            // case "D":
            //     Console.WriteLine("InputSystem: D pressed, emitting debug_toggle");
            //     _eventBus.EmitKeyPressed("debug_toggle");
            //     break;

            case "M":
                Console.WriteLine("InputSystem: M pressed, emitting menu_toggle");
                _eventBus.EmitKeyPressed("menu_toggle");
                break;

            case "F10":
                Console.WriteLine("InputSystem: F10 pressed, emitting menu_toggle");
                _eventBus.EmitKeyPressed("menu_toggle");
                break;

            case "Tab":
                // TAB is now handled directly by PlayingState via IsKeyPressed() - no EventBus needed
                // Console.WriteLine("InputSystem: TAB pressed, emitting menu_toggle");
                // _eventBus.EmitKeyPressed("menu_toggle");
                break;

            case "Home":
                _eventBus.EmitKeyPressed("camera_home");
                break;

            case "F":
                _eventBus.EmitKeyPressed("camera_follow_train");
                break;
        }
    }

    private void HandleLeftMousePress()
    {
        _eventBus.EmitMouseClicked(_mousePosition);
        
        // Start drag tracking
        _isMouseDragging = false;
        _dragStartPosition = _mousePosition;
    }

    private void HandleLeftMouseRelease()
    {
        if (_isMouseDragging)
        {
            // End drag operation
            _isMouseDragging = false;
            _eventBus.EmitKeyPressed("drag_end");
        }
    }

    private void HandleRightMousePress()
    {
        _eventBus.EmitMouseRightClicked(_mousePosition);
    }

    private void HandleMouseScroll(int scrollDelta)
    {
        if (scrollDelta > 0)
        {
            _eventBus.EmitKeyPressed("zoom_in");
        }
        else if (scrollDelta < 0)
        {
            _eventBus.EmitKeyPressed("zoom_out");
        }
    }

    /// <summary>
    /// Gets the mouse scroll delta for this frame.
    /// </summary>
    public int GetMouseScrollDelta()
    {
        return _currentMouseState.ScrollWheelValue - _previousMouseState.ScrollWheelValue;
    }

    /// <summary>
    /// Gets the previous mouse state for click detection.
    /// </summary>
    public MouseState GetPreviousMouseState()
    {
        return _previousMouseState;
    }

    private void HandleMouseMove()
    {
        // Check if we should start dragging
        if (_currentMouseState.LeftButton == ButtonState.Pressed && !_isMouseDragging)
        {
            var dragDistance = System.Numerics.Vector2.Distance(_mousePosition, _dragStartPosition);
            if (dragDistance > 5f) // Minimum drag distance
            {
                _isMouseDragging = true;
                _eventBus.EmitKeyPressed("drag_start");
            }
        }
        
        // Emit mouse move event
        // Note: This could be very frequent, so we might want to throttle it
    }

    /// <summary>
    /// Gets the current mouse position in screen coordinates.
    /// </summary>
    public System.Numerics.Vector2 GetMousePosition()
    {
        return _mousePosition;
    }

    /// <summary>
    /// Gets the current mouse position in world coordinates.
    /// </summary>
    public System.Numerics.Vector2 GetWorldMousePosition(Matrix cameraTransform)
    {
        // Convert screen coordinates to world coordinates using camera transform
        var screenPos = new Microsoft.Xna.Framework.Vector2(_mousePosition.X, _mousePosition.Y);
        var worldPos = Microsoft.Xna.Framework.Vector2.Transform(screenPos, Matrix.Invert(cameraTransform));
        return new System.Numerics.Vector2(worldPos.X, worldPos.Y);
    }

    /// <summary>
    /// Checks if a specific key is currently pressed.
    /// </summary>
    public bool IsKeyDown(Microsoft.Xna.Framework.Input.Keys key)
    {
        return _currentKeyboardState.IsKeyDown(key);
    }

    /// <summary>
    /// Checks if a specific key was just pressed this frame.
    /// </summary>
    public bool IsKeyPressed(Microsoft.Xna.Framework.Input.Keys key)
    {
        return _currentKeyboardState.IsKeyDown(key) && !_previousKeyboardState.IsKeyDown(key);
    }

    /// <summary>
    /// Checks if the left mouse button is currently pressed.
    /// </summary>
    public bool IsLeftMouseDown()
    {
        return _currentMouseState.LeftButton == ButtonState.Pressed;
    }

    /// <summary>
    /// Checks if the right mouse button is currently pressed.
    /// </summary>
    public bool IsRightMouseDown()
    {
        return _currentMouseState.RightButton == ButtonState.Pressed;
    }

    /// <summary>
    /// Checks if the mouse is currently being dragged.
    /// </summary>
    public bool IsMouseDragging()
    {
        return _isMouseDragging;
    }

    /// <summary>
    /// Gets the drag start position.
    /// </summary>
    public System.Numerics.Vector2 GetDragStartPosition()
    {
        return _dragStartPosition;
    }

    /// <summary>
    /// Gets the current drag delta (current position - start position).
    /// </summary>
    public System.Numerics.Vector2 GetDragDelta()
    {
        return _mousePosition - _dragStartPosition;
    }

    /// <summary>
    /// Direct menu input processing - DISABLED for StateManager integration
    /// Menu keys (F10, M, TAB, ESC) are now handled by StateManager
    /// </summary>
    private void ProcessDirectMenuInput()
    {
        // DISABLED: Menu input is now handled by StateManager
        // F10, M, TAB keys are checked via IsKeyPressed() in PlayingState
        // ESC key is handled in MenuState
        return;
    }

    public void Dispose()
    {
        // No resources to dispose currently
    }
}
