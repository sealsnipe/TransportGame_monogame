using TransportGame.Core;

namespace TransportGame;

/// <summary>
/// Entry point for the Transport Game application.
/// </summary>
public static class Program
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    public static void Main()
    {
        Console.WriteLine("=== TRANSPORT GAME STARTING ===");
        try
        {
            Console.WriteLine("Creating game instance...");
            using var game = new TransportGameMain();
            Console.WriteLine("Starting game loop...");
            game.Run();
            Console.WriteLine("Game loop ended.");
        }
        catch (Exception ex)
        {
            // Log critical errors before shutdown
            Console.WriteLine($"CRITICAL ERROR: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");

            // Simple console error output for now
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();

            Environment.Exit(1);
        }
    }
}
