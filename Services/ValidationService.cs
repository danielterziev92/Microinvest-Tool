using System;
using System.Collections.Generic;

public class ValidationService
{
    private ILogService logger;
    private FirewallService firewallService;
    private TcpPortService portService;
    private TcpIpConfigService tcpService;

    public ValidationService(ILogService logService)
    {
        this.logger = logService;
        this.firewallService = new FirewallService(logService);
        this.portService = new TcpPortService(logService);
        this.tcpService = new TcpIpConfigService(logService);
    }

    public SQLServerValidation ValidateInstance(SQLServerInstanceDetails instance)
    {
        logger.LogHeader("VALIDATING INSTANCE: " + instance.InstanceName);

        SQLServerValidation validation = new SQLServerValidation();
        validation.InstanceName = instance.InstanceName;
        validation.IsValid = true; // Will be set to false if critical issues found

        // 1. Check Service Status
        ValidateServiceStatus(instance, validation);

        // 2. Check TCP/IP Configuration
        ValidateTcpIpConfiguration(instance, validation);

        // 3. Check Port Configuration
        ValidatePortConfiguration(instance, validation);

        // 4. Check Firewall Rules
        ValidateFirewallRules(instance, validation);

        logger.Log("Validation complete: " + (validation.IsValid ? "PASSED" : "FAILED"));
        logger.Log("Issues: " + validation.Issues.Count + ", Successes: " + validation.Successes.Count);

        return validation;
    }

    private void ValidateServiceStatus(SQLServerInstanceDetails instance, SQLServerValidation validation)
    {
        logger.Log("Validating service status...");

        if (instance.ServiceStatus == "Running")
        {
            validation.AddSuccess("Service", "SQL Server service is running");
        }
        else if (instance.ServiceStatus == "Stopped")
        {
            validation.AddIssue("Service", "SQL Server service is stopped", ValidationSeverity.Critical);
        }
        else
        {
            validation.AddIssue("Service", "SQL Server service status is: " + instance.ServiceStatus, ValidationSeverity.Error);
        }
    }

    private void ValidateTcpIpConfiguration(SQLServerInstanceDetails instance, SQLServerValidation validation)
    {
        logger.Log("Validating TCP/IP configuration...");

        if (instance.TcpEnabled)
        {
            validation.AddSuccess("Network", "TCP/IP protocol is enabled");

            // Check if at least one IP is enabled
            int enabledIps = 0;
            foreach (TcpIpConfig config in instance.TcpIpConfigs)
            {
                if (config.Enabled)
                    enabledIps++;
            }

            if (enabledIps > 0)
            {
                validation.AddSuccess("Network", string.Format("{0} IP configuration(s) enabled", enabledIps));
            }
            else
            {
                validation.AddIssue("Network", "TCP/IP is enabled but no IP addresses are configured", ValidationSeverity.Error);
            }
        }
        else
        {
            validation.AddIssue("Network", "TCP/IP protocol is disabled", ValidationSeverity.Critical);
        }

        // Check Named Pipes
        if (instance.NamedPipesEnabled)
        {
            validation.AddSuccess("Network", "Named Pipes is enabled");
        }

        // Check Shared Memory
        if (instance.SharedMemoryEnabled)
        {
            validation.AddSuccess("Network", "Shared Memory is enabled");
        }
    }

    private void ValidatePortConfiguration(SQLServerInstanceDetails instance, SQLServerValidation validation)
    {
        logger.Log("Validating port configuration...");

        if (!instance.TcpEnabled)
        {
            validation.AddIssue("Port", "Cannot validate port - TCP/IP is disabled", ValidationSeverity.Info);
            return;
        }

        int primaryPort = portService.GetPrimaryPort(instance.InstanceRegistryPath);

        if (primaryPort > 0)
        {
            validation.AddSuccess("Port", "Primary port configured: " + primaryPort);

            // Check if port is standard or custom
            if (primaryPort == 1433)
            {
                validation.AddIssue("Port", "Using default port 1433 (consider using custom port for security)", ValidationSeverity.Warning);
            }
            else
            {
                validation.AddSuccess("Port", "Using custom port (security best practice)");
            }
        }
        else
        {
            validation.AddIssue("Port", "No static port configured (using dynamic ports)", ValidationSeverity.Warning);
        }
    }

    private void ValidateFirewallRules(SQLServerInstanceDetails instance, SQLServerValidation validation)
    {
        logger.Log("Validating firewall rules...");

        if (!instance.TcpEnabled)
        {
            validation.AddIssue("Firewall", "Cannot validate firewall - TCP/IP is disabled", ValidationSeverity.Info);
            return;
        }

        int primaryPort = portService.GetPrimaryPort(instance.InstanceRegistryPath);

        if (primaryPort == 0)
        {
            validation.AddIssue("Firewall", "Cannot validate firewall - no static port configured", ValidationSeverity.Info);
            return;
        }

        FirewallRule rule = firewallService.GetFirewallRule(primaryPort);

        if (rule != null && rule.Exists)
        {
            validation.AddSuccess("Firewall", string.Format("Firewall rule exists for port {0}", primaryPort));

            if (rule.Enabled)
            {
                validation.AddSuccess("Firewall", "Firewall rule is enabled");
            }
            else
            {
                validation.AddIssue("Firewall", "Firewall rule exists but is disabled", ValidationSeverity.Warning);
            }
        }
        else
        {
            validation.AddIssue("Firewall", string.Format("No firewall rule found for port {0}", primaryPort), ValidationSeverity.Warning);
        }
    }

    public SQLServerHealthCheck GetHealthCheck(SQLServerInstanceDetails instance)
    {
        logger.LogHeader("HEALTH CHECK: " + instance.InstanceName);

        SQLServerHealthCheck healthCheck = new SQLServerHealthCheck();
        healthCheck.InstanceName = instance.InstanceName;

        // Service Status
        healthCheck.ServiceRunning = instance.ServiceStatus == "Running";

        // Network Configuration
        healthCheck.TcpIpEnabled = instance.TcpEnabled;

        // Port Configuration
        healthCheck.PrimaryPort = portService.GetPrimaryPort(instance.InstanceRegistryPath);

        foreach (TcpIpConfig config in instance.TcpIpConfigs)
        {
            if (config.Enabled && !string.IsNullOrEmpty(config.TcpPort))
            {
                int port;
                if (int.TryParse(config.TcpPort, out port) && port > 0)
                {
                    if (!healthCheck.ConfiguredPorts.Contains(port))
                    {
                        healthCheck.ConfiguredPorts.Add(port);
                    }
                }
            }
        }

        // Firewall Status
        if (healthCheck.PrimaryPort > 0)
        {
            FirewallRule rule = firewallService.GetFirewallRule(healthCheck.PrimaryPort);
            healthCheck.FirewallRuleExists = rule != null && rule.Exists;
            healthCheck.FirewallRuleEnabled = rule != null && rule.Enabled;
        }

        // Calculate Overall Health
        healthCheck.CalculateHealth();

        logger.Log("Health Status: " + healthCheck.HealthStatus);

        return healthCheck;
    }

    public List<SQLServerValidation> ValidateAllInstances(List<SQLServerInstanceDetails> instances)
    {
        logger.LogHeader("VALIDATING ALL INSTANCES");

        List<SQLServerValidation> validations = new List<SQLServerValidation>();

        // First pass: Validate each instance
        foreach (SQLServerInstanceDetails instance in instances)
        {
            SQLServerValidation validation = ValidateInstance(instance);
            validations.Add(validation);
        }

        // Second pass: Check for port conflicts
        Dictionary<string, int> instancePorts = portService.GetAllInstancePorts(instances);
        List<string> conflicts = portService.FindPortConflicts(instancePorts);

        if (conflicts.Count > 0)
        {
            foreach (string conflict in conflicts)
            {
                // Add conflict to all affected instances
                foreach (SQLServerValidation validation in validations)
                {
                    if (conflict.Contains(validation.InstanceName))
                    {
                        validation.AddIssue("Port Conflict", conflict, ValidationSeverity.Critical);
                    }
                }
            }
        }

        logger.Log("Validation complete for all instances");

        return validations;
    }
}