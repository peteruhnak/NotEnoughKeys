namespace NotEnoughKeys;

public static class GlobalLog
{
    public static bool IsDebugEnabled { get; private set; } = true;

    public static void ToggleDebug()
    {
        IsDebugEnabled = !IsDebugEnabled;
        Info(IsDebugEnabled ? "Debug log enabled" : "Debug log disabled");
    }

    public static event EventHandler<LogEventArgs>? OnMessage;

    public static void Log(LogLevel level, string message, Exception? exception = null)
    {
        if (level == LogLevel.Debug && !IsDebugEnabled) return;
        OnMessage?.Invoke(null, new LogEventArgs(new LogMessage
        {
            Timestamp = DateTime.Now,
            Level = level,
            Message = message,
            Exception = exception
        }));
    }

    public static void Debug(string message, Exception? exception = null)
        => Log(LogLevel.Debug, message, exception);

    public static void Info(string message, Exception? exception = null)
        => Log(LogLevel.Info, message, exception);

    public static void Warn(string message, Exception? exception = null)
        => Log(LogLevel.Warn, message, exception);

    public static void Error(string message, Exception? exception = null)
        => Log(LogLevel.Error, message, exception);
}

public class LogEventArgs : EventArgs
{
    public LogMessage LogMessage { get; }

    public LogEventArgs(LogMessage logMessage)
    {
        LogMessage = logMessage;
    }
}

public class LogMessage
{
    public LogLevel Level { get; init; }
    public string Message { get; init; } = null!;
    public Exception? Exception { get; init; }
    public DateTime Timestamp { get; init; }
}

public enum LogLevel
{
    Debug,
    Info,
    Warn,
    Error
}