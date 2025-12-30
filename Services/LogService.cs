using System;
using System.IO;
using System.Windows.Forms;

public class LogService : ILogService
{
    private TextBox logBox;
    private string logFile;
    
    public LogService(TextBox logTextBox, string logFilePath)
    {
        this.logBox = logTextBox;
        this.logFile = logFilePath;
        
        // Ensure log directory exists
        if (!string.IsNullOrEmpty(logFile))
        {
            string directory = Path.GetDirectoryName(logFile);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }
    }
    
    public void Log(string message)
    {
        string timestamp = DateTime.Now.ToString("HH:mm:ss");
        string logMessage = string.Format("[{0}] {1}", timestamp, message);
        
        // Log to file
        WriteToFile(logMessage);
        
        // Log to TextBox (if available)
        if (logBox != null && !logBox.IsDisposed)
        {
            if (logBox.InvokeRequired)
            {
                logBox.Invoke(new Action(() => AppendToLog(logMessage)));
            }
            else
            {
                AppendToLog(logMessage);
            }
        }
    }
    
    private void AppendToLog(string message)
    {
        logBox.AppendText(message + Environment.NewLine);
        logBox.SelectionStart = logBox.Text.Length;
        logBox.ScrollToCaret();
        Application.DoEvents();
    }
    
    private void WriteToFile(string message)
    {
        if (!string.IsNullOrEmpty(logFile))
        {
            try
            {
                File.AppendAllText(logFile, message + Environment.NewLine);
            }
            catch
            {
                // Ignore file write errors
            }
        }
    }
    
    public void LogError(string message, Exception ex)
    {
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        string errorLog = string.Format(
            "{0}{1}ERROR OCCURRED{1}{0}Time: {2}{1}Message: {3}{1}",
            new string('=', 40),
            Environment.NewLine,
            timestamp,
            message);
        
        if (ex != null)
        {
            errorLog += string.Format("Exception: {0}{1}Type: {2}{1}Stack Trace:{1}{3}{1}{0}{1}",
                ex.Message,
                Environment.NewLine,
                ex.GetType().FullName,
                ex.StackTrace);
            
            if (ex.InnerException != null)
            {
                errorLog += string.Format("Inner Exception: {0}{1}Inner Stack Trace:{1}{2}{1}{1}",
                    ex.InnerException.Message,
                    Environment.NewLine,
                    ex.InnerException.StackTrace);
            }
        }
        
        WriteToFile(errorLog);
        Log("ERROR: " + message);
        
        if (ex != null)
        {
            Log("  Details: " + ex.Message);
        }
    }
    
    public void LogSuccess(string message)
    {
        Log("SUCCESS: " + message);
    }
    
    public void LogWarning(string message)
    {
        Log("WARNING: " + message);
    }
    
    public void LogSeparator()
    {
        Log("");
    }
    
    public void LogHeader(string header)
    {
        Log("");
        Log("=== " + header + " ===");
    }
}