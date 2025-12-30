using System.Collections.Generic;

public interface ISQLServerService
{
    List<SQLServerInfo> GetAllInstances();
    List<SQLServerInfo> DiscoverInstances(); 
    bool EnableTcpIp(SQLServerInfo info);
    bool RestartService(SQLServerInfo info);
    bool RestartService(string serviceName); 
    bool StartService(string serviceName);
    bool StopService(string serviceName);
}