using System.Collections.Generic;

public class SQLServerHealthCheck
{
    public string InstanceName { get; set; }
    
    // Service Status
    public bool ServiceRunning { get; set; }
    
    // Network Configuration
    public bool TcpIpEnabled { get; set; }
    public List<int> ConfiguredPorts { get; set; }
    public int PrimaryPort { get; set; }
    
    // Firewall Status
    public bool FirewallRuleExists { get; set; }
    public bool FirewallRuleEnabled { get; set; }
    public List<int> FirewallOpenPorts { get; set; }
    
    // Port Conflicts
    public bool HasPortConflicts { get; set; }
    public List<string> ConflictingInstances { get; set; }
    
    // Overall Health
    public bool IsHealthy { get; set; }
    public string HealthStatus { get; set; }
    
    public SQLServerHealthCheck()
    {
        ConfiguredPorts = new List<int>();
        FirewallOpenPorts = new List<int>();
        ConflictingInstances = new List<string>();
    }
    
    public void CalculateHealth()
    {
        IsHealthy = ServiceRunning && 
                    TcpIpEnabled && 
                    PrimaryPort > 0 && 
                    FirewallRuleExists && 
                    FirewallRuleEnabled && 
                    !HasPortConflicts;

        if (IsHealthy)
        {
            HealthStatus = "Healthy";
        }
        else if (!ServiceRunning)
        {
            HealthStatus = "Service Not Running";
        }
        else if (!TcpIpEnabled)
        {
            HealthStatus = "TCP/IP Disabled";
        }
        else if (!FirewallRuleExists || !FirewallRuleEnabled)
        {
            HealthStatus = "Firewall Issue";
        }
        else if (HasPortConflicts)
        {
            HealthStatus = "Port Conflict";
        }
        else
        {
            HealthStatus = "Configuration Issue";
        }
    }
}