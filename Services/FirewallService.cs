using System;
using System.Collections.Generic;
using System.Diagnostics;

public class FirewallService
{
    private ILogService logger;
    
    public FirewallService(ILogService logService)
    {
        this.logger = logService;
    }
    
    public FirewallRule GetFirewallRule(int port)
    {
        logger.Log("Checking firewall rule for port: " + port);
        
        try
        {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = "netsh";
            psi.Arguments = string.Format("advfirewall firewall show rule name=all | findstr /C:\"LocalPort\" /C:\"{0}\"", port);
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.CreateNoWindow = true;
            
            Process process = Process.Start(psi);
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            
            FirewallRule rule = new FirewallRule();
            rule.Port = port;
            rule.Protocol = "TCP";
            rule.Exists = output.Contains(port.ToString());
            
            if (rule.Exists)
            {
                rule.Enabled = true; // Will parse properly below
                logger.Log("  Firewall rule exists for port " + port);
            }
            else
            {
                logger.LogWarning("  No firewall rule found for port " + port);
            }
            
            return rule;
        }
        catch (Exception ex)
        {
            logger.LogError("Failed to check firewall rule", ex);
            return null;
        }
    }
    
    public bool CreateFirewallRule(int port, string instanceName)
    {
        logger.LogHeader("CREATING FIREWALL RULE");
        logger.Log("Port: " + port);
        logger.Log("Instance: " + instanceName);
        
        try
        {
            string ruleName = string.Format("SQL Server {0} - Port {1}", instanceName, port);
            
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = "netsh";
            psi.Arguments = string.Format(
                "advfirewall firewall add rule name=\"{0}\" dir=in action=allow protocol=TCP localport={1}",
                ruleName,
                port);
            psi.UseShellExecute = true;
            psi.Verb = "runas"; // Run as admin
            psi.CreateNoWindow = true;
            
            logger.Log("Executing: netsh " + psi.Arguments);
            
            Process process = Process.Start(psi);
            process.WaitForExit();
            
            if (process.ExitCode == 0)
            {
                logger.LogSuccess("Firewall rule created successfully");
                return true;
            }
            else
            {
                logger.LogError("Failed to create firewall rule", new Exception("Exit code: " + process.ExitCode));
                return false;
            }
        }
        catch (Exception ex)
        {
            logger.LogError("Failed to create firewall rule", ex);
            return false;
        }
    }
    
    public bool DeleteFirewallRule(int port, string instanceName)
    {
        logger.LogHeader("DELETING FIREWALL RULE");
        logger.Log("Port: " + port);
        
        try
        {
            string ruleName = string.Format("SQL Server {0} - Port {1}", instanceName, port);
            
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = "netsh";
            psi.Arguments = string.Format("advfirewall firewall delete rule name=\"{0}\"", ruleName);
            psi.UseShellExecute = true;
            psi.Verb = "runas";
            psi.CreateNoWindow = true;
            
            logger.Log("Executing: netsh " + psi.Arguments);
            
            Process process = Process.Start(psi);
            process.WaitForExit();
            
            if (process.ExitCode == 0)
            {
                logger.LogSuccess("Firewall rule deleted successfully");
                return true;
            }
            else
            {
                logger.LogError("Failed to delete firewall rule", new Exception("Exit code: " + process.ExitCode));
                return false;
            }
        }
        catch (Exception ex)
        {
            logger.LogError("Failed to delete firewall rule", ex);
            return false;
        }
    }
    
    public List<int> GetAllOpenPorts()
    {
        logger.Log("Getting all open firewall ports...");
        
        List<int> ports = new List<int>();
        
        try
        {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = "netsh";
            psi.Arguments = "advfirewall firewall show rule name=all";
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            psi.CreateNoWindow = true;
            
            Process process = Process.Start(psi);
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            
            // Parse output for ports (simplified - can be improved)
            string[] lines = output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                if (line.Contains("LocalPort:"))
                {
                    string portStr = line.Split(':')[1].Trim();
                    int port;
                    if (int.TryParse(portStr, out port))
                    {
                        if (!ports.Contains(port))
                        {
                            ports.Add(port);
                        }
                    }
                }
            }
            
            logger.Log("Found " + ports.Count + " open ports in firewall");
        }
        catch (Exception ex)
        {
            logger.LogError("Failed to get firewall ports", ex);
        }
        
        return ports;
    }
}