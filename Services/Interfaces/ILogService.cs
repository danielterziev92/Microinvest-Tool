using System;

public interface ILogService
{
    void Log(string message);
    void LogError(string message, Exception ex);
    void LogSuccess(string message);
    void LogWarning(string message);
    void LogSeparator();
    void LogHeader(string header);
}