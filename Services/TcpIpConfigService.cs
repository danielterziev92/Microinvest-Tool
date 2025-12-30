using System;
using System.Collections.Generic;
using Microsoft.Win32;

public class TcpIpConfigService
{
    private ILogService logger;

    public TcpIpConfigService(ILogService logService)
    {
        this.logger = logService;
    }

    // 1. Get main TCP/IP enabled status
    public bool GetMainTcpEnabled(string instancePath)
    {
        try
        {
            string tcpPath = instancePath + @"\MSSQLServer\SuperSocketNetLib\Tcp";
            logger.Log("Checking main TCP at: " + tcpPath);

            RegistryKey key = Registry.LocalMachine.OpenSubKey(tcpPath);
            if (key == null)
            {
                logger.LogWarning("  TCP key not found");
                return false;
            }

            object value = key.GetValue("Enabled");
            key.Close();

            if (value == null)
            {
                logger.LogWarning("  Enabled value not found");
                return false;
            }

            bool enabled = Convert.ToInt32(value) == 1;
            logger.Log("  Main TCP Enabled: " + enabled);
            return enabled;
        }
        catch (Exception ex)
        {
            logger.LogError("Failed to get main TCP status", ex);
            return false;
        }
    }

    // 2. Get all IP configurations (IP1, IP2, ..., IPAll)
    public List<TcpIpConfig> GetAllIpConfigs(string instancePath)
    {
        List<TcpIpConfig> configs = new List<TcpIpConfig>();

        try
        {
            string tcpPath = instancePath + @"\MSSQLServer\SuperSocketNetLib\Tcp";
            logger.Log("Reading IP configurations from: " + tcpPath);

            RegistryKey tcpKey = Registry.LocalMachine.OpenSubKey(tcpPath);
            if (tcpKey == null)
            {
                logger.LogError("TCP key not found", new Exception("Path not found"));
                return configs;
            }

            string[] subKeys = tcpKey.GetSubKeyNames();
            logger.Log("  Found " + subKeys.Length + " IP configurations");

            foreach (string subKeyName in subKeys)
            {
                TcpIpConfig config = GetIpConfig(tcpPath, subKeyName);
                if (config != null)
                {
                    configs.Add(config);
                }
            }

            tcpKey.Close();
        }
        catch (Exception ex)
        {
            logger.LogError("Failed to get IP configs", ex);
        }

        return configs;
    }

    // 3. Get specific IP configuration
    private TcpIpConfig GetIpConfig(string tcpPath, string ipName)
    {
        try
        {
            string ipPath = tcpPath + "\\" + ipName;
            RegistryKey ipKey = Registry.LocalMachine.OpenSubKey(ipPath);

            if (ipKey == null)
                return null;

            TcpIpConfig config = new TcpIpConfig();
            config.Name = ipName;
            config.RegistryPath = ipPath;

            object enabledVal = ipKey.GetValue("Enabled");
            config.Enabled = enabledVal != null && Convert.ToInt32(enabledVal) == 1;

            object activeVal = ipKey.GetValue("Active");
            config.Active = activeVal != null && Convert.ToInt32(activeVal) == 1;

            object ipVal = ipKey.GetValue("IpAddress");
            config.IpAddress = ipVal != null ? ipVal.ToString() : "";

            object portVal = ipKey.GetValue("TcpPort");
            config.TcpPort = portVal != null ? portVal.ToString() : "";

            object dynPortVal = ipKey.GetValue("TcpDynamicPorts");
            config.TcpDynamicPorts = dynPortVal != null ? dynPortVal.ToString() : "";

            ipKey.Close();

            logger.Log("    " + ipName + ": Enabled=" + config.Enabled + ", IP=" + config.IpAddress + ", Port=" + config.TcpPort);

            return config;
        }
        catch (Exception ex)
        {
            logger.Log("    Error reading " + ipName + ": " + ex.Message);
            return null;
        }
    }

    // 4. Enable main TCP/IP
    public bool EnableMainTcp(string instancePath)
    {
        try
        {
            string tcpPath = instancePath + @"\MSSQLServer\SuperSocketNetLib\Tcp";
            logger.Log("Enabling main TCP at: " + tcpPath);

            RegistryKey key = Registry.LocalMachine.OpenSubKey(tcpPath, true);
            if (key == null)
            {
                logger.LogError("Cannot open TCP key for writing", new Exception("Access denied"));
                return false;
            }

            key.SetValue("Enabled", 1, RegistryValueKind.DWord);
            key.Close();

            logger.LogSuccess("  Main TCP enabled");
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError("Failed to enable main TCP", ex);
            return false;
        }
    }

    // 5. Enable specific IP configuration
    public bool EnableIpConfig(TcpIpConfig config)
    {
        try
        {
            logger.Log("Enabling " + config.Name + " at: " + config.RegistryPath);

            RegistryKey key = Registry.LocalMachine.OpenSubKey(config.RegistryPath, true);
            if (key == null)
            {
                logger.LogError("Cannot open key for writing", new Exception("Access denied"));
                return false;
            }

            key.SetValue("Enabled", 1, RegistryValueKind.DWord);
            key.Close();

            logger.LogSuccess("  " + config.Name + " enabled");
            config.Enabled = true;
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError("Failed to enable " + config.Name, ex);
            return false;
        }
    }

    // 6. Enable ALL IP configurations
    public int EnableAllIpConfigs(string instancePath)
    {
        logger.LogHeader("ENABLING ALL IP CONFIGURATIONS");

        List<TcpIpConfig> configs = GetAllIpConfigs(instancePath);
        int enabledCount = 0;

        foreach (TcpIpConfig config in configs)
        {
            if (!config.Enabled)
            {
                if (EnableIpConfig(config))
                {
                    enabledCount++;
                }
            }
            else
            {
                logger.Log("  " + config.Name + " already enabled");
            }
        }

        logger.Log("");
        logger.Log("Enabled " + enabledCount + " IP configuration(s)");

        return enabledCount;
    }

    // 7. Enable EVERYTHING (main TCP + all IPs)
    public bool EnableAllTcpIp(string instancePath)
    {
        logger.LogHeader("ENABLING TCP/IP COMPLETELY");

        // Step 1: Enable main TCP
        bool mainEnabled = EnableMainTcp(instancePath);

        // Step 2: Enable all IP configs
        int ipCount = EnableAllIpConfigs(instancePath);

        logger.LogSeparator();
        logger.Log("Summary: Main TCP=" + mainEnabled + ", IP configs enabled=" + ipCount);

        return mainEnabled && ipCount > 0;
    }

    // 8. Check if TCP/IP is fully enabled (main + at least one IP)
    public bool IsTcpFullyEnabled(string instancePath)
    {
        try
        {
            bool mainEnabled = GetMainTcpEnabled(instancePath);
            if (!mainEnabled)
            {
                logger.Log("Main TCP is disabled");
                return false;
            }

            List<TcpIpConfig> configs = GetAllIpConfigs(instancePath);
            int enabledCount = 0;

            foreach (TcpIpConfig config in configs)
            {
                if (config.Enabled)
                    enabledCount++;
            }

            logger.Log("Enabled IP configurations: " + enabledCount + "/" + configs.Count);

            return enabledCount > 0;
        }
        catch (Exception ex)
        {
            logger.LogError("Failed to check TCP status", ex);
            return false;
        }
    }

    // 9. Get full TCP/IP status report
    public string GetStatusReport(string instancePath)
    {
        logger.LogHeader("TCP/IP STATUS REPORT");

        string report = "";

        try
        {
            bool mainEnabled = GetMainTcpEnabled(instancePath);
            report += "Main TCP Enabled: " + mainEnabled + "\n\n";

            List<TcpIpConfig> configs = GetAllIpConfigs(instancePath);
            report += "IP Configurations:\n";

            int enabledCount = 0;
            foreach (TcpIpConfig config in configs)
            {
                string status = config.Enabled ? "[ENABLED]" : "[DISABLED]";
                report += "  " + status + " " + config.Name;

                if (!string.IsNullOrEmpty(config.IpAddress))
                    report += " - " + config.IpAddress;

                if (!string.IsNullOrEmpty(config.TcpPort))
                    report += ":" + config.TcpPort;

                report += "\n";

                if (config.Enabled)
                    enabledCount++;
            }

            report += "\nSummary: " + enabledCount + "/" + configs.Count + " IP configs enabled";
        }
        catch (Exception ex)
        {
            report += "ERROR: " + ex.Message;
        }

        return report;
    }

    public bool DisableMainTcp(string instancePath)
    {
        try
        {
            string tcpPath = instancePath + @"\MSSQLServer\SuperSocketNetLib\Tcp";
            logger.Log("Disabling main TCP at: " + tcpPath);

            RegistryKey key = Registry.LocalMachine.OpenSubKey(tcpPath, true);
            if (key == null)
            {
                logger.LogError("Cannot open TCP key for writing", new Exception("Access denied"));
                return false;
            }

            key.SetValue("Enabled", 0, RegistryValueKind.DWord);
            key.Close();

            logger.LogSuccess("  Main TCP disabled");
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError("Failed to disable main TCP", ex);
            return false;
        }
    }

    public bool DisableIpConfig(TcpIpConfig config)
    {
        try
        {
            logger.Log("Disabling " + config.Name + " at: " + config.RegistryPath);

            RegistryKey key = Registry.LocalMachine.OpenSubKey(config.RegistryPath, true);
            if (key == null)
            {
                logger.LogError("Cannot open key for writing", new Exception("Access denied"));
                return false;
            }

            key.SetValue("Enabled", 0, RegistryValueKind.DWord);
            key.Close();

            logger.LogSuccess("  " + config.Name + " disabled");
            config.Enabled = false;
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError("Failed to disable " + config.Name, ex);
            return false;
        }
    }

    public int DisableAllIpConfigs(string instancePath)
    {
        logger.LogHeader("DISABLING ALL IP CONFIGURATIONS");

        List<TcpIpConfig> configs = GetAllIpConfigs(instancePath);
        int disabledCount = 0;

        foreach (TcpIpConfig config in configs)
        {
            if (config.Enabled)
            {
                if (DisableIpConfig(config))
                {
                    disabledCount++;
                }
            }
            else
            {
                logger.Log("  " + config.Name + " already disabled");
            }
        }

        logger.Log("");
        logger.Log("Disabled " + disabledCount + " IP configuration(s)");

        return disabledCount;
    }

    public bool DisableAllTcpIp(string instancePath)
    {
        logger.LogHeader("DISABLING TCP/IP COMPLETELY");

        // Step 1: Disable main TCP
        bool mainDisabled = DisableMainTcp(instancePath);

        // Step 2: Disable all IP configs
        int ipCount = DisableAllIpConfigs(instancePath);

        logger.LogSeparator();
        logger.Log("Summary: Main TCP disabled=" + mainDisabled + ", IP configs disabled=" + ipCount);

        return mainDisabled;
    }
}