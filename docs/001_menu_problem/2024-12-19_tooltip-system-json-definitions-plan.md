# 🎯 **UMFASSENDER PLAN: JSON-BASIERTE TILE-DEFINITIONEN + MEHRSPRACHIGE TOOLTIPS**

**Datum:** 19. Dezember 2024  
**Status:** Planung  
**Priorität:** Hoch  

---

## **📋 PROJEKTZIELE**

### **Hauptziele:**
1. **Feste Tooltip-Position** unten rechts statt Maus-folgend
2. **JSON-basierte Tile-Definitionen** für bessere Erweiterbarkeit
3. **Mehrsprachiges System** für internationale Unterstützung
4. **Saubere Ordnerstruktur** für einfache Verwaltung
5. **Eigenschaften-System** für Tiles (bebaubar, Ressourcen, etc.)

### **Erwartetes Ergebnis:**
- ✅ Tooltip zeigt deutsche Beschreibungen in fester Position
- ✅ Einfaches Hinzufügen neuer Tiles ohne Code-Änderung
- ✅ Einfaches Hinzufügen neuer Sprachen
- ✅ Tile-Eigenschaften für zukünftige Gameplay-Features

---

## **🗂️ NEUE ORDNERSTRUKTUR**

```
Game/
├── Data/
│   └── Tiles/
│       ├── Definitions/           # Tile-Eigenschaften (sprachunabhängig)
│       │   ├── Grass.json
│       │   ├── Water.json
│       │   ├── Mountain.json
│       │   ├── Forest.json
│       │   ├── Farmland.json
│       │   ├── Desert.json
│       │   ├── Hills.json
│       │   ├── Beach.json
│       │   ├── DeepWater.json
│       │   ├── Dirt.json
│       │   ├── Rail.json
│       │   └── Road.json
│       └── Localization/          # Übersetzungen
│           ├── de.json            # Deutsch (Standard)
│           ├── en.json            # Englisch
│           ├── fr.json            # Französisch (zukünftig)
│           └── es.json            # Spanisch (zukünftig)
├── Managers/
│   ├── TileDefinitionManager.cs   # [NEU] Lädt Tile-Definitionen
│   └── LocalizationManager.cs     # [NEU] Verwaltet Sprachen
└── Systems/
    └── TooltipSystem.cs           # [ÜBERARBEITEN] Feste Position + JSON-Daten
```

---

## **📄 JSON-STRUKTUR DEFINITIONEN**

### **Beispiel: Grass.json (Tile-Eigenschaften)**
```json
{
  "id": "grass",
  "tileType": "Grass",
  "properties": {
    "buildable": true,
    "movementCost": 1.0,
    "fertility": 0.8,
    "resourceProduction": [],
    "allowedBuildings": ["farm", "depot", "station"],
    "terrain": "land",
    "passable": true
  },
  "rendering": {
    "color": "#228B22",
    "texture": "grass_tile"
  },
  "gameplay": {
    "constructionCost": 0,
    "maintenanceCost": 0,
    "maxBuildings": 1
  }
}
```

### **Beispiel: de.json (Deutsche Lokalisierung)**
```json
{
  "language": "de",
  "tiles": {
    "grass": {
      "name": "Grasland",
      "description": "Fruchtbares Grasland, ideal für Landwirtschaft und Weidewirtschaft. Geeignet für die meisten Gebäude.",
      "properties": {
        "buildable": "Bebaubar",
        "fertility": "Fruchtbarkeit",
        "terrain": "Gelände"
      }
    },
    "water": {
      "name": "Wasser",
      "description": "Klares Süßwasser. Unpassierbar für Landfahrzeuge, aber ideal für Schifffahrt und Fischerei.",
      "properties": {
        "buildable": "Nicht bebaubar",
        "terrain": "Wasser"
      }
    },
    "mountain": {
      "name": "Berg",
      "description": "Steile Berghänge mit wertvollen Mineralien. Nur für Bergwerke und spezielle Gebäude geeignet.",
      "properties": {
        "buildable": "Eingeschränkt bebaubar",
        "resources": "Eisenerz, Stein"
      }
    }
  }
}
```

---

## **🔧 IMPLEMENTIERUNGSPLAN**

### **Phase 1: JSON-Infrastruktur erstellen**
**Geschätzte Zeit:** 2-3 Stunden

#### **1.1 TileDefinitionManager erstellen**
- [ ] Neue Klasse `TileDefinitionManager.cs`
- [ ] JSON-Deserialisierung für Tile-Definitionen
- [ ] Caching-System für Performance
- [ ] Validierung der JSON-Struktur
- [ ] Umfangreiche Test-Logs

#### **1.2 LocalizationManager erstellen**
- [ ] Neue Klasse `LocalizationManager.cs`
- [ ] Sprach-Umschaltung zur Laufzeit
- [ ] Fallback auf Deutsch bei fehlenden Übersetzungen
- [ ] Caching für geladene Sprachen
- [ ] Test-Logs für Lokalisierung

#### **1.3 JSON-Dateien erstellen**
- [ ] Alle 12 Tile-Definition JSONs erstellen
- [ ] Deutsche Lokalisierung (de.json) erstellen
- [ ] Englische Lokalisierung (en.json) erstellen
- [ ] JSON-Schema für Validierung (optional)

### **Phase 2: TooltipSystem überarbeiten**
**Geschätzte Zeit:** 1-2 Stunden

#### **2.1 Feste Position implementieren**
- [ ] Tooltip-Position auf unten rechts ändern
- [ ] Responsive Positionierung (20px vom Rand)
- [ ] Größere Tooltip-Box für mehr Text
- [ ] Bessere Formatierung (Titel + Beschreibung)

#### **2.2 JSON-Integration**
- [ ] TileDefinitionManager in TooltipSystem integrieren
- [ ] LocalizationManager in TooltipSystem integrieren
- [ ] Dynamische Beschreibungen aus JSON laden
- [ ] Eigenschaften-Anzeige implementieren

### **Phase 3: Integration & Testing**
**Geschätzte Zeit:** 1 Stunde

#### **3.1 System-Integration**
- [ ] Neue Manager in TransportGameMain registrieren
- [ ] Initialisierungs-Reihenfolge beachten
- [ ] Error-Handling für fehlende JSON-Dateien
- [ ] Performance-Optimierung

#### **3.2 Umfangreiche Tests**
- [ ] Alle 12 Tile-Typen testen
- [ ] Sprach-Umschaltung testen (Deutsch/Englisch)
- [ ] Tooltip-Position bei verschiedenen Auflösungen
- [ ] Performance-Tests (JSON-Loading)
- [ ] Error-Handling Tests

---

## **🎨 TOOLTIP-DESIGN SPEZIFIKATION**

### **Position & Größe:**
- **Position:** Feste Position unten rechts
- **Abstand:** 20px vom rechten Rand, 20px vom unteren Rand
- **Breite:** 300px (fest)
- **Höhe:** Dynamisch je nach Inhalt (min. 80px)

### **Inhalt-Layout:**
```
┌─────────────────────────────────┐
│ 🟢 Grasland (15,23)            │
├─────────────────────────────────┤
│ Fruchtbares Grasland, ideal    │
│ für Landwirtschaft und Weide-  │
│ wirtschaft. Geeignet für die   │
│ meisten Gebäude.               │
├─────────────────────────────────┤
│ • Bebaubar: Ja                 │
│ • Fruchtbarkeit: Hoch          │
│ • Gelände: Land                │
└─────────────────────────────────┘
```

### **Farb-Schema:**
- **Hintergrund:** Schwarz (80% Transparenz)
- **Rahmen:** Weiß
- **Titel:** Weiß + Fett
- **Beschreibung:** Hellgrau
- **Eigenschaften:** Gelb
- **Tile-Indikator:** Original Tile-Farbe

---

## **🌍 MEHRSPRACHIGKEITS-KONZEPT**

### **Unterstützte Sprachen (Initial):**
1. **Deutsch (de)** - Standard/Fallback
2. **Englisch (en)** - Internationale Unterstützung

### **Zukünftige Erweiterungen:**
3. **Französisch (fr)**
4. **Spanisch (es)**
5. **Italienisch (it)**

### **Sprach-Umschaltung:**
- **Zur Laufzeit** über Einstellungen-Menü (zukünftig)
- **Automatische Erkennung** der System-Sprache
- **Fallback-Mechanismus** auf Deutsch bei fehlenden Übersetzungen

---

## **⚡ PERFORMANCE-ÜBERLEGUNGEN**

### **Optimierungen:**
1. **JSON-Caching:** Einmal laden, dann im Speicher halten
2. **Lazy Loading:** Nur benötigte Sprachen laden
3. **Tooltip-Throttling:** Nicht bei jeder Maus-Bewegung aktualisieren
4. **String-Interpolation:** Effiziente Text-Generierung

### **Speicher-Management:**
- **Tile-Definitionen:** ~50KB für alle Tiles
- **Lokalisierung:** ~20KB pro Sprache
- **Gesamt:** <200KB für vollständiges System

---

## **🧪 TEST-STRATEGIE**

### **Automatisierte Tests:**
1. **JSON-Validierung:** Alle Definitionen korrekt formatiert
2. **Lokalisierung:** Alle Tiles in allen Sprachen vorhanden
3. **Performance:** Loading-Zeit unter 100ms
4. **Memory:** Keine Memory-Leaks bei Sprach-Umschaltung

### **Manuelle Tests:**
1. **Tooltip-Anzeige:** Alle 12 Tile-Typen
2. **Position:** Verschiedene Bildschirmauflösungen
3. **Sprachen:** Deutsch/Englisch Umschaltung
4. **Edge-Cases:** Fehlende Dateien, korrupte JSONs

---

## **📈 ZUKÜNFTIGE ERWEITERUNGEN**

### **Kurzfristig (nächste Sprints):**
- [ ] Gebäude-Tooltips mit ähnlichem System
- [ ] Zug-Tooltips mit Fracht-Informationen
- [ ] Ressourcen-Tooltips

### **Mittelfristig:**
- [ ] Mod-Support für Custom-Tiles
- [ ] Erweiterte Tile-Eigenschaften (Wetter, Jahreszeiten)
- [ ] Animierte Tooltip-Übergänge

### **Langfristig:**
- [ ] Vollständige UI-Lokalisierung
- [ ] Community-Übersetzungen
- [ ] Tile-Editor mit JSON-Export

---

## **✅ ERFOLGSKRITERIEN**

### **Muss-Kriterien:**
1. ✅ Tooltip erscheint in fester Position unten rechts
2. ✅ Deutsche Beschreibungen für alle 12 Tile-Typen
3. ✅ Tile-Eigenschaften werden angezeigt
4. ✅ System läuft stabil ohne Performance-Probleme

### **Soll-Kriterien:**
1. ✅ Englische Übersetzungen verfügbar
2. ✅ Einfaches Hinzufügen neuer Tiles via JSON
3. ✅ Einfaches Hinzufügen neuer Sprachen
4. ✅ Umfangreiche Test-Logs für Debugging

### **Kann-Kriterien:**
1. ✅ JSON-Schema Validierung
2. ✅ Automatische Sprach-Erkennung
3. ✅ Erweiterte Tooltip-Formatierung
4. ✅ Performance-Monitoring

---

**🚀 BEREIT FÜR IMPLEMENTIERUNG!**

**Nächster Schritt:** Phase 1.1 - TileDefinitionManager erstellen
