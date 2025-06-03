using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using TransportGame.Game.Managers;

namespace TransportGame.Game.States
{
    /// <summary>
    /// Manages game states using a stack-based approach
    /// Implements MonoGame best practices for state management
    /// </summary>
    public class StateManager : IDisposable
    {
        private readonly Stack<IGameState> _stateStack;
        private readonly ErrorHandler _errorHandler;
        private bool _isInitialized;

        public StateManager(ErrorHandler errorHandler)
        {
            _stateStack = new Stack<IGameState>();
            _errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
            _isInitialized = false;
        }

        /// <summary>
        /// Current active state (top of stack)
        /// </summary>
        public IGameState CurrentState => _stateStack.Count > 0 ? _stateStack.Peek() : null;

        /// <summary>
        /// Number of states in the stack
        /// </summary>
        public int StateCount => _stateStack.Count;

        /// <summary>
        /// Whether any state is currently active
        /// </summary>
        public bool HasActiveState => _stateStack.Count > 0;

        public void Initialize()
        {
            try
            {
                _isInitialized = true;
                _errorHandler.LogInfo("StateManager initialized");
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError($"Error initializing StateManager: {ex.Message}", "StateManager.Initialize");
            }
        }

        /// <summary>
        /// Push a new state onto the stack (opens menu/overlay)
        /// </summary>
        public void PushState(IGameState newState)
        {
            try
            {
                if (newState == null)
                {
                    _errorHandler.HandleError("Cannot push null state", "StateManager.PushState");
                    return;
                }

                // Deactivate current state
                if (_stateStack.Count > 0)
                {
                    var currentState = _stateStack.Peek();
                    currentState.IsActive = false;
                    currentState.OnExit();
                }

                // Add new state
                _stateStack.Push(newState);
                newState.Initialize();
                newState.IsActive = true;
                newState.OnEnter();

                _errorHandler.LogInfo($"State pushed: {newState.GetType().Name}, Stack size: {_stateStack.Count}");
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError($"Error pushing state: {ex.Message}", "StateManager.PushState");
            }
        }

        /// <summary>
        /// Pop the current state from the stack (closes menu/overlay)
        /// </summary>
        public void PopState()
        {
            try
            {
                if (_stateStack.Count == 0)
                {
                    _errorHandler.HandleError("Cannot pop from empty state stack", "StateManager.PopState");
                    return;
                }

                // Remove current state
                var currentState = _stateStack.Pop();
                currentState.IsActive = false;
                currentState.OnExit();
                currentState.Dispose();

                // Reactivate previous state
                if (_stateStack.Count > 0)
                {
                    var previousState = _stateStack.Peek();
                    previousState.IsActive = true;
                    previousState.OnEnter();
                }

                _errorHandler.LogInfo($"State popped: {currentState.GetType().Name}, Stack size: {_stateStack.Count}");
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError($"Error popping state: {ex.Message}", "StateManager.PopState");
            }
        }

        /// <summary>
        /// Replace the current state with a new one
        /// </summary>
        public void ChangeState(IGameState newState)
        {
            try
            {
                if (_stateStack.Count > 0)
                {
                    PopState();
                }
                PushState(newState);
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError($"Error changing state: {ex.Message}", "StateManager.ChangeState");
            }
        }

        /// <summary>
        /// Clear all states from the stack
        /// </summary>
        public void ClearStates()
        {
            try
            {
                while (_stateStack.Count > 0)
                {
                    PopState();
                }
                _errorHandler.LogInfo("All states cleared from stack");
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError($"Error clearing states: {ex.Message}", "StateManager.ClearStates");
            }
        }

        public void Update(GameTime gameTime)
        {
            try
            {
                if (!_isInitialized || _stateStack.Count == 0) return;

                // Reduced logging - only log state changes, not every update
                // _errorHandler.LogInfo($"StateManager Update: Stack size={_stateStack.Count}, Top state={_stateStack.Peek().GetType().Name}");

                // Always update the top state first, then check if it allows states below to update
                var statesToUpdate = new List<IGameState>();

                // Get the top state (most recent)
                var topState = _stateStack.Peek();
                statesToUpdate.Add(topState);

                // If the top state allows states below to update, add them
                if (topState.UpdateBelow)
                {
                    // Add all states except the top one (which we already added)
                    foreach (var state in _stateStack.Skip(1).Reverse())
                    {
                        statesToUpdate.Add(state);

                        // If this state doesn't want states below to update, stop here
                        if (!state.UpdateBelow)
                            break;
                    }
                }

                // Update all states in the list
                foreach (var state in statesToUpdate)
                {
                    // Removed excessive logging - only log state changes, not every update
                    state.Update(gameTime);
                }
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError($"Error in StateManager Update: {ex.Message}", "StateManager.Update");
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            try
            {
                if (!_isInitialized || _stateStack.Count == 0) return;

                // FIXED: Always draw the top state, then check DrawBelow for states underneath
                var statesToDraw = new List<IGameState>();
                var stateArray = _stateStack.ToArray(); // Top to bottom

                // Always add the top state (current active state)
                var topState = stateArray[0];
                statesToDraw.Add(topState);

                // Only add states below if the top state allows it
                if (topState.DrawBelow)
                {
                    for (int i = 1; i < stateArray.Length; i++)
                    {
                        var state = stateArray[i];
                        statesToDraw.Add(state);

                        // If this state doesn't want states below to draw, stop here
                        if (!state.DrawBelow)
                            break;
                    }
                }

                // DON'T REVERSE - draw in order: bottom states first, top state last (in foreground)
                foreach (var state in statesToDraw)
                {
                    state.Draw(spriteBatch);
                }
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError($"Error in StateManager Draw: {ex.Message}", "StateManager.Draw");
            }
        }

        public void Dispose()
        {
            try
            {
                ClearStates();
                _isInitialized = false;
                _errorHandler.LogInfo("StateManager disposed");
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError($"Error disposing StateManager: {ex.Message}", "StateManager.Dispose");
            }
        }
    }
}
