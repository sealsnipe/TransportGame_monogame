using System.Diagnostics;

namespace TransportGame.Game.Managers;

/// <summary>
/// Handles errors and logging throughout the game.
/// Ported from Godot ErrorHandler.gd singleton.
/// </summary>
public class ErrorHandler
{
    private int _errorCount = 0;
    private int _warningCount = 0;
    private const int MAX_ERROR_COUNT = 10;
    private const int MAX_WARNING_COUNT = 50;
    
    private readonly List<string> _errorLog = new();
    private readonly List<string> _warningLog = new();
    
    public event Action<string>? OnError;
    public event Action<string>? OnWarning;
    public event Action<string>? OnInfo;

    /// <summary>
    /// Handles a critical error. May exit the application if too many errors occur.
    /// </summary>
    public void HandleError(string errorMessage, string source = "", bool autoExit = true)
    {
        _errorCount++;
        
        var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        var fullMessage = $"[{timestamp}] [ERROR {_errorCount}] {errorMessage}";
        
        if (!string.IsNullOrEmpty(source))
            fullMessage += $" (Source: {source})";
        
        _errorLog.Add(fullMessage);
        LogError(fullMessage);
        OnError?.Invoke(fullMessage);
        
        if (_errorCount >= MAX_ERROR_COUNT || autoExit)
        {
            var criticalMessage = $"CRITICAL: Too many errors ({_errorCount}). Shutting down.";
            LogError(criticalMessage);
            
            // Save error log before exit
            SaveErrorLog();
            
            // Give a moment for cleanup
            Thread.Sleep(500);
            Environment.Exit(1);
        }
    }

    /// <summary>
    /// Handles a warning that doesn't require application shutdown.
    /// </summary>
    public void HandleWarning(string warningMessage, string source = "")
    {
        _warningCount++;
        
        var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        var fullMessage = $"[{timestamp}] [WARNING {_warningCount}] {warningMessage}";
        
        if (!string.IsNullOrEmpty(source))
            fullMessage += $" (Source: {source})";
        
        _warningLog.Add(fullMessage);
        LogWarning(fullMessage);
        OnWarning?.Invoke(fullMessage);
        
        if (_warningCount >= MAX_WARNING_COUNT)
        {
            HandleError($"Too many warnings ({_warningCount}). This may indicate a serious problem.", "ErrorHandler");
        }
    }

    /// <summary>
    /// Logs an informational message.
    /// </summary>
    public void LogInfo(string message, string source = "")
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        var fullMessage = $"[{timestamp}] [INFO] {message}";
        
        if (!string.IsNullOrEmpty(source))
            fullMessage += $" (Source: {source})";
        
        Console.WriteLine(fullMessage);
        Debug.WriteLine(fullMessage);
        OnInfo?.Invoke(fullMessage);
    }

    /// <summary>
    /// Logs an error message to console and debug output.
    /// </summary>
    private void LogError(string message)
    {
        Console.WriteLine(message);
        Debug.WriteLine(message);
        
        // Also write to stderr
        Console.Error.WriteLine(message);
    }

    /// <summary>
    /// Logs a warning message to console and debug output.
    /// </summary>
    private void LogWarning(string message)
    {
        Console.WriteLine(message);
        Debug.WriteLine(message);
    }

    /// <summary>
    /// Saves the error log to a file for debugging purposes.
    /// </summary>
    private void SaveErrorLog()
    {
        try
        {
            var logDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TransportGame", "Logs");
            Directory.CreateDirectory(logDirectory);
            
            var logFileName = $"error_log_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            var logFilePath = Path.Combine(logDirectory, logFileName);
            
            var logContent = new List<string>
            {
                "=== TRANSPORT GAME ERROR LOG ===",
                $"Generated: {DateTime.Now}",
                $"Total Errors: {_errorCount}",
                $"Total Warnings: {_warningCount}",
                "",
                "=== ERRORS ===",
            };
            
            logContent.AddRange(_errorLog);
            logContent.Add("");
            logContent.Add("=== WARNINGS ===");
            logContent.AddRange(_warningLog);
            
            File.WriteAllLines(logFilePath, logContent);
            
            Console.WriteLine($"Error log saved to: {logFilePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to save error log: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets the current error count.
    /// </summary>
    public int GetErrorCount() => _errorCount;

    /// <summary>
    /// Gets the current warning count.
    /// </summary>
    public int GetWarningCount() => _warningCount;

    /// <summary>
    /// Clears all logged errors and warnings.
    /// </summary>
    public void ClearLogs()
    {
        _errorCount = 0;
        _warningCount = 0;
        _errorLog.Clear();
        _warningLog.Clear();
        
        LogInfo("Error and warning logs cleared");
    }
}
