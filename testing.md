# 🧪 **TESTPROTOKOLL: Building & Tooltip System**

## 📋 **Testziel:**
Systematische Überprüfung aller Features des Building & Tooltip Systems mit detaillierter Protokollierung.

## 🎯 **Testumgebung:**
- **Spiel starten**: `dotnet run`
- **Erwartete Ausgabe**: Industrie-Generierung, UI-Initialisierung
- **Bildschirmauflösung**: 1280x720 (Standard)
- **Testdauer**: ca. 5-10 Minuten

---

## **📝 TESTSEQUENZ (Bitte genau befolgen):**

### **PHASE 1: System-Initialisierung (0-10 Sekunden)**
```
✅ AUFGABEN:
1. Spiel starten mit: dotnet run
2. Warten bis "Content loaded successfully" erscheint
3. Notieren: Anzahl generierter Farmen und Minen
4. Screenshot machen (optional)

📝 ERWARTETE LOG-AUSGABEN:
- [INDUSTRY-GEN] Generated X farms
- [INDUSTRY-GEN] Generated Y mines
- [INDUSTRY-GEN] Generated Z industries total
- BuildingUISystem content loaded

✍️ ERGEBNIS:
Farmen generiert: ___
Minen generiert: ___
Fehler aufgetreten: ___
```

### **PHASE 2: Building UI Test (10-30 Sekunden)**
```
✅ AUFGABEN:
5. Schaue unten in der Mitte des Bildschirms
6. Notiere: Ist das horizontale Building UI sichtbar?
7. Notiere: Welche 4 Gebäude werden angezeigt?
8. Notiere: Sind die Tasten 1,2,3,4 sichtbar?

📝 ERWARTETES VERHALTEN:
- Horizontales Panel unten-mitte mit 4 Buttons
- Buttons zeigen: Steel Works(1), Food Factory(2), Train Depot(3), Station(4)
- Größen angezeigt: 3x3, 3x3, 4x4, 1x4

✍️ ERGEBNIS:
UI sichtbar: ___
Position korrekt: ___
Alle 4 Buttons da: ___
Größen korrekt: ___
```

### **PHASE 3: Tastatur-Steuerung Test (30-60 Sekunden)**
```
✅ AUFGABEN:
9. Drücke Taste "1" (Steel Works)
10. Notiere: Erscheint grün/rot Vorschau beim Mausbewegen?
11. Notiere: Ändert sich der Button-Stil im UI?
12. Drücke "ESC" zum Abbrechen
13. Wiederhole für Tasten "2", "3", "4"

📝 ERWARTETE LOG-AUSGABEN:
- [BUILDING-PLACEMENT] Started placement: steel_works
- [BUILDING-PLACEMENT] Placement cancelled
- KEIN Log-Spam beim Mausbewegen

✍️ ERGEBNIS:
Taste 1 funktioniert: ___
Taste 2 funktioniert: ___
Taste 3 funktioniert: ___
Taste 4 funktioniert: ___
Vorschau sichtbar: ___
Kein Log-Spam: ___
```

### **PHASE 4: Maus-UI Test (60-90 Sekunden)**
```
✅ AUFGABEN:
14. Klicke auf Button "1" (Steel Works) im UI
15. Notiere: Startet Platzierungs-Modus?
16. Klicke auf Button "4" (Station) im UI
17. Notiere: Wechselt zu Station-Platzierung?
18. Drücke "ESC" zum Abbrechen

📝 ERWARTETE LOG-AUSGABEN:
- CLICK: Handled by Building UI
- Building selected via UI: steel_works

✍️ ERGEBNIS:
UI-Buttons klickbar: ___
Platzierung startet: ___
Button-Feedback: ___
```

### **PHASE 5: Gebäude-Platzierung Test (90-120 Sekunden)**
```
✅ AUFGABEN:
19. Drücke "1" für Steel Works
20. Bewege Maus über grüne Bereiche (Grass/Dirt)
21. Linksklick zum Platzieren
22. Notiere: Wurde Gebäude platziert?
23. Drücke "4" für Station
24. Platziere Station (1x4) auf freiem Bereich

📝 ERWARTETE LOG-AUSGABEN:
- [BUILDING-PLACEMENT] Building placed: steel_works at (X,Y)
- [BUILDING-PLACEMENT] Building placed: station at (X,Y)

✍️ ERGEBNIS:
Steel Works platziert: ___
Station platziert: ___
Station ist 1x4: ___
```

### **PHASE 6: Tooltip Test - Terrain (120-150 Sekunden)**
```
✅ AUFGABEN:
25. Linksklick auf leeres Gras-Terrain
26. Notiere: Erscheint Terrain-Tooltip unten-rechts?
27. Drücke "ESC" zum Schließen
28. Linksklick auf Berg-Terrain
29. Notiere: Zeigt Berg-Informationen?

📝 ERWARTETE LOG-AUSGABEN:
- [TOOLTIP-TEST] Valid tile clicked - TileType(Grass)
- [TOOLTIP-TEST] Valid tile clicked - TileType(Mountain)

✍️ ERGEBNIS:
Terrain-Tooltips erscheinen: ___
Position unten-rechts: ___
ESC schließt Tooltip: ___
```

### **PHASE 7: Tooltip Test - Industrien (150-180 Sekunden)**
```
✅ AUFGABEN:
30. Linksklick auf eine automatisch generierte Farm (gelber Punkt)
31. Notiere: Erscheint Farm-Tooltip mit Produktions-Info?
32. Drücke "ESC" zum Schließen
33. Linksklick auf eine Mine (falls vorhanden)
34. Notiere: Zeigt Mine-Informationen?

📝 ERWARTETE LOG-AUSGABEN:
- [TOOLTIP-TEST] Industry clicked - farm at (X,Y)
- [TOOLTIP-TEST] Industry clicked - mine at (X,Y)

✍️ ERGEBNIS:
Farm-Tooltip funktioniert: ___
Mine-Tooltip funktioniert: ___
Produktions-Info sichtbar: ___
```

### **PHASE 8: Tooltip Test - Spieler-Gebäude (180-210 Sekunden)**
```
✅ AUFGABEN:
35. Linksklick auf das platzierte Steel Works
36. Notiere: Erscheint Gebäude-Tooltip mit Details?
37. Notiere: Werden Status, Produktion, Kosten angezeigt?
38. Linksklick auf die platzierte Station
39. Notiere: Zeigt Station-Informationen (1x4 Größe)?

📝 ERWARTETE LOG-AUSGABEN:
- [TOOLTIP-TEST] Building clicked - steel_works at (X,Y)
- [TOOLTIP-TEST] Building clicked - station at (X,Y)

✍️ ERGEBNIS:
Gebäude-Tooltips funktionieren: ___
Details vollständig: ___
Station zeigt 1x4: ___
```

### **PHASE 9: Navigation Test (210-240 Sekunden)**
```
✅ AUFGABEN:
40. Rechtsklick + Ziehen für Kamera-Bewegung
41. Mausrad zum Zoomen
42. WASD-Tasten für Kamera-Bewegung
43. Notiere: Funktioniert Navigation ohne Probleme?
44. Notiere: Bleibt Building UI immer sichtbar?

✍️ ERGEBNIS:
Rechtsklick-Ziehen: ___
Mausrad-Zoom: ___
WASD-Navigation: ___
UI bleibt sichtbar: ___
```

### **PHASE 10: Abschluss-Test (240-270 Sekunden)**
```
✅ AUFGABEN:
45. Drücke "B" zum Umschalten des Building UI
46. Notiere: Verschwindet/erscheint das UI?
47. Teste alle 4 Gebäude-Typen mindestens einmal
48. Notiere: Gibt es Fehler oder Abstürze?
49. Spiel beenden (Alt+F4 oder Fenster schließen)

✍️ ERGEBNIS:
B-Taste funktioniert: ___
Alle Gebäude getestet: ___
Keine Abstürze: ___
```

---

## **📊 BEWERTUNGSKRITERIEN:**

### **✅ ERFOLGREICH wenn:**
- Alle 4 Gebäude-Buttons sichtbar und funktional
- Tastatur (1-4) und Maus-Klicks funktionieren
- Tooltips für Terrain, Industrien und Gebäude erscheinen
- Kein Log-Spam beim Mausbewegen
- Station zeigt 1x4 Größe
- Building UI bleibt stabil sichtbar

### **❌ PROBLEME wenn:**
- Building UI nicht sichtbar oder falsch positioniert
- Buttons nicht klickbar oder falsche Gebäude
- Tooltips erscheinen nicht oder zeigen falsche Infos
- Log-Spam oder Fehler-Meldungen
- Abstürze oder Performance-Probleme

---

## **📝 PROTOKOLL-VORLAGE:**

```
=== TESTPROTOKOLL BUILDING & TOOLTIP SYSTEM ===
Datum: [DATUM]
Tester: [NAME]
Betriebssystem: [Windows 10/11]
.NET Version: [8.0.x]

PHASE 1 - Initialisierung:
- Farmen generiert: [ANZAHL]
- Minen generiert: [ANZAHL]
- UI geladen: [JA/NEIN]

PHASE 2 - Building UI:
- UI sichtbar: [JA/NEIN]
- Position: [BESCHREIBUNG]
- Buttons: [LISTE]

PHASE 3-4 - Steuerung:
- Tastatur 1-4: [FUNKTIONAL/PROBLEME]
- Maus-Klicks: [FUNKTIONAL/PROBLEME]
- UI-Feedback: [BESCHREIBUNG]

PHASE 5 - Platzierung:
- Steel Works: [ERFOLGREICH/FEHLER]
- Station (1x4): [ERFOLGREICH/FEHLER]

PHASE 6-8 - Tooltips:
- Terrain: [FUNKTIONAL/PROBLEME]
- Industrien: [FUNKTIONAL/PROBLEME]
- Gebäude: [FUNKTIONAL/PROBLEME]

PHASE 9-10 - Navigation:
- Kamera: [FUNKTIONAL/PROBLEME]
- Stabilität: [GUT/PROBLEME]

GESAMTBEWERTUNG: [ERFOLGREICH/TEILWEISE/FEHLGESCHLAGEN]

BEMERKUNGEN:
[DETAILS ZU PROBLEMEN ODER BESONDEREN BEOBACHTUNGEN]

EMPFEHLUNGEN:
[VERBESSERUNGSVORSCHLÄGE]
```

---

## **🔧 TROUBLESHOOTING WÄHREND DES TESTS:**

### **Spiel startet nicht:**
- .NET 8.0 SDK installiert? (`dotnet --version`)
- `dotnet restore` ausgeführt?
- Alle Dateien vorhanden?

### **Building UI nicht sichtbar:**
- Taste "B" drücken zum Ein-/Ausblenden
- Bildschirmauflösung mindestens 1280x720?
- Spiel im Vollbild-Modus?

### **Tooltips erscheinen nicht:**
- Linksklick verwenden (nicht Rechtsklick)
- Auf Terrain/Gebäude klicken (nicht auf UI)
- ESC drücken um vorherige Tooltips zu schließen

### **Performance-Probleme:**
- Grafiktreiber aktuell?
- Andere Programme schließen
- Task-Manager prüfen (CPU/RAM Auslastung)

### **Log-Ausgaben fehlen:**
- Terminal/Konsole sichtbar?
- Spiel mit `dotnet run` gestartet?
- Logs werden in Konsole ausgegeben

---

**Bitte führe diesen Test sorgfältig durch und dokumentiere alle Ergebnisse!** 🧪📋

**Bei Problemen: Testprotokoll trotzdem ausfüllen und alle beobachteten Probleme detailliert beschreiben.** 🐛📝