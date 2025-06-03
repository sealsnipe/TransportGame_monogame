using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Numerics;
using TransportGame.Game.Constants;

namespace TransportGame.Game.Entities;

/// <summary>
/// A simple demo train that moves around the screen for proof of concept.
/// </summary>
public class DemoTrain
{
    public System.Numerics.Vector2 GridPosition { get; set; }
    public System.Numerics.Vector2 TargetGridPosition { get; set; }
    public Microsoft.Xna.Framework.Color Color { get; set; }
    public float Size { get; set; } = 5f; // One tile size

    private readonly float _moveSpeed = 2f; // tiles per second
    private readonly int _worldWidth;
    private readonly int _worldHeight;
    private float _moveProgress = 0f;
    private bool _isMoving = false;
    
    public DemoTrain(int screenWidth, int screenHeight)
    {
        _worldWidth = GameConstants.WORLD_WIDTH;   // Use actual world size from constants
        _worldHeight = GameConstants.WORLD_HEIGHT;

        // Start in center of world (in grid coordinates)
        GridPosition = new System.Numerics.Vector2(_worldWidth / 2f, _worldHeight / 2f);
        TargetGridPosition = GridPosition;

        Color = Microsoft.Xna.Framework.Color.Blue;

        // Start first movement
        ChooseNextTarget();
    }
    
    public void Update(float deltaTime)
    {
        if (!_isMoving)
        {
            ChooseNextTarget();
        }
        else
        {
            // Move towards target
            _moveProgress += deltaTime * _moveSpeed;

            if (_moveProgress >= 1f)
            {
                // Reached target
                GridPosition = TargetGridPosition;
                _moveProgress = 0f;
                _isMoving = false;
            }
            else
            {
                // Interpolate between current and target position
                var startPos = new System.Numerics.Vector2(
                    TargetGridPosition.X - (TargetGridPosition.X - GridPosition.X),
                    TargetGridPosition.Y - (TargetGridPosition.Y - GridPosition.Y)
                );

                // Smooth interpolation
                var t = _moveProgress;
                GridPosition = System.Numerics.Vector2.Lerp(startPos, TargetGridPosition, t);
            }
        }
    }

    private void ChooseNextTarget()
    {
        var random = new Random();
        var directions = new[]
        {
            new System.Numerics.Vector2(1, 0),   // Right
            new System.Numerics.Vector2(-1, 0),  // Left
            new System.Numerics.Vector2(0, 1),   // Down
            new System.Numerics.Vector2(0, -1)   // Up
        };

        // Try to find a valid direction
        for (int attempts = 0; attempts < 10; attempts++)
        {
            var direction = directions[random.Next(directions.Length)];
            var newTarget = new System.Numerics.Vector2(
                GridPosition.X + direction.X,
                GridPosition.Y + direction.Y
            );

            // Check bounds
            if (newTarget.X >= 0 && newTarget.X < _worldWidth &&
                newTarget.Y >= 0 && newTarget.Y < _worldHeight)
            {
                TargetGridPosition = newTarget;
                _isMoving = true;
                _moveProgress = 0f;
                break;
            }
        }

        // If no valid direction found, stay in place
        if (!_isMoving)
        {
            TargetGridPosition = GridPosition;
        }
    }
    
    public void Draw(SpriteBatch spriteBatch, Texture2D pixelTexture)
    {
        if (pixelTexture == null)
            return;

        // Convert grid position to screen position
        var screenX = (int)(GridPosition.X * Size);
        var screenY = (int)(GridPosition.Y * Size);

        var rectangle = new Microsoft.Xna.Framework.Rectangle(
            screenX,
            screenY,
            (int)Size,
            (int)Size
        );

        // Draw train body (bright blue to stand out)
        spriteBatch.Draw(pixelTexture, rectangle, Microsoft.Xna.Framework.Color.CornflowerBlue);

        // Draw a small white border for visibility
        DrawBorder(spriteBatch, pixelTexture, rectangle);
    }
    
    private void DrawBorder(SpriteBatch spriteBatch, Texture2D pixelTexture, Microsoft.Xna.Framework.Rectangle rectangle)
    {
        // For 5px tiles, just draw a 1px white border
        var borderColor = Microsoft.Xna.Framework.Color.White;
        var thickness = 1;

        // Only draw border if tile is big enough
        if (rectangle.Width > 2 && rectangle.Height > 2)
        {
            // Top
            spriteBatch.Draw(pixelTexture, new Microsoft.Xna.Framework.Rectangle(rectangle.X, rectangle.Y, rectangle.Width, thickness), borderColor);

            // Bottom
            spriteBatch.Draw(pixelTexture, new Microsoft.Xna.Framework.Rectangle(rectangle.X, rectangle.Bottom - thickness, rectangle.Width, thickness), borderColor);

            // Left
            spriteBatch.Draw(pixelTexture, new Microsoft.Xna.Framework.Rectangle(rectangle.X, rectangle.Y, thickness, rectangle.Height), borderColor);

            // Right
            spriteBatch.Draw(pixelTexture, new Microsoft.Xna.Framework.Rectangle(rectangle.Right - thickness, rectangle.Y, thickness, rectangle.Height), borderColor);
        }
    }
}
