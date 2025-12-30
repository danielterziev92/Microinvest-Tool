using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

public class CollationService
{
    private ILogService logger;
    
    public CollationService(ILogService logService)
    {
        this.logger = logService;
    }
    
    public string GetServerCollation(string instanceName)
    {
        SqlConnection connection = null;
        SqlCommand command = null;
        
        try
        {
            string connectionString = BuildConnectionString(instanceName);
            
            connection = new SqlConnection(connectionString);
            connection.Open();
            
            command = new SqlCommand("SELECT SERVERPROPERTY('Collation') AS Collation", connection);
            object result = command.ExecuteScalar();
            
            if (result != null && result != DBNull.Value)
            {
                string collation = result.ToString();
                logger.Log("Retrieved collation: " + collation);
                return collation;
            }
        }
        catch (Exception ex)
        {
            logger.LogError("Failed to get collation via SQL connection", ex);
        }
        finally
        {
            if (command != null)
            {
                command.Dispose();
            }
            
            if (connection != null)
            {
                connection.Close();
                connection.Dispose();
            }
        }
        
        return "Unknown";
    }
    
    private string BuildConnectionString(string instanceName)
    {
        string serverName = ".";
        
        if (!string.IsNullOrEmpty(instanceName) && !instanceName.Equals("MSSQLSERVER", StringComparison.OrdinalIgnoreCase))
        {
            serverName = ".\\" + instanceName;
        }
        
        return string.Format("Server={0};Integrated Security=true;Connection Timeout=5;", serverName);
    }
    
    public bool ChangeServerCollation(string instanceName, string newCollation, string serviceName)
    {
        Process process = null;
        
        try
        {
            logger.Log("Starting collation change process...");
            
            string sqlServerPath = GetSqlServerBinnPath(instanceName);
            
            if (string.IsNullOrEmpty(sqlServerPath))
            {
                logger.LogError("Could not find SQL Server installation path", null);
                return false;
            }
            
            logger.Log("SQL Server path: " + sqlServerPath);
            
            logger.Log("Stopping SQL Server service: " + serviceName);
            
            if (!StopSqlService(serviceName))
            {
                logger.LogError("Failed to stop SQL Server service", null);
                return false;
            }
            
            System.Threading.Thread.Sleep(3000);
            
            logger.Log("Starting SQL Server in single-user mode for collation change...");
            
            string sqlservrExe = Path.Combine(sqlServerPath, "sqlservr.exe");
            
            if (!File.Exists(sqlservrExe))
            {
                logger.LogError("sqlservr.exe not found at: " + sqlservrExe, null);
                return false;
            }
            
            string instanceParam = instanceName.Equals("MSSQLSERVER", StringComparison.OrdinalIgnoreCase) 
                ? "MSSQLSERVER" 
                : instanceName;
            
            string arguments = string.Format("-m -T4022 -T3659 -s\"{0}\" -q\"{1}\"", instanceParam, newCollation);
            
            logger.Log("Running: " + sqlservrExe + " " + arguments);
            
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = sqlservrExe;
            startInfo.Arguments = arguments;
            startInfo.WorkingDirectory = sqlServerPath;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            
            process = new Process();
            process.StartInfo = startInfo;
            
            process.OutputDataReceived += delegate(object sender, DataReceivedEventArgs e)
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    logger.Log("SQLSERVR: " + e.Data);
                }
            };
            
            process.ErrorDataReceived += delegate(object sender, DataReceivedEventArgs e)
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    logger.Log("SQLSERVR ERROR: " + e.Data);
                }
            };
            
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            
            logger.Log("Waiting for collation change to complete (this may take 30-60 seconds)...");
            
            bool completed = process.WaitForExit(90000);
            
            if (!completed)
            {
                logger.LogWarning("Collation change timed out, killing process");
                process.Kill();
                return false;
            }
            
            int exitCode = process.ExitCode;
            logger.Log("sqlservr.exe exit code: " + exitCode);
            
            logger.Log("Starting SQL Server service...");
            System.Threading.Thread.Sleep(2000);
            
            if (!StartSqlService(serviceName))
            {
                logger.LogError("Failed to start SQL Server service", null);
                return false;
            }
            
            logger.LogSuccess("Collation changed successfully!");
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError("Error changing collation", ex);
            return false;
        }
        finally
        {
            if (process != null)
            {
                process.Dispose();
            }
        }
    }
    
    private string GetSqlServerBinnPath(string instanceName)
    {
        RegistryKey key = null;
        
        try
        {
            string registryPath;
            
            if (instanceName.Equals("MSSQLSERVER", StringComparison.OrdinalIgnoreCase))
            {
                registryPath = "SOFTWARE\\Microsoft\\Microsoft SQL Server\\MSSQL16.MSSQLSERVER\\Setup";
            }
            else
            {
                registryPath = string.Format("SOFTWARE\\Microsoft\\Microsoft SQL Server\\MSSQL16.{0}\\Setup", instanceName);
            }
            
            key = Registry.LocalMachine.OpenSubKey(registryPath);
            
            if (key != null)
            {
                object sqlPath = key.GetValue("SQLPath");
                
                if (sqlPath != null)
                {
                    key.Close();
                    return Path.Combine(sqlPath.ToString(), "MSSQL", "Binn");
                }
                
                key.Close();
            }
            
            string[] versions = { "15", "14", "13", "12", "11" };
            
            foreach (string version in versions)
            {
                if (instanceName.Equals("MSSQLSERVER", StringComparison.OrdinalIgnoreCase))
                {
                    registryPath = string.Format("SOFTWARE\\Microsoft\\Microsoft SQL Server\\MSSQL{0}.MSSQLSERVER\\Setup", version);
                }
                else
                {
                    registryPath = string.Format("SOFTWARE\\Microsoft\\Microsoft SQL Server\\MSSQL{0}.{1}\\Setup", version, instanceName);
                }
                
                key = Registry.LocalMachine.OpenSubKey(registryPath);
                
                if (key != null)
                {
                    object sqlPath = key.GetValue("SQLPath");
                    
                    if (sqlPath != null)
                    {
                        string path = Path.Combine(sqlPath.ToString(), "MSSQL", "Binn");
                        key.Close();
                        return path;
                    }
                    
                    key.Close();
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError("Error getting SQL Server path", ex);
        }
        finally
        {
            if (key != null)
            {
                key.Close();
            }
        }
        
        return null;
    }
    
    private bool StopSqlService(string serviceName)
    {
        Process stopProcess = null;
        
        try
        {
            ProcessStartInfo stopInfo = new ProcessStartInfo();
            stopInfo.FileName = "net";
            stopInfo.Arguments = "stop \"" + serviceName + "\"";
            stopInfo.UseShellExecute = false;
            stopInfo.CreateNoWindow = true;
            stopInfo.RedirectStandardOutput = true;
            stopInfo.Verb = "runas";
            
            stopProcess = Process.Start(stopInfo);
            stopProcess.WaitForExit(30000);
            
            bool success = stopProcess.ExitCode == 0;
            
            stopProcess.Close();
            
            return success;
        }
        catch (Exception ex)
        {
            logger.LogError("Error stopping service", ex);
            return false;
        }
        finally
        {
            if (stopProcess != null)
            {
                stopProcess.Dispose();
            }
        }
    }
    
    private bool StartSqlService(string serviceName)
    {
        Process startProcess = null;
        
        try
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "net";
            startInfo.Arguments = "start \"" + serviceName + "\"";
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.Verb = "runas";
            
            startProcess = Process.Start(startInfo);
            startProcess.WaitForExit(60000);
            
            bool success = startProcess.ExitCode == 0;
            
            startProcess.Close();
            
            return success;
        }
        catch (Exception ex)
        {
            logger.LogError("Error starting service", ex);
            return false;
        }
        finally
        {
            if (startProcess != null)
            {
                startProcess.Dispose();
            }
        }
    }
}
