using System.Collections.Generic;

public class SqlServerUser
{
    public string Name { get; set; }
    public string Type { get; set; }  // WindowsUser, WindowsGroup, SqlLogin
    public string TypeDescription { get; set; }
    public string CreateDate { get; set; }
    public List<string> ServerRoles { get; set; }
    public bool IsDisabled { get; set; }
    
    public SqlServerUser()
    {
        ServerRoles = new List<string>();
    }
    
    public bool IsWindowsAuthentication()
    {
        return Type == "U" || Type == "G";  // U = Windows User, G = Windows Group
    }
    
    public bool IsSqlAuthentication()
    {
        return Type == "S";  // S = SQL Login
    }
}