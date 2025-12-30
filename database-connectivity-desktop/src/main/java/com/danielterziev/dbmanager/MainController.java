package com.danielterziev.dbmanager;

import javafx.fxml.FXML;
import javafx.scene.control.*;

import java.util.List;

public class MainController {

    @FXML
    private ComboBox<String> dbTypeCombo;
    @FXML
    private TextField hostField;
    @FXML
    private TextField portField;
    @FXML
    private TextField usernameField;
    @FXML
    private PasswordField passwordField;
    @FXML
    private Button loadDbButton;
    @FXML
    private Label statusLabel;
    @FXML
    private ComboBox<String> databaseCombo;
    @FXML
    private Label urlPreviewLabel;
    @FXML
    private TextArea resultArea;

    private DatabaseManager dbManager;

    @FXML
    public void initialize() {
        dbManager = new DatabaseManager();

        dbTypeCombo.getItems().addAll("PostgreSQL", "MySQL", "MariaDB", "SQL Server");
        dbTypeCombo.setValue("PostgreSQL");

        hostField.setText("localhost");
        portField.setText("5432");
        databaseCombo.setDisable(true);

        hostField.textProperty().addListener((obs, old, newVal) -> updateUrlPreview());
        portField.textProperty().addListener((obs, old, newVal) -> updateUrlPreview());

        // Инициално обновяване на URL preview
        updateUrlPreview();
    }

    @FXML
    private void onDatabaseTypeChanged() {
        String dbType = dbTypeCombo.getValue();

        // Автоматично попълване на порт според типа база данни
        switch (dbType) {
            case "PostgreSQL":
                portField.setText("5432");
                break;
            case "MySQL":
                portField.setText("3306");
                break;
            case "MariaDB":
                portField.setText("3306");
                break;
            case "SQL Server":
                portField.setText("1433");
                break;
        }

        // Reset на избраната база данни
        databaseCombo.getItems().clear();
        databaseCombo.setDisable(true);
        databaseCombo.setPromptText("First load databases...");
        statusLabel.setText("");

        updateUrlPreview();
    }

    @FXML
    private void onDatabaseSelected() {
        updateUrlPreview();
    }

    @FXML
    private void onLoadDatabases() {
        String dbType = dbTypeCombo.getValue();
        String host = hostField.getText().trim();
        String port = portField.getText().trim();
        String username = usernameField.getText().trim();
        String password = passwordField.getText();

        // Валидация
        if (host.isEmpty() || port.isEmpty() || username.isEmpty()) {
            showAlert(Alert.AlertType.WARNING, "Missing Information",
                    "Please fill in Host, Port, Username and Password!");
            return;
        }

        // Изчистване на предишни данни
        databaseCombo.getItems().clear();
        databaseCombo.setDisable(true);
        statusLabel.setText("⏳ Loading databases...");
        statusLabel.setStyle("-fx-font-size: 12px; -fx-padding: 5; -fx-text-fill: #f39c12;");

        // Зареждане на бази данни
        List<String> databases = dbManager.getDatabases(dbType, host, port, username, password);

        if (databases != null && !databases.isEmpty()) {
            databaseCombo.getItems().addAll(databases);
            databaseCombo.setDisable(false);
            databaseCombo.setPromptText("Select a database...");
            statusLabel.setText("✅ Found " + databases.size() + " database(s)");
            statusLabel.setStyle("-fx-font-size: 12px; -fx-padding: 5; -fx-text-fill: #27ae60; -fx-font-weight: bold;");
            resultArea.setText("Successfully loaded " + databases.size() + " databases:\n\n" +
                    String.join("\n", databases));
        } else {
            statusLabel.setText("❌ Failed to load databases. Check your credentials.");
            statusLabel.setStyle("-fx-font-size: 12px; -fx-padding: 5; -fx-text-fill: #e74c3c; -fx-font-weight: bold;");
            resultArea.setText("Failed to connect or retrieve databases. Please check:\n" +
                    "- Host and port are correct\n" +
                    "- Username and password are valid\n" +
                    "- Database server is running\n" +
                    "- User has permission to list databases");
        }
    }

    @FXML
    private void onTestConnection() {
        String dbType = dbTypeCombo.getValue();
        String host = hostField.getText().trim();
        String port = portField.getText().trim();
        String database = databaseCombo.getValue();
        String username = usernameField.getText().trim();
        String password = passwordField.getText();

        // Валидация
        if (host.isEmpty() || port.isEmpty() || username.isEmpty()) {
            showAlert(Alert.AlertType.WARNING, "Missing Information",
                    "Please fill in Host, Port and Username!");
            return;
        }

        if (database == null || database.isEmpty()) {
            showAlert(Alert.AlertType.WARNING, "No Database Selected",
                    "Please select a database first!");
            return;
        }

        // Тестване на връзката
        String result = dbManager.testConnection(dbType, host, port, database, username, password);
        resultArea.setText(result);
    }

    @FXML
    private void onExecuteQuery() {
        String dbType = dbTypeCombo.getValue();
        String host = hostField.getText().trim();
        String port = portField.getText().trim();
        String database = databaseCombo.getValue();
        String username = usernameField.getText().trim();
        String password = passwordField.getText();

        // Валидация
        if (host.isEmpty() || port.isEmpty() || username.isEmpty()) {
            showAlert(Alert.AlertType.WARNING, "Missing Information",
                    "Please fill in Host, Port and Username!");
            return;
        }

        if (database == null || database.isEmpty()) {
            showAlert(Alert.AlertType.WARNING, "No Database Selected",
                    "Please select a database first!");
            return;
        }

        // Диалог за въвеждане на query
        TextInputDialog dialog = new TextInputDialog("SELECT version();");
        dialog.setTitle("Execute SQL Query");
        dialog.setHeaderText("Enter your SQL query:");
        dialog.setContentText("Query:");
        dialog.getDialogPane().setPrefWidth(500);

        dialog.showAndWait().ifPresent(query -> {
            if (!query.trim().isEmpty()) {
                String result = dbManager.executeQuery(dbType, host, port, database, username, password, query);
                resultArea.setText(result);
            }
        });
    }

    @FXML
    private void onClearResults() {
        resultArea.clear();
    }

    private void updateUrlPreview() {
        String dbType = dbTypeCombo.getValue();
        String host = hostField.getText().isEmpty() ? "localhost" : hostField.getText();
        String port = portField.getText();
        String database = databaseCombo.getValue();

        if (database == null || database.isEmpty()) {
            database = "<select database>";
        }

        String url = "";
        switch (dbType) {
            case "PostgreSQL":
                url = "jdbc:postgresql://" + host + ":" + port + "/" + database;
                break;
            case "MySQL":
                url = "jdbc:mysql://" + host + ":" + port + "/" + database;
                break;
            case "MariaDB":
                url = "jdbc:mariadb://" + host + ":" + port + "/" + database;
                break;
            case "SQL Server":
                url = "jdbc:sqlserver://" + host + ":" + port + ";databaseName=" + database;
                break;
        }

        urlPreviewLabel.setText(url);
    }

    private void showAlert(Alert.AlertType type, String title, String message) {
        Alert alert = new Alert(type);
        alert.setTitle(title);
        alert.setHeaderText(null);
        alert.setContentText(message);
        alert.showAndWait();
    }
}
