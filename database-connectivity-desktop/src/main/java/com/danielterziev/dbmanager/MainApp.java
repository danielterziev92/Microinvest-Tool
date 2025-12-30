package com.danielterziev.dbmanager;

import javafx.application.Application;
import javafx.fxml.FXML;
import javafx.fxml.FXMLLoader;
import javafx.geometry.Rectangle2D;
import javafx.scene.Scene;
import javafx.stage.Screen;
import javafx.stage.Stage;

import java.awt.*;
import java.io.IOException;

public class MainApp extends Application {
    @FXML
    public void start(Stage stage) throws IOException {
        FXMLLoader fxmlLoader = new FXMLLoader(MainApp.class.getResource("main-view.fxml"));
        Scene scene = new Scene(fxmlLoader.load(), 750, 850);
        stage.setTitle("Database Connection Manager");
        stage.setScene(scene);

        Screen targetScreen = getScreenWithMouse();
        Rectangle2D screenBounds = targetScreen.getVisualBounds();

        double centerX = screenBounds.getMinX() + (screenBounds.getWidth() - 750) / 2;
        double centerY = screenBounds.getMinY() + (screenBounds.getHeight() - 850) / 2;

        stage.setX(centerX);
        stage.setY(centerY);

        stage.show();
    }

    private Screen getScreenWithMouse() {
        Point mouseLocation = MouseInfo.getPointerInfo().getLocation();

        for (Screen screen : Screen.getScreens()) {
            Rectangle2D bounds = screen.getBounds();

            if (bounds.contains(mouseLocation.x, mouseLocation.y)) {
                return screen;
            }
        }

        return Screen.getPrimary();
    }


    public static void main(String[] args) {
        launch();
    }
}
