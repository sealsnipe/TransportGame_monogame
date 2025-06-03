# 🚂 Transport Game - MonoGame Edition

Ein Transport Fever 2-inspiriertes Spiel mit 2D Top-Down Perspektive, entwickelt in C# mit MonoGame.

## 🎯 **Features**

### **✅ Implementiert:**
- **Große Spielwelt** (768x432 Tiles, 5px pro Tile bei 1080p)
- **Terrain-System** mit 12 verschiedenen Tile-Typen
- **Automatische Industrie-Generierung** (Farmen auf Farmland, Minen auf Bergen)
- **Building System** mit 4 spieler-baubaren Gebäuden
- **Horizontales Building UI** (unten-mitte, Transport Fever 2 Stil)
- **Umfassendes Tooltip-System** (Terrain, Industrien, Gebäude)
- **Ressourcen-System** mit Produktionsketten
- **Kamera-System** (WASD, Mausrad-Zoom, Rechtsklick-Ziehen)
- **Mehrsprachigkeit** (Deutsch/Englisch)
- **Settings-Persistierung**

### **🏗️ Gebäude-Typen:**
- **🌾 Farmen** (2x2) - Automatisch generiert, produziert Grain
- **⛏️ Minen** (2x2) - Automatisch generiert, produziert Iron Ore
- **🏭 Steel Works** (3x3) - Spieler-baubar, verarbeitet Iron Ore → Steel
- **🍞 Food Factory** (3x3) - Spieler-baubar, verarbeitet Grain → Food
- **🚂 Train Depot** (4x4) - Spieler-baubar, verwaltet Züge
- **🚉 Station** (1x4) - Spieler-baubar, Lade-/Entladepunkt

## ⚙️ **Installation**

### **Voraussetzungen:**
- **Windows 10/11** (64-bit)
- **.NET 8.0 SDK** oder höher

### **Schritt-für-Schritt:**

1. **.NET 8.0 SDK installieren**
   ```
   https://dotnet.microsoft.com/download/dotnet/8.0
   → "SDK x64" für Windows herunterladen und installieren
   ```

2. **Repository klonen**
   ```bash
   git clone https://github.com/sealsnipe/TransportGame_monogame.git
   cd TransportGame_monogame
   ```

3. **Dependencies installieren**
   ```bash
   dotnet restore
   ```

4. **Spiel starten**
   ```bash
   dotnet run
   ```

### **Erwartete Ausgabe:**
```
=== TRANSPORT GAME STARTING ===
[INFO] Generated 15 farms
[INFO] Generated 2 mines
[INFO] BuildingUISystem content loaded
Content loaded successfully
```

## 🎮 **Steuerung**

### **Kamera:**
- **WASD** - Bewegen
- **Mausrad** - Zoomen
- **Rechtsklick + Ziehen** - Schwenken

### **Gebäude:**
- **Taste 1** - Steel Works (3x3, $2000)
- **Taste 2** - Food Factory (3x3, $2500)
- **Taste 3** - Train Depot (4x4, $5000)
- **Taste 4** - Station (1x4, $1200)
- **ESC** - Platzierung abbrechen

### **Tooltips:**
- **Linksklick** - Tooltip anzeigen
- **ESC** - Tooltip schließen

### **UI:**
- **B** - Building UI ein-/ausblenden
- **F10** - Hauptmenü
- **F11** - Debug-Panel

## 🧪 **Testing**

Für detaillierte Testanweisungen siehe: **[TESTING.md](TESTING.md)**

## 📋 **TODOs**

### **🔄 In Arbeit:**
- [ ] Transport-System (Züge, Schienen)
- [ ] Erweiterte Wirtschafts-Simulation
- [ ] Performance-Optimierungen

### **📅 Geplant:**
- [ ] Map-Editor
- [ ] Multiplayer-Features
- [ ] Mobile Unterstützung
- [ ] Steam Workshop Integration
- [ ] Erweiterte Produktions-Ketten
- [ ] Achievements & Progression
- [ ] Sound & Musik
- [ ] Partikel-Effekte
- [ ] Animationen
- [ ] Tutorial-System

### **🐛 Bekannte Issues:**
- Font-Loading kann bei manchen Systemen fehlschlagen
- Performance bei sehr großen Maps (>1000x1000)
- Tooltip-Position bei sehr kleinen Bildschirmen

## 🏗️ **Architektur**

### **Hauptkomponenten:**
- **Core/** - Hauptspiel-Loop und Engine
- **Game/Systems/** - Spielsysteme (Building, Tooltip, Camera, etc.)
- **Game/Managers/** - Datenmanager (Tilemap, Buildings, Resources)
- **Game/Data/** - JSON-Definitionen für Tiles, Buildings, Resources

### **Wichtige Systeme:**
- **BuildingPlacementSystem** - Gebäude-Platzierung
- **TooltipSystem** - Tooltip-Anzeige
- **MouseInteractionSystem** - Maus-Input
- **BuildingUISystem** - Building-Interface
- **IndustryGenerationSystem** - Industrie-Generierung

## 🐛 **Troubleshooting**

### **"SDK not found"**
- .NET 8.0 SDK von https://dotnet.microsoft.com/download/dotnet/8.0 installieren

### **"Font not found"**
- Sollte automatisch behoben werden (Arial wird von Windows geladen)

### **Building UI nicht sichtbar**
- Drücke "B" zum Ein-/Ausblenden
- Bildschirmauflösung prüfen (min. 1280x720)

### **Performance-Probleme**
- Grafiktreiber aktualisieren
- Niedrigere Auflösung verwenden

## 📞 **Support**

Bei Problemen:
1. Prüfe Troubleshooting-Sektion
2. Führe Testprotokoll durch (TESTING.md)
3. Erstelle Issue mit detailliertem Log

**Viel Spaß beim Spielen!** 🚂🎮