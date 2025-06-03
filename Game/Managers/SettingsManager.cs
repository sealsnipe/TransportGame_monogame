using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using TransportGame.Game.Models;

namespace TransportGame.Game.Managers
{
    /// <summary>
    /// Manages game settings persistence and provides access to user preferences.
    /// Follows the same pattern as TileDefinitionManager for consistency.
    /// </summary>
    public class SettingsManager : IDisposable
    {
        private readonly ErrorHandler _errorHandler;
        private readonly string _settingsPath;
        private readonly string _defaultSettingsPath;
        private readonly string _userSettingsPath;
        private GameSettings _currentSettings;
        private bool _isInitialized = false;

        // Events for settings changes
        public event Action<GameSettings>? SettingsChanged;
        public event Action<DisplaySettings>? DisplaySettingsChanged;
        public event Action<ControlSettings>? ControlSettingsChanged;
        public event Action<AudioSettings>? AudioSettingsChanged;

        public SettingsManager(ErrorHandler errorHandler)
        {
            _errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
            _settingsPath = Path.Combine("Game", "Data", "Settings");
            _defaultSettingsPath = Path.Combine(_settingsPath, "default-settings.json");
            _userSettingsPath = Path.Combine(_settingsPath, "settings.json");
            _currentSettings = new GameSettings();

            Console.WriteLine("[SETTINGS] SettingsManager: Constructor called");
            _errorHandler.LogInfo("SettingsManager initialized");
        }

        /// <summary>
        /// Loads settings from user file, or creates from defaults if not found.
        /// </summary>
        public void LoadSettings()
        {
            try
            {
                Console.WriteLine($"[SETTINGS] SettingsManager: Loading settings from {_settingsPath}");

                if (!Directory.Exists(_settingsPath))
                {
                    _errorHandler.HandleError($"Settings directory not found: {_settingsPath}", "SettingsManager.LoadSettings");
                    return;
                }

                // Try to load user settings first
                if (File.Exists(_userSettingsPath))
                {
                    Console.WriteLine("[SETTINGS] SettingsManager: Loading user settings");
                    LoadUserSettings();
                }
                else
                {
                    Console.WriteLine("[SETTINGS] SettingsManager: No user settings found, creating from defaults");
                    CreateDefaultUserSettings();
                }

                _isInitialized = true;
                _errorHandler.LogInfo("Settings loaded successfully");
                Console.WriteLine("[SETTINGS] SettingsManager: Settings loaded successfully");
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError($"Failed to load settings: {ex.Message}", "SettingsManager.LoadSettings");
                Console.WriteLine($"[SETTINGS] SettingsManager: ERROR loading settings: {ex.Message}");
                
                // Fall back to default settings
                _currentSettings = new GameSettings();
                _isInitialized = true;
            }
        }

        /// <summary>
        /// Loads user settings from settings.json.
        /// </summary>
        private void LoadUserSettings()
        {
            try
            {
                var jsonContent = File.ReadAllText(_userSettingsPath);
                var settings = JsonSerializer.Deserialize<GameSettings>(jsonContent, GetJsonOptions());

                if (settings == null)
                {
                    _errorHandler.HandleError("Failed to deserialize user settings", "SettingsManager.LoadUserSettings");
                    CreateDefaultUserSettings();
                    return;
                }

                // Validate settings
                if (!ValidateSettings(settings))
                {
                    _errorHandler.HandleWarning("Invalid user settings detected, using defaults for invalid values", "SettingsManager.LoadUserSettings");
                    // Keep valid parts, replace invalid with defaults
                    settings = MergeWithDefaults(settings);
                }

                _currentSettings = settings;
                Console.WriteLine("[SETTINGS] SettingsManager: User settings loaded and validated");
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError($"Error loading user settings: {ex.Message}", "SettingsManager.LoadUserSettings");
                CreateDefaultUserSettings();
            }
        }

        /// <summary>
        /// Creates user settings from default template.
        /// </summary>
        private void CreateDefaultUserSettings()
        {
            try
            {
                if (!File.Exists(_defaultSettingsPath))
                {
                    _errorHandler.HandleError($"Default settings file not found: {_defaultSettingsPath}", "SettingsManager.CreateDefaultUserSettings");
                    _currentSettings = new GameSettings(); // Use hardcoded defaults
                    return;
                }

                var jsonContent = File.ReadAllText(_defaultSettingsPath);
                var defaultSettings = JsonSerializer.Deserialize<GameSettings>(jsonContent, GetJsonOptions());

                if (defaultSettings == null)
                {
                    _errorHandler.HandleError("Failed to deserialize default settings", "SettingsManager.CreateDefaultUserSettings");
                    _currentSettings = new GameSettings();
                    return;
                }

                _currentSettings = defaultSettings;
                
                // Save as user settings
                SaveSettings();
                
                Console.WriteLine("[SETTINGS] SettingsManager: Default settings copied to user settings");
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError($"Error creating default user settings: {ex.Message}", "SettingsManager.CreateDefaultUserSettings");
                _currentSettings = new GameSettings();
            }
        }

        /// <summary>
        /// Validates settings and returns true if all are valid.
        /// </summary>
        private bool ValidateSettings(GameSettings settings)
        {
            var isValid = settings.IsValid();
            
            if (!isValid)
            {
                Console.WriteLine("[SETTINGS] SettingsManager: Settings validation failed");
            }
            else
            {
                Console.WriteLine("[SETTINGS] SettingsManager: Settings validation passed");
            }
            
            return isValid;
        }

        /// <summary>
        /// Merges user settings with defaults for any invalid values.
        /// </summary>
        private GameSettings MergeWithDefaults(GameSettings userSettings)
        {
            var defaults = new GameSettings();
            var merged = userSettings.Clone();

            // Replace invalid display settings
            if (!userSettings.Display.IsValid())
            {
                merged.Display = defaults.Display.Clone();
                Console.WriteLine("[SETTINGS] SettingsManager: Replaced invalid display settings with defaults");
            }

            // Replace invalid control settings
            if (!userSettings.Controls.IsValid())
            {
                merged.Controls = defaults.Controls.Clone();
                Console.WriteLine("[SETTINGS] SettingsManager: Replaced invalid control settings with defaults");
            }

            // Replace invalid audio settings
            if (!userSettings.Audio.IsValid())
            {
                merged.Audio = defaults.Audio.Clone();
                Console.WriteLine("[SETTINGS] SettingsManager: Replaced invalid audio settings with defaults");
            }

            return merged;
        }

        /// <summary>
        /// Gets JSON serialization options.
        /// </summary>
        private JsonSerializerOptions GetJsonOptions()
        {
            return new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
                WriteIndented = true // Make user settings file readable
            };
        }

        /// <summary>
        /// Saves current settings to user settings file.
        /// </summary>
        public void SaveSettings()
        {
            try
            {
                if (!_isInitialized)
                {
                    _errorHandler.HandleWarning("Attempted to save settings before initialization", "SettingsManager.SaveSettings");
                    return;
                }

                var jsonContent = JsonSerializer.Serialize(_currentSettings, GetJsonOptions());
                File.WriteAllText(_userSettingsPath, jsonContent);

                Console.WriteLine("[SETTINGS] SettingsManager: Settings saved successfully");
                _errorHandler.LogInfo("Settings saved to file");
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError($"Failed to save settings: {ex.Message}", "SettingsManager.SaveSettings");
            }
        }

        /// <summary>
        /// Gets the current settings (read-only copy).
        /// </summary>
        public GameSettings GetSettings()
        {
            if (!_isInitialized)
            {
                Console.WriteLine("[SETTINGS] SettingsManager: GetSettings called but not initialized");
                return new GameSettings();
            }

            return _currentSettings.Clone();
        }

        /// <summary>
        /// Updates settings and saves them.
        /// </summary>
        public void UpdateSettings(GameSettings newSettings)
        {
            try
            {
                if (!_isInitialized)
                {
                    _errorHandler.HandleWarning("Attempted to update settings before initialization", "SettingsManager.UpdateSettings");
                    return;
                }

                if (!ValidateSettings(newSettings))
                {
                    _errorHandler.HandleError("Invalid settings provided to UpdateSettings", "SettingsManager.UpdateSettings");
                    return;
                }

                var oldSettings = _currentSettings.Clone();
                _currentSettings = newSettings.Clone();

                // Save to file
                SaveSettings();

                // Emit events for changes
                SettingsChanged?.Invoke(_currentSettings.Clone());

                // Check for specific category changes
                if (!AreDisplaySettingsEqual(oldSettings.Display, _currentSettings.Display))
                {
                    DisplaySettingsChanged?.Invoke(_currentSettings.Display.Clone());
                    Console.WriteLine("[SETTINGS] SettingsManager: Display settings changed");
                }

                if (!AreControlSettingsEqual(oldSettings.Controls, _currentSettings.Controls))
                {
                    ControlSettingsChanged?.Invoke(_currentSettings.Controls.Clone());
                    Console.WriteLine("[SETTINGS] SettingsManager: Control settings changed");
                }

                if (!AreAudioSettingsEqual(oldSettings.Audio, _currentSettings.Audio))
                {
                    AudioSettingsChanged?.Invoke(_currentSettings.Audio.Clone());
                    Console.WriteLine("[SETTINGS] SettingsManager: Audio settings changed");
                }

                _errorHandler.LogInfo("Settings updated successfully");
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError($"Error updating settings: {ex.Message}", "SettingsManager.UpdateSettings");
            }
        }

        /// <summary>
        /// Gets display settings.
        /// </summary>
        public DisplaySettings GetDisplaySettings()
        {
            return _isInitialized ? _currentSettings.Display.Clone() : new DisplaySettings();
        }

        /// <summary>
        /// Gets control settings.
        /// </summary>
        public ControlSettings GetControlSettings()
        {
            return _isInitialized ? _currentSettings.Controls.Clone() : new ControlSettings();
        }

        /// <summary>
        /// Gets audio settings.
        /// </summary>
        public AudioSettings GetAudioSettings()
        {
            return _isInitialized ? _currentSettings.Audio.Clone() : new AudioSettings();
        }

        /// <summary>
        /// Updates only display settings.
        /// </summary>
        public void UpdateDisplaySettings(DisplaySettings displaySettings)
        {
            if (!_isInitialized) return;

            var newSettings = _currentSettings.Clone();
            newSettings.Display = displaySettings.Clone();
            UpdateSettings(newSettings);
        }

        /// <summary>
        /// Updates only control settings.
        /// </summary>
        public void UpdateControlSettings(ControlSettings controlSettings)
        {
            if (!_isInitialized) return;

            var newSettings = _currentSettings.Clone();
            newSettings.Controls = controlSettings.Clone();
            UpdateSettings(newSettings);
        }

        /// <summary>
        /// Updates only audio settings.
        /// </summary>
        public void UpdateAudioSettings(AudioSettings audioSettings)
        {
            if (!_isInitialized) return;

            var newSettings = _currentSettings.Clone();
            newSettings.Audio = audioSettings.Clone();
            UpdateSettings(newSettings);
        }

        /// <summary>
        /// Compares two display settings for equality.
        /// </summary>
        private bool AreDisplaySettingsEqual(DisplaySettings a, DisplaySettings b)
        {
            return a.ResolutionWidth == b.ResolutionWidth &&
                   a.ResolutionHeight == b.ResolutionHeight &&
                   a.Fullscreen == b.Fullscreen &&
                   a.VSync == b.VSync &&
                   Math.Abs(a.UIScale - b.UIScale) < 0.001f;
        }

        /// <summary>
        /// Compares two control settings for equality.
        /// </summary>
        private bool AreControlSettingsEqual(ControlSettings a, ControlSettings b)
        {
            return a.MenuKey == b.MenuKey &&
                   a.MenuCloseKey == b.MenuCloseKey &&
                   Math.Abs(a.CameraSpeed - b.CameraSpeed) < 0.001f &&
                   Math.Abs(a.TooltipScale - b.TooltipScale) < 0.001f;
        }

        /// <summary>
        /// Compares two audio settings for equality.
        /// </summary>
        private bool AreAudioSettingsEqual(AudioSettings a, AudioSettings b)
        {
            return Math.Abs(a.MasterVolume - b.MasterVolume) < 0.001f &&
                   Math.Abs(a.SfxVolume - b.SfxVolume) < 0.001f &&
                   Math.Abs(a.MusicVolume - b.MusicVolume) < 0.001f &&
                   a.Muted == b.Muted;
        }

        /// <summary>
        /// Gets debug information about the settings manager.
        /// </summary>
        public string GetDebugInfo()
        {
            return $"SettingsManager: Initialized={_isInitialized}, " +
                   $"Resolution={_currentSettings.Display.ResolutionWidth}x{_currentSettings.Display.ResolutionHeight}, " +
                   $"CameraSpeed={_currentSettings.Controls.CameraSpeed}, " +
                   $"TooltipScale={_currentSettings.Controls.TooltipScale}";
        }

        /// <summary>
        /// Checks if the settings manager is initialized.
        /// </summary>
        public bool IsInitialized => _isInitialized;

        public void Dispose()
        {
            try
            {
                // Save current settings before disposing
                if (_isInitialized)
                {
                    SaveSettings();
                }

                // Clear events
                SettingsChanged = null;
                DisplaySettingsChanged = null;
                ControlSettingsChanged = null;
                AudioSettingsChanged = null;

                _isInitialized = false;
                Console.WriteLine("[SETTINGS] SettingsManager: Disposed");
                _errorHandler.LogInfo("SettingsManager disposed");
            }
            catch (Exception ex)
            {
                _errorHandler?.HandleError($"Error disposing SettingsManager: {ex.Message}", "SettingsManager.Dispose");
            }
        }
    }
}
