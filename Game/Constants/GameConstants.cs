namespace TransportGame.Game.Constants;

/// <summary>
/// Game constants and configuration values.
/// Ported from Godot GameConstants.gd.
/// </summary>
public static class GameConstants
{
    #region Building Types
    
    public const string BUILDING_FARM = "farm";
    public const string BUILDING_MINE = "mine";
    public const string BUILDING_DEPOT = "depot";
    public const string BUILDING_FACTORY = "factory";
    public const string BUILDING_CITY = "city";
    public const string BUILDING_STATION = "station";
    
    #endregion

    #region Resource Types
    
    public const string RESOURCE_WHEAT = "wheat";
    public const string RESOURCE_IRON = "iron";
    public const string RESOURCE_FOOD = "food";
    public const string RESOURCE_STEEL = "steel";
    public const string RESOURCE_MONEY = "money";
    
    #endregion

    #region Game Settings

    public const int TILE_SIZE = 5;
    public const int WORLD_WIDTH = 768;  // 3840 / 5 (doubled from 384)
    public const int WORLD_HEIGHT = 432; // 2160 / 5 (doubled from 216)
    
    // Camera settings
    public const float CAMERA_MIN_ZOOM = 0.25f;
    public const float CAMERA_MAX_ZOOM = 4.0f;
    public const float CAMERA_ZOOM_SPEED = 0.1f;
    public const float CAMERA_PAN_SPEED = 300.0f;
    
    // Game timing
    public const float PRODUCTION_TICK_INTERVAL = 2.0f; // seconds
    public const float AUTOSAVE_INTERVAL = 300.0f; // 5 minutes
    
    #endregion

    #region Building Costs
    
    public const int COST_FARM = 1000;
    public const int COST_MINE = 1500;
    public const int COST_DEPOT = 800;
    public const int COST_FACTORY = 2500;
    public const int COST_STATION = 1200;
    public const int COST_TRAIN = 5000;
    public const int COST_RAIL_PER_TILE = 100;
    
    #endregion

    #region Production Rates
    
    public const int FARM_WHEAT_PRODUCTION = 10;
    public const int MINE_IRON_PRODUCTION = 8;
    public const int FACTORY_FOOD_PRODUCTION = 6;
    public const int FACTORY_STEEL_PRODUCTION = 4;
    
    // Resource conversion ratios
    public const int WHEAT_TO_FOOD_RATIO = 2; // 2 wheat = 1 food
    public const int IRON_TO_STEEL_RATIO = 3; // 3 iron = 1 steel
    
    #endregion

    #region Storage Capacities
    
    public const int FARM_STORAGE_CAPACITY = 100;
    public const int MINE_STORAGE_CAPACITY = 80;
    public const int DEPOT_STORAGE_CAPACITY = 500;
    public const int FACTORY_STORAGE_CAPACITY = 200;
    public const int TRAIN_CARGO_CAPACITY = 50;
    
    #endregion

    #region UI Constants
    
    public const int UI_PANEL_WIDTH = 300;
    public const int UI_BUTTON_HEIGHT = 40;
    public const int UI_MARGIN = 10;
    
    // Colors (as hex strings for easy conversion)
    public const string COLOR_UI_BACKGROUND = "#2C3E50";
    public const string COLOR_UI_BUTTON = "#3498DB";
    public const string COLOR_UI_BUTTON_HOVER = "#2980B9";
    public const string COLOR_UI_TEXT = "#FFFFFF";
    public const string COLOR_UI_SUCCESS = "#27AE60";
    public const string COLOR_UI_WARNING = "#F39C12";
    public const string COLOR_UI_ERROR = "#E74C3C";
    
    #endregion

    #region File Paths
    
    public const string SAVE_DIRECTORY = "Saves";
    public const string SAVE_FILE_EXTENSION = ".tgsave";
    public const string CONFIG_FILE_NAME = "config.json";
    public const string LOG_DIRECTORY = "Logs";
    
    #endregion

    #region Network Constants (for future multiplayer)
    
    public const int DEFAULT_PORT = 7777;
    public const int MAX_PLAYERS = 8;
    public const float NETWORK_TICK_RATE = 20.0f; // Hz
    public const int MAX_PACKET_SIZE = 1024;
    
    #endregion

    #region Debug Settings
    
    public const bool DEBUG_SHOW_GRID = true;
    public const bool DEBUG_SHOW_COLLISION_BOXES = false;
    public const bool DEBUG_SHOW_PATHFINDING = false;
    public const bool DEBUG_SHOW_FPS = true;
    public const bool DEBUG_AUTO_EXIT_ON_ERROR = true;
    public const float DEBUG_AUTO_EXIT_TIMEOUT = 30.0f; // seconds
    
    #endregion
}

/// <summary>
/// Enumerations for game entities and states.
/// Ported from Godot Enums.gd.
/// </summary>
public enum EntityType
{
    Building,
    Train,
    Resource,
    UI,
    Tile
}

public enum GameState
{
    Menu,
    Playing,
    Paused,
    GameOver,
    Loading,
    Saving
}

public enum BuildingState
{
    UnderConstruction,
    Operational,
    Damaged,
    Destroyed,
    Upgrading
}

public enum TrainState
{
    Idle,
    Moving,
    Loading,
    Unloading,
    Broken
}

public enum ResourceType
{
    Wheat,
    Iron,
    Food,
    Steel,
    Money
}

public enum TileType
{
    Grass,
    Water,
    Mountain,
    Forest,
    Desert,
    Rail,
    Road,
    Farmland,
    Dirt,
    Hills,
    DeepWater,
    Beach
}

public enum InputMode
{
    Normal,
    BuildingPlacement,
    RailPlacement,
    Selection,
    Camera
}
