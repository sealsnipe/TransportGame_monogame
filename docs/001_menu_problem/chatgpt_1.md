# MonoGame Menü‑Rendering – Debug-Guide

## 1 | Warum erscheinen keine `Console.WriteLine()`‑Ausgaben?

| Ursache | Abhilfe |
|---------|---------|
| **Projekt läuft als *WinExe* (Fenster‑App)** – damit wird **kein Konsolenfenster** angelegt. | Im Projekt‑Explorer → *Eigenschaften* → **Application** → **Output Type → Console Application** einstellen. |
| DesktopGL‑Template ist ebenfalls als *WinExe* vorkonfiguriert. | Manuell `<OutputType>Exe</OutputType>` → `Console` setzen oder per `AllocConsole()` zur Laufzeit eine Konsole öffnen. |
| Logs landen im **Debug‑Output‑Fenster** statt in der Konsole. | `System.Diagnostics.Debug.WriteLine` nutzen; Ausgaben erscheinen in **VS → Ausgabe → Debug**. |
| Release‑Build ohne Debug‑Fenster. | Für Release Logging auf Datei‑ oder In‑Game‑Overlay (SpriteFont) umstellen. |

```xml
<!-- Beispiel csproj -->
<PropertyGroup>
  <OutputType>ConsoleApplication</OutputType>
</PropertyGroup>
```

---

## 2 | Warum wird nur das **erste** `spriteBatch.Draw()` gezeichnet?

1. **`spriteBatch.Begin()`‑Parameter prüfen**

```csharp
_spriteBatch.Begin(
    SpriteSortMode.Deferred,
    BlendState.NonPremultiplied,
    SamplerState.PointClamp,
    DepthStencilState.None);
```

2. **Begin/End‑Paare** – kein verschachteltes Begin ohne End.

3. **Viewport statt DisplayMode verwenden**

```csharp
var vp = GraphicsDevice.Viewport;
var screenWidth  = vp.Width;
var screenHeight = vp.Height;
```

4. **Alpha‑Blending & Texture**: sicherstellen, dass `pixelTexture` vollständig weiß / opak ist.

5. **LayerDepth** explizit setzen (`layerDepth: 0f`).

---

## 3 | Rendering ohne Konsole debuggen

| Tool / Technik | Nutzen |
|----------------|--------|
| **VS Graphics Diagnostics** | Frame‑Capture, Draw‑Calls & States analysieren |
| **RenderDoc / PIX** | GPU‑Analyse, Pixel‑History |
| **In‑Game Overlay** | SpriteFont‑HUD für States & FPS |
| **Farb‑Tests** | Grelle Farben oder dicker Rand zum Sichtbarkeitstest |
| **Log‑Frameworks** | Serilog, NLog → Datei‑ oder Debug‑Window |
| **Breakpoints** | In `DrawMainMenu()` Werte inspizieren |

---

### Schnellstart‑Checkliste

1. Projekt als **Console Application** → Logs sichtbar.  
2. **Viewport**‑Größe nutzen.  
3. Genau **ein** `spriteBatch.Begin()/End()` pro Frame.  
4. Testweise grelle Farbe für Menürechteck.  
5. Frame‑Capture mit RenderDoc / VS Graphics Diagnostics.  
6. BlendState & LayerDepth kontrollieren, falls weiterhin unsichtbar.

Viel Erfolg!
