# Settings Persistence System - Implementation Plan
**Date:** 2024-12-19  
**Status:** PLANNING PHASE  
**Priority:** HIGH - Foundation for Options Menu

## Overview
Implement a robust, JSON-based settings persistence system that integrates seamlessly with the existing JSON infrastructure (TileDefinitionManager, LocalizationManager). This system will store user preferences and provide the foundation for the Options Menu.

## Goals
1. **Persistent Settings**: Save/load user preferences across game sessions
2. **Type Safety**: Strongly-typed settings with validation
3. **Extensibility**: Easy to add new settings categories
4. **Integration**: Leverage existing JSON infrastructure
5. **Error Handling**: Graceful fallbacks to defaults

## Architecture Overview

### Core Components
1. **SettingsManager.cs** - Main settings management class
2. **GameSettings.cs** - Settings data model (similar to TileDefinition)
3. **settings.json** - User settings file (in `Game/Data/Settings/`)
4. **default-settings.json** - Default settings template

### File Structure
```
Game/
├── Data/
│   ├── Settings/
│   │   ├── default-settings.json    # Default values
│   │   └── settings.json            # User settings (auto-created)
│   ├── Tiles/ (existing)
│   └── ...
├── Managers/
│   ├── SettingsManager.cs           # New
│   ├── TileDefinitionManager.cs     # Existing (reference)
│   └── ...
├── Models/
│   ├── GameSettings.cs              # New
│   ├── TileDefinition.cs            # Existing (reference)
│   └── ...
```

## Implementation Plan

### Phase 1: Core Settings Infrastructure (30-45 min)

#### Step 1.1: Create Settings Data Model
**File:** `Game/Models/GameSettings.cs`
```csharp
public class GameSettings
{
    public DisplaySettings Display { get; set; } = new();
    public ControlSettings Controls { get; set; } = new();
    public AudioSettings Audio { get; set; } = new();
}

public class DisplaySettings
{
    public int ResolutionWidth { get; set; } = 1280;
    public int ResolutionHeight { get; set; } = 720;
    public bool Fullscreen { get; set; } = false;
    public bool VSync { get; set; } = true;
    public float UIScale { get; set; } = 1.0f;
}

public class ControlSettings
{
    public string MenuKey { get; set; } = "Tab";
    public string MenuCloseKey { get; set; } = "Escape";
    public float CameraSpeed { get; set; } = 5.0f;
    public float TooltipScale { get; set; } = 1.5f;
}

public class AudioSettings
{
    public float MasterVolume { get; set; } = 0.8f;
    public float SfxVolume { get; set; } = 0.6f;
    public float MusicVolume { get; set; } = 0.4f;
    public bool Muted { get; set; } = false;
}
```

#### Step 1.2: Create Default Settings JSON
**File:** `Game/Data/Settings/default-settings.json`
```json
{
  "display": {
    "resolutionWidth": 1280,
    "resolutionHeight": 720,
    "fullscreen": false,
    "vSync": true,
    "uiScale": 1.0
  },
  "controls": {
    "menuKey": "Tab",
    "menuCloseKey": "Escape", 
    "cameraSpeed": 5.0,
    "tooltipScale": 1.5
  },
  "audio": {
    "masterVolume": 0.8,
    "sfxVolume": 0.6,
    "musicVolume": 0.4,
    "muted": false
  }
}
```

#### Step 1.3: Create SettingsManager
**File:** `Game/Managers/SettingsManager.cs`
- Follow same pattern as TileDefinitionManager
- Load default settings on first run
- Create user settings.json if it doesn't exist
- Validate settings on load
- Provide Save() and Load() methods
- Event system for settings changes

### Phase 2: Integration with Existing Systems (20-30 min)

#### Step 2.1: Integrate with TransportGameMain
- Initialize SettingsManager in constructor
- Load settings before other systems
- Pass settings to relevant systems

#### Step 2.2: Apply Settings to Current Systems
- **CameraSystem**: Use `controls.cameraSpeed`
- **TooltipSystem**: Use `controls.tooltipScale`
- **InputSystem**: Use `controls.menuKey` and `controls.menuCloseKey`
- **Graphics**: Apply `display.fullscreen`, `display.vSync`

#### Step 2.3: Settings Change Events
- Implement event system for real-time settings application
- No restart required for most settings

### Phase 3: Options Menu Foundation (15-20 min)

#### Step 3.1: Create OptionsState.cs
- Basic structure similar to MenuState
- Three tabs: Display, Controls, Audio
- Left navigation as requested
- Settings modification interface

#### Step 3.2: Connect to MenuState
- Add "Optionen" button functionality
- State stack management (MenuState → OptionsState)

## Technical Specifications

### JSON Schema Validation
```csharp
// Similar to TileDefinition validation
private bool ValidateSettings(GameSettings settings)
{
    // Validate ranges, required fields, etc.
    // Return false if invalid, log errors
}
```

### Error Handling Strategy
1. **Missing File**: Create from default-settings.json
2. **Corrupted JSON**: Backup corrupted file, restore defaults
3. **Invalid Values**: Use defaults for invalid fields, log warnings
4. **Write Errors**: Log error, continue with current settings

### Performance Considerations
- **Lazy Loading**: Load settings once at startup
- **Batch Saves**: Don't save on every change, batch saves
- **Memory Efficient**: Keep single instance in memory
- **Fast Access**: Cache frequently used settings

## Testing Strategy

### Unit Tests
1. **Settings Loading**: Test default and user settings loading
2. **Validation**: Test invalid settings handling
3. **Persistence**: Test save/load cycle
4. **Error Handling**: Test corrupted files, missing files

### Integration Tests
1. **System Integration**: Test settings application to game systems
2. **Options Menu**: Test UI → Settings → System flow
3. **Real-time Changes**: Test settings changes without restart

### Manual Testing Checklist
- [ ] First run creates default settings.json
- [ ] Settings persist across game restarts
- [ ] Invalid settings fall back to defaults
- [ ] Options menu reflects current settings
- [ ] Settings changes apply immediately
- [ ] Corrupted settings.json handled gracefully

## File Locations Summary

### New Files to Create
1. `Game/Models/GameSettings.cs`
2. `Game/Managers/SettingsManager.cs`
3. `Game/Data/Settings/default-settings.json`
4. `Game/States/OptionsState.cs` (Phase 3)

### Files to Modify
1. `TransportGameMain.cs` - Add SettingsManager initialization
2. `Game/Systems/CameraSystem.cs` - Apply camera speed setting
3. `Game/Systems/TooltipSystem.cs` - Apply tooltip scale setting
4. `Game/Systems/InputSystem.cs` - Apply key binding settings
5. `Game/States/MenuState.cs` - Connect Options button

## Success Criteria

### Phase 1 Complete When:
- [ ] Settings load from JSON successfully
- [ ] Default settings.json created on first run
- [ ] SettingsManager integrated into main game loop
- [ ] Basic validation and error handling working

### Phase 2 Complete When:
- [ ] Camera speed configurable via settings
- [ ] Tooltip scale configurable via settings
- [ ] Menu keys configurable via settings
- [ ] Settings changes apply without restart

### Phase 3 Complete When:
- [ ] Options menu accessible from main menu
- [ ] All settings visible and editable in UI
- [ ] Settings save immediately when changed
- [ ] Navigation works as specified (left nav)

## Next Steps After Completion
1. **Advanced Options**: Resolution selection, key binding UI
2. **Settings Categories**: Add gameplay settings, accessibility options
3. **Import/Export**: Settings backup and sharing
4. **Cloud Sync**: Steam Cloud or similar integration

---

**Ready to implement!** Start with Phase 1, Step 1.1 - Creating the GameSettings data model.
