# 🎯 **ENTWICKLUNGSPLAN: Maus-Interaktion + Gebäude-Platzierung**

## **Projektstatus:**
- ✅ Basis-Engine (MonoGame)
- ✅ Kamera-System (Zoom, Pan, Follow)
- ✅ Tilemap-System (384x216 Tiles à 5px)
- ✅ Demo-Zug (grid-basierte Bewegung)
- ✅ Input-System (Tastatur, Maus, Events)
- ✅ Organisierte Landschaft (Farmland, Berge, Wälder, Seen)

## **ZIEL:** 
Erstes echtes Gameplay - Gebäude mit der Maus platzieren!

---

## **Phase 1: Maus-Interaktion** 🖱️

### **1.1 Maus-zu-Grid Konvertierung**
**Aufgabe:** Maus-Position in Grid-Koordinaten umrechnen
- [ ] `MouseInteractionSystem` erstellen
- [ ] Maus-Position → Welt-Koordinaten (Kamera-Transform berücksichtigen)
- [ ] Welt-Koordinaten → Grid-Koordinaten (÷ 5px)
- [ ] Debug-Ausgabe: "Maus über Tile (X,Y): [TileType]"

**Technische Details:**
```csharp
// CameraSystem.ScreenToWorld() verwenden
var worldPos = cameraSystem.ScreenToWorld(mousePos);
var gridX = (int)(worldPos.X / GameConstants.TILE_SIZE);
var gridY = (int)(worldPos.Y / GameConstants.TILE_SIZE);
```

### **1.2 Hover-Effekte**
**Aufgabe:** Tile unter Maus visuell hervorheben
- [ ] Hover-Tile Detection (welches Tile ist unter der Maus?)
- [ ] Hover-Rendering (weißer Rahmen um aktuelles Tile)
- [ ] Tile-Typ Anzeige (Console-Output: "Hovering: Farmland")
- [ ] Nur bei gültigen Tiles (innerhalb der Welt-Grenzen)

**Rendering:**
```csharp
// In RenderSystem.DrawUI()
if (hoveredTile.HasValue)
{
    DrawTileHighlight(spriteBatch, hoveredTile.Value, Color.White);
}
```

### **1.3 Klick-Events erweitern**
**Aufgabe:** Maus-Klicks für Gameplay nutzen
- [ ] Linksklick: Grid-Position erfassen
- [ ] Event-System erweitern: `TileClicked(gridX, gridY, tileType)`
- [ ] Rechtsklick: Kamera zentrieren (✅ bereits implementiert)

---

## **Phase 2: Gebäude-System** 🏗️

### **2.1 Gebäude-Typen definieren**
**Aufgabe:** Verschiedene Gebäude mit Regeln
- [ ] `BuildingType` Enum erstellen
- [ ] Gebäude-Definitionen:
  ```csharp
  Farm (gelb)   → nur auf Farmland platzierbar
  Mine (orange) → nur auf Bergen platzierbar  
  Depot (lila)  → nur auf Gras platzierbar
  ```
- [ ] `BuildingManager` Klasse erstellen
- [ ] Platzierungs-Regeln implementieren

### **2.2 Gebäude-Auswahl System**
**Aufgabe:** Tastatur-Hotkeys für Gebäude-Auswahl
- [ ] Input-System erweitern:
  - **1-Taste** = Farm auswählen
  - **2-Taste** = Mine auswählen  
  - **3-Taste** = Depot auswählen
  - **ESC** = Auswahl aufheben
- [ ] Aktueller Gebäude-Modus speichern
- [ ] Visual Feedback für gewählten Modus

### **2.3 Platzierungs-Logik**
**Aufgabe:** Intelligente Gebäude-Platzierung
- [ ] Gültigkeits-Check implementieren:
  ```csharp
  bool CanPlaceBuilding(BuildingType type, int gridX, int gridY)
  {
      var tileType = tilemapManager.GetTileType(gridX, gridY);
      return type switch {
          Farm => tileType == TileType.Farmland,
          Mine => tileType == TileType.Mountain,
          Depot => tileType == TileType.Grass,
          _ => false
      };
  }
  ```
- [ ] Gebäude-Platzierung bei Linksklick
- [ ] Doppel-Platzierung verhindern
- [ ] Gebäude-Datenbank (wo sind welche Gebäude?)

### **2.4 Gebäude-Rendering**
**Aufgabe:** Gebäude visuell darstellen
- [ ] Gebäude als 5x5 Pixel Quadrate
- [ ] Farb-Schema:
  - Farm: `Color.Gold` (gelb)
  - Mine: `Color.Orange` 
  - Depot: `Color.Purple`
- [ ] Weißer Rand für bessere Sichtbarkeit
- [ ] Gebäude über Tiles rendern (höhere Z-Order)

---

## **Phase 3: UI-Feedback** 📊

### **3.1 Einfache Status-Anzeigen**
**Aufgabe:** Spieler-Information verbessern
- [ ] Aktueller Gebäude-Modus (oben links anzeigen)
- [ ] Maus-Position als Grid-Koordinaten
- [ ] Tile-Typ unter Maus
- [ ] Anzahl platzierter Gebäude pro Typ

### **3.2 Platzierungs-Feedback**
**Aufgabe:** Visuelles Feedback für Platzierung
- [ ] Cursor-Farbe je nach Gültigkeit:
  - **Grün**: Gebäude kann hier platziert werden
  - **Rot**: Platzierung nicht möglich
  - **Grau**: Kein Gebäude ausgewählt
- [ ] Grund für ungültige Platzierung anzeigen
- [ ] Erfolgs-/Fehler-Meldungen

### **3.3 Debug-Informationen erweitern**
**Aufgabe:** Entwickler-Tools verbessern
- [ ] Debug-Panel erweitern:
  - Maus-Position (Screen + World + Grid)
  - Aktueller Gebäude-Modus
  - Hover-Tile Information
  - Gebäude-Statistiken

---

## **Technische Implementierung:**

### **Neue Klassen erstellen:**
1. **`MouseInteractionSystem`** - Maus-zu-Grid Konvertierung
2. **`BuildingManager`** - Gebäude-Logik und -Verwaltung
3. **`BuildingType`** - Enum für Gebäude-Typen
4. **`Building`** - Einzelnes Gebäude (Position, Typ)

### **Bestehende Klassen erweitern:**
1. **`TilemapManager`** - Gebäude-Layer hinzufügen
2. **`InputSystem`** - Gebäude-Hotkeys (1,2,3)
3. **`RenderSystem`** - Hover-Effekte und Gebäude-Rendering
4. **`EventBus`** - Neue Events (TileClicked, BuildingPlaced)

### **Datei-Struktur:**
```
Game/
├── Systems/
│   ├── MouseInteractionSystem.cs    [NEU]
│   ├── InputSystem.cs               [ERWEITERN]
│   └── RenderSystem.cs              [ERWEITERN]
├── Managers/
│   ├── BuildingManager.cs           [NEU]
│   └── TilemapManager.cs            [ERWEITERN]
├── Entities/
│   └── Building.cs                  [NEU]
└── Enums/
    └── BuildingType.cs              [NEU]
```

---

## **Erwartetes Ergebnis:**

🎮 **Vollständiges Gebäude-Platzierungs-System!**
- ✅ Maus-gesteuerte Interaktion
- ✅ 3 verschiedene Gebäude-Typen
- ✅ Intelligente Platzierungs-Regeln
- ✅ Visuelles Feedback
- ✅ Tastatur-Hotkeys
- ✅ Hover-Effekte

**Das wäre ein riesiger Schritt Richtung echtes Transport-Spiel!** 🚂🏗️

---

## **Nächste Schritte:**
1. **Phase 1.1** starten: MouseInteractionSystem erstellen
2. Maus-zu-Grid Konvertierung implementieren
3. Erste Tests mit Console-Output
4. Schritt für Schritt durch alle Phasen

**Geschätzte Entwicklungszeit:** 2-3 Stunden für alle Phasen

**Status:** 📋 Bereit zur Umsetzung!
