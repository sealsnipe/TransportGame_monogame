# Transport Game Multiplayer - Projekt Dokumentation

## 🎯 Projektziel

Ein **Transport Fever 2-inspiriertes Spiel** mit folgenden Kernfeatures:
- **2D Top-Down Perspektive** mit Tilemap-basiertem Gameplay
- **Züge** als Haupttransportmittel (ein Zug pro Gleis)
- **Ressourcen-System**: Weizen, Eisen, Nahrung, Stahl
- **Produktionsgebäude**: Farmen, Minen, Depots, Fabriken
- **Städte** mit spezifischen Mechaniken
- **Multiplayer-Funktionalität** (geplant)

## 🛠️ Technische Entscheidungen

### Ursprünglicher Ansatz: Godot
- **Engine**: Godot 4.4.1 (.NET Version)
- **Sprache**: C# mit GDScript für Singletons
- **Architektur**: Singleton-basiert mit EventBus-Pattern

### Warum Wechsel weg von Godot?
1. **Persistente Parse-Fehler** trotz syntaktisch korrektem Code
2. **Encoding-Probleme** mit unsichtbaren Zeichen
3. **Godot 3.x → 4.x Migration** war komplex und fehleranfällig
4. **Debugging-Schwierigkeiten** mit Endlosschleifen

## 📁 Aktuelle Projektstruktur (Godot)

```
Transportgame_Multiplayer/
├── docs/
│   ├── godot/
│   │   ├── architecture/
│   │   ├── workflow/
│   │   └── Godot_v4.4.1-stable_win64.exe
│   └── author_notes/
│       └── debugging_methods.md
├── scripts/
│   ├── singletons/
│   │   ├── ErrorHandler.gd ✅
│   │   ├── GameManager.gd ✅
│   │   ├── EventBus.gd ✅
│   │   ├── ResourceManager.gd ✅
│   │   ├── UIManager.gd ✅
│   │   ├── AudioManager.gd ✅
│   │   └── SaveSystem.gd ❌ (Parse-Fehler)
│   ├── entities/
│   │   ├── Entity.gd ✅
│   │   ├── Building.gd ✅
│   │   ├── ProductionBuilding.gd ✅
│   │   └── Train.gd ✅
│   ├── buildings/
│   │   ├── Farm.gd ✅
│   │   ├── Mine.gd ✅
│   │   ├── Depot.gd ✅
│   │   └── Factory.gd ✅
│   ├── managers/
│   │   └── TilemapManager.gd ✅
│   ├── constants/
│   │   ├── GameConstants.gd ✅
│   │   └── Enums.gd ✅
│   └── main/
│       └── Main.gd ✅
├── scenes/
│   ├── main/Main.tscn ✅
│   ├── buildings/ ✅
│   └── entities/ ✅
└── resources/
    └── tilesets/ ✅
```

## 🎮 Implementierte Features

### ✅ Erfolgreich implementiert:
1. **Singleton-System** mit ErrorHandler, GameManager, EventBus
2. **Entity-System** mit Vererbungshierarchie
3. **Building-System** mit Produktionslogik
4. **Resource-Management** für Weizen, Eisen, etc.
5. **Tilemap-System** für Weltgenerierung
6. **UI-System** mit HUD und Buttons
7. **Audio-System** mit Godot 4.x Tween API
8. **Sicheres Debug-System** mit Auto-Exit

### ❌ Problematisch:
1. **SaveSystem** - Persistente Parse-Fehler
2. **Graues Feld Problem** - Tilemap wird nicht angezeigt
3. **TilemapManager** - Wird nicht gefunden (null reference)

## 🔧 Entwickelte Tools & Methoden

### Debug-System
```bash
# Sicherer Test (Auto-Exit nach 30s)
"docs/godot/Godot_v4.4.1-stable_win64.exe" --path . --script res://scripts/debug_runner.gd

# Headless Test
"docs/godot/Godot_v4.4.1-stable_win64.exe" --headless --quit --verbose

# Debug mit vollem Logging
"docs/godot/Godot_v4.4.1-stable_win64.exe" --path . --main-scene res://scenes/main/Main.tscn --verbose --debug
```

### Godot 3.x → 4.x Migration
- ✅ `File.new()` → `FileAccess.open()`
- ✅ `Directory.new()` → `DirAccess.open()`
- ✅ `JSON.parse()` → `JSON.new().parse()`
- ✅ `yield` → `await`
- ✅ `empty()` → `is_empty()`
- ✅ Tween API: `interpolate_property()` → `tween_property()`

## 🎯 Geplante Features

### Kern-Gameplay:
1. **Zug-System**: Automatische Routen zwischen Stationen
2. **Produktionsketten**: Weizen → Nahrung, Eisen → Stahl
3. **Stadt-System**: Nachfrage nach Ressourcen
4. **Wirtschafts-System**: Preise, Gewinn/Verlust
5. **Gleis-System**: Schienen bauen und verwalten

### Erweiterte Features:
1. **Multiplayer**: Mehrere Spieler auf einer Karte
2. **Kampagnen-Modus**: Verschiedene Szenarien
3. **Mod-Support**: Eigene Gebäude und Züge
4. **Statistiken**: Detaillierte Wirtschaftsanalyse

## 🚀 Nächste Schritte: Neuer Ansatz

### Warum nicht Godot?
1. **Zu viele Engine-spezifische Probleme**
2. **Komplexe Debugging-Situation**
3. **Parse-Fehler ohne erkennbare Ursache**

### Alternative Technologien:

#### Option 1: Unity + C#
**Vorteile:**
- Mature 2D-System
- Excellent C# Support
- Große Community
- Gute Multiplayer-Tools

**Nachteile:**
- Lizenzkosten bei Erfolg
- Größere Engine

#### Option 2: Custom Engine (C# + MonoGame/FNA)
**Vorteile:**
- Vollständige Kontrolle
- Keine Engine-Limitierungen
- Optimiert für unser Spiel
- Kein Vendor Lock-in

**Nachteile:**
- Mehr Entwicklungszeit
- Alles selbst implementieren

#### Option 3: Web-basiert (TypeScript + Canvas/WebGL)
**Vorteile:**
- Plattformunabhängig
- Einfache Multiplayer-Integration
- Schnelle Iteration
- Keine Installation nötig

**Nachteile:**
- Performance-Limitierungen
- Browser-Kompatibilität

#### Option 4: Bevy (Rust)
**Vorteile:**
- Moderne ECS-Architektur
- Excellent Performance
- Type Safety
- Growing Community

**Nachteile:**
- Steile Lernkurve
- Weniger Ressourcen

## 💡 Empfehlung für Neustart

### Bevorzugter Ansatz: **Custom Engine mit C# + MonoGame**

**Begründung:**
1. **Kontrolle**: Keine Engine-spezifischen Bugs
2. **Performance**: Optimiert für unser Spiel
3. **Flexibilität**: Genau die Features die wir brauchen
4. **Erfahrung**: Wir kennen bereits C# gut
5. **Multiplayer**: Einfacher zu implementieren

### Architektur-Plan:
```
TransportGame/
├── Core/
│   ├── Engine/          # Basis-Engine (Rendering, Input, Audio)
│   ├── ECS/             # Entity-Component-System
│   └── Networking/      # Multiplayer-Infrastruktur
├── Game/
│   ├── Systems/         # Gameplay-Systeme
│   ├── Components/      # Spiel-Komponenten
│   └── Resources/       # Spiel-Ressourcen
├── Content/
│   ├── Textures/
│   ├── Audio/
│   └── Data/
└── Tools/
    ├── MapEditor/
    └── AssetPipeline/
```

### Erste Schritte:
1. **MonoGame-Projekt** aufsetzen
2. **Basis-Rendering** implementieren
3. **Tilemap-System** erstellen
4. **Entity-System** portieren
5. **Einfaches Gameplay** implementieren

## 📊 Lessons Learned

### Was gut funktioniert hat:
- ✅ Singleton-Pattern für Manager
- ✅ EventBus für lose Kopplung
- ✅ Klare Vererbungshierarchie
- ✅ Umfangreiche Dokumentation
- ✅ Sicheres Debug-System

### Was problematisch war:
- ❌ Engine-spezifische Parse-Fehler
- ❌ Godot 3.x → 4.x Migration
- ❌ Komplexe Debugging-Situation
- ❌ Encoding-Probleme

### Für neues Projekt beachten:
- 🎯 Einfache, kontrollierbare Technologie wählen
- 🎯 Von Anfang an gute Debugging-Tools
- 🎯 Klare Architektur-Entscheidungen
- 🎯 Regelmäßige Tests und Validierung

## 🎨 Game Design Dokument

### Kern-Mechaniken:

#### Transport-System:
- **Züge**: Haupttransportmittel, automatische Routen
- **Gleise**: Ein Zug pro Gleis-Segment
- **Stationen**: Lade-/Entladepunkte für Ressourcen
- **Routen**: Spieler definiert Start → Ziel → Start Schleifen

#### Ressourcen-System:
```
Primär-Ressourcen:
├── Weizen (von Farmen)
└── Eisen (von Minen)

Sekundär-Ressourcen:
├── Nahrung (Weizen → Fabrik)
└── Stahl (Eisen → Fabrik)
```

#### Gebäude-Typen:
1. **Farm**: Produziert Weizen
2. **Mine**: Produziert Eisen
3. **Fabrik**: Verarbeitet Primär- zu Sekundär-Ressourcen
4. **Depot**: Lagert Ressourcen zwischen
5. **Stadt**: Konsumiert Ressourcen, zahlt Geld

#### Wirtschafts-System:
- **Kosten**: Gebäude bauen, Züge kaufen, Gleise legen
- **Einnahmen**: Ressourcen an Städte verkaufen
- **Preise**: Dynamisch basierend auf Angebot/Nachfrage
- **Ziel**: Profitables Transport-Netzwerk aufbauen

### UI/UX Design:
- **Top-Down 2D**: Einfache, klare Sicht
- **Tile-basiert**: 32x32 oder 64x64 Pixel Tiles
- **Drag & Drop**: Intuitive Gebäude-Platzierung
- **Kontext-Menüs**: Rechtsklick für Optionen
- **Hotkeys**: Schneller Zugriff auf Tools

## 🔄 Migration Plan: Godot → Custom Engine

### Phase 1: Setup (1-2 Wochen)
1. **MonoGame-Projekt** erstellen
2. **Basis-Window** und Rendering
3. **Input-System** (Maus, Tastatur)
4. **Asset-Loading** (Texturen, Audio)

### Phase 2: Core Systems (2-3 Wochen)
1. **Tilemap-Rendering** System
2. **Camera-System** mit Zoom/Pan
3. **Entity-Component-System**
4. **Basic UI-Framework**

### Phase 3: Game Logic (3-4 Wochen)
1. **Building-System** portieren
2. **Resource-Management** implementieren
3. **Train-Movement** System
4. **Basic AI** für Züge

### Phase 4: Polish (2-3 Wochen)
1. **Audio-System** integrieren
2. **Save/Load** System
3. **UI-Polish** und Feedback
4. **Performance-Optimierung**

### Phase 5: Multiplayer (4-6 Wochen)
1. **Networking-Layer**
2. **Client-Server-Architektur**
3. **Synchronisation**
4. **Multiplayer-UI**

## 📋 Technische Spezifikationen

### Minimum System Requirements:
- **OS**: Windows 10, macOS 10.15, Ubuntu 18.04
- **RAM**: 4 GB
- **GPU**: DirectX 11 / OpenGL 3.3
- **Storage**: 500 MB

### Target Performance:
- **FPS**: 60 FPS konstant
- **Map Size**: 1000x1000 Tiles
- **Entities**: 10,000+ gleichzeitig
- **Network**: <100ms Latenz für Multiplayer

### Code Standards:
- **C# 11.0** mit .NET 7.0
- **Async/Await** für I/O Operations
- **SOLID Principles**
- **Unit Tests** für kritische Systeme
- **XML Documentation** für Public APIs

## 🎯 Success Metrics

### MVP (Minimum Viable Product):
- ✅ Karte mit Tiles anzeigen
- ✅ Gebäude platzieren können
- ✅ Züge spawnen und bewegen
- ✅ Ressourcen produzieren und transportieren
- ✅ Basis-Wirtschaftssystem

### Version 1.0:
- ✅ Vollständiges Singleplayer-Spiel
- ✅ 5+ Gebäude-Typen
- ✅ 3+ Zug-Typen
- ✅ Kampagnen-Modus mit 10+ Levels
- ✅ Save/Load System
- ✅ Statistiken und Achievements

### Version 2.0:
- ✅ Multiplayer für 2-8 Spieler
- ✅ Map-Editor
- ✅ Mod-Support
- ✅ Steam Workshop Integration
- ✅ Leaderboards

## 📞 Nächste Schritte

1. **Entscheidung treffen**: Welche Technologie für Neustart?
2. **Repository erstellen**: Neues Git-Repo für Custom Engine
3. **Proof of Concept**: Einfache Tilemap mit beweglichem Objekt
4. **Architektur definieren**: ECS vs. Traditional OOP
5. **Asset Pipeline**: Wie Texturen/Audio verwalten?

**Empfehlung**: Beginnen wir mit einem **MonoGame Proof of Concept** - 2-3 Tage für eine einfache Tilemap mit beweglichem Zug. Dann können wir entscheiden, ob dieser Ansatz vielversprechend ist.
