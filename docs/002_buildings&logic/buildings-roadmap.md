# Buildings & Logic System - Roadmap

## 🎯 Vision & Konzept

### Multi-Tile Gebäude System (Anno-Style)
Das Spiel soll ein **Multi-Tile Building System** implementieren, ähnlich wie in Anno-Spielen:

- **Verschiedene Gebäudegrößen**: 1x1 (Straße), 2x2 (Haus), 3x3 (Fabrik), etc.
- **Tile-basierte Platzierung**: Gebäude bestehen aus mehreren zusammenhängenden Tiles
- **Rotation & Validation**: Gebäude können gedreht und nur auf gültigen Positionen platziert werden

### Map-Skalierung Anforderung
**KRITISCH**: Aktuelle Map (384x216 Tiles) ist zu klein für Multi-Tile Gebäude!
- **Neue Größe**: 768x432 Tiles (Faktor 2 Vergrößerung)
- **Grund**: Mehr Platz für größere Gebäude und komplexere Layouts
- **Impact**: Kamera-System, Performance, Memory Usage

## 🏗️ Gebäude-Kategorien

### 1. Map-generierte Gebäude (Vorgegeben)
**Städte**:
- Bestehen aus mehreren Gebäuden und Straßen
- Produzieren/konsumieren Passagiere
- Verschiedene Größen (Dorf 3x3, Stadt 5x5, Metropole 7x7)

**Rohstoffquellen**:
- **Farm** (2x2): Produziert Getreide (1 pro Sekunde)
- **Mine** (2x2): Produziert Eisenerz (1 pro Sekunde)
- Haben integrierte Lager (Kapazität: 100 Einheiten)

### 2. Spieler-baubare Gebäude
**Produktionsgebäude**:
- **Stahlwerk** (3x3): Erz → Stahl (1:1 Ratio, 1 pro Sekunde)
- **Nahrungsfabrik** (3x3): Getreide → Nahrung (1:1 Ratio, 1 pro Sekunde)
- Haben Input- und Output-Lager (je 50 Kapazität)

**Transport-Infrastruktur**:
- **Zugdepot** (4x4): Züge kaufen, zusammenstellen, verwalten
- **Gleise** (1x1): Verbindungen zwischen Stationen
- **Station** (2x2): Passagier- oder Gütertransport

## 📦 Resource & Production System

### Resource Types
```csharp
enum ResourceType {
    // Rohstoffe
    IronOre,    // Eisenerz (von Minen)
    Grain,      // Getreide (von Farmen)
    
    // Verarbeitete Güter
    Steel,      // Stahl (aus Eisenerz)
    Food,       // Nahrung (aus Getreide)
    
    // Transport
    Passengers  // Passagiere (von Städten)
}
```

### Production Logic
- **Timing**: Jede Sekunde wird 1 Gut produziert
- **Input-Validation**: Produktion nur wenn Input-Lager gefüllt
- **Output-Validation**: Produktion stoppt wenn Output-Lager voll
- **Transport-Integration**: Züge können Güter von/zu Lagern transportieren

## 🚂 Transport Integration

### Station-System
- **Güter-Stationen**: Laden/Entladen von Rohstoffen und Produkten
- **Passagier-Stationen**: Transport von Personen zwischen Städten
- **Reichweite**: Stationen können Gebäude in 3-Tile Radius bedienen

### Zug-Routen
- Züge fahren zwischen Stationen auf Gleisen
- Automatisches Laden/Entladen basierend auf Routen-Konfiguration
- Kapazitäts-Management (verschiedene Waggon-Typen)

## 🎮 UI/UX Anforderungen

### Advanced Tooltips
**Rohstoffquellen** (Farm, Mine):
- Aktueller Lagerbestand (z.B. "Getreide: 45/100")
- Produktionsrate ("1 pro Sekunde")
- Status ("Produziert" / "Lager voll")

**Produktionsgebäude** (Stahlwerk, Nahrungsfabrik):
- Input-Lager ("Erz: 20/50")
- Output-Lager ("Stahl: 35/50") 
- Produktionsstatus ("Arbeitet" / "Wartet auf Input" / "Output voll")
- Effizienz-Anzeige

### Building Placement UI
- **Placement Mode**: Gebäude-Auswahl und Vorschau
- **Validation Feedback**: Grün (gültig) / Rot (ungültig)
- **Cost Display**: Baukosten vor Platzierung
- **Rotation Controls**: R-Taste für Drehung

## 🔧 Technische Implementierung

### Priorisierte Phasen

#### PHASE 1: Fundament (HÖCHSTE PRIORITÄT)
1. **Map Scaling** (768x432) - Foundation
2. **Multi-Tile Building System** - Core Mechanic
3. **Basic Resource System** - Game Logic
4. **Building Placement UI** - User Interaction

#### PHASE 2: Produktion (HOHE PRIORITÄT)
5. **Production System** - Game Loop
6. **Advanced Tooltips** - User Feedback
7. **Resource Storage & Transfer** - Logistics
8. **Basic Economy** - Progression

#### PHASE 3: Transport (MITTLERE PRIORITÄT)
9. **Railway Building** - Infrastructure
10. **Train System** - Automation
11. **Station System** - Connections
12. **Route Management** - Optimization

#### PHASE 4: Polish (NIEDRIGE PRIORITÄT)
13. **Advanced UI/UX** - Quality of Life
14. **Save/Load System** - Persistence
15. **Performance Optimization** - Scalability
16. **Audio/Visual Effects** - Immersion

### Kritische Abhängigkeiten
- **Map Scaling** muss ZUERST implementiert werden
- **Building System** ist Grundlage für alles andere
- **Resource System** vor Production System
- **Transport System** benötigt vollständiges Building System

## 📊 Success Metrics
- [ ] Gebäude können platziert und gedreht werden
- [ ] Produktionsketten funktionieren (Erz → Stahl)
- [ ] Lager füllen/leeren sich korrekt
- [ ] Züge transportieren Güter automatisch
- [ ] UI zeigt alle relevanten Informationen
- [ ] Performance bleibt bei größerer Map stabil

## 🚀 Nächste Schritte
1. **Map Scaling implementieren** (sofortiger Start)
2. **Building System Foundation** entwickeln
3. **Resource System Core** aufbauen
4. **Iterative Entwicklung** mit regelmäßigen Tests

---
*Erstellt: 2024 | Status: Planning Phase*
