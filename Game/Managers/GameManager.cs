using Microsoft.Xna.Framework;
using TransportGame.Game.Constants;

namespace TransportGame.Game.Managers;

/// <summary>
/// Central game manager that handles game state and coordination.
/// Ported from Godot GameManager.gd singleton.
/// </summary>
public class GameManager : IDisposable
{
    private readonly EventBus _eventBus;
    private readonly ErrorHandler _errorHandler;
    
    // Game State
    private GameState _currentState = GameState.Menu;
    private bool _isPaused = false;
    private float _gameTime = 0f;
    private float _lastProductionTick = 0f;
    private float _lastAutoSave = 0f;
    
    // Game Statistics
    private int _totalMoney = 10000; // Starting money
    private int _totalBuildings = 0;
    private int _totalTrains = 0;
    private float _totalPlayTime = 0f;
    
    public GameState CurrentState => _currentState;
    public bool IsPaused => _isPaused;
    public int TotalMoney => _totalMoney;
    public int TotalBuildings => _totalBuildings;
    public int TotalTrains => _totalTrains;
    public float TotalPlayTime => _totalPlayTime;

    public GameManager(EventBus eventBus)
    {
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _errorHandler = new ErrorHandler();
        
        // Subscribe to relevant events
        SubscribeToEvents();
        
        _errorHandler.LogInfo("GameManager initialized");
    }

    private void SubscribeToEvents()
    {
        _eventBus.BuildingPlaced += OnBuildingPlaced;
        _eventBus.BuildingRemoved += OnBuildingRemoved;
        _eventBus.TrainSpawned += OnTrainSpawned;
        _eventBus.TrainDestroyed += OnTrainDestroyed;
        _eventBus.ResourceChanged += OnResourceChanged;
    }

    /// <summary>
    /// Starts a new game session.
    /// </summary>
    public void StartNewGame()
    {
        try
        {
            _errorHandler.LogInfo("Starting new game...");
            
            // Reset game state
            _currentState = GameState.Loading;
            _gameTime = 0f;
            _totalPlayTime = 0f;
            _lastProductionTick = 0f;
            _lastAutoSave = 0f;
            
            // Reset statistics
            _totalMoney = 10000;
            _totalBuildings = 0;
            _totalTrains = 0;
            
            // Initialize game world
            InitializeWorld();
            
            // Change to playing state
            _currentState = GameState.Playing;
            _eventBus.EmitGameStarted();
            
            _errorHandler.LogInfo("New game started successfully");
        }
        catch (Exception ex)
        {
            _errorHandler.HandleError($"Failed to start new game: {ex.Message}", "GameManager.StartNewGame");
        }
    }

    /// <summary>
    /// Updates the game manager each frame.
    /// </summary>
    public void Update(GameTime gameTime)
    {
        if (_currentState != GameState.Playing || _isPaused)
            return;
            
        try
        {
            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _gameTime += deltaTime;
            _totalPlayTime += deltaTime;
            
            // Handle production ticks
            if (_gameTime - _lastProductionTick >= GameConstants.PRODUCTION_TICK_INTERVAL)
            {
                ProcessProductionTick();
                _lastProductionTick = _gameTime;
            }
            
            // Handle auto-save
            if (_gameTime - _lastAutoSave >= GameConstants.AUTOSAVE_INTERVAL)
            {
                AutoSave();
                _lastAutoSave = _gameTime;
            }
        }
        catch (Exception ex)
        {
            _errorHandler.HandleError($"Error in GameManager.Update: {ex.Message}", "GameManager.Update");
        }
    }

    /// <summary>
    /// Pauses or resumes the game.
    /// </summary>
    public void SetPaused(bool paused)
    {
        if (_isPaused == paused)
            return;
            
        _isPaused = paused;
        
        if (_isPaused)
        {
            _eventBus.EmitGamePaused();
            _errorHandler.LogInfo("Game paused");
        }
        else
        {
            _eventBus.EmitGameResumed();
            _errorHandler.LogInfo("Game resumed");
        }
    }

    /// <summary>
    /// Checks if the player has enough money for a purchase.
    /// </summary>
    public bool HasMoney(int amount)
    {
        return _totalMoney >= amount;
    }

    /// <summary>
    /// Spends money if available.
    /// </summary>
    public bool SpendMoney(int amount, string reason = "")
    {
        if (!HasMoney(amount))
        {
            _errorHandler.HandleWarning($"Insufficient funds: need {amount}, have {_totalMoney}");
            return false;
        }
        
        _totalMoney -= amount;
        _eventBus.EmitResourceChanged(GameConstants.RESOURCE_MONEY, _totalMoney);
        
        if (!string.IsNullOrEmpty(reason))
        {
            _errorHandler.LogInfo($"Spent {amount} money for: {reason}. Remaining: {_totalMoney}");
        }
        
        return true;
    }

    /// <summary>
    /// Adds money to the player's account.
    /// </summary>
    public void AddMoney(int amount, string reason = "")
    {
        _totalMoney += amount;
        _eventBus.EmitResourceChanged(GameConstants.RESOURCE_MONEY, _totalMoney);
        
        if (!string.IsNullOrEmpty(reason))
        {
            _errorHandler.LogInfo($"Earned {amount} money from: {reason}. Total: {_totalMoney}");
        }
    }

    private void InitializeWorld()
    {
        // This will be expanded when we implement the tilemap system
        _errorHandler.LogInfo("World initialized");
    }

    private void ProcessProductionTick()
    {
        // This will handle all production buildings updating their output
        // Production tick logging reduced - only log when there's actual production activity
        // _errorHandler.LogInfo($"Production tick at game time: {_gameTime:F1}s");
    }

    private void AutoSave()
    {
        try
        {
            var fileName = $"autosave_{DateTime.Now:yyyyMMdd_HHmmss}";
            // TODO: Implement actual save system
            _eventBus.EmitGameSaved(fileName);
            _errorHandler.LogInfo($"Auto-saved game as: {fileName}");
        }
        catch (Exception ex)
        {
            _errorHandler.HandleError($"Auto-save failed: {ex.Message}", "GameManager.AutoSave");
        }
    }

    #region Event Handlers

    private void OnBuildingPlaced(Entities.Building building, System.Numerics.Vector2 position)
    {
        _totalBuildings++;
        _errorHandler.LogInfo($"Building placed: {building.BuildingType} at {position}. Total buildings: {_totalBuildings}");
    }

    private void OnBuildingRemoved(Entities.Building building)
    {
        _totalBuildings = Math.Max(0, _totalBuildings - 1);
        _errorHandler.LogInfo($"Building removed: {building.BuildingType}. Total buildings: {_totalBuildings}");
    }

    private void OnTrainSpawned(Entities.Train train, System.Numerics.Vector2 position)
    {
        _totalTrains++;
        _errorHandler.LogInfo($"Train spawned at {position}. Total trains: {_totalTrains}");
    }

    private void OnTrainDestroyed(Entities.Train train)
    {
        _totalTrains = Math.Max(0, _totalTrains - 1);
        _errorHandler.LogInfo($"Train destroyed. Total trains: {_totalTrains}");
    }

    private void OnResourceChanged(string resourceType, int newAmount)
    {
        if (resourceType == GameConstants.RESOURCE_MONEY)
        {
            // Money changes are handled directly by SpendMoney/AddMoney
            return;
        }
        
        _errorHandler.LogInfo($"Resource changed: {resourceType} = {newAmount}");
    }

    #endregion

    public void Dispose()
    {
        // Unsubscribe from events
        _eventBus.BuildingPlaced -= OnBuildingPlaced;
        _eventBus.BuildingRemoved -= OnBuildingRemoved;
        _eventBus.TrainSpawned -= OnTrainSpawned;
        _eventBus.TrainDestroyed -= OnTrainDestroyed;
        _eventBus.ResourceChanged -= OnResourceChanged;
        
        _errorHandler.LogInfo("GameManager disposed");
    }
}
