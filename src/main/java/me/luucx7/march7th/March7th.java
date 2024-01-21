package me.luucx7.march7th;

import lombok.Getter;
import me.luucx7.march7th.database.DatabaseManager;
import me.luucx7.march7th.discord.commands.AboutCommand;
import me.luucx7.march7th.discord.commands.CheckBirthdayCommand;
import me.luucx7.march7th.discord.commands.set_own_birthday.SetBirthdayCommand;
import me.luucx7.march7th.discord.services.BirthdayService;
import me.luucx7.march7th.settings.Settings;
import me.luucx7.march7th.settings.model.MarchSettings;
import org.javacord.api.DiscordApi;
import org.javacord.api.DiscordApiBuilder;

import java.io.IOException;

public class March7th {

    @Getter
    private static final String Version = "1.0.1-BETA";

    @Getter
    private static DiscordApi discordApi;

    public static void main(String[] args) {
        try {
            Settings.loadSettings();
        } catch (IOException exception) {
            exception.printStackTrace();
            System.out.println("Settings file not found. Terminating...");
            return;
        }

        MarchSettings settings = Settings.get();

        if (!DatabaseManager.testConnection()) {
            System.out.println("Failure while loading database. Terminating...");
            return;
        }

        discordApi = new DiscordApiBuilder()
                .setToken(settings.getDiscord().getToken())
                .setAllIntents()
                .login()
                .join();

        discordApi.addListener(new SetBirthdayCommand(discordApi));
        discordApi.addListener(new CheckBirthdayCommand(discordApi));
        discordApi.addListener(new AboutCommand(discordApi));

        BirthdayService.scheduleBirthdayTask();

        System.out.println("Aplicação iniciada.");

        do {
            try {
                Thread.sleep(1000);
            } catch (Exception ignored) { }
        } while(true);
    }
}
