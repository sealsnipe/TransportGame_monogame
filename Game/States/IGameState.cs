using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TransportGame.Game.States
{
    /// <summary>
    /// Interface for all game states (Playing, Paused, Menu, Options)
    /// Based on MonoGame best practices for state management
    /// </summary>
    public interface IGameState
    {
        /// <summary>
        /// Whether this state is active and should receive input
        /// </summary>
        bool IsActive { get; set; }

        /// <summary>
        /// Whether this state should be updated when not on top of stack
        /// </summary>
        bool UpdateBelow { get; }

        /// <summary>
        /// Whether this state should be drawn when not on top of stack
        /// </summary>
        bool DrawBelow { get; }

        /// <summary>
        /// Initialize the state
        /// </summary>
        void Initialize();

        /// <summary>
        /// Update the state logic
        /// </summary>
        /// <param name="gameTime">Game timing information</param>
        void Update(GameTime gameTime);

        /// <summary>
        /// Draw the state
        /// </summary>
        /// <param name="spriteBatch">SpriteBatch for rendering</param>
        void Draw(SpriteBatch spriteBatch);

        /// <summary>
        /// Called when state becomes active (pushed to top of stack)
        /// </summary>
        void OnEnter();

        /// <summary>
        /// Called when state becomes inactive (another state pushed on top)
        /// </summary>
        void OnExit();

        /// <summary>
        /// Cleanup resources
        /// </summary>
        void Dispose();
    }
}
