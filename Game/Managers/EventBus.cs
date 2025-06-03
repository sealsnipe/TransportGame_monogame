using System;
using System.Numerics;
using TransportGame.Game.Entities;
using TransportGame.Game.Constants;
using TransportGame.Game.Systems;

namespace TransportGame.Game.Managers;

/// <summary>
/// Central event bus for loose coupling between game systems.
/// Ported from Godot EventBus.gd singleton.
/// Replaces Godot signals with C# events.
/// </summary>
public class EventBus
{
    // Building Events
    public event Action<Building, Vector2>? BuildingPlaced;
    public event Action<Building>? BuildingRemoved;
    public event Action<Building>? BuildingSelected;
    
    // Resource Events
    public event Action<string, int>? ResourceChanged;
    public event Action<string, int, int>? ResourceProduced;
    public event Action<string, int, int>? ResourceConsumed;
    
    // Train Events
    public event Action<Train, Vector2>? TrainSpawned;
    public event Action<Train>? TrainDestroyed;
    public event Action<Train, Vector2, Vector2>? TrainMoved;
    
    // Game State Events
    public event Action? GameStarted;
    public event Action? GamePaused;
    public event Action? GameResumed;
    public event Action<string>? GameSaved;
    public event Action<string>? GameLoaded;
    
    // UI Events
    public event Action<string>? UINotification;
    public event Action<string, object>? UIStateChanged;
    
    // Input Events
    public event Action<Vector2>? MouseClicked;
    public event Action<Vector2>? MouseRightClicked;
    public event Action<string>? KeyPressed;

    // Mouse Interaction Events (for Tooltip System)
    public event Action<Vector2, int, int, TileType>? MouseHover; // screenPos, gridX, gridY, tileType

    #region Building Event Emitters
    
    public void EmitBuildingPlaced(Building building, Vector2 position)
    {
        BuildingPlaced?.Invoke(building, position);
    }

    public void EmitBuildingPlaced(PlacedBuilding placedBuilding, Vector2 position)
    {
        // For now, skip the Building entity creation since it's abstract
        // TODO: Refactor to use PlacedBuilding directly or create concrete Building implementation
        Console.WriteLine($"[EVENT] Building placed: {placedBuilding.BuildingId} at {position}");
        // BuildingPlaced?.Invoke(building, position);
    }
    
    public void EmitBuildingRemoved(Building building)
    {
        BuildingRemoved?.Invoke(building);
    }
    
    public void EmitBuildingSelected(Building building)
    {
        BuildingSelected?.Invoke(building);
    }
    
    #endregion

    #region Resource Event Emitters
    
    public void EmitResourceChanged(string resourceType, int newAmount)
    {
        ResourceChanged?.Invoke(resourceType, newAmount);
    }
    
    public void EmitResourceProduced(string resourceType, int amount, int totalAmount)
    {
        ResourceProduced?.Invoke(resourceType, amount, totalAmount);
    }
    
    public void EmitResourceConsumed(string resourceType, int amount, int remainingAmount)
    {
        ResourceConsumed?.Invoke(resourceType, amount, remainingAmount);
    }
    
    #endregion

    #region Train Event Emitters
    
    public void EmitTrainSpawned(Train train, Vector2 position)
    {
        TrainSpawned?.Invoke(train, position);
    }
    
    public void EmitTrainDestroyed(Train train)
    {
        TrainDestroyed?.Invoke(train);
    }
    
    public void EmitTrainMoved(Train train, Vector2 fromPosition, Vector2 toPosition)
    {
        TrainMoved?.Invoke(train, fromPosition, toPosition);
    }
    
    #endregion

    #region Game State Event Emitters
    
    public void EmitGameStarted()
    {
        GameStarted?.Invoke();
    }
    
    public void EmitGamePaused()
    {
        GamePaused?.Invoke();
    }
    
    public void EmitGameResumed()
    {
        GameResumed?.Invoke();
    }
    
    public void EmitGameSaved(string fileName)
    {
        GameSaved?.Invoke(fileName);
    }
    
    public void EmitGameLoaded(string fileName)
    {
        GameLoaded?.Invoke(fileName);
    }
    
    #endregion

    #region UI Event Emitters
    
    public void EmitUINotification(string message)
    {
        UINotification?.Invoke(message);
    }
    
    public void EmitUIStateChanged(string stateName, object stateValue)
    {
        UIStateChanged?.Invoke(stateName, stateValue);
    }
    
    #endregion

    #region Input Event Emitters
    
    public void EmitMouseClicked(Vector2 position)
    {
        MouseClicked?.Invoke(position);
    }
    
    public void EmitMouseRightClicked(Vector2 position)
    {
        MouseRightClicked?.Invoke(position);
    }
    
    public void EmitKeyPressed(string key)
    {
        Console.WriteLine($"EventBus: Emitting KeyPressed event: '{key}'");
        KeyPressed?.Invoke(key);
        Console.WriteLine($"EventBus: KeyPressed event '{key}' emitted to {KeyPressed?.GetInvocationList().Length ?? 0} subscribers");
    }

    /// <summary>
    /// Emits a mouse hover event with detailed tile information for tooltip system.
    /// </summary>
    public void EmitMouseHover(Vector2 screenPosition, int gridX, int gridY, TileType tileType)
    {
        // Removed excessive logging - only log clicks, not hover events
        MouseHover?.Invoke(screenPosition, gridX, gridY, tileType);
    }

    #endregion

    /// <summary>
    /// Clears all event subscriptions. Use with caution!
    /// </summary>
    public void ClearAllEvents()
    {
        BuildingPlaced = null;
        BuildingRemoved = null;
        BuildingSelected = null;
        
        ResourceChanged = null;
        ResourceProduced = null;
        ResourceConsumed = null;
        
        TrainSpawned = null;
        TrainDestroyed = null;
        TrainMoved = null;
        
        GameStarted = null;
        GamePaused = null;
        GameResumed = null;
        GameSaved = null;
        GameLoaded = null;
        
        UINotification = null;
        UIStateChanged = null;
        
        MouseClicked = null;
        MouseRightClicked = null;
        KeyPressed = null;
        MouseHover = null;
    }

    /// <summary>
    /// Gets the number of subscribers for a specific event type.
    /// Useful for debugging event system usage.
    /// </summary>
    public int GetSubscriberCount(string eventName)
    {
        return eventName switch
        {
            nameof(BuildingPlaced) => BuildingPlaced?.GetInvocationList().Length ?? 0,
            nameof(BuildingRemoved) => BuildingRemoved?.GetInvocationList().Length ?? 0,
            nameof(ResourceChanged) => ResourceChanged?.GetInvocationList().Length ?? 0,
            nameof(TrainSpawned) => TrainSpawned?.GetInvocationList().Length ?? 0,
            nameof(GameStarted) => GameStarted?.GetInvocationList().Length ?? 0,
            nameof(UINotification) => UINotification?.GetInvocationList().Length ?? 0,
            nameof(MouseClicked) => MouseClicked?.GetInvocationList().Length ?? 0,
            nameof(MouseHover) => MouseHover?.GetInvocationList().Length ?? 0,
            _ => 0
        };
    }
}
