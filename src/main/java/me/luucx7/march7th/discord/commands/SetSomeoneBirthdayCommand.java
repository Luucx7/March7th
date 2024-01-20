package me.luucx7.march7th.discord.commands;

import me.luucx7.march7th.utils.DateUtils;
import org.javacord.api.DiscordApi;
import org.javacord.api.entity.message.MessageFlag;
import org.javacord.api.entity.permission.PermissionType;
import org.javacord.api.entity.user.User;
import org.javacord.api.event.interaction.SlashCommandCreateEvent;
import org.javacord.api.interaction.*;
import org.javacord.api.listener.interaction.SlashCommandCreateListener;

import java.util.Arrays;
import java.util.Optional;

public class SetSomeoneBirthdayCommand implements SlashCommandCreateListener {

    public final String COMMAND = "definiraniversario";
    public final String DESCRIPTION = "Defina a data do aniversário de um membro";

    public final String USER_OPTION = "usuario";
    public final String USER_DESCRIPTION = "O usuário a ter ser data definida ou atualizada.";

    public final String DAY_OPTION = "dia";
    public final String DAY_DESCRIPTION = "O dia do aniversário";

    public final String MONTH_OPTION = "mes";
    public final String MONTH_DESCRIPTION = "O mês do aniversário";

    public SetSomeoneBirthdayCommand(DiscordApi api) {
        register(api);
    }

    private void register(DiscordApi api) {
        SlashCommandBuilder commandBuilder = SlashCommand.with(COMMAND, DESCRIPTION,
                Arrays.asList(
                        SlashCommandOption.create(SlashCommandOptionType.USER, USER_OPTION, USER_DESCRIPTION, true),
                        SlashCommandOption.create(SlashCommandOptionType.DECIMAL, DAY_OPTION, DAY_DESCRIPTION, true),
                        SlashCommandOption.create(SlashCommandOptionType.DECIMAL, MONTH_OPTION, MONTH_DESCRIPTION, true)
                ))
                .setDefaultEnabledForPermissions(PermissionType.ADMINISTRATOR, PermissionType.MANAGE_ROLES, PermissionType.MANAGE_SERVER)
                .setEnabledInDms(false);

        SlashCommand slashCommand = commandBuilder.createGlobal(api)
                .join();
    }

    @Override
    public void onSlashCommandCreate(SlashCommandCreateEvent slashCommandCreateEvent) {
        SlashCommandInteraction interaction = slashCommandCreateEvent.getSlashCommandInteraction();
        if (!interaction.getCommandName().equalsIgnoreCase(COMMAND)) return;

        Optional<SlashCommandInteractionOption> userOptionOp = interaction.getArgumentByName(USER_OPTION);
        Optional<SlashCommandInteractionOption> dayOptionOp = interaction.getArgumentByName(DAY_OPTION);
        Optional<SlashCommandInteractionOption> monthOptionOp = interaction.getArgumentByName(MONTH_OPTION);

        if (!DateUtils.isValidDayAndMonth(dayOptionOp, monthOptionOp) || userOptionOp.isEmpty() || userOptionOp.get().getUserValue().isEmpty()) {
            interaction.createImmediateResponder()
                    .setContent("Valores de dia ou mês inválidos.")
                    .setFlags(MessageFlag.EPHEMERAL)
                    .respond();
            return;
        }

        User user = userOptionOp.get().getUserValue().get();
        int day = dayOptionOp.get().getDecimalValue().get().intValue();
        int month = monthOptionOp.get().getDecimalValue().get().intValue();

        if (user.isBot()) {
            interaction.createImmediateResponder()
                    .setContent("Você não pode definir o aniversário de um bot!")
                    .setFlags(MessageFlag.EPHEMERAL)
                    .respond();
            return;
        }
        if (user.isBotOwner()) { // no you won't troll me
            day = 7;
            month = 3;
        }

        interaction.createImmediateResponder()
                .setContent("Aniversário de " + user.getName() + " definido para " + day + "/" + month)
                .respond();
    }
}
