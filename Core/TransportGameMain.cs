using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TransportGame.Game.Managers;
using TransportGame.Game.Systems;
using TransportGame.Game.States;

namespace TransportGame.Core;

/// <summary>
/// Main game class that handles the core game loop and initialization.
/// Replaces the Godot Main scene functionality.
/// </summary>
public class TransportGameMain : Microsoft.Xna.Framework.Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch? _spriteBatch;
    
    // Core Managers (ported from Godot singletons)
    private GameManager? _gameManager;
    private EventBus? _eventBus;
    private ResourceManager? _resourceManager;
    private TilemapManager? _tilemapManager;
    private ErrorHandler? _errorHandler;

    // JSON-based Data Managers
    private TileDefinitionManager? _tileDefinitionManager;
    private LocalizationManager? _localizationManager;
    private SettingsManager? _settingsManager;
    private BuildingDefinitionManager? _buildingDefinitionManager;
    private ResourceDefinitionManager? _resourceDefinitionManager;
    
    // Game Systems
    private InputSystem? _inputSystem;
    private RenderSystem? _renderSystem;
    private CameraSystem? _cameraSystem;

    // Mouse Interaction & Tooltip Systems
    private MouseInteractionSystem? _mouseInteractionSystem;
    private TooltipSystem? _tooltipSystem;
    private BuildingPlacementSystem? _buildingPlacementSystem;
    private BuildingRenderSystem? _buildingRenderSystem;
    private ProductionSystem? _productionSystem;

    // Industry & Building UI Systems
    private IndustryGenerationSystem? _industryGenerationSystem;
    private BuildingUISystem? _buildingUISystem;

    // State Management System (replaces old MenuSystem)
    private StateManager? _stateManager;
    private PlayingState? _playingState;
    private TransportGame.Game.States.MenuState? _menuState;
    private OptionsState? _optionsState;
    
    // Game State
    private bool _isInitialized = false;
    
    public TransportGameMain()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        // Set window properties
        Window.Title = "Transport Game - MonoGame Edition";
        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.PreferredBackBufferHeight = 720;

        // CRITICAL: Ensure window can receive input
        Window.AllowUserResizing = false;
        IsFixedTimeStep = true;

        _graphics.ApplyChanges();

        Console.WriteLine("*** TransportGameMain: Constructor completed, window should be focusable ***");
    }

    protected override void Initialize()
    {
        try
        {
            // Initialize error handler first
            _errorHandler = new ErrorHandler();
            
            // Initialize core managers (similar to Godot autoload singletons)
            InitializeManagers();
            
            // Initialize game systems
            InitializeSystems();

            // Subscribe to camera events
            if (_eventBus != null)
            {
                _eventBus.KeyPressed += OnKeyPressed;
            }

            _isInitialized = true;
            _errorHandler.LogInfo("TransportGameMain initialized successfully");
            
            base.Initialize();
        }
        catch (Exception ex)
        {
            _errorHandler?.HandleError($"Failed to initialize game: {ex.Message}", "TransportGameMain.Initialize");
            throw;
        }
    }

    private void InitializeManagers()
    {
        // Initialize in dependency order (similar to Godot autoload order)
        _eventBus = new EventBus();
        _gameManager = new GameManager(_eventBus);
        _resourceManager = new ResourceManager(_eventBus);
        _tilemapManager = new TilemapManager(_eventBus);

        // Initialize JSON-based data managers
        Console.WriteLine("[JSON-INIT] TransportGameMain: Initializing JSON data managers...");
        _tileDefinitionManager = new TileDefinitionManager(_errorHandler!);
        _localizationManager = new LocalizationManager(_errorHandler!);
        _settingsManager = new SettingsManager(_errorHandler!);
        _buildingDefinitionManager = new BuildingDefinitionManager();
        _resourceDefinitionManager = new ResourceDefinitionManager();

        // Load JSON data
        _tileDefinitionManager.LoadDefinitions();
        _localizationManager.LoadLanguages();
        _settingsManager.LoadSettings();
        _buildingDefinitionManager.LoadDefinitions();
        _resourceDefinitionManager.LoadDefinitions();
        Console.WriteLine("[JSON-INIT] TransportGameMain: JSON data managers initialized");

        _errorHandler?.LogInfo("Core managers and JSON data managers initialized");
    }
    
    private void InitializeSystems()
    {
        _inputSystem = new InputSystem(_eventBus!);
        _renderSystem = new RenderSystem(_eventBus!);
        _cameraSystem = new CameraSystem(_eventBus!, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);

        // Initialize building placement system first
        _buildingPlacementSystem = new BuildingPlacementSystem(_eventBus!, _tilemapManager!, _buildingDefinitionManager!);

        // Initialize tooltip systems (after building system is ready)
        _tooltipSystem = new TooltipSystem(_eventBus!, _errorHandler!);
        _mouseInteractionSystem = new MouseInteractionSystem(_eventBus!, _tilemapManager!, _buildingPlacementSystem!, _errorHandler!);

        // Initialize building render system
        _buildingRenderSystem = new BuildingRenderSystem(_eventBus!, _buildingPlacementSystem!, _buildingDefinitionManager!);
        _buildingRenderSystem.SetIndustryGenerationSystem(_industryGenerationSystem!);

        // Initialize production system
        _productionSystem = new ProductionSystem(_eventBus!, _buildingPlacementSystem!, _resourceDefinitionManager!);

        // Initialize industry generation system
        _industryGenerationSystem = new IndustryGenerationSystem(_tilemapManager!, _buildingDefinitionManager!);

        // Initialize building UI system
        _buildingUISystem = new BuildingUISystem(_buildingDefinitionManager!, _buildingPlacementSystem!);

        // Update tooltip system with correct screen size
        _tooltipSystem.UpdateScreenSize(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);

        // Connect tooltip system to mouse interaction system
        _mouseInteractionSystem.SetTooltipSystem(_tooltipSystem);
        _mouseInteractionSystem.SetIndustryGenerationSystem(_industryGenerationSystem!);
        _mouseInteractionSystem.SetBuildingUISystem(_buildingUISystem!);

        // Connect JSON data managers to tooltip system
        _tooltipSystem.SetDataManagers(_tileDefinitionManager!, _localizationManager!);
        _tooltipSystem.SetSettingsManager(_settingsManager!);

        // Initialize state management system
        _stateManager = new StateManager(_errorHandler!);
        _stateManager.Initialize();

        // Create game states
        _playingState = new PlayingState(_errorHandler!, _renderSystem!, _inputSystem!, _tooltipSystem!, _mouseInteractionSystem!, _stateManager!, GraphicsDevice, _cameraSystem!, _settingsManager!);
        _menuState = new TransportGame.Game.States.MenuState(_errorHandler!, _stateManager!, GraphicsDevice, _inputSystem!);
        _optionsState = new OptionsState(_errorHandler!, _stateManager!, GraphicsDevice, _inputSystem!, _settingsManager!);

        // Initialize states
        _playingState.Initialize();
        _menuState.Initialize();
        _optionsState.Initialize();

        // Connect states
        _menuState.SetOptionsState(_optionsState);
        _playingState.SetMenuState(_menuState);

        // Start with playing state
        _stateManager.PushState(_playingState);

        Console.WriteLine("TransportGameMain: State management system initialized");
        _errorHandler?.LogInfo("Game systems initialized (including state management)");
    }

    protected override void LoadContent()
    {
        try
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            
            // Load game content
            _renderSystem?.LoadContent(Content, GraphicsDevice);
            _tilemapManager?.LoadContent(Content);
            _buildingRenderSystem?.LoadContent(GraphicsDevice);
            _buildingUISystem?.LoadContent(GraphicsDevice);
            _buildingUISystem?.UpdateScreenSize(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);

            // Generate natural industries after world is created
            _industryGenerationSystem?.GenerateIndustries();

            // Start new game (equivalent to Godot's _ready)
            _gameManager?.StartNewGame();
            
            _errorHandler?.LogInfo("Content loaded successfully");
        }
        catch (Exception ex)
        {
            _errorHandler?.HandleError($"Failed to load content: {ex.Message}", "TransportGameMain.LoadContent");
            throw;
        }
    }

    protected override void Update(GameTime gameTime)
    {
        if (!_isInitialized)
            return;

        try
        {
            // Removed frame-by-frame logging

            // Handle exit conditions (removed ESC key - now only gamepad back button exits)
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                Exit();
                return;
            }

            // Menu control is now handled directly in InputSystem

            // Update camera system (always needed)
            _cameraSystem?.Update(gameTime, _inputSystem!);

            // Update core managers (always needed)
            _gameManager?.Update(gameTime);
            _resourceManager?.Update(gameTime);
            _tilemapManager?.Update(gameTime);

            // Update production system (always needed for building production)
            _productionSystem?.Update(gameTime);

            // Update state management system (handles game/menu states)
            _stateManager?.Update(gameTime);

            // Update mouse interaction system only when in PlayingState (not in menu)
            if (_stateManager?.CurrentState is PlayingState)
            {
                _mouseInteractionSystem?.Update(gameTime, _inputSystem!, _cameraSystem!);

                // Update building placement system (always update mouse position)
                if (_buildingPlacementSystem != null)
                {
                    var mousePos = _inputSystem!.GetMousePosition();
                    var mouseWorldPos = _cameraSystem?.ScreenToWorld(mousePos) ?? System.Numerics.Vector2.Zero;

                    // Debug output for placement mode
                    if (_buildingPlacementSystem.IsPlacementMode && DateTime.Now.Millisecond % 1000 < 50)
                    {
                        Console.WriteLine($"[MAIN] Placement mode active: {_buildingPlacementSystem.SelectedBuildingId}, Mouse: Screen({mousePos.X},{mousePos.Y}) -> World({mouseWorldPos.X:F1},{mouseWorldPos.Y:F1})");
                    }

                    _buildingPlacementSystem.UpdatePreview(mouseWorldPos);
                }
            }

            base.Update(gameTime);
        }
        catch (Exception ex)
        {
            _errorHandler?.HandleError($"Error in Update: {ex.Message}", "TransportGameMain.Update");
        }
    }

    protected override void Draw(GameTime gameTime)
    {
        if (!_isInitialized || _spriteBatch == null)
            return;
            
        try
        {
            GraphicsDevice.Clear(Color.DarkGreen); // Temporary background color

            // Begin with camera transform for world objects
            var cameraTransform = _cameraSystem?.Transform ?? Matrix.Identity;
            _spriteBatch.Begin(transformMatrix: cameraTransform);

            // Render world objects (affected by camera)
            _tilemapManager?.Draw(_spriteBatch, gameTime);
            _renderSystem?.Draw(_spriteBatch, gameTime);
            _buildingRenderSystem?.Draw(_spriteBatch, gameTime);

            _spriteBatch.End();

            // Begin without transform for UI and state rendering
            _spriteBatch.Begin();

            // Render UI elements (not affected by camera)
            _renderSystem?.DrawUI(_spriteBatch, gameTime, _cameraSystem, _tooltipSystem);

            // Draw building UI (only when in playing state)
            if (_stateManager?.CurrentState is PlayingState)
            {
                _buildingUISystem?.Draw(_spriteBatch, gameTime);
            }

            // Draw current state (handles game/menu rendering)
            _stateManager?.Draw(_spriteBatch);

            _spriteBatch.End();

            // SCREENSHOT SYSTEM - Press F12 to capture
            if (Keyboard.GetState().IsKeyDown(Keys.F12))
            {
                CaptureScreenshot();
            }

            base.Draw(gameTime);
        }
        catch (Exception ex)
        {
            _errorHandler?.HandleError($"Error in Draw: {ex.Message}", "TransportGameMain.Draw");
        }
    }

    private void CaptureScreenshot()
    {
        try
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            var filename = $"screenshot_{timestamp}.png";

            // Create render target with screen size
            var renderTarget = new RenderTarget2D(GraphicsDevice,
                GraphicsDevice.Viewport.Width,
                GraphicsDevice.Viewport.Height);

            // Set render target
            GraphicsDevice.SetRenderTarget(renderTarget);
            GraphicsDevice.Clear(Color.DarkGreen);

            // Redraw everything to render target
            var cameraTransform = _cameraSystem?.Transform ?? Matrix.Identity;
            _spriteBatch.Begin(transformMatrix: cameraTransform);
            _tilemapManager?.Draw(_spriteBatch, new GameTime());
            _renderSystem?.Draw(_spriteBatch, new GameTime());
            _spriteBatch.End();

            _spriteBatch.Begin();
            _renderSystem?.DrawUI(_spriteBatch, new GameTime(), _cameraSystem, _tooltipSystem);
            _stateManager?.Draw(_spriteBatch);
            _spriteBatch.End();

            // Reset render target
            GraphicsDevice.SetRenderTarget(null);

            // Save screenshot
            using (var stream = File.Create(filename))
            {
                renderTarget.SaveAsPng(stream, renderTarget.Width, renderTarget.Height);
            }

            renderTarget.Dispose();
            _errorHandler?.LogInfo($"Screenshot saved: {filename}");
        }
        catch (Exception ex)
        {
            _errorHandler?.HandleError($"Error capturing screenshot: {ex.Message}", "TransportGameMain.CaptureScreenshot");
        }
    }

    protected override void UnloadContent()
    {
        try
        {
            _gameManager?.Dispose();
            _resourceManager?.Dispose();
            _tilemapManager?.Dispose();
            _renderSystem?.Dispose();
            _inputSystem?.Dispose();
            _cameraSystem?.Dispose();

            // Dispose JSON data managers
            _tileDefinitionManager?.Dispose();
            _localizationManager?.Dispose();
            _settingsManager?.Dispose();
            _buildingDefinitionManager?.Dispose();
            _resourceDefinitionManager?.Dispose();

            // Dispose tooltip systems
            _tooltipSystem?.Dispose();
            _mouseInteractionSystem?.Dispose();
            _buildingPlacementSystem?.Dispose();
            _buildingRenderSystem?.Dispose();
            _productionSystem?.Dispose();

            // Dispose industry and building UI systems
            _industryGenerationSystem?.Dispose();
            _buildingUISystem?.Dispose();

            // Dispose state management system
            _stateManager?.Dispose();
            
            _errorHandler?.LogInfo("Game unloaded successfully");
        }
        catch (Exception ex)
        {
            _errorHandler?.HandleError($"Error during unload: {ex.Message}", "TransportGameMain.UnloadContent");
        }
        
        base.UnloadContent();
    }

    private void OnKeyPressed(string key)
    {
        if (key == "camera_follow_train" && _cameraSystem != null && _renderSystem != null)
        {
            var trainPos = _renderSystem.GetDemoTrainPosition();
            if (trainPos.HasValue)
            {
                _cameraSystem.SetFollowTarget(trainPos.Value);
                _errorHandler?.LogInfo("Camera now following train");
            }
        }
        else if (key == "game_exit")
        {
            _errorHandler?.LogInfo("Game exit requested via menu");
            Exit();
        }
    }
}
