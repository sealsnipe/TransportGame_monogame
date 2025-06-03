using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Numerics;
using TransportGame.Game.Constants;
using TransportGame.Game.Managers;

namespace TransportGame.Game.Systems;

/// <summary>
/// Handles camera movement, zoom, and view transformations.
/// </summary>
public class CameraSystem
{
    private readonly EventBus _eventBus;
    
    // Camera properties
    public System.Numerics.Vector2 Position { get; private set; }
    public float Zoom { get; private set; } = 0.5f; // Start zoomed out for larger map
    public Matrix Transform { get; private set; }

    // Camera settings
    private readonly float _minZoom = 0.25f; // Allow more zoom out for larger map
    private readonly float _maxZoom = 8.0f;
    private readonly float _zoomSpeed = 0.1f;
    private readonly float _panSpeed = 300.0f;
    
    // Screen properties
    private int _screenWidth;
    private int _screenHeight;
    
    // World bounds
    private readonly float _worldWidth;
    private readonly float _worldHeight;
    
    // Follow target
    private System.Numerics.Vector2? _followTarget = null;
    private bool _isFollowing = false;
    private float _followSmoothness = 5.0f;
    
    // Input tracking
    private bool _isDragging = false;
    private System.Numerics.Vector2 _lastMousePosition;

    public CameraSystem(EventBus eventBus, int screenWidth, int screenHeight)
    {
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _screenWidth = screenWidth;
        _screenHeight = screenHeight;
        
        _worldWidth = GameConstants.WORLD_WIDTH * GameConstants.TILE_SIZE;
        _worldHeight = GameConstants.WORLD_HEIGHT * GameConstants.TILE_SIZE;
        
        // Start camera in center of world
        Position = new System.Numerics.Vector2(_worldWidth / 2f, _worldHeight / 2f);
        
        UpdateTransform();
        
        // Subscribe to input events
        _eventBus.KeyPressed += OnKeyPressed;
        _eventBus.MouseClicked += OnMouseClicked;
        _eventBus.MouseRightClicked += OnMouseRightClicked;
        
        Console.WriteLine($"CameraSystem initialized - World: {_worldWidth}x{_worldHeight}, Screen: {_screenWidth}x{_screenHeight}");
    }

    /// <summary>
    /// Updates the camera system.
    /// </summary>
    public void Update(GameTime gameTime, InputSystem inputSystem)
    {
        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        
        // Handle keyboard input for camera movement
        HandleKeyboardInput(deltaTime, inputSystem);
        
        // Handle mouse input for dragging and zooming
        HandleMouseInput(deltaTime, inputSystem);
        
        // Handle follow target
        if (_isFollowing && _followTarget.HasValue)
        {
            var targetPos = _followTarget.Value;
            Position = System.Numerics.Vector2.Lerp(Position, targetPos, deltaTime * _followSmoothness);
        }
        
        // Clamp camera to world bounds
        ClampToWorldBounds();
        
        // Update transform matrix
        UpdateTransform();
    }

    private void HandleKeyboardInput(float deltaTime, InputSystem inputSystem)
    {
        var moveSpeed = _panSpeed * deltaTime / Zoom; // Move slower when zoomed in
        var moved = false;
        
        // WASD movement
        if (inputSystem.IsKeyDown(Keys.W) || inputSystem.IsKeyDown(Keys.Up))
        {
            Position = new System.Numerics.Vector2(Position.X, Position.Y - moveSpeed);
            moved = true;
        }
        if (inputSystem.IsKeyDown(Keys.S) || inputSystem.IsKeyDown(Keys.Down))
        {
            Position = new System.Numerics.Vector2(Position.X, Position.Y + moveSpeed);
            moved = true;
        }
        if (inputSystem.IsKeyDown(Keys.A) || inputSystem.IsKeyDown(Keys.Left))
        {
            Position = new System.Numerics.Vector2(Position.X - moveSpeed, Position.Y);
            moved = true;
        }
        if (inputSystem.IsKeyDown(Keys.D) || inputSystem.IsKeyDown(Keys.Right))
        {
            Position = new System.Numerics.Vector2(Position.X + moveSpeed, Position.Y);
            moved = true;
        }
        
        // Stop following if manually moved
        if (moved)
        {
            _isFollowing = false;
        }
    }

    private void HandleMouseInput(float deltaTime, InputSystem inputSystem)
    {
        var mousePos = inputSystem.GetMousePosition();

        // Handle mouse wheel zoom
        var scrollDelta = inputSystem.GetMouseScrollDelta();
        if (scrollDelta != 0)
        {
            HandleZoom(scrollDelta > 0 ? 1 : -1);
        }

        // Handle mouse dragging (changed to RIGHT mouse button)
        if (inputSystem.IsRightMouseDown())
        {
            if (!_isDragging)
            {
                _isDragging = true;
                _lastMousePosition = mousePos;
                Console.WriteLine("[CAMERA] Started dragging with right mouse button");
            }
            else
            {
                var delta = mousePos - _lastMousePosition;
                // Invert delta and scale by zoom
                var worldDelta = new System.Numerics.Vector2(-delta.X / Zoom, -delta.Y / Zoom);
                Position += worldDelta;
                _lastMousePosition = mousePos;

                // Stop following when dragging
                _isFollowing = false;
            }
        }
        else
        {
            if (_isDragging)
            {
                Console.WriteLine("[CAMERA] Stopped dragging");
            }
            _isDragging = false;
        }
    }

    /// <summary>
    /// Handles zoom input from mouse wheel.
    /// </summary>
    public void HandleZoom(float zoomDelta)
    {
        var oldZoom = Zoom;
        
        // Apply zoom
        if (zoomDelta > 0)
        {
            Zoom = Math.Min(_maxZoom, Zoom + _zoomSpeed);
        }
        else if (zoomDelta < 0)
        {
            Zoom = Math.Max(_minZoom, Zoom - _zoomSpeed);
        }
        
        // If zoom changed, update transform
        if (Math.Abs(Zoom - oldZoom) > 0.001f)
        {
            UpdateTransform();
            Console.WriteLine($"Camera zoom: {Zoom:F2}x");
        }
    }

    /// <summary>
    /// Sets a target for the camera to follow.
    /// </summary>
    public void SetFollowTarget(System.Numerics.Vector2 target)
    {
        _followTarget = target;
        _isFollowing = true;
    }

    /// <summary>
    /// Stops following the current target.
    /// </summary>
    public void StopFollowing()
    {
        _isFollowing = false;
        _followTarget = null;
    }

    /// <summary>
    /// Centers the camera on a specific world position.
    /// </summary>
    public void CenterOn(System.Numerics.Vector2 worldPosition)
    {
        Position = worldPosition;
        _isFollowing = false;
        ClampToWorldBounds();
        UpdateTransform();
    }

    /// <summary>
    /// Converts screen coordinates to world coordinates.
    /// </summary>
    public System.Numerics.Vector2 ScreenToWorld(System.Numerics.Vector2 screenPosition)
    {
        var screenPos = new Microsoft.Xna.Framework.Vector2(screenPosition.X, screenPosition.Y);
        var worldPos = Microsoft.Xna.Framework.Vector2.Transform(screenPos, Matrix.Invert(Transform));
        return new System.Numerics.Vector2(worldPos.X, worldPos.Y);
    }

    /// <summary>
    /// Converts world coordinates to screen coordinates.
    /// </summary>
    public System.Numerics.Vector2 WorldToScreen(System.Numerics.Vector2 worldPosition)
    {
        var worldPos = new Microsoft.Xna.Framework.Vector2(worldPosition.X, worldPosition.Y);
        var screenPos = Microsoft.Xna.Framework.Vector2.Transform(worldPos, Transform);
        return new System.Numerics.Vector2(screenPos.X, screenPos.Y);
    }

    private void ClampToWorldBounds()
    {
        // Calculate visible area
        var visibleWidth = _screenWidth / Zoom;
        var visibleHeight = _screenHeight / Zoom;

        // If the visible area is larger than the world, center the camera
        if (visibleWidth >= _worldWidth)
        {
            Position = new System.Numerics.Vector2(_worldWidth / 2f, Position.Y);
        }
        else
        {
            // Clamp X position to keep camera within world bounds
            var minX = visibleWidth / 2f;
            var maxX = _worldWidth - visibleWidth / 2f;
            Position = new System.Numerics.Vector2(
                Math.Clamp(Position.X, minX, maxX),
                Position.Y
            );
        }

        if (visibleHeight >= _worldHeight)
        {
            Position = new System.Numerics.Vector2(Position.X, _worldHeight / 2f);
        }
        else
        {
            // Clamp Y position to keep camera within world bounds
            var minY = visibleHeight / 2f;
            var maxY = _worldHeight - visibleHeight / 2f;
            Position = new System.Numerics.Vector2(
                Position.X,
                Math.Clamp(Position.Y, minY, maxY)
            );
        }
    }

    private void UpdateTransform()
    {
        // Create transformation matrix
        Transform = Matrix.CreateTranslation(-Position.X, -Position.Y, 0) *
                   Matrix.CreateScale(Zoom, Zoom, 1) *
                   Matrix.CreateTranslation(_screenWidth / 2f, _screenHeight / 2f, 0);
    }

    private void OnKeyPressed(string key)
    {
        switch (key)
        {
            case "zoom_in":
                HandleZoom(1);
                break;
                
            case "zoom_out":
                HandleZoom(-1);
                break;
                
            case "camera_home":
                CenterOn(new System.Numerics.Vector2(_worldWidth / 2f, _worldHeight / 2f));
                Console.WriteLine("Camera centered on world");
                break;
                
            case "camera_follow_train":
                // This will be called from outside when we have a train reference
                break;
        }
    }

    private void OnMouseClicked(System.Numerics.Vector2 position)
    {
        // Convert to world coordinates for debugging
        var worldPos = ScreenToWorld(position);
        var gridX = (int)(worldPos.X / GameConstants.TILE_SIZE);
        var gridY = (int)(worldPos.Y / GameConstants.TILE_SIZE);
        
        Console.WriteLine($"Clicked: Screen({position.X:F0}, {position.Y:F0}) -> World({worldPos.X:F1}, {worldPos.Y:F1}) -> Grid({gridX}, {gridY})");
    }

    private void OnMouseRightClicked(System.Numerics.Vector2 position)
    {
        // Right click to center camera
        var worldPos = ScreenToWorld(position);
        CenterOn(worldPos);
        Console.WriteLine($"Camera centered on {worldPos.X:F1}, {worldPos.Y:F1}");
    }

    /// <summary>
    /// Gets camera information for debugging.
    /// </summary>
    public string GetDebugInfo()
    {
        return $"Cam: ({Position.X:F0}, {Position.Y:F0}) Zoom: {Zoom:F2}x" +
               $"{(_isFollowing ? " [FOLLOWING]" : "")}";
    }

    public void Dispose()
    {
        _eventBus.KeyPressed -= OnKeyPressed;
        _eventBus.MouseClicked -= OnMouseClicked;
        _eventBus.MouseRightClicked -= OnMouseRightClicked;
    }
}
