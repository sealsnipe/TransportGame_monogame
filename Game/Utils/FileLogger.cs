using System;
using System.IO;

namespace TransportGame.Game.Utils;

/// <summary>
/// Simple file logger for debugging when Console.WriteLine doesn't work.
/// Based on Gemini's recommendation.
/// </summary>
public static class FileLogger
{
    private static readonly string LogFilePath = "debug_log.txt";
    
    public static void Log(string message)
    {
        try
        {
            var logEntry = $"{DateTime.Now:HH:mm:ss.fff}: {message}";
            File.AppendAllText(LogFilePath, logEntry + Environment.NewLine);
        }
        catch
        {
            // Silently fail if logging itself fails
        }
    }
    
    public static void Clear()
    {
        try
        {
            if (File.Exists(LogFilePath))
            {
                File.Delete(LogFilePath);
            }
        }
        catch
        {
            // Silently fail
        }
    }
}
