using System.Collections.Generic;

public class SQLServerInstanceDetails
{
    public string InstanceName { get; set; }
    public string Version { get; set; }
    public string Edition { get; set; }
    public string ServiceName { get; set; }
    public string ServiceStatus { get; set; }
    public string ServiceAccount { get; set; }
    
    public bool TcpEnabled { get; set; }
    public bool NamedPipesEnabled { get; set; }
    public bool SharedMemoryEnabled { get; set; }
    public List<TcpIpConfig> TcpIpConfigs { get; set; }
    
    public string InstallPath { get; set; }
    public string DataPath { get; set; }
    public string LogPath { get; set; }
    public string BackupPath { get; set; }
    public string ErrorLogPath { get; set; }
    
    public string InstanceRegistryPath { get; set; }
    public string TcpRegistryPath { get; set; }
    
    public string Collation { get; set; }
    public int MaxMemoryMB { get; set; }
    public int MinMemoryMB { get; set; }
    
    public SQLServerInstanceDetails()
    {
        TcpIpConfigs = new List<TcpIpConfig>();
    }
    
    public string GetSummary()
    {
        return string.Format("{0} - {1} ({2})", InstanceName, Version, ServiceStatus);
    }
    
    public bool IsRunning()
    {
        return ServiceStatus == "Running";
    }
}