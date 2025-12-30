package com.danielterziev.dbmanager;

import java.sql.*;
import java.util.ArrayList;
import java.util.List;

public class DatabaseManager {

    public String getConnectionUrl(String dbType, String host, String port, String database) {
        return switch (dbType) {
            case "PostgreSQL" -> "jdbc:postgresql://" + host + ":" + port + "/" + database;
            case "MySQL" ->
                    "jdbc:mysql://" + host + ":" + port + "/" + database + "?useSSL=false&allowPublicKeyRetrieval=true";
            case "MariaDB" -> "jdbc:mariadb://" + host + ":" + port + "/" + database;
            case "SQL Server" ->
                    "jdbc:sqlserver://" + host + ":" + port + ";databaseName=" + database + ";encrypt=false";
            default -> null;
        };
    }

    /**
     * Връща списък с всички бази данни на сървъра
     */
    public List<String> getDatabases(String dbType, String host, String port,
                                     String username, String password) {
        List<String> databases = new ArrayList<>();
        String url;
        String query;

        // За всеки тип база данни използваме различна system database за initial connection
        switch (dbType) {
            case "PostgreSQL":
                url = "jdbc:postgresql://" + host + ":" + port + "/postgres";
                query = "SELECT datname FROM pg_database WHERE datistemplate = false ORDER BY datname;";
                break;
            case "MySQL":
                url = "jdbc:mysql://" + host + ":" + port + "/?useSSL=false&allowPublicKeyRetrieval=true";
                query = "SHOW DATABASES;";
                break;
            case "MariaDB":
                url = "jdbc:mariadb://" + host + ":" + port + "/";
                query = "SHOW DATABASES;";
                break;
            case "SQL Server":
                url = "jdbc:sqlserver://" + host + ":" + port + ";encrypt=false";
                query = "SELECT name FROM sys.databases WHERE name NOT IN ('master', 'tempdb', 'model', 'msdb') ORDER BY name;";
                break;
            default:
                return null;
        }

        try (Connection conn = DriverManager.getConnection(url, username, password);
             Statement stmt = conn.createStatement();
             ResultSet rs = stmt.executeQuery(query)) {

            while (rs.next()) {
                String dbName = rs.getString(1);

                // Филтрираме system databases за MySQL/MariaDB
                if ((dbType.equals("MySQL") || dbType.equals("MariaDB")) &&
                        (dbName.equals("information_schema") ||
                                dbName.equals("mysql") ||
                                dbName.equals("performance_schema") ||
                                dbName.equals("sys"))) {
                    continue;
                }

                databases.add(dbName);
            }

        } catch (SQLException e) {
            System.err.println("Error loading databases: " + e.getMessage());
            return null;
        }

        return databases;
    }

    public String testConnection(String dbType, String host, String port,
                                 String database, String username, String password) {
        String url = getConnectionUrl(dbType, host, port, database);

        if (url == null) {
            return "❌ Unsupported database type!";
        }

        try (Connection conn = DriverManager.getConnection(url, username, password)) {
            return getString(database, username, conn, url);

        } catch (SQLException e) {
            return "❌ CONNECTION FAILED!\n\n" +
                    "═══════════════════════════════════════\n\n" +
                    "Error Code: " + e.getErrorCode() + "\n" +
                    "SQL State: " + e.getSQLState() + "\n" +
                    "Message: " + e.getMessage() + "\n\n" +
                    "═══════════════════════════════════════\n";
        }
    }

    private static String getString(String database, String username, Connection conn, String url) throws SQLException {
        DatabaseMetaData metaData = conn.getMetaData();

        return "✅ CONNECTION SUCCESSFUL!\n\n" +
                "═══════════════════════════════════════\n\n" +
                "Database Product: " + metaData.getDatabaseProductName() + "\n" +
                "Database Version: " + metaData.getDatabaseProductVersion() + "\n" +
                "Driver Name: " + metaData.getDriverName() + "\n" +
                "Driver Version: " + metaData.getDriverVersion() + "\n" +
                "Connection URL: " + url + "\n" +
                "Username: " + username + "\n" +
                "Current Database: " + database + "\n\n" +
                "═══════════════════════════════════════\n";
    }

    public String executeQuery(String dbType, String host, String port,
                               String database, String username, String password, String query) {
        String url = getConnectionUrl(dbType, host, port, database);

        if (url == null) {
            return "❌ Unsupported database type!";
        }

        StringBuilder result = new StringBuilder();

        try (Connection conn = DriverManager.getConnection(url, username, password);
             Statement stmt = conn.createStatement();
             ResultSet rs = stmt.executeQuery(query)) {

            ResultSetMetaData metaData = rs.getMetaData();
            int columnCount = metaData.getColumnCount();

            result.append("QUERY RESULTS\n");
            result.append("═══════════════════════════════════════\n\n");

            // Header with column names
            for (int i = 1; i <= columnCount; i++) {
                result.append(String.format("%-20s", metaData.getColumnName(i)));
                if (i < columnCount) result.append(" | ");
            }
            result.append("\n");
            result.append("─".repeat(Math.min(80, columnCount * 23))).append("\n");

            // Data rows
            int rowCount = 0;
            while (rs.next() && rowCount < 100) {
                for (int i = 1; i <= columnCount; i++) {
                    String value = rs.getString(i);
                    if (value == null) value = "NULL";
                    result.append(String.format("%-20s",
                            value.length() > 20 ? value.substring(0, 17) + "..." : value));
                    if (i < columnCount) result.append(" | ");
                }
                result.append("\n");
                rowCount++;
            }

            result.append("\n═══════════════════════════════════════\n");
            result.append("Total rows: ").append(rowCount);
            if (rowCount == 100) {
                result.append(" (limited to 100 rows)");
            }

        } catch (SQLException e) {
            result.append("❌ QUERY EXECUTION FAILED!\n\n");
            result.append("Error: ").append(e.getMessage()).append("\n");
            result.append("SQL State: ").append(e.getSQLState()).append("\n");
        }

        return result.toString();
    }
}