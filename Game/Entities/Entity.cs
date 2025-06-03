using System.Numerics;
using TransportGame.Game.Constants;

namespace TransportGame.Game.Entities;

/// <summary>
/// Base class for all game entities.
/// Ported from Godot Entity.gd.
/// </summary>
public abstract class Entity
{
    // Core properties
    public string EntityId { get; protected set; }
    public EntityType EntityType { get; protected set; }
    public Vector2 Position { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Health system
    public int MaxHealth { get; protected set; } = 100;
    public int CurrentHealth { get; protected set; } = 100;
    
    // Timing
    public float CreationTime { get; private set; }
    public float LastUpdateTime { get; protected set; }
    
    protected Entity(EntityType entityType)
    {
        EntityId = Guid.NewGuid().ToString();
        EntityType = entityType;
        CreationTime = 0f; // Will be set when added to game world
        LastUpdateTime = 0f;
    }

    /// <summary>
    /// Initializes the entity with data (used for loading from save files).
    /// </summary>
    public virtual void Initialize(Dictionary<string, object> data)
    {
        if (data.TryGetValue("entity_id", out var id) && id is string entityId)
            EntityId = entityId;
            
        if (data.TryGetValue("position_x", out var x) && data.TryGetValue("position_y", out var y))
        {
            if (x is float fx && y is float fy)
                Position = new Vector2(fx, fy);
        }
        
        if (data.TryGetValue("is_active", out var active) && active is bool isActive)
            IsActive = isActive;
            
        if (data.TryGetValue("current_health", out var health) && health is int currentHealth)
            CurrentHealth = Math.Clamp(currentHealth, 0, MaxHealth);
            
        if (data.TryGetValue("creation_time", out var time) && time is float creationTime)
            CreationTime = creationTime;
    }

    /// <summary>
    /// Gets save data for this entity.
    /// </summary>
    public virtual Dictionary<string, object> GetSaveData()
    {
        return new Dictionary<string, object>
        {
            ["entity_id"] = EntityId,
            ["entity_type"] = EntityType.ToString(),
            ["position_x"] = Position.X,
            ["position_y"] = Position.Y,
            ["is_active"] = IsActive,
            ["max_health"] = MaxHealth,
            ["current_health"] = CurrentHealth,
            ["creation_time"] = CreationTime,
            ["last_update_time"] = LastUpdateTime
        };
    }

    /// <summary>
    /// Updates the entity. Called each frame.
    /// </summary>
    public virtual void Update(float deltaTime, float gameTime)
    {
        LastUpdateTime = gameTime;
        
        if (!IsActive)
            return;
            
        // Override in derived classes for specific update logic
    }

    /// <summary>
    /// Damages the entity.
    /// </summary>
    public virtual bool TakeDamage(int damage, string source = "")
    {
        if (!IsActive || CurrentHealth <= 0)
            return false;
            
        CurrentHealth = Math.Max(0, CurrentHealth - damage);
        
        if (CurrentHealth <= 0)
        {
            OnDestroyed(source);
            return true; // Entity was destroyed
        }
        
        OnDamaged(damage, source);
        return false; // Entity survived
    }

    /// <summary>
    /// Heals the entity.
    /// </summary>
    public virtual void Heal(int healAmount)
    {
        if (!IsActive)
            return;
            
        var oldHealth = CurrentHealth;
        CurrentHealth = Math.Min(MaxHealth, CurrentHealth + healAmount);
        
        if (CurrentHealth > oldHealth)
        {
            OnHealed(CurrentHealth - oldHealth);
        }
    }

    /// <summary>
    /// Gets the distance to another entity.
    /// </summary>
    public float GetDistanceTo(Entity other)
    {
        return Vector2.Distance(Position, other.Position);
    }

    /// <summary>
    /// Gets the distance to a position.
    /// </summary>
    public float GetDistanceTo(Vector2 position)
    {
        return Vector2.Distance(Position, position);
    }

    /// <summary>
    /// Checks if this entity is within range of another entity.
    /// </summary>
    public bool IsInRange(Entity other, float range)
    {
        return GetDistanceTo(other) <= range;
    }

    /// <summary>
    /// Checks if this entity is within range of a position.
    /// </summary>
    public bool IsInRange(Vector2 position, float range)
    {
        return GetDistanceTo(position) <= range;
    }

    /// <summary>
    /// Gets the health percentage (0.0 to 1.0).
    /// </summary>
    public float GetHealthPercentage()
    {
        return MaxHealth > 0 ? (float)CurrentHealth / MaxHealth : 0f;
    }

    /// <summary>
    /// Checks if the entity is damaged.
    /// </summary>
    public bool IsDamaged()
    {
        return CurrentHealth < MaxHealth;
    }

    /// <summary>
    /// Checks if the entity is destroyed.
    /// </summary>
    public bool IsDestroyed()
    {
        return CurrentHealth <= 0;
    }

    /// <summary>
    /// Sets the creation time (called when entity is added to world).
    /// </summary>
    public void SetCreationTime(float gameTime)
    {
        CreationTime = gameTime;
    }

    /// <summary>
    /// Gets the age of the entity in seconds.
    /// </summary>
    public float GetAge(float currentGameTime)
    {
        return currentGameTime - CreationTime;
    }

    #region Virtual Event Methods

    /// <summary>
    /// Called when the entity takes damage but survives.
    /// </summary>
    protected virtual void OnDamaged(int damage, string source)
    {
        // Override in derived classes
    }

    /// <summary>
    /// Called when the entity is destroyed.
    /// </summary>
    protected virtual void OnDestroyed(string source)
    {
        IsActive = false;
        // Override in derived classes for cleanup
    }

    /// <summary>
    /// Called when the entity is healed.
    /// </summary>
    protected virtual void OnHealed(int healAmount)
    {
        // Override in derived classes
    }

    #endregion

    public override string ToString()
    {
        return $"{GetType().Name}({EntityId[..8]}...) at {Position} [{CurrentHealth}/{MaxHealth}HP]";
    }

    public override bool Equals(object? obj)
    {
        return obj is Entity other && EntityId == other.EntityId;
    }

    public override int GetHashCode()
    {
        return EntityId.GetHashCode();
    }
}
