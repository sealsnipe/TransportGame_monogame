using System.Numerics;
using TransportGame.Game.Constants;

namespace TransportGame.Game.Entities;

/// <summary>
/// Base class for all buildings in the game.
/// Ported from Godot Building.gd.
/// </summary>
public abstract class Building : Entity
{
    // Building properties
    public string BuildingType { get; set; } = "";
    public BuildingState BuildingState { get; protected set; } = BuildingState.UnderConstruction;
    public Vector2 GridPosition { get; set; }
    public int BuildingCost { get; protected set; }
    
    // Construction
    public float ConstructionTime { get; protected set; } = 5.0f; // seconds
    public float ConstructionProgress { get; protected set; } = 0f;
    
    // Operation
    public bool IsOperational => BuildingState == BuildingState.Operational;
    public float OperationTime { get; protected set; } = 0f;
    
    // Upgrade system
    public int UpgradeLevel { get; protected set; } = 1;
    public int MaxUpgradeLevel { get; protected set; } = 3;
    public bool CanUpgrade => UpgradeLevel < MaxUpgradeLevel && IsOperational;

    protected Building(string buildingType) : base(EntityType.Building)
    {
        BuildingType = buildingType;
        MaxHealth = 100;
        CurrentHealth = MaxHealth;
    }

    public override void Initialize(Dictionary<string, object> data)
    {
        base.Initialize(data);
        
        if (data.TryGetValue("building_type", out var type) && type is string buildingType)
            BuildingType = buildingType;
            
        if (data.TryGetValue("building_state", out var state) && state is string stateStr)
            Enum.TryParse<BuildingState>(stateStr, out var buildingState);
            
        if (data.TryGetValue("grid_x", out var gx) && data.TryGetValue("grid_y", out var gy))
        {
            if (gx is float gridX && gy is float gridY)
                GridPosition = new Vector2(gridX, gridY);
        }
        
        if (data.TryGetValue("construction_progress", out var progress) && progress is float constructionProgress)
            ConstructionProgress = Math.Clamp(constructionProgress, 0f, 1f);
            
        if (data.TryGetValue("upgrade_level", out var level) && level is int upgradeLevel)
            UpgradeLevel = Math.Clamp(upgradeLevel, 1, MaxUpgradeLevel);
            
        if (data.TryGetValue("operation_time", out var opTime) && opTime is float operationTime)
            OperationTime = operationTime;
    }

    public override Dictionary<string, object> GetSaveData()
    {
        var data = base.GetSaveData();
        
        data["building_type"] = BuildingType;
        data["building_state"] = BuildingState.ToString();
        data["grid_x"] = GridPosition.X;
        data["grid_y"] = GridPosition.Y;
        data["construction_progress"] = ConstructionProgress;
        data["construction_time"] = ConstructionTime;
        data["upgrade_level"] = UpgradeLevel;
        data["max_upgrade_level"] = MaxUpgradeLevel;
        data["operation_time"] = OperationTime;
        data["building_cost"] = BuildingCost;
        
        return data;
    }

    public override void Update(float deltaTime, float gameTime)
    {
        base.Update(deltaTime, gameTime);
        
        if (!IsActive)
            return;
            
        switch (BuildingState)
        {
            case BuildingState.UnderConstruction:
                UpdateConstruction(deltaTime);
                break;
                
            case BuildingState.Operational:
                UpdateOperation(deltaTime, gameTime);
                break;
                
            case BuildingState.Upgrading:
                UpdateUpgrade(deltaTime);
                break;
        }
    }

    /// <summary>
    /// Checks if the building can be placed at the specified position.
    /// </summary>
    public virtual bool CanPlaceAt(Vector2 gridPosition)
    {
        // Basic implementation - override in derived classes for specific rules
        return true;
    }

    /// <summary>
    /// Starts construction of the building.
    /// </summary>
    public virtual void StartConstruction()
    {
        BuildingState = BuildingState.UnderConstruction;
        ConstructionProgress = 0f;
        CurrentHealth = 1; // Building starts with minimal health during construction
    }

    /// <summary>
    /// Completes construction and makes the building operational.
    /// </summary>
    protected virtual void CompleteConstruction()
    {
        BuildingState = BuildingState.Operational;
        ConstructionProgress = 1f;
        CurrentHealth = MaxHealth;
        OperationTime = 0f;
        
        OnConstructionCompleted();
    }

    /// <summary>
    /// Starts upgrading the building to the next level.
    /// </summary>
    public virtual bool StartUpgrade()
    {
        if (!CanUpgrade)
            return false;
            
        BuildingState = BuildingState.Upgrading;
        return true;
    }

    /// <summary>
    /// Completes the upgrade process.
    /// </summary>
    protected virtual void CompleteUpgrade()
    {
        UpgradeLevel++;
        BuildingState = BuildingState.Operational;
        
        // Increase health with upgrade
        MaxHealth += 25;
        CurrentHealth = MaxHealth;
        
        OnUpgradeCompleted();
    }

    /// <summary>
    /// Damages the building and may affect its operation.
    /// </summary>
    public override bool TakeDamage(int damage, string source = "")
    {
        var wasDestroyed = base.TakeDamage(damage, source);
        
        if (wasDestroyed)
        {
            BuildingState = BuildingState.Destroyed;
            return true;
        }
        
        // Building becomes damaged if health drops below 50%
        if (GetHealthPercentage() < 0.5f && BuildingState == BuildingState.Operational)
        {
            BuildingState = BuildingState.Damaged;
            OnBuildingDamaged();
        }
        
        return false;
    }

    /// <summary>
    /// Repairs the building back to operational state.
    /// </summary>
    public virtual void Repair()
    {
        if (BuildingState == BuildingState.Destroyed)
            return;
            
        CurrentHealth = MaxHealth;
        
        if (BuildingState == BuildingState.Damaged)
        {
            BuildingState = BuildingState.Operational;
            OnBuildingRepaired();
        }
    }

    /// <summary>
    /// Gets the efficiency of the building (0.0 to 1.0).
    /// Affected by health and upgrade level.
    /// </summary>
    public virtual float GetEfficiency()
    {
        if (!IsOperational)
            return 0f;
            
        var healthEfficiency = GetHealthPercentage();
        var upgradeBonus = 1f + (UpgradeLevel - 1) * 0.2f; // 20% bonus per upgrade level
        
        return Math.Min(1f, healthEfficiency * upgradeBonus);
    }

    /// <summary>
    /// Gets the world position from grid position.
    /// </summary>
    public Vector2 GetWorldPosition()
    {
        return new Vector2(
            GridPosition.X * GameConstants.TILE_SIZE,
            GridPosition.Y * GameConstants.TILE_SIZE
        );
    }

    /// <summary>
    /// Sets the grid position and updates world position.
    /// </summary>
    public void SetGridPosition(Vector2 gridPosition)
    {
        GridPosition = gridPosition;
        Position = GetWorldPosition();
    }

    #region Update Methods

    protected virtual void UpdateConstruction(float deltaTime)
    {
        ConstructionProgress += deltaTime / ConstructionTime;
        
        if (ConstructionProgress >= 1f)
        {
            CompleteConstruction();
        }
    }

    protected virtual void UpdateOperation(float deltaTime, float gameTime)
    {
        OperationTime += deltaTime;
        
        // Override in derived classes for specific operation logic
    }

    protected virtual void UpdateUpgrade(float deltaTime)
    {
        // Simple upgrade - takes half the construction time
        ConstructionProgress += deltaTime / (ConstructionTime * 0.5f);
        
        if (ConstructionProgress >= 1f)
        {
            CompleteUpgrade();
        }
    }

    #endregion

    #region Virtual Event Methods

    protected virtual void OnConstructionCompleted()
    {
        // Override in derived classes
    }

    protected virtual void OnUpgradeCompleted()
    {
        // Override in derived classes
    }

    protected virtual void OnBuildingDamaged()
    {
        // Override in derived classes
    }

    protected virtual void OnBuildingRepaired()
    {
        // Override in derived classes
    }

    #endregion

    public override string ToString()
    {
        return $"{BuildingType} at Grid({GridPosition.X}, {GridPosition.Y}) - {BuildingState} - Level {UpgradeLevel}";
    }
}
