using System;

public class SQLServerInfo
{
    public string InstanceName { get; set; }
    public string Version { get; set; }
    public string Edition { get; set; }
    public string ServiceName { get; set; }
    public string ServiceStatus { get; set; }
    public bool TcpEnabled { get; set; }
    public bool NamedPipesEnabled { get; set; }
    public bool SharedMemoryEnabled { get; set; }
    public string InstanceRegistryPath { get; set; }
    public int TcpPort { get; set; }
    public string DataPath { get; set; }
    public string LogPath { get; set; }
    public string BackupPath { get; set; }
    
    public SQLServerInfo()
    {
        InstanceName = string.Empty;
        Version = string.Empty;
        Edition = "Unknown"; 
        ServiceName = string.Empty;
        ServiceStatus = "Unknown";
        TcpEnabled = false;
        NamedPipesEnabled = false; 
        SharedMemoryEnabled = false; 
        InstanceRegistryPath = string.Empty; 
        TcpPort = 0;
        DataPath = string.Empty;
        LogPath = string.Empty;
        BackupPath = string.Empty;
    }

    public bool IsRunning() 
    {
        return ServiceStatus != null && ServiceStatus.Equals("Running", StringComparison.OrdinalIgnoreCase);
    }

    public override string ToString()
    {
        return string.Format("{0} ({1}) - {2}", InstanceName, Version, ServiceStatus);
    }
}