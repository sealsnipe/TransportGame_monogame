# Code Patterns & Architecture Reference

## 🏗️ Bewährte Architektur-Patterns

### 1. Singleton Manager Pattern
```csharp
// Beispiel: GameManager
public class GameManager : Node
{
    public static GameManager Instance { get; private set; }
    
    public override void _Ready()
    {
        Instance = this;
        // Initialization logic
    }
    
    // Public API methods
    public void StartNewGame() { /* ... */ }
    public bool HasMoney(int amount) { /* ... */ }
}
```

### 2. EventBus Pattern (Lose Kopplung)
```csharp
// EventBus für globale Events
public class EventBus : Node
{
    [Signal] public delegate void BuildingPlacedEventHandler(Building building, Vector2 position);
    [Signal] public delegate void ResourceChangedEventHandler(string resourceType, int newAmount);
    
    public static void EmitBuildingPlaced(Building building, Vector2 position)
    {
        Instance.EmitSignal(nameof(BuildingPlaced), building, position);
    }
}

// Usage in anderen Klassen:
EventBus.BuildingPlaced += OnBuildingPlaced;
EventBus.EmitBuildingPlaced(building, position);
```

### 3. Entity-Component Hierarchie
```csharp
// Basis-Entity
public abstract class Entity : Node2D
{
    public string EntityId { get; protected set; }
    public EntityType EntityType { get; protected set; }
    public int MaxHealth { get; protected set; }
    public int CurrentHealth { get; protected set; }
    
    public virtual void Initialize(Dictionary data) { }
    public virtual Dictionary GetSaveData() { return new Dictionary(); }
}

// Spezialisierte Entities
public abstract class Building : Entity
{
    public string BuildingType { get; set; }
    public bool IsOperational { get; protected set; }
    public Vector2 GridPosition { get; set; }
    
    public virtual bool CanPlaceAt(Vector2 position) { return true; }
    public virtual void StartConstruction() { }
}

public class ProductionBuilding : Building
{
    public string ProducedResource { get; protected set; }
    public int ProductionRate { get; protected set; }
    public int CurrentStorage { get; protected set; }
    public int MaxStorage { get; protected set; }
    
    protected virtual void UpdateProduction(float delta) { }
}
```

### 4. Resource Management Pattern
```csharp
public class ResourceManager : Node
{
    private Dictionary<string, int> _resources = new Dictionary<string, int>();
    
    [Signal] public delegate void ResourceAmountChangedEventHandler(string resourceType, int newAmount);
    
    public bool HasResource(string resourceType, int amount)
    {
        return GetResourceAmount(resourceType) >= amount;
    }
    
    public bool ConsumeResource(string resourceType, int amount)
    {
        if (!HasResource(resourceType, amount)) return false;
        
        _resources[resourceType] -= amount;
        EmitSignal(nameof(ResourceAmountChanged), resourceType, _resources[resourceType]);
        return true;
    }
    
    public void AddResource(string resourceType, int amount)
    {
        if (!_resources.ContainsKey(resourceType))
            _resources[resourceType] = 0;
            
        _resources[resourceType] += amount;
        EmitSignal(nameof(ResourceAmountChanged), resourceType, _resources[resourceType]);
    }
}
```

### 5. Safe Error Handling Pattern
```csharp
public class ErrorHandler : Node
{
    private int _errorCount = 0;
    private const int MAX_ERROR_COUNT = 10;
    
    public void HandleError(string errorMessage, string source = "", bool autoExit = true)
    {
        _errorCount++;
        
        var fullMessage = $"[ERROR {_errorCount}] {errorMessage}";
        if (!string.IsNullOrEmpty(source))
            fullMessage += $" (Source: {source})";
        
        LogError(fullMessage);
        
        if (_errorCount >= MAX_ERROR_COUNT || autoExit)
        {
            LogError($"CRITICAL: Too many errors ({_errorCount}). Shutting down.");
            await GetTree().CreateTimer(0.5f).Timeout;
            GetTree().Quit(1);
        }
    }
    
    public void HandleWarning(string warningMessage, string source = "")
    {
        var fullMessage = $"[WARNING] {warningMessage}";
        if (!string.IsNullOrEmpty(source))
            fullMessage += $" (Source: {source})";
        
        LogWarning(fullMessage);
    }
}
```

### 6. Save/Load System Pattern
```csharp
public class SaveSystem : Node
{
    public async Task<bool> SaveGameAsync(string fileName = "")
    {
        if (string.IsNullOrEmpty(fileName))
            fileName = "autosave_" + GetTimestamp();
        
        var filePath = _saveDirectory + fileName + SAVE_FILE_EXTENSION;
        
        try
        {
            var saveData = CollectSaveData();
            await WriteSaveFileAsync(filePath, saveData);
            
            EmitSignal(nameof(SaveCompleted), true, filePath);
            return true;
        }
        catch (Exception ex)
        {
            ErrorHandler.HandleError($"Failed to save game: {ex.Message}", "SaveSystem");
            EmitSignal(nameof(SaveCompleted), false, filePath);
            return false;
        }
    }
    
    private Dictionary CollectSaveData()
    {
        return new Dictionary
        {
            ["save_version"] = SAVE_VERSION,
            ["game_version"] = ProjectSettings.GetSetting("application/config/version", "1.0"),
            ["save_time"] = Time.GetUnixTimeFromSystem(),
            ["game_data"] = GameManager.GetSaveData(),
            ["resource_data"] = ResourceManager.GetSaveData()
        };
    }
}
```

## 🛠️ Utility Patterns

### 1. Constants Organization
```csharp
public static class GameConstants
{
    // Building Types
    public const string BUILDING_FARM = "farm";
    public const string BUILDING_MINE = "mine";
    public const string BUILDING_DEPOT = "depot";
    public const string BUILDING_FACTORY = "factory";
    
    // Resource Types
    public const string RESOURCE_WHEAT = "wheat";
    public const string RESOURCE_IRON = "iron";
    public const string RESOURCE_FOOD = "food";
    public const string RESOURCE_STEEL = "steel";
    
    // Game Settings
    public const int TILE_SIZE = 64;
    public const int WORLD_WIDTH = 100;
    public const int WORLD_HEIGHT = 100;
    
    // Costs
    public const int COST_FARM = 1000;
    public const int COST_MINE = 1500;
    public const int COST_TRAIN = 5000;
}

public enum EntityType
{
    Building,
    Train,
    Resource,
    UI
}

public enum GameState
{
    Menu,
    Playing,
    Paused,
    GameOver
}
```

### 2. Async/Await Pattern für I/O
```csharp
public async Task<bool> LoadGameAsync(string filePath)
{
    try
    {
        if (!File.Exists(filePath))
        {
            ErrorHandler.HandleWarning($"Save file does not exist: {filePath}");
            return false;
        }
        
        var jsonContent = await File.ReadAllTextAsync(filePath);
        var saveData = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonContent);
        
        await ApplySaveDataAsync(saveData);
        
        EmitSignal(nameof(LoadCompleted), true, filePath);
        return true;
    }
    catch (Exception ex)
    {
        ErrorHandler.HandleError($"Failed to load game: {ex.Message}", "SaveSystem");
        EmitSignal(nameof(LoadCompleted), false, filePath);
        return false;
    }
}
```

### 3. Factory Pattern für Entities
```csharp
public static class EntityFactory
{
    private static readonly Dictionary<string, Func<Building>> BuildingFactories = new()
    {
        [GameConstants.BUILDING_FARM] = () => new Farm(),
        [GameConstants.BUILDING_MINE] = () => new Mine(),
        [GameConstants.BUILDING_DEPOT] = () => new Depot(),
        [GameConstants.BUILDING_FACTORY] = () => new Factory()
    };
    
    public static Building CreateBuilding(string buildingType)
    {
        if (BuildingFactories.TryGetValue(buildingType, out var factory))
        {
            var building = factory();
            building.BuildingType = buildingType;
            return building;
        }
        
        ErrorHandler.HandleError($"Unknown building type: {buildingType}", "EntityFactory");
        return null;
    }
}
```

## 🎯 Best Practices

### 1. Null-Safe Programming
```csharp
// Immer null-checks
if (tilemapManager == null)
{
    ErrorHandler.HandleError("TilemapManager is null!", "Main._start_new_game");
    return;
}

// Null-conditional operators verwenden
var buildingCount = buildings?.Count ?? 0;
player?.TakeDamage(damage);
```

### 2. Defensive Programming
```csharp
public bool PlaceBuilding(Building building, Vector2 gridPosition)
{
    // Validate inputs
    if (building == null)
    {
        ErrorHandler.HandleWarning("Cannot place null building");
        return false;
    }
    
    if (!IsValidGridPosition(gridPosition))
    {
        ErrorHandler.HandleWarning($"Invalid grid position: {gridPosition}");
        return false;
    }
    
    // Check if position is buildable
    if (!IsTileBuildable(gridPosition))
    {
        return false;
    }
    
    // Perform the action
    _buildingGrid[gridPosition] = building;
    building.GridPosition = gridPosition;
    
    return true;
}
```

### 3. Performance-Optimized Patterns
```csharp
// Object pooling für häufig erstellte Objekte
public class TrainPool
{
    private readonly Queue<Train> _availableTrains = new();
    
    public Train GetTrain()
    {
        if (_availableTrains.Count > 0)
        {
            return _availableTrains.Dequeue();
        }
        
        return new Train();
    }
    
    public void ReturnTrain(Train train)
    {
        train.Reset();
        _availableTrains.Enqueue(train);
    }
}

// Caching für teure Berechnungen
private readonly Dictionary<Vector2, List<Vector2>> _pathCache = new();

public List<Vector2> FindPath(Vector2 start, Vector2 end)
{
    var key = new Vector2(start.GetHashCode(), end.GetHashCode());
    
    if (_pathCache.TryGetValue(key, out var cachedPath))
    {
        return new List<Vector2>(cachedPath);
    }
    
    var path = CalculatePath(start, end);
    _pathCache[key] = path;
    
    return path;
}
```

## 🔄 Migration Guidelines

### Von Godot zu Custom Engine:
1. **Signals → Events**: `[Signal]` wird zu `event Action<T>`
2. **Node → GameObject**: Hierarchie-System beibehalten
3. **Vector2 → System.Numerics.Vector2**: Ähnliche API
4. **await → Task.Run**: Async patterns anpassen
5. **GDScript → C#**: Logik 1:1 portieren

### Architektur-Entscheidungen:
- **ECS vs OOP**: Für Performance ECS, für Einfachheit OOP
- **MonoGame vs Unity**: Kontrolle vs Convenience
- **Custom vs Engine**: Flexibilität vs Entwicklungszeit
