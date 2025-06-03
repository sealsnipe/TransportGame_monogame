using System.Numerics;
using TransportGame.Game.Constants;

namespace TransportGame.Game.Entities;

/// <summary>
/// Represents a train that can transport resources between buildings.
/// Ported from Godot Train.gd.
/// </summary>
public class Train : Entity
{
    // Train properties
    public TrainState TrainState { get; protected set; } = TrainState.Idle;
    public float Speed { get; protected set; } = 100f; // pixels per second
    public int CargoCapacity { get; protected set; } = GameConstants.TRAIN_CARGO_CAPACITY;
    
    // Movement
    public Vector2 TargetPosition { get; protected set; }
    public Vector2 PreviousPosition { get; protected set; }
    public float MovementProgress { get; protected set; } = 0f;
    
    // Cargo system
    private readonly Dictionary<string, int> _cargo = new();
    public int TotalCargoAmount => _cargo.Values.Sum();
    public bool IsCargoFull => TotalCargoAmount >= CargoCapacity;
    public bool IsCargoEmpty => TotalCargoAmount == 0;
    
    // Route system
    private readonly List<Vector2> _route = new();
    private int _currentRouteIndex = 0;
    public bool HasRoute => _route.Count > 0;
    
    // Operation timing
    public float LoadingTime { get; protected set; } = 2f; // seconds to load/unload
    public float LoadingProgress { get; protected set; } = 0f;

    public Train() : base(EntityType.Train)
    {
        MaxHealth = 75;
        CurrentHealth = MaxHealth;
        
        // Initialize cargo for all resource types
        _cargo[GameConstants.RESOURCE_WHEAT] = 0;
        _cargo[GameConstants.RESOURCE_IRON] = 0;
        _cargo[GameConstants.RESOURCE_FOOD] = 0;
        _cargo[GameConstants.RESOURCE_STEEL] = 0;
    }

    public override void Initialize(Dictionary<string, object> data)
    {
        base.Initialize(data);
        
        if (data.TryGetValue("train_state", out var state) && state is string stateStr)
            Enum.TryParse<TrainState>(stateStr, out var trainState);
            
        if (data.TryGetValue("speed", out var spd) && spd is float speed)
            Speed = speed;
            
        if (data.TryGetValue("cargo_capacity", out var capacity) && capacity is int cargoCapacity)
            CargoCapacity = cargoCapacity;
            
        if (data.TryGetValue("target_x", out var tx) && data.TryGetValue("target_y", out var ty))
        {
            if (tx is float targetX && ty is float targetY)
                TargetPosition = new Vector2(targetX, targetY);
        }
        
        // Load cargo
        if (data.TryGetValue("cargo", out var cargoData) && cargoData is Dictionary<string, object> cargo)
        {
            foreach (var kvp in cargo)
            {
                if (kvp.Value is int amount)
                    _cargo[kvp.Key] = amount;
            }
        }
        
        // Load route
        if (data.TryGetValue("route", out var routeData) && routeData is List<object> route)
        {
            _route.Clear();
            foreach (var point in route)
            {
                if (point is Dictionary<string, object> pointData &&
                    pointData.TryGetValue("x", out var x) && pointData.TryGetValue("y", out var y) &&
                    x is float px && y is float py)
                {
                    _route.Add(new Vector2(px, py));
                }
            }
        }
        
        if (data.TryGetValue("current_route_index", out var routeIndex) && routeIndex is int currentRouteIndex)
            _currentRouteIndex = Math.Clamp(currentRouteIndex, 0, _route.Count - 1);
    }

    public override Dictionary<string, object> GetSaveData()
    {
        var data = base.GetSaveData();
        
        data["train_state"] = TrainState.ToString();
        data["speed"] = Speed;
        data["cargo_capacity"] = CargoCapacity;
        data["target_x"] = TargetPosition.X;
        data["target_y"] = TargetPosition.Y;
        data["movement_progress"] = MovementProgress;
        data["loading_progress"] = LoadingProgress;
        data["current_route_index"] = _currentRouteIndex;
        
        // Save cargo
        var cargoData = new Dictionary<string, object>();
        foreach (var kvp in _cargo)
        {
            cargoData[kvp.Key] = kvp.Value;
        }
        data["cargo"] = cargoData;
        
        // Save route
        var routeData = new List<object>();
        foreach (var point in _route)
        {
            routeData.Add(new Dictionary<string, object>
            {
                ["x"] = point.X,
                ["y"] = point.Y
            });
        }
        data["route"] = routeData;
        
        return data;
    }

    public override void Update(float deltaTime, float gameTime)
    {
        base.Update(deltaTime, gameTime);
        
        if (!IsActive)
            return;
            
        switch (TrainState)
        {
            case TrainState.Moving:
                UpdateMovement(deltaTime);
                break;
                
            case TrainState.Loading:
            case TrainState.Unloading:
                UpdateLoading(deltaTime);
                break;
                
            case TrainState.Idle:
                UpdateIdle(deltaTime);
                break;
        }
    }

    #region Movement System

    /// <summary>
    /// Sets a route for the train to follow.
    /// </summary>
    public void SetRoute(List<Vector2> route)
    {
        _route.Clear();
        _route.AddRange(route);
        _currentRouteIndex = 0;
        
        if (_route.Count > 0)
        {
            StartMovingToNextWaypoint();
        }
    }

    /// <summary>
    /// Adds a waypoint to the current route.
    /// </summary>
    public void AddWaypoint(Vector2 waypoint)
    {
        _route.Add(waypoint);
        
        if (TrainState == TrainState.Idle && _route.Count == 1)
        {
            StartMovingToNextWaypoint();
        }
    }

    /// <summary>
    /// Clears the current route and stops the train.
    /// </summary>
    public void ClearRoute()
    {
        _route.Clear();
        _currentRouteIndex = 0;
        
        if (TrainState == TrainState.Moving)
        {
            TrainState = TrainState.Idle;
        }
    }

    private void StartMovingToNextWaypoint()
    {
        if (_currentRouteIndex >= _route.Count)
        {
            // Route completed
            TrainState = TrainState.Idle;
            return;
        }
        
        PreviousPosition = Position;
        TargetPosition = _route[_currentRouteIndex];
        MovementProgress = 0f;
        TrainState = TrainState.Moving;
    }

    private void UpdateMovement(float deltaTime)
    {
        var distance = Vector2.Distance(PreviousPosition, TargetPosition);
        if (distance <= 0.1f)
        {
            // Already at target
            ReachCurrentWaypoint();
            return;
        }
        
        var moveDistance = Speed * deltaTime;
        MovementProgress += moveDistance / distance;
        
        if (MovementProgress >= 1f)
        {
            // Reached waypoint
            Position = TargetPosition;
            ReachCurrentWaypoint();
        }
        else
        {
            // Interpolate position
            Position = Vector2.Lerp(PreviousPosition, TargetPosition, MovementProgress);
        }
    }

    private void ReachCurrentWaypoint()
    {
        Position = TargetPosition;
        _currentRouteIndex++;
        
        if (_currentRouteIndex >= _route.Count)
        {
            // Route completed
            TrainState = TrainState.Idle;
            OnRouteCompleted();
        }
        else
        {
            // Move to next waypoint
            StartMovingToNextWaypoint();
        }
    }

    #endregion

    #region Cargo System

    /// <summary>
    /// Gets the amount of a specific resource in cargo.
    /// </summary>
    public int GetCargoAmount(string resourceType)
    {
        return _cargo.TryGetValue(resourceType, out var amount) ? amount : 0;
    }

    /// <summary>
    /// Attempts to load cargo onto the train.
    /// </summary>
    public int LoadCargo(string resourceType, int amount)
    {
        var availableSpace = CargoCapacity - TotalCargoAmount;
        var actualAmount = Math.Min(amount, availableSpace);
        
        if (actualAmount > 0)
        {
            _cargo[resourceType] += actualAmount;
        }
        
        return actualAmount;
    }

    /// <summary>
    /// Attempts to unload cargo from the train.
    /// </summary>
    public int UnloadCargo(string resourceType, int amount)
    {
        var currentAmount = GetCargoAmount(resourceType);
        var actualAmount = Math.Min(amount, currentAmount);
        
        if (actualAmount > 0)
        {
            _cargo[resourceType] -= actualAmount;
        }
        
        return actualAmount;
    }

    /// <summary>
    /// Unloads all cargo of a specific type.
    /// </summary>
    public int UnloadAllCargo(string resourceType)
    {
        var amount = GetCargoAmount(resourceType);
        _cargo[resourceType] = 0;
        return amount;
    }

    /// <summary>
    /// Gets all cargo as a dictionary.
    /// </summary>
    public Dictionary<string, int> GetAllCargo()
    {
        return new Dictionary<string, int>(_cargo);
    }

    #endregion

    #region Loading/Unloading Operations

    /// <summary>
    /// Starts loading cargo at the current position.
    /// </summary>
    public void StartLoading()
    {
        if (TrainState == TrainState.Moving)
            return;
            
        TrainState = TrainState.Loading;
        LoadingProgress = 0f;
    }

    /// <summary>
    /// Starts unloading cargo at the current position.
    /// </summary>
    public void StartUnloading()
    {
        if (TrainState == TrainState.Moving)
            return;
            
        TrainState = TrainState.Unloading;
        LoadingProgress = 0f;
    }

    private void UpdateLoading(float deltaTime)
    {
        LoadingProgress += deltaTime / LoadingTime;
        
        if (LoadingProgress >= 1f)
        {
            LoadingProgress = 1f;
            
            if (TrainState == TrainState.Loading)
            {
                OnLoadingCompleted();
            }
            else if (TrainState == TrainState.Unloading)
            {
                OnUnloadingCompleted();
            }
            
            TrainState = TrainState.Idle;
        }
    }

    private void UpdateIdle(float deltaTime)
    {
        // Check if we should continue on route
        if (HasRoute && _currentRouteIndex < _route.Count)
        {
            StartMovingToNextWaypoint();
        }
    }

    #endregion

    #region Virtual Event Methods

    protected virtual void OnRouteCompleted()
    {
        // Override in derived classes or use events
    }

    protected virtual void OnLoadingCompleted()
    {
        // Override in derived classes or use events
    }

    protected virtual void OnUnloadingCompleted()
    {
        // Override in derived classes or use events
    }

    #endregion

    public override string ToString()
    {
        return $"Train({EntityId[..8]}...) at {Position} - {TrainState} - Cargo: {TotalCargoAmount}/{CargoCapacity}";
    }
}
