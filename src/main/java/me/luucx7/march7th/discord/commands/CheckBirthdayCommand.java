package me.luucx7.march7th.discord.commands;

import me.luucx7.march7th.database.DatabaseManager;
import me.luucx7.march7th.database.models.BirthdayInformation;
import me.luucx7.march7th.utils.DateUtils;
import me.luucx7.march7th.utils.MarchImages;
import org.javacord.api.DiscordApi;
import org.javacord.api.entity.message.MessageFlag;
import org.javacord.api.entity.message.embed.EmbedBuilder;
import org.javacord.api.entity.user.User;
import org.javacord.api.event.interaction.SlashCommandCreateEvent;
import org.javacord.api.interaction.*;
import org.javacord.api.listener.interaction.SlashCommandCreateListener;

import java.awt.*;
import java.sql.SQLException;
import java.util.Arrays;
import java.util.Optional;

public class CheckBirthdayCommand implements SlashCommandCreateListener {
    private static final String COMMAND = "verificaraniversario";
    private static final String DESCRIPTION = "Verifique de maneira privada o aniversário de alguém.";

    private static final String USER_OPTION = "usuario";
    private static final String USER_OPTION_DESCRIPTION = "O usuário que você deseja ver o aniversário.";

    public CheckBirthdayCommand(DiscordApi api) {
        register(api);
    }

    private void register(DiscordApi api) {
        SlashCommandBuilder commandBuilder = SlashCommand.with(COMMAND, DESCRIPTION,
                Arrays.asList(
                    SlashCommandOption.create(SlashCommandOptionType.USER, USER_OPTION, USER_OPTION_DESCRIPTION, true)
                ))
                .setDefaultEnabledForEveryone()
                .setEnabledInDms(true);

        SlashCommand slashCommand = commandBuilder.createGlobal(api)
                .join();
    }

    @Override
    public void onSlashCommandCreate(SlashCommandCreateEvent event) {
        SlashCommandInteraction interaction = event.getSlashCommandInteraction();
        if (!interaction.getCommandName().equalsIgnoreCase(COMMAND)) return;

        Optional<SlashCommandInteractionOption> userOptionOp = interaction.getArgumentByName(USER_OPTION);
        if (userOptionOp.isEmpty()) {
            EmbedBuilder errorBuilder = new EmbedBuilder()
                    .setTitle("Aniversário")
                    .setColor(Color.red)
                    .setDescription("Valor do usuário não encontrado! Verifique seu comando e tente novamente...")
                    .setThumbnail(MarchImages.MARCH_ANGRY.getUrl());

            interaction.createImmediateResponder().addEmbed(errorBuilder).setFlags(MessageFlag.EPHEMERAL).respond();
            return;
        }
        SlashCommandInteractionOption option = userOptionOp.get();

        Optional<User> userOp = option.getUserValue();
        if (userOp.isEmpty()) {
            EmbedBuilder errorBuilder = new EmbedBuilder()
                    .setTitle("Aniversário")
                    .setDescription("Usuário não encontrado!")
                    .setColor(Color.red)
                    .setThumbnail(MarchImages.MARCH_SLEEPING.getUrl());

            interaction.createImmediateResponder().addEmbed(errorBuilder).setFlags(MessageFlag.EPHEMERAL).respond();
            return;
        }
        User user = userOp.get();
        long id = user.getId();

        Optional<BirthdayInformation> birthdayInformationOptional;
        try {
            birthdayInformationOptional = DatabaseManager.loadUserBirthday(id);
        } catch (SQLException e) {
            EmbedBuilder errorBuilder = new EmbedBuilder()
                    .setTitle("Aniversário")
                    .setColor(Color.RED)
                    .setDescription("Erro ao buscar aniversário do usuário! Tente novamente mais tarde...")
                    .setThumbnail(MarchImages.MARCH_SLEEPING.getUrl());

            interaction.createImmediateResponder().addEmbed(errorBuilder).setFlags(MessageFlag.EPHEMERAL).respond();
            return;
        }

        EmbedBuilder builder;
        if (birthdayInformationOptional.isPresent()) {
            BirthdayInformation birthdayInformation = birthdayInformationOptional.get();

            builder = new EmbedBuilder()
                    .setTitle("Aniversário de " + user.getName())
                    .setColor(Color.PINK)
                    .setThumbnail(user.getAvatar().getUrl().toString())
                    .addInlineField("Dia", DateUtils.padDay(birthdayInformation.getDay()))
                    .addInlineField("Mês", DateUtils.getMonthName(birthdayInformation.getMonth()));
//                    .setDescription("O aniversário de " + user.getName() + " é: " + DateUtils.getFullDate(birthdayInformation.getDay(), birthdayInformation.getMonth()));
        } else {
            builder = new EmbedBuilder()
                    .setTitle("Aniversário")
                    .setColor(Color.RED)
                    .setThumbnail(MarchImages.MARCH_SLEEPING.getUrl())
                    .setDescription("Nada foi encontrado! Este usuário não cadastrou seu aniversário.");
        }

        interaction.createImmediateResponder()
                .addEmbed(builder)
                .setFlags(MessageFlag.EPHEMERAL)
                .respond();
    }
}
