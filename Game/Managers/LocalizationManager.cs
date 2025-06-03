using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using TransportGame.Game.Constants;

namespace TransportGame.Game.Managers;

/// <summary>
/// Manages localization data for multiple languages.
/// Loads and provides translated text for tiles and UI elements.
/// </summary>
public class LocalizationManager : IDisposable
{
    private readonly ErrorHandler _errorHandler;
    private readonly Dictionary<string, LocalizationData> _languages;
    private readonly string _localizationPath;
    private string _currentLanguage = "de"; // Default to German
    private bool _isInitialized = false;

    public string CurrentLanguage => _currentLanguage;
    public bool IsInitialized => _isInitialized;

    public LocalizationManager(ErrorHandler errorHandler)
    {
        _errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
        _languages = new Dictionary<string, LocalizationData>();
        _localizationPath = Path.Combine("Game", "Data", "Tiles", "Localization");
        
        Console.WriteLine("[LOCALIZATION] LocalizationManager: Constructor called");
        _errorHandler.LogInfo("LocalizationManager initialized");
    }

    /// <summary>
    /// Loads all available language files.
    /// </summary>
    public void LoadLanguages()
    {
        try
        {
            Console.WriteLine($"[LOCALIZATION] LocalizationManager: Loading languages from {_localizationPath}");
            
            if (!Directory.Exists(_localizationPath))
            {
                _errorHandler.HandleError($"Localization directory not found: {_localizationPath}", "LocalizationManager.LoadLanguages");
                return;
            }

            var jsonFiles = Directory.GetFiles(_localizationPath, "*.json");
            Console.WriteLine($"[LOCALIZATION] LocalizationManager: Found {jsonFiles.Length} language files");

            foreach (var filePath in jsonFiles)
            {
                LoadLanguageFile(filePath);
            }

            _isInitialized = true;
            Console.WriteLine($"[LOCALIZATION] LocalizationManager: Loaded {_languages.Count} languages");
            _errorHandler.LogInfo($"Loaded {_languages.Count} languages successfully");
        }
        catch (Exception ex)
        {
            _errorHandler.HandleError($"Failed to load languages: {ex.Message}", "LocalizationManager.LoadLanguages");
            Console.WriteLine($"[LOCALIZATION] LocalizationManager: ERROR loading languages: {ex.Message}");
        }
    }

    /// <summary>
    /// Loads a single language file.
    /// </summary>
    private void LoadLanguageFile(string filePath)
    {
        try
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            Console.WriteLine($"[LOCALIZATION] LocalizationManager: Loading language file {fileName}");

            var jsonContent = File.ReadAllText(filePath);
            var localizationData = JsonSerializer.Deserialize<LocalizationData>(jsonContent, GetJsonOptions());

            if (localizationData == null)
            {
                _errorHandler.HandleError($"Failed to deserialize language file: {fileName}", "LocalizationManager.LoadLanguageFile");
                return;
            }

            // Validate language data
            if (!ValidateLanguageData(localizationData, fileName))
            {
                return;
            }

            _languages[localizationData.Language] = localizationData;
            Console.WriteLine($"[LOCALIZATION] LocalizationManager: Successfully loaded language '{localizationData.Language}' from {fileName}");
        }
        catch (Exception ex)
        {
            _errorHandler.HandleError($"Error loading language file {filePath}: {ex.Message}", "LocalizationManager.LoadLanguageFile");
            Console.WriteLine($"[LOCALIZATION] LocalizationManager: ERROR loading file {filePath}: {ex.Message}");
        }
    }

    /// <summary>
    /// Validates language data for required fields.
    /// </summary>
    private bool ValidateLanguageData(LocalizationData data, string fileName)
    {
        var errors = new List<string>();

        if (string.IsNullOrEmpty(data.Language))
            errors.Add("Missing 'language' field");

        if (data.Tiles == null || data.Tiles.Count == 0)
            errors.Add("Missing or empty 'tiles' section");

        if (errors.Count > 0)
        {
            var errorMessage = $"Validation failed for {fileName}: {string.Join(", ", errors)}";
            _errorHandler.HandleError(errorMessage, "LocalizationManager.ValidateLanguageData");
            Console.WriteLine($"[LOCALIZATION] LocalizationManager: VALIDATION ERROR: {errorMessage}");
            return false;
        }

        Console.WriteLine($"[LOCALIZATION] LocalizationManager: Validation passed for {fileName}");
        return true;
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
            AllowTrailingCommas = true
        };
    }

    /// <summary>
    /// Sets the current language.
    /// </summary>
    public bool SetLanguage(string languageCode)
    {
        if (!_isInitialized)
        {
            Console.WriteLine("[LOCALIZATION] LocalizationManager: SetLanguage called but not initialized");
            return false;
        }

        if (_languages.ContainsKey(languageCode))
        {
            _currentLanguage = languageCode;
            Console.WriteLine($"[LOCALIZATION] LocalizationManager: Language changed to '{languageCode}'");
            _errorHandler.LogInfo($"Language changed to '{languageCode}'");
            return true;
        }

        Console.WriteLine($"[LOCALIZATION] LocalizationManager: Language '{languageCode}' not found");
        return false;
    }

    /// <summary>
    /// Gets localized tile name.
    /// </summary>
    public string GetTileName(TileType tileType)
    {
        return GetTileText(tileType, "name");
    }

    /// <summary>
    /// Gets localized tile description.
    /// </summary>
    public string GetTileDescription(TileType tileType)
    {
        return GetTileText(tileType, "description");
    }

    /// <summary>
    /// Gets localized tile text with fallback to German.
    /// </summary>
    private string GetTileText(TileType tileType, string textType)
    {
        if (!_isInitialized)
        {
            Console.WriteLine("[LOCALIZATION] LocalizationManager: GetTileText called but not initialized");
            return tileType.ToString();
        }

        var tileId = tileType.ToString().ToLower();

        // Try current language first
        if (_languages.TryGetValue(_currentLanguage, out var currentLang) &&
            currentLang.Tiles.TryGetValue(tileId, out var currentTile))
        {
            var text = textType switch
            {
                "name" => currentTile.Name,
                "description" => currentTile.Description,
                _ => ""
            };

            if (!string.IsNullOrEmpty(text))
            {
                return text;
            }
        }

        // Fallback to German
        if (_currentLanguage != "de" && _languages.TryGetValue("de", out var germanLang) &&
            germanLang.Tiles.TryGetValue(tileId, out var germanTile))
        {
            var text = textType switch
            {
                "name" => germanTile.Name,
                "description" => germanTile.Description,
                _ => ""
            };

            if (!string.IsNullOrEmpty(text))
            {
                Console.WriteLine($"[LOCALIZATION] LocalizationManager: Using German fallback for {tileId}.{textType}");
                return text;
            }
        }

        // Final fallback to enum name
        Console.WriteLine($"[LOCALIZATION] LocalizationManager: No translation found for {tileId}.{textType}, using enum name");
        return tileType.ToString();
    }

    /// <summary>
    /// Gets all available language codes.
    /// </summary>
    public IReadOnlyList<string> GetAvailableLanguages()
    {
        return _languages.Keys.ToList();
    }

    /// <summary>
    /// Gets debug information about loaded languages.
    /// </summary>
    public string GetDebugInfo()
    {
        return $"Languages: {_languages.Count} loaded, Current: {_currentLanguage}, Initialized: {_isInitialized}";
    }

    public void Dispose()
    {
        _languages.Clear();
        _isInitialized = false;
        Console.WriteLine("[LOCALIZATION] LocalizationManager: Disposed");
        _errorHandler.LogInfo("LocalizationManager disposed");
    }
}

/// <summary>
/// Represents localization data for a single language.
/// </summary>
public class LocalizationData
{
    [JsonPropertyName("language")]
    public string Language { get; set; } = "";

    [JsonPropertyName("tiles")]
    public Dictionary<string, TileLocalization> Tiles { get; set; } = new();
}

/// <summary>
/// Represents localization data for a single tile.
/// </summary>
public class TileLocalization
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("description")]
    public string Description { get; set; } = "";

    [JsonPropertyName("properties")]
    public Dictionary<string, string> Properties { get; set; } = new();
}
