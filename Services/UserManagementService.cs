using System;
using System.Collections.Generic;
using System.Data.SqlClient;

public class UserManagementService
{
    private ILogService logger;
    
    public UserManagementService(ILogService logService)
    {
        this.logger = logService;
    }
    
    public List<SqlServerUser> GetAllUsers(string instanceName)
    {
        List<SqlServerUser> users = new List<SqlServerUser>();
        SqlConnection connection = null;
        SqlCommand command = null;
        SqlDataReader reader = null;
        
        try
        {
            string connectionString = BuildConnectionString(instanceName);
            
            connection = new SqlConnection(connectionString);
            connection.Open();
            
            string query = @"
                SELECT 
                    name,
                    type,
                    type_desc,
                    CONVERT(varchar, create_date, 120) as create_date,
                    is_disabled
                FROM sys.server_principals 
                WHERE type IN ('S', 'U', 'G')
                AND name NOT LIKE '##%'
                AND name NOT LIKE 'NT %'
                ORDER BY name";
            
            command = new SqlCommand(query, connection);
            reader = command.ExecuteReader();
            
            while (reader.Read())
            {
                SqlServerUser user = new SqlServerUser();
                user.Name = reader["name"].ToString();
                user.Type = reader["type"].ToString();
                user.TypeDescription = reader["type_desc"].ToString();
                user.CreateDate = reader["create_date"].ToString();
                user.IsDisabled = Convert.ToBoolean(reader["is_disabled"]);
                
                users.Add(user);
            }
            
            reader.Close();
            reader = null;
            
            // Get roles for each user
            foreach (SqlServerUser user in users)
            {
                user.ServerRoles = GetUserRoles(connection, user.Name);
            }
            
            logger.Log("Retrieved " + users.Count + " users");
        }
        catch (Exception ex)
        {
            logger.LogError("Failed to get users", ex);
        }
        finally
        {
            if (reader != null)
            {
                reader.Close();
                reader.Dispose();
            }
            
            if (command != null)
            {
                command.Dispose();
            }
            
            if (connection != null)
            {
                connection.Close();
                connection.Dispose();
            }
        }
        
        return users;
    }
    
    private List<string> GetUserRoles(SqlConnection connection, string userName)
    {
        List<string> roles = new List<string>();
        SqlCommand command = null;
        SqlDataReader reader = null;
        
        try
        {
            string query = @"
                SELECT r.name as role_name
                FROM sys.server_role_members rm
                JOIN sys.server_principals r ON rm.role_principal_id = r.principal_id
                JOIN sys.server_principals m ON rm.member_principal_id = m.principal_id
                WHERE m.name = @userName";
            
            command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@userName", userName);
            
            reader = command.ExecuteReader();
            
            while (reader.Read())
            {
                roles.Add(reader["role_name"].ToString());
            }
        }
        catch (Exception ex)
        {
            logger.LogError("Failed to get user roles for: " + userName, ex);
        }
        finally
        {
            if (reader != null)
            {
                reader.Close();
                reader.Dispose();
            }
            
            if (command != null)
            {
                command.Dispose();
            }
        }
        
        return roles;
    }
    
    public bool CreateWindowsUser(string instanceName, string windowsAccount, List<string> roles)
    {
        SqlConnection connection = null;
        SqlCommand command = null;
        
        try
        {
            logger.Log("Creating Windows user: " + windowsAccount);
            
            string connectionString = BuildConnectionString(instanceName);
            
            connection = new SqlConnection(connectionString);
            connection.Open();
            
            // Create login
            string createQuery = "CREATE LOGIN [" + windowsAccount + "] FROM WINDOWS";
            command = new SqlCommand(createQuery, connection);
            command.ExecuteNonQuery();
            
            logger.LogSuccess("Windows user created: " + windowsAccount);
            
            command.Dispose();
            
            // Add to roles
            foreach (string role in roles)
            {
                if (!string.IsNullOrEmpty(role))
                {
                    AddUserToRole(connection, windowsAccount, role);
                }
            }
            
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError("Failed to create Windows user", ex);
            return false;
        }
        finally
        {
            if (command != null)
            {
                command.Dispose();
            }
            
            if (connection != null)
            {
                connection.Close();
                connection.Dispose();
            }
        }
    }
    
    public bool CreateSqlUser(string instanceName, string userName, string password, List<string> roles)
    {
        SqlConnection connection = null;
        SqlCommand command = null;
        
        try
        {
            logger.Log("Creating SQL user: " + userName);
            
            string connectionString = BuildConnectionString(instanceName);
            
            connection = new SqlConnection(connectionString);
            connection.Open();
            
            // Create login
            string createQuery = "CREATE LOGIN [" + userName + "] WITH PASSWORD = N'" + password + "'";
            command = new SqlCommand(createQuery, connection);
            command.ExecuteNonQuery();
            
            logger.LogSuccess("SQL user created: " + userName);
            
            command.Dispose();
            
            // Add to roles
            foreach (string role in roles)
            {
                if (!string.IsNullOrEmpty(role))
                {
                    AddUserToRole(connection, userName, role);
                }
            }
            
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError("Failed to create SQL user", ex);
            return false;
        }
        finally
        {
            if (command != null)
            {
                command.Dispose();
            }
            
            if (connection != null)
            {
                connection.Close();
                connection.Dispose();
            }
        }
    }
    
    public bool ChangeUserPassword(string instanceName, string userName, string newPassword)
    {
        SqlConnection connection = null;
        SqlCommand command = null;
        
        try
        {
            logger.Log("Changing password for: " + userName);
            
            string connectionString = BuildConnectionString(instanceName);
            
            connection = new SqlConnection(connectionString);
            connection.Open();
            
            string query = "ALTER LOGIN [" + userName + "] WITH PASSWORD = N'" + newPassword + "'";
            command = new SqlCommand(query, connection);
            command.ExecuteNonQuery();
            
            logger.LogSuccess("Password changed for: " + userName);
            
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError("Failed to change password", ex);
            return false;
        }
        finally
        {
            if (command != null)
            {
                command.Dispose();
            }
            
            if (connection != null)
            {
                connection.Close();
                connection.Dispose();
            }
        }
    }
    
    public bool DeleteUser(string instanceName, string userName)
    {
        SqlConnection connection = null;
        SqlCommand command = null;
        
        try
        {
            logger.Log("Deleting user: " + userName);
            
            string connectionString = BuildConnectionString(instanceName);
            
            connection = new SqlConnection(connectionString);
            connection.Open();
            
            string query = "DROP LOGIN [" + userName + "]";
            command = new SqlCommand(query, connection);
            command.ExecuteNonQuery();
            
            logger.LogSuccess("User deleted: " + userName);
            
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError("Failed to delete user", ex);
            return false;
        }
        finally
        {
            if (command != null)
            {
                command.Dispose();
            }
            
            if (connection != null)
            {
                connection.Close();
                connection.Dispose();
            }
        }
    }
    
    public bool UpdateUserRoles(string instanceName, string userName, List<string> newRoles)
    {
        SqlConnection connection = null;
        
        try
        {
            logger.Log("Updating roles for: " + userName);
            
            string connectionString = BuildConnectionString(instanceName);
            
            connection = new SqlConnection(connectionString);
            connection.Open();
            
            // Get current roles
            List<string> currentRoles = GetUserRoles(connection, userName);
            
            // Remove from old roles
            foreach (string role in currentRoles)
            {
                if (!newRoles.Contains(role))
                {
                    RemoveUserFromRole(connection, userName, role);
                }
            }
            
            // Add to new roles
            foreach (string role in newRoles)
            {
                if (!currentRoles.Contains(role))
                {
                    AddUserToRole(connection, userName, role);
                }
            }
            
            logger.LogSuccess("Roles updated for: " + userName);
            
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError("Failed to update roles", ex);
            return false;
        }
        finally
        {
            if (connection != null)
            {
                connection.Close();
                connection.Dispose();
            }
        }
    }
    
    private void AddUserToRole(SqlConnection connection, string userName, string roleName)
    {
        SqlCommand command = null;
        
        try
        {
            string query = "ALTER SERVER ROLE [" + roleName + "] ADD MEMBER [" + userName + "]";
            command = new SqlCommand(query, connection);
            command.ExecuteNonQuery();
            
            logger.Log("Added " + userName + " to role: " + roleName);
        }
        catch (Exception ex)
        {
            logger.LogError("Failed to add user to role: " + roleName, ex);
        }
        finally
        {
            if (command != null)
            {
                command.Dispose();
            }
        }
    }
    
    private void RemoveUserFromRole(SqlConnection connection, string userName, string roleName)
    {
        SqlCommand command = null;
        
        try
        {
            string query = "ALTER SERVER ROLE [" + roleName + "] DROP MEMBER [" + userName + "]";
            command = new SqlCommand(query, connection);
            command.ExecuteNonQuery();
            
            logger.Log("Removed " + userName + " from role: " + roleName);
        }
        catch (Exception ex)
        {
            logger.LogError("Failed to remove user from role: " + roleName, ex);
        }
        finally
        {
            if (command != null)
            {
                command.Dispose();
            }
        }
    }
    
    public List<string> GetAvailableServerRoles(string instanceName)
    {
        List<string> roles = new List<string>();
        SqlConnection connection = null;
        SqlCommand command = null;
        SqlDataReader reader = null;
        
        try
        {
            string connectionString = BuildConnectionString(instanceName);
            
            connection = new SqlConnection(connectionString);
            connection.Open();
            
            string query = "SELECT name FROM sys.server_principals WHERE type = 'R' AND is_fixed_role = 1 ORDER BY name";
            command = new SqlCommand(query, connection);
            reader = command.ExecuteReader();
            
            while (reader.Read())
            {
                roles.Add(reader["name"].ToString());
            }
        }
        catch (Exception ex)
        {
            logger.LogError("Failed to get server roles", ex);
        }
        finally
        {
            if (reader != null)
            {
                reader.Close();
                reader.Dispose();
            }
            
            if (command != null)
            {
                command.Dispose();
            }
            
            if (connection != null)
            {
                connection.Close();
                connection.Dispose();
            }
        }
        
        return roles;
    }
    
    private string BuildConnectionString(string instanceName)
    {
        string serverName = ".";
        
        if (!string.IsNullOrEmpty(instanceName) && !instanceName.Equals("MSSQLSERVER", StringComparison.OrdinalIgnoreCase))
        {
            serverName = ".\\" + instanceName;
        }
        
        return string.Format("Server={0};Integrated Security=true;Connection Timeout=5;", serverName);
    }
}