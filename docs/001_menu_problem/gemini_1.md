Okay, this is a well-structured problem analysis! It clearly outlines what's working, what's not, and provides relevant code snippets. Let's break down the issues and address your specific questions.

The two main problems seem to be:

Lack of Console.WriteLine() output, which severely hampers debugging.
Partial rendering with SpriteBatch, where only the first draw call (the overlay) is visible.
Here's an analysis and answers to your questions:

🔍 Analysis of Potential Issues
Based on your MenuSystem.DrawMainMenu code:

C#

// In Game/Systems/MenuSystem.cs
public void Draw(SpriteBatch spriteBatch, Texture2D pixelTexture) {
    if (!_isMenuVisible && !_isOptionsVisible) return; // Wird NICHT ausgeführt
    
    if (_isMenuVisible) {
        DrawMainMenu(spriteBatch, pixelTexture); // SOLLTE aufgerufen werden
    }
}

private void DrawMainMenu(SpriteBatch spriteBatch, Texture2D pixelTexture) {
    Console.WriteLine("*** DrawMainMenu called ***"); // LOG ERSCHEINT NICHT
    
    var screenWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
    var screenHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
    
    // OVERLAY - FUNKTIONIERT (wird angezeigt)
    var overlayRect = new Rectangle(0, 0, screenWidth, screenHeight);
    spriteBatch.Draw(pixelTexture, overlayRect, Color.Black * 0.7f); // <<< THIS WORKS
    
    // MENU BACKGROUND - FUNKTIONIERT NICHT (wird nicht angezeigt)
    var menuX = (screenWidth - MENU_WIDTH) / 2;
    var menuY = (screenHeight - MENU_HEIGHT) / 2;
    var menuRect = new Rectangle(menuX, menuY, MENU_WIDTH, MENU_HEIGHT);
    spriteBatch.Draw(pixelTexture, menuRect, Color.DarkGray); // <<< THIS DOESN'T
    
    // BORDER - FUNKTIONIERT NICHT
    DrawBorder(spriteBatch, pixelTexture, menuRect, Color.White, 2); // <<< THIS DOESN'T
}
Regarding the SpriteBatch Issue:
The most common reason for the first SpriteBatch.Draw() working and subsequent ones failing within the same Begin/End block (assuming _spriteBatch.Begin() was called before _menuSystem.Draw() in TransportGameMain.cs) is often related to the pixelTexture or how SpriteBatch handles textures and batching.

Is pixelTexture valid? You state _renderSystem.GetPixelTexture() provides it.
If pixelTexture is a 1x1 white texture (a common technique): This should generally work fine for drawing solid colored rectangles.
If pixelTexture is something else or becomes invalid: This could be an issue. However, it working for the first draw makes this less likely unless its state changes.
SpriteBatch Begin parameters: The way SpriteBatch.Begin() is called in TransportGameMain.cs (which isn't shown but is crucial) can affect rendering. Default parameters are usually fine, but custom SpriteSortMode, BlendState, SamplerState, DepthStencilState, or Effect could interfere.
SpriteSortMode.Immediate would cause each sprite to render as Draw is called. If there's an issue with the graphics state after the first draw, subsequent ones might fail.
SpriteSortMode.Deferred (default) batches draw calls and processes them on End. This is usually more performant and less prone to state issues between individual draws within the batch.
Graphics Device State: It's rare, but a graphics device state corruption could occur.
Given that the overlay draws, the SpriteBatch itself is working, and the pixelTexture is at least initially valid. The problem is likely subtle.

A key detail is missing: _spriteBatch.Begin() call.
In your Core/TransportGameMain.cs - Draw():

C#

protected override void Draw(GameTime gameTime) {
    // ... andere Rendering ...
    // >>> WHERE IS _spriteBatch.Begin()? It MUST be called before any Draw.
    // >>> If other rendering also uses _spriteBatch, it might have been called there.
    
    // Draw menu system (on top of everything)
    if (_menuSystem?.IsMenuVisible == true && _renderSystem != NULL) {
        Console.WriteLine("*** DRAWING MENU SYSTEM ***"); // LOG ERSCHEINT NICHT
        _menuSystem.Draw(_spriteBatch, _renderSystem.GetPixelTexture());
    }
    
    _spriteBatch.End(); // This implies Begin() was called somewhere.
    base.Draw(gameTime);
}
If _spriteBatch.Begin() is called inside the "andere Rendering" section, and then an _spriteBatch.End() is also called there, the _spriteBatch.End() in your Draw method would be an unmatched call, or the menu would be drawn outside a Begin/End pair.

However, since the first draw (overlay) works, it suggests Begin() has been called.

Let's consider the DrawBorder method (code not shown). If it also uses spriteBatch.Draw with the same pixelTexture, it should behave similarly to the menu background draw.

💬 Answers to Your Specific Questions
Here are answers to your questions, incorporating common MonoGame knowledge:

MonoGame Console Logging:
Warum erscheinen Console.WriteLine() Logs nicht in MonoGame?

Project Type: By default, MonoGame templates (especially for Windows DX) often create a "Windows Application" type project, not a "Console Application." Such applications don't automatically have a console window attached.
Output Window: Logs from Console.WriteLine() usually go to the Output window in Visual Studio (View -> Output, then select "Debug" from the "Show output from:" dropdown). They won't appear in a separate command prompt window unless you've configured the project differently.
Release vs. Debug Builds: Sometimes, build configurations might strip Console.WriteLine calls or redirect output differently in Release builds, but this is less common for standard Console.WriteLine.
Wie kann man MonoGame Rendering ohne Console-Logs debuggen?

Visual Debugging: Change colors, positions, or sizes of elements drastically to see if any change occurs. For example, make your menu background bright red and fullscreen.
Breakpoints: Set breakpoints in your DrawMainMenu method and step through the code. Inspect variable values (like menuRect, pixelTexture.IsDisposed, colors) to ensure they are what you expect.
RenderDoc, Pix, or NVIDIA Nsight: These are graphics debuggers that can capture a frame and let you inspect draw calls, textures, and states. RenderDoc is free and very powerful.
Simplified Test Case: Create a minimal new project or a separate, simple drawing routine with just two colored rectangles to see if the issue is specific to your MenuSystem or more fundamental.
Gibt es alternative Logging-Methoden für MonoGame?

System.Diagnostics.Debug.WriteLine(): This also writes to the Visual Studio Output window and is often preferred for debug messages as it can be conditionally compiled out.
File Logging: Implement a simple file logger (e.g., using System.IO.File.AppendAllText). This is robust and works regardless of project type or IDE.
On-Screen Debug Display: For runtime values, draw text directly to the screen using SpriteBatch.DrawString(). This is very useful for seeing dynamic values in real-time. Create a small debug panel in a corner of your screen.
Dedicated Logging Libraries: Consider libraries like Serilog or NLog for more advanced logging features (structured logging, different outputs, etc.), though this might be overkill for this specific problem.
SpriteBatch Rendering:
Warum funktioniert nur der erste spriteBatch.Draw() Aufruf?
This is the core of your rendering problem. Possible reasons:

pixelTexture State: While unlikely if it's a simple 1x1 texture, if pixelTexture were, for example, a RenderTarget2D that is being written to and then read from without proper synchronization or if it gets disposed, it could cause issues. However, for a shared "pixel" texture, this is less probable.
SpriteBatch.Begin() Parameters: As mentioned, if SpriteBatch.Begin() was called with SpriteSortMode.Immediate and some graphics state (like BlendState or DepthStencilState) was altered by the first Draw call (or the underlying shader/effect used by SpriteBatch) in a way that breaks subsequent draws, this could happen. If you are using a custom effect with SpriteBatch.Begin(effect: myEffect), that effect could be the culprit.
Graphics Device Reset/Lost Context: Very rare on modern PCs, but if the graphics device encountered an issue, it might only partially complete rendering. You'd usually get an exception for this, though.
Incorrect SpriteBatch.Begin() / End() Pairing: If you have nested Begin/End calls, or if End() is called prematurely, subsequent draws won't work. However, the error message usually indicates this. Since the overlay works, your main Begin/End in TransportGameMain.cs seems to encompass the menu drawing.
Shader/Effect Issues: If pixelTexture or SpriteBatch is implicitly or explicitly using a shader, that shader might have an issue that only manifests after the first draw or with certain parameters.
The pixelTexture itself: If _renderSystem.GetPixelTexture() somehow returns a different or modified texture object on subsequent (internal) calls or if its content changes in a way that makes it invisible (e.g., fully transparent for the subsequent draws), this could be it. This is unlikely for a simple pixel texture.
Most Likely Suspect given the code: A subtle issue with the SpriteBatch state or the pixelTexture itself. A common mistake is if pixelTexture is not a simple 1x1 white texture but something more complex, or if its alpha is zero. However, Color.DarkGray and Color.White have full alpha by default.

Crucial Test: Try replacing pixelTexture in the problematic draw calls with a newly created 1x1 white texture right before the draw call to isolate if pixelTexture is the issue.

C#

// Inside DrawMainMenu before drawing the menu background
Texture2D testPixel = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
testPixel.SetData(new[] { Color.White });
// ...
spriteBatch.Draw(testPixel, menuRect, Color.DarkGray); // Use testPixel
// ...
// Remember to dispose testPixel if you create many, or make it a class member
Kann ein spriteBatch.Draw() nachfolgende Draws in derselben Methode blockieren?

No, not directly "block" in the sense of preventing execution. However, a Draw call can change a graphics device state (especially if using SpriteSortMode.Immediate or custom effects) that makes subsequent draws render incorrectly or not at all (e.g., drawing with full transparency, or outside the viewport due to a changed transformation matrix).
If an unhandled exception occurred during a Draw call, it would halt further execution in that block, but you'd typically see the exception.
Gibt es SpriteBatch State-Probleme, die das verursachen können?
Yes. The primary states managed or affected by SpriteBatch.Begin() are:

SpriteSortMode: Deferred (default) vs. Immediate. Immediate sends draw calls to the GPU one by one. Deferred batches them until End() is called. If you use Immediate, the GPU state set for one draw call persists for the next unless explicitly changed.
BlendState: Controls how colors blend with what's already on the screen (e.g., BlendState.AlphaBlend for transparency, BlendState.Opaque for no transparency). If this somehow got set to a state where alpha makes things invisible, that could be an issue. Your Color.Black * 0.7f implies AlphaBlend is likely active and working for the overlay.
SamplerState: Controls how textures are sampled (e.g., SamplerState.PointClamp for pixel art, SamplerState.LinearClamp for smooth). Unlikely to cause invisibility unless it's a very specific custom shader interaction.
DepthStencilState: Controls depth and stencil buffer operations. Usually DepthStencilState.None for 2D sprite rendering. If depth testing got enabled and your menu is drawn at the same depth or behind something opaque, it could be hidden. This is a common cause of "disappearing" sprites.
RasterizerState: Controls things like culling and fill mode. RasterizerState.CullCounterClockwise is common. Unlikely to be the issue here for 2D sprites unless a custom state is causing clipping.
Effect: A custom shader (Effect) passed to SpriteBatch.Begin() will be used for all draw calls in that batch. If this effect has an error or behaves unexpectedly with certain vertex/pixel data, it could cause issues.
If SpriteBatch.Begin() is called without parameters, it uses sensible defaults (SpriteSortMode.Deferred, BlendState.AlphaBlend, DepthStencilState.None, SamplerState.LinearClamp, RasterizerState.CullCounterClockwise, null effect). These defaults are generally robust for the kind of menu rendering you're doing.

Test this: Try explicitly calling _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend); right before your _menuSystem.Draw() call and _spriteBatch.End(); right after, just for the menu, to ensure default states are used for menu rendering, in case other rendering changed them. This will help isolate if it's a state bleed from "andere Rendering".

Debugging Strategien:
Wie debuggt man MonoGame Rendering-Probleme effektiv?

Isolate: Create the simplest possible scenario that reproduces the problem. Remove other game objects, systems, and post-processing effects.
Visualize:
Use bright, distinct colors for different elements.
Draw outlines around your sprites/rectangles: DrawRectangle(yourRect, Color.Yellow, 1);
Temporarily make problematic elements very large or place them at (0,0) to ensure they aren't off-screen.
Step Through Code: Use the debugger to step line-by-line through your Draw calls. Inspect all variables: positions, sizes, colors, texture validity.
Check Texture Contents: If using RenderDoc or similar, you can inspect the actual texture (pixelTexture) being used. You can also save textures to disk from your code for inspection (Texture2D.SaveAsPng).
Graphics Device States: In the debugger, you can inspect GraphicsDevice.BlendState, GraphicsDevice.DepthStencilState, etc., before and after SpriteBatch.Begin() and SpriteBatch.End() and between draw calls if using SpriteSortMode.Immediate.
Incremental Rendering: Start with just one Draw call (e.g., the menu background). If it appears, add the next one (e.g., border). This helps pinpoint exactly which call fails or causes subsequent failures.
Welche Tools gibt es für MonoGame Visual Debugging?

RenderDoc: (Highly Recommended) A free standalone graphics debugger. You configure it to launch your game, then you can capture a frame and inspect every draw call, GPU state, texture, buffer, and shader. This is invaluable for complex rendering issues.
PIX for Windows: (If using DirectX version of MonoGame) A performance tuning and debugging tool for DirectX applications.
NVIDIA Nsight Graphics / AMD Radeon GPU Profiler: GPU vendor-specific tools that offer deep insights into graphics performance and debugging.
In-Game Debug Displays: As mentioned, drawing debug information (FPS, object counts, variable values, state information) directly onto the screen using SpriteBatch.DrawString().
MonoGame.Extended: This library has some debug rendering utilities (e.g., for shapes) that can be helpful.
💡 Recommended Next Steps & Workaround Ideas from Your Document (Reiterated with Priority)
Solve Console Logging First (or use an alternative immediately):

Check Visual Studio's "Output" window (Show output from: Debug).
Try System.Diagnostics.Debug.WriteLine().
Implement basic file logging now. This will be your eyes.
C#

// Simple File Logger - quick and dirty for immediate use
public static class FileLogger {
    private static string logFilePath = "debug_log.txt";
    public static void Log(string message) {
        try {
            System.IO.File.AppendAllText(logFilePath, $"{System.DateTime.Now}: {message}\n");
        } catch { /* Silently fail if logging itself fails, or handle */ }
    }
}
// Usage: FileLogger.Log("DrawMainMenu called");
Verify SpriteBatch.Begin() / End() Calls:

Ensure _spriteBatch.Begin() is called before _menuSystem.Draw().
Ensure there are no other _spriteBatch.End() calls between the Begin() that starts the scene rendering and the End() after the menu.
Crucial: For testing, try an explicit _spriteBatch.Begin(); right before _menuSystem.Draw() and an _spriteBatch.End(); right after it. This isolates the menu drawing into its own batch with default states.
C#

// In Core/TransportGameMain.cs - Draw()
// ...
if (_menuSystem?.IsMenuVisible == true && _renderSystem != null) {
    // Potentially call _spriteBatch.End() if a batch is already active from "andere Rendering"
    // Then start a new one for the menu
    // _spriteBatch.End(); // If needed
    _spriteBatch.Begin(); // Use default states for menu
    FileLogger.Log("*** DRAWING MENU SYSTEM (inside new batch) ***");
    _menuSystem.Draw(_spriteBatch, _renderSystem.GetPixelTexture());
    _spriteBatch.End(); // End menu batch
    // Potentially start a new batch if subsequent rendering needs it
    // _spriteBatch.Begin(); // For base.Draw or other UI
}
// ...
// The _spriteBatch.End(); base.Draw(gameTime); might need adjustment
// depending on how base.Draw uses SpriteBatch.
Be careful with nesting or multiple Begin/End calls. A common pattern is one Begin at the start of Draw and one End at the end. If so, ensure no other End is called prematurely.
Isolate pixelTexture:

In DrawMainMenu, temporarily create a new Texture2D(GraphicsDevice, 1, 1) and SetData(new[] { Color.White }) to it. Use this new texture for the menu background and border draws. If this works, the issue lies with how _renderSystem.GetPixelTexture() is providing or managing the original pixelTexture.
C#

// In MenuSystem.DrawMainMenu
// Texture2D localPixel = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
// localPixel.SetData(new[] { Color.White });
// spriteBatch.Draw(localPixel, menuRect, Color.DarkGray);
// spriteBatch.Draw(localPixel, borderRect, borderColor); // Assuming DrawBorder uses it
// localPixel.Dispose(); // If created per frame, dispose it. Better to make it a member if used often.
Simplify DrawMainMenu:

Comment out the DrawBorder call.
Comment out the Overlay draw.
Focus only on getting spriteBatch.Draw(pixelTexture, menuRect, Color.DarkGray); to show up. Use a very bright color like Color.Red or Color.Lime temporarily.
Check MENU_WIDTH and MENU_HEIGHT:

Log these values. Are they positive? Are they excessively large or small?
Log menuX, menuY, menuRect. Ensure they are within screen bounds. If MENU_WIDTH is larger than screenWidth, menuX might be negative, but the rectangle should still be partially visible if part of it is on screen.
The fact that the overlay Color.Black * 0.7f works suggests BlendState.AlphaBlend is active. The subsequent draws not working, even with opaque colors like Color.DarkGray, is very puzzling if the texture is a simple white pixel and SpriteBatch states are default. This often points to the items being drawn off-screen, behind something, or with a transparent color/texture, or an issue with the texture itself after the first use.

Keep us updated on what the file logs reveal, especially around the SpriteBatch.Begin/End calls and the texture details!