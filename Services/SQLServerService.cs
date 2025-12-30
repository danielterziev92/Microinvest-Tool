using System;
using System.Collections.Generic;
using System.ServiceProcess;
using Microsoft.Win32;

public class SQLServerService : ISQLServerService
{
    private const string REGISTRY_PATH = @"SOFTWARE\Microsoft\Microsoft SQL Server\Instance Names\SQL";
    
    private ILogService logger;
    private TcpIpConfigService tcpService;
    
    public SQLServerService(ILogService logService)
    {
        this.logger = logService;
        this.tcpService = new TcpIpConfigService(logService);
    }
    
    public List<SQLServerInfo> DiscoverInstances()
    {
        return GetAllInstances();
    }
    
    public List<SQLServerInfo> GetAllInstances()
    {
        List<SQLServerInfo> instances = new List<SQLServerInfo>();
        
        logger.Log("Scanning for SQL Server instances...");
        
        try
        {
            RegistryKey instanceKey = Registry.LocalMachine.OpenSubKey(REGISTRY_PATH);
            
            if (instanceKey == null)
            {
                logger.LogError("No SQL Server instances found in registry", new Exception("Registry key not found: " + REGISTRY_PATH));
                return instances;
            }
            
            string[] instanceNames = instanceKey.GetValueNames();
            logger.Log("Found " + instanceNames.Length + " instance(s) in registry");
            
            foreach (string instanceName in instanceNames)
            {
                SQLServerInfo info = GetInstanceInfo(instanceName, instanceKey);
                if (info != null)
                {
                    instances.Add(info);
                }
            }
            
            instanceKey.Close();
            logger.Log("Scan complete. Found " + instances.Count + " instance(s)");
        }
        catch (Exception ex)
        {
            logger.LogError("Failed to scan for SQL Server instances", ex);
        }
        
        return instances;
    }
    
    private SQLServerInfo GetInstanceInfo(string instanceName, RegistryKey instanceKey)
    {
        try
        {
            string instanceValue = instanceKey.GetValue(instanceName).ToString();
            logger.Log("Processing: " + instanceName + " (" + instanceValue + ")");
            
            SQLServerInfo info = new SQLServerInfo();
            info.InstanceName = instanceName;
            info.Version = GetVersionFromInstanceValue(instanceValue);
            info.ServiceName = GetServiceName(instanceName);
            
            string instancePath = @"SOFTWARE\Microsoft\Microsoft SQL Server\" + instanceValue;
            info.InstanceRegistryPath = instancePath;  // ← SET THIS
            
            info.ServiceStatus = GetServiceStatus(info.ServiceName);
            info.TcpEnabled = tcpService.IsTcpFullyEnabled(instancePath);
            
            try
            {
                RegistryKey setupKey = Registry.LocalMachine.OpenSubKey(instancePath + @"\Setup");
                if (setupKey != null)
                {
                    object edition = setupKey.GetValue("Edition");
                    info.Edition = edition != null ? edition.ToString() : "Unknown";
                    setupKey.Close();
                }
            }
            catch { info.Edition = "Unknown"; }
            
            try
            {
                string netLibPath = instancePath + @"\MSSQLServer\SuperSocketNetLib";
                
                // Named Pipes
                RegistryKey npKey = Registry.LocalMachine.OpenSubKey(netLibPath + @"\Np");
                if (npKey != null)
                {
                    object enabled = npKey.GetValue("Enabled");
                    info.NamedPipesEnabled = enabled != null && Convert.ToInt32(enabled) == 1;
                    npKey.Close();
                }
                
                // Shared Memory
                RegistryKey smKey = Registry.LocalMachine.OpenSubKey(netLibPath + @"\Sm");
                if (smKey != null)
                {
                    object enabled = smKey.GetValue("Enabled");
                    info.SharedMemoryEnabled = enabled != null && Convert.ToInt32(enabled) == 1;
                    smKey.Close();
                }
            }
            catch { }
            
            return info;
        }
        catch (Exception ex)
        {
            logger.LogError("Failed to process instance: " + instanceName, ex);
            return null;
        }
    }
    
    private string GetVersionFromInstanceValue(string instanceValue)
    {
        if (!instanceValue.Contains("."))
            return "Unknown";
            
        string versionNum = instanceValue.Split('.')[0].Replace("MSSQL", "");
        
        switch (versionNum)
        {
            case "16": return "SQL Server 2022";
            case "15": return "SQL Server 2019";
            case "14": return "SQL Server 2017";
            case "13": return "SQL Server 2016";
            case "12": return "SQL Server 2014";
            case "11": return "SQL Server 2012";
            case "10": return "SQL Server 2008/R2";
            default: return "SQL Server " + versionNum;
        }
    }
    
    private string GetServiceName(string instanceName)
    {
        return instanceName == "MSSQLSERVER" ? "MSSQLSERVER" : "MSSQL$" + instanceName;
    }
    
    private string GetServiceStatus(string serviceName)
    {
        try
        {
            ServiceController sc = new ServiceController(serviceName);
            string status = sc.Status.ToString();
            logger.Log("  Service status: " + status);
            return status;
        }
        catch (Exception ex)
        {
            logger.Log("  Could not check service status: " + ex.Message);
            return "Unknown";
        }
    }
    
    public bool EnableTcpIp(SQLServerInfo info)
    {
        logger.LogSeparator();
        logger.Log("Enabling TCP/IP for: " + info.InstanceName);
        
        try
        {
            RegistryKey instanceKey = Registry.LocalMachine.OpenSubKey(REGISTRY_PATH);
            if (instanceKey == null)
            {
                logger.LogError("Cannot open registry key", new Exception("Registry key not found: " + REGISTRY_PATH));
                return false;
            }
            
            string instanceValue = instanceKey.GetValue(info.InstanceName).ToString();
            instanceKey.Close();
            
            string instancePath = @"SOFTWARE\Microsoft\Microsoft SQL Server\" + instanceValue;
            
            bool success = tcpService.EnableAllTcpIp(instancePath);
            
            if (success)
            {
                info.TcpEnabled = true;
                logger.LogSuccess("TCP/IP enabled for " + info.InstanceName);
            }
            else
            {
                logger.LogWarning("Failed to enable TCP/IP for " + info.InstanceName);
            }
            
            return success;
        }
        catch (Exception ex)
        {
            logger.LogError("Exception while enabling TCP/IP for " + info.InstanceName, ex);
            return false;
        }
    }
    
    public bool RestartService(SQLServerInfo info)
    {
        return RestartService(info.ServiceName);
    }
    
    public bool RestartService(string serviceName)
    {
        logger.LogSeparator();
        logger.Log("Restarting service: " + serviceName);
        
        try
        {
            ServiceController sc = new ServiceController(serviceName);
            
            logger.Log("  Current status: " + sc.Status.ToString());
            
            if (sc.Status == ServiceControllerStatus.Running)
            {
                logger.Log("  Stopping service...");
                sc.Stop();
                sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(60));
                logger.Log("  Service stopped");
            }
            
            logger.Log("  Starting service...");
            sc.Start();
            sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(60));
            
            logger.LogSuccess("Service started successfully");
            
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError("Failed to restart service: " + serviceName, ex);
            return false;
        }
    }
    
    public bool StartService(string serviceName)
    {
        logger.LogSeparator();
        logger.Log("Starting service: " + serviceName);
        
        try
        {
            ServiceController sc = new ServiceController(serviceName);
            
            logger.Log("  Current status: " + sc.Status.ToString());
            
            if (sc.Status == ServiceControllerStatus.Running)
            {
                logger.LogWarning("  Service is already running");
                return true;
            }
            
            logger.Log("  Starting service...");
            sc.Start();
            sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(60));
            
            logger.LogSuccess("Service started successfully");
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError("Failed to start service: " + serviceName, ex);
            return false;
        }
    }
    
    public bool StopService(string serviceName)
    {
        logger.LogSeparator();
        logger.Log("Stopping service: " + serviceName);
        
        try
        {
            ServiceController sc = new ServiceController(serviceName);
            
            logger.Log("  Current status: " + sc.Status.ToString());
            
            if (sc.Status == ServiceControllerStatus.Stopped)
            {
                logger.LogWarning("  Service is already stopped");
                return true;
            }
            
            logger.Log("  Stopping service...");
            sc.Stop();
            sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(60));
            
            logger.LogSuccess("Service stopped successfully");
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError("Failed to stop service: " + serviceName, ex);
            return false;
        }
    }
}