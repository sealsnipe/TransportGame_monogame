# Menu System Input Problem - Debug Documentation

**Datum:** 2024-12-19
**Status:** ✅ GELÖST - Menü-Tasten funktionieren
**Priorität:** ERLEDIGT - Menü-Funktionalität implementiert

## 📋 Problem Zusammenfassung

Das **F10/TAB/M Menü-System** wurde vollständig implementiert, aber **keine Tasten werden erkannt**. Das InputSystem scheint die Tasteneingaben nicht zu registrieren.

## 🎯 Was implementiert wurde

### ✅ Erfolgreich implementiert:
1. **MenuSystem.cs** - Vollständiges Menü-System mit:
   - Hauptmenü (Weiter/Optionen/Beenden)
   - Optionen-Menü mit Navigation (Display/Controls/Audio)
   - Controls-Tab zeigt Tastenbelegungen
   - Maus-Navigation funktioniert
   - ESC-Navigation zwischen Menüs

2. **Tooltip-System** - Funktioniert perfekt:
   - ✅ Linksklick zeigt Tooltips
   - ✅ 1.5x skalierter Bitmap-Text
   - ✅ Erweiterte Buchstaben (A-Z, 0-9, ä/ö/ü)
   - ✅ ESC schließt Tooltips

3. **D-Taste Problem gelöst**:
   - ❌ D-Taste entfernt (WASD-Konflikt vermieden)
   - ✅ Debug-Balken nur noch über F1/F2

## 🔴 Aktuelles Problem

### Problem: **Keine Tastenerkennung**
- **F10** → Keine Reaktion
- **TAB** → Keine Reaktion  
- **M** → Keine Reaktion
- **Keine Console-Logs** von Tastenerkennung

### Getestete Tasten:
```
F10 ❌ (System-Konflikt vermutet)
TAB ❌ (Unbekannter Grund)
M   ❌ (Sollte funktionieren)
```

## 🔍 Debug-Versuche

### 1. Enhanced Logging hinzugefügt:
```csharp
// InputSystem.cs - Zeile 122
Console.WriteLine($"*** InputSystem: Raw key detected: {key} -> String: '{keyString}' ***");

// MenuSystem.cs - Zeile 417  
Console.WriteLine($"*** MenuSystem: Received key event: '{key}' ***");
```

### 2. Erweiterte Tastenerkennung:
```csharp
// InputSystem.cs - ProcessKeyboardInput()
var allPressedKeys = _currentKeyboardState.GetPressedKeys();
Console.WriteLine($"*** KEYS CURRENTLY PRESSED: {string.Join(", ", allPressedKeys)} ***");
```

### 3. Ergebnis der Tests:
- **Keine Console-Ausgabe** bei Tastendruck
- **Tooltip-System funktioniert** (ESC wird erkannt)
- **Maus-Input funktioniert** (Tooltips per Linksklick)

## 🎯 Verdächtige Bereiche

### 1. InputSystem Integration
**Datei:** `Core/TransportGameMain.cs`
```csharp
// Zeile 164-165: InputSystem Update
_inputSystem?.Update(gameTime);
_cameraSystem?.Update(gameTime, _inputSystem!);
```

### 2. Event-Bus Verbindung
**Datei:** `Game/Systems/MenuSystem.cs`
```csharp
// Zeile 52: Event Subscription
_eventBus.KeyPressed += OnKeyPressed;
```

### 3. Key Mapping
**Datei:** `Game/Systems/InputSystem.cs`
```csharp
// Zeile 162-165: M-Taste Mapping
case "M":
    Console.WriteLine("InputSystem: M pressed, emitting menu_toggle");
    _eventBus.EmitKeyPressed("menu_toggle");
    break;
```

## 🔧 Mögliche Ursachen

### 1. **InputSystem läuft nicht**
- Update() wird nicht aufgerufen
- Keyboard.GetState() funktioniert nicht
- MonoGame Input-Pipeline Problem

### 2. **Event-Bus Problem**
- Events werden nicht weitergeleitet
- MenuSystem erhält keine Events
- Subscription funktioniert nicht

### 3. **MonoGame Focus Problem**
- Fenster hat keinen Input-Focus
- Keyboard-State wird nicht aktualisiert
- Platform-spezifisches Problem

## 🛠️ Nächste Debug-Schritte

### 1. **InputSystem Basis-Test**
```csharp
// In InputSystem.Update() hinzufügen:
Console.WriteLine($"*** InputSystem.Update() called at {DateTime.Now} ***");
var keys = Keyboard.GetState().GetPressedKeys();
if (keys.Length > 0) {
    Console.WriteLine($"*** RAW KEYS: {string.Join(",", keys)} ***");
}
```

### 2. **Event-Bus Test**
```csharp
// In MenuSystem.OnKeyPressed() hinzufügen:
Console.WriteLine($"*** MenuSystem received: '{key}' at {DateTime.Now} ***");
```

### 3. **Alternative Input-Test**
```csharp
// Direkter Keyboard-Test in TransportGameMain.Update():
var keyboardState = Keyboard.GetState();
if (keyboardState.IsKeyDown(Keys.M)) {
    Console.WriteLine("*** DIRECT M KEY DETECTED ***");
}
```

## 📁 Betroffene Dateien

### Hauptdateien:
- `Game/Systems/MenuSystem.cs` - Menü-Logik ✅
- `Game/Systems/InputSystem.cs` - Input-Verarbeitung ❌
- `Core/TransportGameMain.cs` - System-Integration ❓

### Funktioniert:
- `Game/Systems/TooltipSystem.cs` - ESC-Taste funktioniert ✅
- `Game/Systems/CameraSystem.cs` - WASD funktioniert ✅

## 🎯 Für nächsten Entwickler

### Sofort testen:
1. **Console-Output prüfen** - Kommen überhaupt InputSystem-Logs?
2. **Direkter Keyboard-Test** - Funktioniert Keyboard.GetState()?
3. **Event-Bus Test** - Kommen Events beim MenuSystem an?

### Wenn InputSystem nicht läuft:
- TransportGameMain.cs Update-Reihenfolge prüfen
- InputSystem Initialisierung prüfen
- MonoGame Window-Focus prüfen

### Wenn Events nicht ankommen:
- Event-Bus Subscription prüfen
- MenuSystem Initialisierung prüfen
- Event-Namen Matching prüfen

## 💡 Workaround-Idee

**Temporärer Fix:** Menü über **Mausklick** aktivieren:
- Button in Ecke platzieren
- Maus-Input funktioniert bereits
- Menü-System ist vollständig implementiert

---

## ✅ **LÖSUNG IMPLEMENTIERT**

**Problem gelöst durch:** Direkte Menü-Steuerung im InputSystem (wie WASD-Kamera)

**Funktionierende Tasten:**
- **M** → Menü öffnen/schließen
- **F10** → Menü öffnen/schließen
- **TAB** → Menü öffnen/schließen
- **ESC** → Menü schließen

**Technische Lösung:**
1. **InputSystem.SetMenuSystem()** - Direkte Verbindung zum MenuSystem
2. **ProcessDirectMenuInput()** - Direkte Tastenerkennung (wie WASD)
3. **IsKeyPressed()** - Verhindert übersensible Wiederholung

**Warum es funktioniert:**
- WASD funktionierte bereits → InputSystem läuft
- Problem war Event-Bus vs. direkte Abfrage
- Lösung: Direkte Abfrage wie bei Kamera-Steuerung

**Status:** ✅ VOLLSTÄNDIG FUNKTIONSFÄHIG
