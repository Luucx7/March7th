package me.luucx7.march7th.discord.commands.set_own_birthday;

import me.luucx7.march7th.database.models.BirthdayInformation;
import me.luucx7.march7th.discord.services.BirthdayService;
import me.luucx7.march7th.discord.services.InteractionsService;
import me.luucx7.march7th.utils.DateUtils;
import me.luucx7.march7th.utils.MarchImages;
import org.javacord.api.DiscordApi;
import org.javacord.api.entity.message.MessageFlag;
import org.javacord.api.entity.message.component.ActionRow;
import org.javacord.api.entity.message.component.Button;
import org.javacord.api.entity.message.embed.EmbedBuilder;
import org.javacord.api.event.interaction.MessageComponentCreateEvent;
import org.javacord.api.event.interaction.SlashCommandCreateEvent;
import org.javacord.api.interaction.*;
import org.javacord.api.listener.interaction.MessageComponentCreateListener;
import org.javacord.api.listener.interaction.SlashCommandCreateListener;

import java.awt.*;
import java.util.Arrays;
import java.util.Optional;

public class SetBirthdayCommand implements SlashCommandCreateListener, MessageComponentCreateListener {

    public final String COMMAND = "aniversario";
    public final String DESCRIPTION = "Defina a data de seu aniversário para obter uma surpresa quando o dia chegar!";

    public final String DAY_OPTION = "dia";
    public final String DAY_DESCRIPTION = "O dia de seu aniversário";

    public final String MONTH_OPTION = "mes";
    public final String MONTH_DESCRIPTION = "O mês de seu aniversário";

    public static final String BIRTHDAY_CONFIRM_ID = "setbirthday_confirm";
    public static final String BIRTHDAY_CANCEL_ID = "setbirthday_cancel";

    public SetBirthdayCommand(DiscordApi api) {
        register(api);
    }

    private void register(DiscordApi api) {
        SlashCommandBuilder commandBuilder = SlashCommand.with(COMMAND, DESCRIPTION,
                Arrays.asList(
                        SlashCommandOption.create(SlashCommandOptionType.DECIMAL, DAY_OPTION, DAY_DESCRIPTION, true),
                        SlashCommandOption.create(SlashCommandOptionType.DECIMAL, MONTH_OPTION, MONTH_DESCRIPTION, true)
                ))
                .setDefaultEnabledForEveryone()
                .setEnabledInDms(false);

        SlashCommand slashCommand = commandBuilder.createGlobal(api)
                .join();
    }

    @Override
    public void onSlashCommandCreate(SlashCommandCreateEvent slashCommandCreateEvent) {
        SlashCommandInteraction interaction = slashCommandCreateEvent.getSlashCommandInteraction();
        if (!interaction.getCommandName().equalsIgnoreCase(COMMAND)) return;

        long userId = interaction.getUser().getId();

        Optional<SetBirthdayInteraction> interactionOptional = InteractionsService.getInteraction(userId, SetBirthdayInteraction.class);
        if (interactionOptional.isPresent()) {
            EmbedBuilder builder  = new EmbedBuilder()
                    .setTitle("Aniversário")
                    .setDescription("Você já possui uma operação em andamento.")
                    .setThumbnail(MarchImages.MARCH_SLEEPING.getUrl())
                    .setColor(Color.RED);

            interaction.createImmediateResponder()
                    .addEmbed(builder)
                    .setFlags(MessageFlag.EPHEMERAL)
                    .respond();
            return;
        }

        Optional<SlashCommandInteractionOption> dayOptionOp = interaction.getArgumentByName(DAY_OPTION);
        Optional<SlashCommandInteractionOption> monthOptionOp = interaction.getArgumentByName(MONTH_OPTION);

        if (!DateUtils.isValidDayAndMonth(dayOptionOp, monthOptionOp)) {
            EmbedBuilder builder  = new EmbedBuilder()
                    .setTitle("Aniversário")
                    .setDescription("Valores de dia e/ou mês inválidos, corrija-os e tente novamente")
                    .setThumbnail(MarchImages.MARCH_CRYING.getUrl())
                    .setColor(Color.RED);

            interaction.createImmediateResponder()
                    .addEmbed(builder)
                    .setFlags(MessageFlag.EPHEMERAL)
                    .respond();

            return;
        }

        short day = dayOptionOp.get().getDecimalValue().get().shortValue(); // everything was validated already the get() is safe
        short month = monthOptionOp.get().getDecimalValue().get().shortValue();

        EmbedBuilder confirmationEmbed = new EmbedBuilder()
                .setTitle("Aniversário")
                .setColor(Color.PINK)
                .setThumbnail(MarchImages.MARCH_BIRTHDAY.getUrl())
                .setDescription("Você quer definir seu aniversário como " + DateUtils.getFullDate(day, month) + "? Você não poderá alterar isso depois.");

        SetBirthdayInteraction birthdayInteraction = new SetBirthdayInteraction(userId, new BirthdayInformation(userId, day, month), slashCommandCreateEvent);
        InteractionsService.addInteraction(userId, birthdayInteraction);

        interaction.createImmediateResponder()
                .addEmbed(confirmationEmbed)
                .addComponents(
                        ActionRow.of(Button.success(BIRTHDAY_CONFIRM_ID, "Confirmar"),
                                Button.danger(BIRTHDAY_CANCEL_ID, "Cancelar"))
                )
                .respond();
    }

    @Override
    public void onComponentCreate(MessageComponentCreateEvent event) {
        MessageComponentInteraction messageComponentInteraction = event.getMessageComponentInteraction();
        String customId = messageComponentInteraction.getCustomId();
        long userId = event.getInteraction().getUser().getId();

        Optional<SetBirthdayInteraction> birthdayInteraction = InteractionsService.getInteraction(userId, SetBirthdayInteraction.class);

        if (birthdayInteraction.isEmpty()) {
            event.getMessageComponentInteraction().acknowledge();
            return;
        }

        BirthdayInformation birthdayInformation = birthdayInteraction.get().getBirthdayInformation();

        switch (customId) {
            case BIRTHDAY_CONFIRM_ID:
                BirthdayService.setBirthday(messageComponentInteraction, birthdayInformation);
                break;
            case BIRTHDAY_CANCEL_ID:
                BirthdayService.cancelBirthday(messageComponentInteraction);
                break;
            default:
                event.getMessageComponentInteraction().acknowledge();
                return;
        }

        birthdayInteraction.get().end();

        InteractionsService.endInteraction(birthdayInteraction.get());
    }
}
