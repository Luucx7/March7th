package me.luucx7.march7th.discord.commands.set_own_birthday;

import lombok.AccessLevel;
import lombok.Getter;
import lombok.Setter;
import me.luucx7.march7th.database.models.BirthdayInformation;
import me.luucx7.march7th.discord.models.UserInteraction;
import me.luucx7.march7th.discord.services.InteractionsService;
import me.luucx7.march7th.utils.MarchImages;
import org.javacord.api.entity.message.MessageFlag;
import org.javacord.api.entity.message.embed.EmbedBuilder;
import org.javacord.api.event.interaction.SlashCommandCreateEvent;

import java.awt.*;
import java.time.LocalDateTime;
import java.util.concurrent.Executors;
import java.util.concurrent.ScheduledExecutorService;
import java.util.concurrent.ScheduledFuture;
import java.util.concurrent.TimeUnit;

@Getter
public class SetBirthdayInteraction implements UserInteraction {

    private final long userId;

    private final LocalDateTime startDate;
    private LocalDateTime lastActionDate;

    private final SlashCommandCreateEvent commandCreateEvent;

    @Setter
    private BirthdayInformation birthdayInformation;

    @Getter(AccessLevel.PRIVATE)
    @Setter(AccessLevel.PRIVATE)
    private ScheduledFuture<?> scheduledAbort;

    private final ScheduledExecutorService scheduler;

    public SetBirthdayInteraction(long userId, BirthdayInformation information, SlashCommandCreateEvent commandCreateEvent) {
        this.userId = userId;
        this.birthdayInformation = information;
        this.commandCreateEvent = commandCreateEvent;

        this.startDate = LocalDateTime.now();
        this.lastActionDate = this.startDate;

        this.scheduler = Executors.newScheduledThreadPool(1);

        scheduleAbort();
    }

    private void scheduleAbort() {
        setScheduledAbort(scheduler.schedule(() -> { abort(true); }, TimeUnit.SECONDS.toMillis(30), TimeUnit.MILLISECONDS));
    }

    @Override
    public void setLastActionDate(LocalDateTime lastActionDate) {
        this.lastActionDate = lastActionDate;

        getScheduledAbort().cancel(true);
        scheduleAbort();
    }

    @Override
    public void end() {
        abort(false);
    }

    @Override
    public void abort(boolean followupMessage) {
        if (followupMessage) {
            EmbedBuilder builder = new EmbedBuilder()
                    .setDescription("Você demorou demais e a operação foi cancelada.")
                    .setThumbnail(MarchImages.MARCH_SLEEPING.getUrl())
                    .setColor(Color.RED);

            commandCreateEvent.getSlashCommandInteraction().createFollowupMessageBuilder()
                    .addEmbed(builder)
                    .setFlags(MessageFlag.EPHEMERAL)
                    .send();
        }

        InteractionsService.endInteraction(this);

        scheduledAbort.cancel(false);
        scheduler.shutdownNow();
    }
}
