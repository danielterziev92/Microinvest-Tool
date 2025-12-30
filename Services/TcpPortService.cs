using System;
using System.Collections.Generic;
using Microsoft.Win32;

public class TcpPortService
{
    private ILogService logger;
    
    public TcpPortService(ILogService logService)
    {
        this.logger = logService;
    }
    
    public int GetPrimaryPort(string instancePath)
    {
        logger.Log("Getting primary port for instance");
        
        try
        {
            string tcpPath = instancePath + @"\MSSQLServer\SuperSocketNetLib\Tcp\IPAll";
            
            RegistryKey key = Registry.LocalMachine.OpenSubKey(tcpPath);
            if (key == null)
            {
                logger.LogWarning("  IPAll key not found");
                return 0;
            }
            
            object portValue = key.GetValue("TcpPort");
            key.Close();
            
            if (portValue != null && !string.IsNullOrEmpty(portValue.ToString()))
            {
                int port = int.Parse(portValue.ToString());
                logger.Log("  Primary port: " + port);
                return port;
            }
            
            logger.LogWarning("  No port configured in IPAll");
            return 0;
        }
        catch (Exception ex)
        {
            logger.LogError("Failed to get primary port", ex);
            return 0;
        }
    }
    
    public bool SetPrimaryPort(string instancePath, int newPort)
    {
        logger.LogHeader("CHANGING PRIMARY PORT");
        logger.Log("New port: " + newPort);
        
        try
        {
            string tcpPath = instancePath + @"\MSSQLServer\SuperSocketNetLib\Tcp\IPAll";
            
            RegistryKey key = Registry.LocalMachine.OpenSubKey(tcpPath, true);
            if (key == null)
            {
                logger.LogError("Cannot open IPAll key for writing", new Exception("Access denied"));
                return false;
            }
            
            // Set static port
            key.SetValue("TcpPort", newPort.ToString(), RegistryValueKind.String);
            
            // Clear dynamic port
            key.SetValue("TcpDynamicPorts", "", RegistryValueKind.String);
            
            key.Close();
            
            logger.LogSuccess("Port changed to " + newPort);
            logger.LogWarning("SQL Server service must be restarted for changes to take effect");
            
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError("Failed to set primary port", ex);
            return false;
        }
    }
    
    public Dictionary<string, int> GetAllInstancePorts(List<SQLServerInstanceDetails> instances)
    {
        logger.Log("Checking ports for all instances...");
        
        Dictionary<string, int> instancePorts = new Dictionary<string, int>();
        
        foreach (SQLServerInstanceDetails instance in instances)
        {
            if (instance.TcpEnabled)
            {
                int port = GetPrimaryPort(instance.InstanceRegistryPath);
                if (port > 0)
                {
                    instancePorts[instance.InstanceName] = port;
                    logger.Log("  " + instance.InstanceName + " -> Port " + port);
                }
            }
        }
        
        return instancePorts;
    }
    
    public List<string> FindPortConflicts(Dictionary<string, int> instancePorts)
    {
        logger.Log("Checking for port conflicts...");
        
        List<string> conflicts = new List<string>();
        Dictionary<int, List<string>> portMap = new Dictionary<int, List<string>>();
        
        foreach (var kvp in instancePorts)
        {
            if (!portMap.ContainsKey(kvp.Value))
            {
                portMap[kvp.Value] = new List<string>();
            }
            portMap[kvp.Value].Add(kvp.Key);
        }
        
        foreach (var kvp in portMap)
        {
            if (kvp.Value.Count > 1)
            {
                string conflict = string.Format("Port {0} used by: {1}", kvp.Key, string.Join(", ", kvp.Value.ToArray()));
                conflicts.Add(conflict);
                logger.LogWarning("  CONFLICT: " + conflict);
            }
        }
        
        if (conflicts.Count == 0)
        {
            logger.Log("  No port conflicts found");
        }
        
        return conflicts;
    }
}