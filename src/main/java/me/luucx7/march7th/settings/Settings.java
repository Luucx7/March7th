package me.luucx7.march7th.settings;

import me.luucx7.march7th.March7th;
import me.luucx7.march7th.settings.model.MarchSettings;
import org.apache.commons.io.FileUtils;
import org.yaml.snakeyaml.Yaml;

import java.io.*;
import java.net.URL;
import java.nio.file.Path;

public class Settings {

    private static MarchSettings settings;

    public static void loadSettings() throws IOException {
        String jarFilePath = March7th.class.getProtectionDomain().getCodeSource().getLocation().getPath();

        // bullshit time
        String pathString = "";
        String[] splittedShit = jarFilePath.split("/");
        for (int i = 0; i < (splittedShit.length - 1); i++) {
            pathString += splittedShit[i] + "/";
        }

        System.out.println(pathString);
        String settingsFilePath = pathString + "settings.yml";
        System.out.println(settingsFilePath);

        try (Reader reader = new FileReader(settingsFilePath)) {
            Yaml yaml = new Yaml();

            settings = yaml.loadAs(reader, MarchSettings.class);
        }
    }
    private static void createSettingsFileIfNotExists(String settingsFilePath) throws IOException {
        String resourceName = "settings.example.yml";

        ClassLoader classLoader = Settings.class.getClassLoader();
        URL resourceUrl = classLoader.getResource(resourceName);

        if (resourceUrl != null) {
            File sourceFile = new File(resourceUrl.getFile());
            File destinationFile = new File(settingsFilePath);

            FileUtils.copyFile(sourceFile, destinationFile);
        } else {
            throw new FileNotFoundException("Default example settings file not found.");
        }
    }

    public static MarchSettings get() {
        return settings;
    }
}
