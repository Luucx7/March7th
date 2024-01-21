package me.luucx7.march7th.discord.commands;

import me.luucx7.march7th.March7th;
import me.luucx7.march7th.utils.MarchImages;
import org.javacord.api.DiscordApi;
import org.javacord.api.entity.message.embed.EmbedBuilder;
import org.javacord.api.event.interaction.SlashCommandCreateEvent;
import org.javacord.api.interaction.*;
import org.javacord.api.listener.interaction.SlashCommandCreateListener;

import java.awt.*;

public class AboutCommand implements SlashCommandCreateListener {

    private static final String COMMAND = "sobre";
    private static final String DESCRIPTION = "Informações sobre o bot";

    public AboutCommand(DiscordApi api) {
        register(api);
    }

    private void register(DiscordApi api) {
        SlashCommandBuilder commandBuilder = SlashCommand.with(COMMAND, DESCRIPTION)
                .setEnabledInDms(true);

        SlashCommand slashCommand = commandBuilder.createGlobal(api)
                .join();
    }

    @Override
    public void onSlashCommandCreate(SlashCommandCreateEvent event) {
        SlashCommandInteraction interaction = event.getSlashCommandInteraction();
        if (!interaction.getCommandName().equalsIgnoreCase(COMMAND)) return;

        EmbedBuilder builder = new EmbedBuilder()
                .setColor(Color.PINK)
                .setTitle("7 de Março")
                .setDescription("Um bot para dar um cargo especial aos aniversariantes do dia!")
                .addField("GitHub", "https://github.com/Luucx7/March7th")
                .addInlineField("Autor", "nier.automata")
                .addInlineField("Versão", March7th.getVersion())
                .setThumbnail(MarchImages.MARCH_PROUD.getUrl());


        interaction.createImmediateResponder()
                .addEmbed(builder)
                .respond();
    }
}
