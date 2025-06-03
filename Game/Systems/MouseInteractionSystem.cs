using System;
using System.Numerics;
using Microsoft.Xna.Framework;
using TransportGame.Game.Managers;
using TransportGame.Game.Constants;

namespace TransportGame.Game.Systems;

/// <summary>
/// Handles mouse interaction with the game world, specifically for tooltip system.
/// Converts mouse position to grid coordinates and emits hover events.
/// </summary>
public class MouseInteractionSystem : IDisposable
{
    private readonly EventBus _eventBus;
    private readonly TilemapManager _tilemapManager;
    private readonly ErrorHandler _errorHandler;
    private readonly BuildingPlacementSystem _buildingPlacementSystem;
    private TooltipSystem? _tooltipSystem;
    private IndustryGenerationSystem? _industryGenerationSystem;
    private BuildingUISystem? _buildingUISystem;
    
    // Mouse tracking
    private System.Numerics.Vector2 _lastMousePosition = System.Numerics.Vector2.Zero;
    private int _lastGridX = -1;
    private int _lastGridY = -1;
    private TileType _lastTileType = TileType.Water;

    // Click handling
    private bool _pendingClick = false;
    private System.Numerics.Vector2 _clickPosition = System.Numerics.Vector2.Zero;

    // Throttling to prevent spam
    private float _hoverUpdateTimer = 0f;
    private const float HOVER_UPDATE_INTERVAL = 0.1f; // Update every 100ms
    
    // Test logging
    private float _logTimer = 0f;
    private const float LOG_INTERVAL = 2.0f; // Log every 2 seconds
    private bool _isInitialized = false;
    private bool _hasRecentActivity = false; // Track if there's been recent activity worth logging

    public MouseInteractionSystem(EventBus eventBus, TilemapManager tilemapManager, BuildingPlacementSystem buildingPlacementSystem, ErrorHandler errorHandler)
    {
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _tilemapManager = tilemapManager ?? throw new ArgumentNullException(nameof(tilemapManager));
        _buildingPlacementSystem = buildingPlacementSystem ?? throw new ArgumentNullException(nameof(buildingPlacementSystem));
        _errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));

        Console.WriteLine("[TOOLTIP-TEST] MouseInteractionSystem: Constructor called");

        // Subscribe to mouse click events for tooltip handling
        _eventBus.MouseClicked += OnMouseClicked;
        Console.WriteLine("[TOOLTIP-TEST] MouseInteractionSystem: Subscribed to MouseClicked events");

        _isInitialized = true;
        _errorHandler.LogInfo("MouseInteractionSystem initialized with click handling");
        Console.WriteLine("[TOOLTIP-TEST] MouseInteractionSystem: Initialization complete");
    }

    /// <summary>
    /// Sets the tooltip system reference for click handling.
    /// </summary>
    public void SetTooltipSystem(TooltipSystem tooltipSystem)
    {
        _tooltipSystem = tooltipSystem;
        Console.WriteLine("[TOOLTIP-TEST] MouseInteractionSystem: TooltipSystem reference set");
    }

    public void SetIndustryGenerationSystem(IndustryGenerationSystem industryGenerationSystem)
    {
        _industryGenerationSystem = industryGenerationSystem;
        Console.WriteLine("[TOOLTIP-TEST] MouseInteractionSystem: IndustryGenerationSystem reference set");
    }

    public void SetBuildingUISystem(BuildingUISystem buildingUISystem)
    {
        _buildingUISystem = buildingUISystem;
        Console.WriteLine("[TOOLTIP-TEST] MouseInteractionSystem: BuildingUISystem reference set");
    }

    /// <summary>
    /// Updates the mouse interaction system.
    /// </summary>
    public void Update(GameTime gameTime, InputSystem inputSystem, CameraSystem cameraSystem)
    {
        if (!_isInitialized || inputSystem == null || cameraSystem == null)
        {
            Console.WriteLine("[TOOLTIP-TEST] MouseInteractionSystem: Update skipped - not initialized or missing dependencies");
            return;
        }

        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _hoverUpdateTimer += deltaTime;
        _logTimer += deltaTime;

        // Log system status periodically - only when something interesting happens
        if (_logTimer >= LOG_INTERVAL && _hasRecentActivity)
        {
            // Removed excessive logging - only log meaningful events
            _logTimer = 0f;
            _hasRecentActivity = false; // Reset activity flag
        }

        // Check for mouse clicks and process them immediately
        ProcessMouseClicks(inputSystem, cameraSystem);

        // Only update hover detection at intervals to prevent spam
        if (_hoverUpdateTimer >= HOVER_UPDATE_INTERVAL)
        {
            UpdateMouseHover(inputSystem, cameraSystem);
            _hoverUpdateTimer = 0f;
        }
    }

    /// <summary>
    /// Processes pending mouse clicks and triggers tooltip display.
    /// </summary>
    private void ProcessMouseClicks(InputSystem inputSystem, CameraSystem cameraSystem)
    {
        try
        {
            // Check if we have a pending click to process
            if (_pendingClick && _tooltipSystem != null)
            {
                // Processing click (reduced logging)

                // Convert to world coordinates
                var mouseWorldPos = cameraSystem.ScreenToWorld(_clickPosition);

                // Convert to grid coordinates
                var gridX = (int)(mouseWorldPos.X / GameConstants.TILE_SIZE);
                var gridY = (int)(mouseWorldPos.Y / GameConstants.TILE_SIZE);

                Console.WriteLine($"CLICK: Clicked on Grid({gridX},{gridY})");

                // Check if grid position is valid
                if (_tilemapManager.IsValidGridPosition(gridX, gridY))
                {
                    // First check if there's a player-built building at this position
                    var building = GetBuildingAtPosition(gridX, gridY);
                    if (building != null)
                    {
                        Console.WriteLine($"[TOOLTIP-TEST] MouseInteractionSystem: Building clicked - {building.BuildingId} at ({gridX},{gridY})");

                        // Trigger building tooltip display
                        _tooltipSystem.OnBuildingClicked(_clickPosition, gridX, gridY, building);
                    }
                    else
                    {
                        // Check if there's a natural industry at this position
                        var industry = _industryGenerationSystem?.GetIndustryAtGridPosition(gridX, gridY);
                        if (industry != null)
                        {
                            Console.WriteLine($"[TOOLTIP-TEST] MouseInteractionSystem: Industry clicked - {industry.BuildingId} at ({gridX},{gridY})");

                            // Trigger building tooltip display for industry
                            _tooltipSystem.OnBuildingClicked(_clickPosition, gridX, gridY, industry);
                        }
                        else
                        {
                            // No building or industry, show tile tooltip
                            var tileType = _tilemapManager.GetTileType(gridX, gridY);
                            Console.WriteLine($"[TOOLTIP-TEST] MouseInteractionSystem: Valid tile clicked - TileType({tileType})");

                            // Trigger tile tooltip display
                            _tooltipSystem.OnTileClicked(_clickPosition, gridX, gridY, tileType);
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"CLICK: Invalid grid position ({gridX},{gridY}) - outside world");
                }

                // Clear the pending click
                _pendingClick = false;
            }
        }
        catch (Exception ex)
        {
            _errorHandler.HandleError($"Error in ProcessMouseClicks: {ex.Message}", "MouseInteractionSystem.ProcessMouseClicks");
            Console.WriteLine($"[TOOLTIP-TEST] MouseInteractionSystem: ERROR in ProcessMouseClicks: {ex.Message}");
        }
    }

    /// <summary>
    /// Handles mouse click events and triggers tooltip display.
    /// </summary>
    private void OnMouseClicked(System.Numerics.Vector2 screenPosition)
    {
        try
        {
            Console.WriteLine($"CLICK: Mouse clicked at ({screenPosition.X:F0},{screenPosition.Y:F0})");

            // First check if click was on Building UI
            if (_buildingUISystem?.HandleMouseClick(new Microsoft.Xna.Framework.Vector2(screenPosition.X, screenPosition.Y)) == true)
            {
                Console.WriteLine("CLICK: Handled by Building UI");
                return; // UI consumed the click
            }

            // Store the click for processing in the next Update call (when we have camera access)
            _pendingClick = true;
            _clickPosition = screenPosition;
        }
        catch (Exception ex)
        {
            _errorHandler.HandleError($"Error in OnMouseClicked: {ex.Message}", "MouseInteractionSystem.OnMouseClicked");
            Console.WriteLine($"[TOOLTIP-TEST] MouseInteractionSystem: ERROR in OnMouseClicked: {ex.Message}");
        }
    }

    /// <summary>
    /// Updates mouse hover detection and emits hover events.
    /// </summary>
    private void UpdateMouseHover(InputSystem inputSystem, CameraSystem cameraSystem)
    {
        try
        {
            // Get current mouse position
            var mouseScreenPos = inputSystem.GetMousePosition();

            // Convert to world coordinates
            var mouseWorldPos = cameraSystem.ScreenToWorld(mouseScreenPos);

            // Convert to grid coordinates
            var gridX = (int)(mouseWorldPos.X / GameConstants.TILE_SIZE);
            var gridY = (int)(mouseWorldPos.Y / GameConstants.TILE_SIZE);

            // Check if grid position is valid
            if (!_tilemapManager.IsValidGridPosition(gridX, gridY))
            {
                // Removed excessive invalid position logging - only log clicks, not hover
                return;
            }

            // Get tile type at this position
            var tileType = _tilemapManager.GetTileType(gridX, gridY);

            // Check if we've moved to a different tile
            if (gridX != _lastGridX || gridY != _lastGridY || tileType != _lastTileType)
            {
                // Removed excessive hover logging - only log clicks, not every mouse movement

                // Update tracking
                _lastMousePosition = mouseScreenPos;
                _lastGridX = gridX;
                _lastGridY = gridY;
                _lastTileType = tileType;
                _hasRecentActivity = true; // Mark that we have activity worth logging

                // Emit hover event (no logging to reduce spam)
                _eventBus.EmitMouseHover(mouseScreenPos, gridX, gridY, tileType);
            }
            else
            {
                // Still on same tile - no need to emit event (logging removed to prevent spam)
            }
        }
        catch (Exception ex)
        {
            _errorHandler.HandleError($"Error in UpdateMouseHover: {ex.Message}", "MouseInteractionSystem.UpdateMouseHover");
            Console.WriteLine($"[TOOLTIP-TEST] MouseInteractionSystem: ERROR in UpdateMouseHover: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets a building at the specified grid position, if any.
    /// </summary>
    private PlacedBuilding? GetBuildingAtPosition(int gridX, int gridY)
    {
        try
        {
            var placedBuildings = _buildingPlacementSystem.GetPlacedBuildings();

            foreach (var building in placedBuildings.Values)
            {
                // Check if the click position is within this building's bounds
                var buildingX = (int)building.GridPosition.X;
                var buildingY = (int)building.GridPosition.Y;
                var buildingWidth = building.Definition.Size.Width;
                var buildingHeight = building.Definition.Size.Height;

                if (gridX >= buildingX && gridX < buildingX + buildingWidth &&
                    gridY >= buildingY && gridY < buildingY + buildingHeight)
                {
                    return building;
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            _errorHandler.HandleError($"Error getting building at position: {ex.Message}", "MouseInteractionSystem.GetBuildingAtPosition");
            return null;
        }
    }

    /// <summary>
    /// Gets debug information about the current mouse state.
    /// </summary>
    public string GetDebugInfo()
    {
        return $"Mouse Grid: ({_lastGridX},{_lastGridY}) TileType: {_lastTileType}";
    }

    /// <summary>
    /// Disposes of the mouse interaction system.
    /// </summary>
    public void Dispose()
    {
        Console.WriteLine("[TOOLTIP-TEST] MouseInteractionSystem: Dispose called");

        if (_eventBus != null)
        {
            _eventBus.MouseClicked -= OnMouseClicked;
            Console.WriteLine("[TOOLTIP-TEST] MouseInteractionSystem: Unsubscribed from MouseClicked events");
        }

        _isInitialized = false;
        _errorHandler.LogInfo("MouseInteractionSystem disposed");
    }
}
