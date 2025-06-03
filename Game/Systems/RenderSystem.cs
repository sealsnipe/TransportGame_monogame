using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TransportGame.Game.Managers;
using TransportGame.Game.Entities;

namespace TransportGame.Game.Systems;

/// <summary>
/// Handles all rendering operations for the game.
/// </summary>
public class RenderSystem : IDisposable
{
    private readonly EventBus _eventBus;
    
    // Graphics resources
    private GraphicsDevice? _graphicsDevice;
    private SpriteFont? _defaultFont;
    private Texture2D? _pixelTexture;
    
    // Debug rendering
    private bool _showDebugInfo = false; // Debug info OFF by default
    private bool _showGrid = false; // Grid standardmäßig AUS für saubere Optik
    
    // Performance tracking
    private int _frameCount = 0;
    private float _frameTime = 0f;
    private float _fps = 0f;

    // Demo objects
    private DemoTrain? _demoTrain;

    public RenderSystem(EventBus eventBus)
    {
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

        Console.WriteLine($"RenderSystem: Constructor - Grid={_showGrid}, Debug={_showDebugInfo}");

        // Subscribe to relevant events
        _eventBus.KeyPressed += OnKeyPressed;
        Console.WriteLine("RenderSystem: Subscribed to KeyPressed events");
    }

    /// <summary>
    /// Loads content for the render system.
    /// </summary>
    public void LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
    {
        _graphicsDevice = graphicsDevice;
        
        try
        {
            // Load default font
            try
            {
                _defaultFont = content.Load<SpriteFont>("DefaultFont");
                Console.WriteLine("RenderSystem: Default font loaded successfully");
            }
            catch (Exception fontEx)
            {
                Console.WriteLine($"RenderSystem: Could not load font: {fontEx.Message}");
                _defaultFont = null;
            }

            // Create a 1x1 white pixel texture for drawing shapes
            _pixelTexture = new Texture2D(_graphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Microsoft.Xna.Framework.Color.White });

            Console.WriteLine("RenderSystem content loaded successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not load some render content: {ex.Message}");
            
            // Create fallback resources
            CreateFallbackResources();
        }

        // Create demo train
        if (_graphicsDevice != null)
        {
            _demoTrain = new DemoTrain(_graphicsDevice.Viewport.Width, _graphicsDevice.Viewport.Height);
        }
    }

    private void CreateFallbackResources()
    {
        // Create a simple 1x1 white texture if we don't have one
        if (_pixelTexture == null && _graphicsDevice != null)
        {
            _pixelTexture = new Texture2D(_graphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Microsoft.Xna.Framework.Color.White });
        }
    }

    /// <summary>
    /// Main draw method called each frame.
    /// </summary>
    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        if (_graphicsDevice == null || _pixelTexture == null)
            return;

        UpdatePerformanceStats(gameTime);

        // Update demo train
        if (_demoTrain != null)
        {
            _demoTrain.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
        }

        // Draw debug grid if enabled
        if (_showGrid)
        {
            DrawGrid(spriteBatch);
        }

        // Draw demo train
        if (_demoTrain != null)
        {
            _demoTrain.Draw(spriteBatch, _pixelTexture);
        }

        // Draw a simple test rectangle to verify rendering works (reduced)
        DrawSimpleTestContent(spriteBatch);

        // Note: Debug info is now drawn in DrawUI method
    }

    /// <summary>
    /// Draws UI elements that should not be affected by camera transform.
    /// </summary>
    public void DrawUI(SpriteBatch spriteBatch, GameTime gameTime, CameraSystem? cameraSystem, TooltipSystem? tooltipSystem = null)
    {
        // Draw debug info if enabled
        if (_showDebugInfo)
        {
            DrawDebugInfo(spriteBatch, cameraSystem);
        }

        // Draw tooltip if available
        if (tooltipSystem != null && _pixelTexture != null)
        {
            tooltipSystem.Draw(spriteBatch, _pixelTexture, _defaultFont);
        }
    }

    private void DrawGrid(SpriteBatch spriteBatch)
    {
        if (_pixelTexture == null || _graphicsDevice == null)
            return;

        var viewport = _graphicsDevice.Viewport;
        var gridColor = Microsoft.Xna.Framework.Color.Gray * 0.2f; // Subtil aber sichtbar
        var tileSize = 5; // From GameConstants.TILE_SIZE

        // Nur jede 10. Linie zeichnen für bessere Sichtbarkeit bei 5px Tiles
        var gridSpacing = tileSize * 10; // Alle 50 Pixel eine Linie

        // Draw vertical lines (every 10th tile)
        for (int x = 0; x < viewport.Width; x += gridSpacing)
        {
            var rect = new Microsoft.Xna.Framework.Rectangle(x, 0, 1, viewport.Height);
            spriteBatch.Draw(_pixelTexture, rect, gridColor);
        }

        // Draw horizontal lines (every 10th tile)
        for (int y = 0; y < viewport.Height; y += gridSpacing)
        {
            var rect = new Microsoft.Xna.Framework.Rectangle(0, y, viewport.Width, 1);
            spriteBatch.Draw(_pixelTexture, rect, gridColor);
        }
    }

    private void DrawDebugInfo(SpriteBatch spriteBatch, CameraSystem? cameraSystem)
    {
        // Draw debug info as simple colored rectangles since we don't have a font
        var y = 10;
        var lineHeight = 20;

        if (_pixelTexture == null)
            return;

        // FPS indicator
        var fpsColor = _fps > 50 ? Microsoft.Xna.Framework.Color.Green :
                      _fps > 30 ? Microsoft.Xna.Framework.Color.Yellow :
                      Microsoft.Xna.Framework.Color.Red;
        var fpsWidth = (int)Math.Min(200, _fps * 4); // Scale FPS to width
        spriteBatch.Draw(_pixelTexture, new Microsoft.Xna.Framework.Rectangle(10, y, fpsWidth, 10), fpsColor);
        y += lineHeight;

        // Grid indicator - show always, different colors for on/off
        var gridColor = _showGrid ? Microsoft.Xna.Framework.Color.Gray : Microsoft.Xna.Framework.Color.DarkRed;
        spriteBatch.Draw(_pixelTexture, new Microsoft.Xna.Framework.Rectangle(10, y, 50, 10), gridColor);
        y += lineHeight;

        // Camera zoom indicator
        if (cameraSystem != null)
        {
            var zoomWidth = (int)(cameraSystem.Zoom * 20); // Scale zoom to width
            var zoomColor = Microsoft.Xna.Framework.Color.Blue;
            spriteBatch.Draw(_pixelTexture, new Microsoft.Xna.Framework.Rectangle(10, y, zoomWidth, 10), zoomColor);
        }

        // Simple text replacement with colored bars
        // FPS logging removed - was spamming console every frame
    }

    private void DrawSimpleTestContent(SpriteBatch spriteBatch)
    {
        if (_pixelTexture == null || _graphicsDevice == null)
            return;

        // Just draw a small indicator in the corner to show rendering works
        var testRect = new Microsoft.Xna.Framework.Rectangle(10, 10, 20, 20);
        spriteBatch.Draw(_pixelTexture, testRect, Microsoft.Xna.Framework.Color.Red);
    }

    private void DrawRectangleOutline(SpriteBatch spriteBatch, Microsoft.Xna.Framework.Rectangle rectangle, Microsoft.Xna.Framework.Color color, int thickness)
    {
        if (_pixelTexture == null)
            return;
            
        // Top
        spriteBatch.Draw(_pixelTexture, new Microsoft.Xna.Framework.Rectangle(rectangle.X, rectangle.Y, rectangle.Width, thickness), color);

        // Bottom
        spriteBatch.Draw(_pixelTexture, new Microsoft.Xna.Framework.Rectangle(rectangle.X, rectangle.Bottom - thickness, rectangle.Width, thickness), color);

        // Left
        spriteBatch.Draw(_pixelTexture, new Microsoft.Xna.Framework.Rectangle(rectangle.X, rectangle.Y, thickness, rectangle.Height), color);

        // Right
        spriteBatch.Draw(_pixelTexture, new Microsoft.Xna.Framework.Rectangle(rectangle.Right - thickness, rectangle.Y, thickness, rectangle.Height), color);
    }

    private void UpdatePerformanceStats(GameTime gameTime)
    {
        _frameCount++;
        _frameTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
        
        // Update FPS every second
        if (_frameTime >= 1000f)
        {
            _fps = _frameCount * 1000f / _frameTime;
            _frameCount = 0;
            _frameTime = 0f;
        }
    }

    private void OnKeyPressed(string key)
    {
        Console.WriteLine($"RenderSystem received key: {key}"); // Debug output

        switch (key)
        {
            case "F12":
            case "debug_toggle":
                _showDebugInfo = !_showDebugInfo;
                Console.WriteLine($"Debug info: {(_showDebugInfo ? "ON" : "OFF")}");
                break;

            case "F2":
            case "grid_toggle":
                _showGrid = !_showGrid;
                Console.WriteLine($"Grid: {(_showGrid ? "ON" : "OFF")}");
                break;
        }
    }

    /// <summary>
    /// Draws a filled rectangle.
    /// </summary>
    public void DrawRectangle(SpriteBatch spriteBatch, Microsoft.Xna.Framework.Rectangle rectangle, Microsoft.Xna.Framework.Color color)
    {
        if (_pixelTexture != null)
        {
            spriteBatch.Draw(_pixelTexture, rectangle, color);
        }
    }

    /// <summary>
    /// Draws a line between two points.
    /// </summary>
    public void DrawLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, Microsoft.Xna.Framework.Color color, int thickness = 1)
    {
        if (_pixelTexture == null)
            return;

        var distance = Vector2.Distance(start, end);
        var angle = (float)Math.Atan2(end.Y - start.Y, end.X - start.X);

        var rectangle = new Microsoft.Xna.Framework.Rectangle(
            (int)start.X,
            (int)start.Y - thickness / 2,
            (int)distance,
            thickness
        );

        spriteBatch.Draw(_pixelTexture, rectangle, null, color, angle, Vector2.Zero, SpriteEffects.None, 0f);
    }

    /// <summary>
    /// Gets the current FPS.
    /// </summary>
    public float GetFPS()
    {
        return _fps;
    }

    /// <summary>
    /// Gets whether debug info is currently shown.
    /// </summary>
    public bool IsDebugInfoVisible()
    {
        return _showDebugInfo;
    }

    /// <summary>
    /// Gets whether the grid is currently shown.
    /// </summary>
    public bool IsGridVisible()
    {
        return _showGrid;
    }

    /// <summary>
    /// Gets the demo train's position for camera following.
    /// </summary>
    public System.Numerics.Vector2? GetDemoTrainPosition()
    {
        if (_demoTrain == null)
            return null;

        // Convert grid position to world position
        return new System.Numerics.Vector2(
            _demoTrain.GridPosition.X * 5f, // TILE_SIZE
            _demoTrain.GridPosition.Y * 5f
        );
    }

    /// <summary>
    /// Gets the pixel texture for drawing UI elements.
    /// </summary>
    public Texture2D? GetPixelTexture()
    {
        return _pixelTexture;
    }

    public void Dispose()
    {
        // Unsubscribe from events
        _eventBus.KeyPressed -= OnKeyPressed;
        
        // Dispose graphics resources
        _pixelTexture?.Dispose();
        
        Console.WriteLine("RenderSystem disposed");
    }
}
