public class FirewallRule
{
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public int Port { get; set; }
    public string Protocol { get; set; }
    public bool Enabled { get; set; }
    public string Direction { get; set; }
    public string Action { get; set; }
    public bool Exists { get; set; }
    
    public string GetRuleName()
    {
        return string.Format("SQL Server - Port {0}", Port);
    }
}