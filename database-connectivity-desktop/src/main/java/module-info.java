module com.danielterziev.dbmanager {
    requires javafx.controls;
    requires javafx.fxml;
    requires java.sql;
    requires java.desktop;


    opens com.danielterziev.dbmanager to javafx.fxml;
    exports com.danielterziev.dbmanager;
}