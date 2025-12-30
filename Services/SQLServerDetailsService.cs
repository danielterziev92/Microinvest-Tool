using System;
using System.Collections.Generic;
using Microsoft.Win32;
using System.ServiceProcess;
using System.IO;

public class SQLServerDetailsService
{
    private ILogService logger;
    
    public SQLServerDetailsService(ILogService logService)
    {
        this.logger = logService;
    }
    
    public SQLServerInstanceDetails GetInstanceDetails(string instanceName)
    {
        logger.LogHeader("GATHERING INSTANCE DETAILS: " + instanceName);
        
        try
        {
            SQLServerInstanceDetails details = new SQLServerInstanceDetails();
            details.InstanceName = instanceName;
            
            // Get instance registry value
            RegistryKey instanceKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Microsoft SQL Server\Instance Names\SQL");
            if (instanceKey == null)
            {
                logger.LogError("Cannot find instance registry", new Exception("Registry key not found"));
                return null;
            }
            
            string instanceValue = instanceKey.GetValue(instanceName).ToString();
            instanceKey.Close();
            
            string instancePath = @"SOFTWARE\Microsoft\Microsoft SQL Server\" + instanceValue;
            details.InstanceRegistryPath = instancePath;
            
            // Gather all information
            GatherBasicInfo(details, instanceValue, instancePath);
            GatherServiceInfo(details, instanceName);
            GatherNetworkConfig(details, instancePath);
            GatherPaths(details, instancePath);
            GatherPerformanceSettings(details, instancePath);
            
            // Get Collation via SQL connection
            logger.Log("Retrieving collation...");
            CollationService collationService = new CollationService(logger);
            details.Collation = collationService.GetServerCollation(instanceName);
            
            logger.LogSuccess("Instance details gathered successfully");
            
            return details;
        }
        catch (Exception ex)
        {
            logger.LogError("Failed to gather instance details", ex);
            return null;
        }
    }
    
    private void GatherBasicInfo(SQLServerInstanceDetails details, string instanceValue, string instancePath)
    {
        logger.Log("Gathering basic information...");
        
        try
        {
            RegistryKey setupKey = Registry.LocalMachine.OpenSubKey(instancePath + @"\Setup");
            
            if (setupKey != null)
            {
                object version = setupKey.GetValue("Version");
                if (version != null)
                {
                    details.Version = version.ToString();
                    logger.Log("  Version: " + details.Version);
                }
                
                object edition = setupKey.GetValue("Edition");
                if (edition != null)
                {
                    details.Edition = edition.ToString();
                    logger.Log("  Edition: " + details.Edition);
                }
                
                setupKey.Close();
            }
        }
        catch (Exception ex)
        {
            logger.LogError("Error gathering basic info", ex);
        }
    }
    
    private void GatherServiceInfo(SQLServerInstanceDetails details, string instanceName)
    {
        logger.Log("Gathering service information...");
        
        try
        {
            string serviceName;
            
            if (instanceName.Equals("MSSQLSERVER", StringComparison.OrdinalIgnoreCase))
            {
                serviceName = "MSSQLSERVER";
            }
            else
            {
                serviceName = "MSSQL$" + instanceName;
            }
            
            details.ServiceName = serviceName;
            logger.Log("  Service Name: " + serviceName);
            
            ServiceController service = new ServiceController(serviceName);
            
            details.ServiceStatus = service.Status.ToString();
            logger.Log("  Service Status: " + details.ServiceStatus);
            
            // Get service account from registry
            string serviceRegistryPath = @"SYSTEM\CurrentControlSet\Services\" + serviceName;
            RegistryKey serviceKey = Registry.LocalMachine.OpenSubKey(serviceRegistryPath);
            
            if (serviceKey != null)
            {
                object account = serviceKey.GetValue("ObjectName");
                
                if (account != null)
                {
                    details.ServiceAccount = account.ToString();
                    logger.Log("  Service Account: " + details.ServiceAccount);
                }
                
                serviceKey.Close();
            }
            
            service.Close();
        }
        catch (Exception ex)
        {
            logger.LogError("Error gathering service info", ex);
            details.ServiceStatus = "Unknown";
        }
    }
    
    private void GatherNetworkConfig(SQLServerInstanceDetails details, string instancePath)
    {
        logger.Log("Gathering network configuration...");
        
        try
        {
            string superSocketPath = instancePath + @"\MSSQLServer\SuperSocketNetLib";
            
            // TCP/IP
            RegistryKey tcpKey = Registry.LocalMachine.OpenSubKey(superSocketPath + @"\Tcp");
            if (tcpKey != null)
            {
                object enabled = tcpKey.GetValue("Enabled");
                details.TcpEnabled = enabled != null && enabled.ToString() == "1";
                logger.Log("  TCP/IP Enabled: " + details.TcpEnabled);
                
                // Get TCP/IP configurations
                details.TcpIpConfigs = GetTcpIpConfigurations(superSocketPath + @"\Tcp");
                
                tcpKey.Close();
            }
            
            // Named Pipes
            RegistryKey npKey = Registry.LocalMachine.OpenSubKey(superSocketPath + @"\Np");
            if (npKey != null)
            {
                object enabled = npKey.GetValue("Enabled");
                details.NamedPipesEnabled = enabled != null && enabled.ToString() == "1";
                logger.Log("  Named Pipes Enabled: " + details.NamedPipesEnabled);
                npKey.Close();
            }
            
            // Shared Memory
            RegistryKey smKey = Registry.LocalMachine.OpenSubKey(superSocketPath + @"\Sm");
            if (smKey != null)
            {
                object enabled = smKey.GetValue("Enabled");
                details.SharedMemoryEnabled = enabled != null && enabled.ToString() == "1";
                logger.Log("  Shared Memory Enabled: " + details.SharedMemoryEnabled);
                smKey.Close();
            }
        }
        catch (Exception ex)
        {
            logger.LogError("Error gathering network config", ex);
        }
    }
    
    private List<TcpIpConfig> GetTcpIpConfigurations(string tcpPath)
    {
        List<TcpIpConfig> configs = new List<TcpIpConfig>();
        
        try
        {
            RegistryKey tcpKey = Registry.LocalMachine.OpenSubKey(tcpPath);
            
            if (tcpKey != null)
            {
                string[] subKeyNames = tcpKey.GetSubKeyNames();
                
                foreach (string subKeyName in subKeyNames)
                {
                    if (subKeyName.StartsWith("IP"))
                    {
                        string ipPath = tcpPath + "\\" + subKeyName;
                        RegistryKey ipKey = Registry.LocalMachine.OpenSubKey(ipPath);
                        
                        if (ipKey != null)
                        {
                            TcpIpConfig config = new TcpIpConfig();
                            config.Name = subKeyName;
                            config.RegistryPath = ipPath;
                            
                            object enabled = ipKey.GetValue("Enabled");
                            config.Enabled = enabled != null && enabled.ToString() == "1";
                            
                            object active = ipKey.GetValue("Active");
                            config.Active = active != null && active.ToString() == "1";
                            
                            object ipAddress = ipKey.GetValue("IpAddress");
                            config.IpAddress = ipAddress != null ? ipAddress.ToString() : "";
                            
                            object tcpPort = ipKey.GetValue("TcpPort");
                            config.TcpPort = tcpPort != null ? tcpPort.ToString() : "";
                            
                            object tcpDynamicPorts = ipKey.GetValue("TcpDynamicPorts");
                            config.TcpDynamicPorts = tcpDynamicPorts != null ? tcpDynamicPorts.ToString() : "";
                            
                            configs.Add(config);
                            ipKey.Close();
                        }
                    }
                }
                
                tcpKey.Close();
            }
        }
        catch (Exception ex)
        {
            logger.LogError("Error getting TCP/IP configurations", ex);
        }
        
        return configs;
    }
    
    private void GatherPaths(SQLServerInstanceDetails details, string instancePath)
    {
        logger.Log("Gathering paths...");
        
        try
        {
            RegistryKey setupKey = Registry.LocalMachine.OpenSubKey(instancePath + @"\Setup");
            
            if (setupKey != null)
            {
                object sqlPath = setupKey.GetValue("SQLPath");
                if (sqlPath != null)
                {
                    details.InstallPath = sqlPath.ToString();
                    logger.Log("  Install Path: " + details.InstallPath);
                }
                
                object sqlDataRoot = setupKey.GetValue("SQLDataRoot");
                if (sqlDataRoot != null)
                {
                    string dataRoot = sqlDataRoot.ToString();
                    
                    details.DataPath = Path.Combine(dataRoot, "DATA");
                    details.LogPath = Path.Combine(dataRoot, "DATA");
                    details.BackupPath = Path.Combine(dataRoot, "BACKUP");
                    
                    logger.Log("  Data Path: " + details.DataPath);
                    logger.Log("  Log Path: " + details.LogPath);
                    logger.Log("  Backup Path: " + details.BackupPath);
                }
                
                setupKey.Close();
            }
            
            // Error log path
            RegistryKey parametersKey = Registry.LocalMachine.OpenSubKey(instancePath + @"\MSSQLServer\Parameters");
            
            if (parametersKey != null)
            {
                string[] valueNames = parametersKey.GetValueNames();
                
                foreach (string valueName in valueNames)
                {
                    object value = parametersKey.GetValue(valueName);
                    
                    if (value != null && value.ToString().StartsWith("-e"))
                    {
                        details.ErrorLogPath = value.ToString().Substring(2);
                        logger.Log("  Error Log: " + details.ErrorLogPath);
                        break;
                    }
                }
                
                parametersKey.Close();
            }
        }
        catch (Exception ex)
        {
            logger.LogError("Error gathering paths", ex);
        }
    }
    
    private void GatherPerformanceSettings(SQLServerInstanceDetails details, string instancePath)
    {
        logger.Log("Gathering performance settings...");
        
        try
        {
            RegistryKey mssqlKey = Registry.LocalMachine.OpenSubKey(instancePath + @"\MSSQLServer");
            
            if (mssqlKey != null)
            {
                object maxMemory = mssqlKey.GetValue("max server memory (MB)");
                if (maxMemory != null)
                {
                    details.MaxMemoryMB = Convert.ToInt32(maxMemory);
                    logger.Log("  Max Memory: " + details.MaxMemoryMB + " MB");
                }
                
                object minMemory = mssqlKey.GetValue("min server memory (MB)");
                if (minMemory != null)
                {
                    details.MinMemoryMB = Convert.ToInt32(minMemory);
                    logger.Log("  Min Memory: " + details.MinMemoryMB + " MB");
                }
                
                mssqlKey.Close();
            }
        }
        catch (Exception ex)
        {
            logger.LogError("Error gathering performance settings", ex);
        }
    }
}