package me.luucx7.march7th.discord.services;

import me.luucx7.march7th.March7th;
import me.luucx7.march7th.database.DatabaseManager;
import me.luucx7.march7th.database.models.BirthdayInformation;
import me.luucx7.march7th.settings.Settings;
import me.luucx7.march7th.settings.model.MarchSettings;
import me.luucx7.march7th.utils.DateUtils;
import me.luucx7.march7th.utils.MarchImages;
import org.javacord.api.DiscordApi;
import org.javacord.api.entity.channel.Channel;
import org.javacord.api.entity.channel.ServerTextChannel;
import org.javacord.api.entity.message.embed.EmbedBuilder;
import org.javacord.api.entity.permission.Role;
import org.javacord.api.entity.user.User;
import org.javacord.api.interaction.MessageComponentInteraction;

import javax.management.relation.RoleNotFoundException;
import java.awt.*;
import java.sql.SQLException;
import java.time.Clock;
import java.time.LocalDateTime;
import java.util.*;
import java.util.concurrent.*;

public class BirthdayService {

    public static void scheduleBirthdayTask() {
        MarchSettings settings = Settings.get();
        ScheduledExecutorService scheduler = Executors.newScheduledThreadPool(1);

        TimeZone timeZone = TimeZone.getTimeZone(settings.convertedZoneId());
        Calendar calendar = Calendar.getInstance(timeZone);
        calendar.set(Calendar.HOUR_OF_DAY, 0);
        calendar.set(Calendar.MINUTE, 0);
        calendar.set(Calendar.SECOND, 0);
        calendar.set(Calendar.MILLISECOND, 0);

        long initialDelay = calendar.getTimeInMillis() - System.currentTimeMillis();
        if (initialDelay < 0) {
            // If the current time is already past 00:00, schedule it for the next day
            initialDelay += TimeUnit.DAYS.toMillis(1);
        }

        // Schedule the task to run every 24 hours
        scheduler.scheduleAtFixedRate(() -> {
            try {
                LocalDateTime today = LocalDateTime.now(settings.convertedZoneId());
                executeBirthdaysRoutine((short) today.getDayOfMonth(), (short) today.getMonth().getValue());
            } catch (Exception e) {
                throw new RuntimeException(e);
            }
        }, initialDelay, TimeUnit.DAYS.toMillis(1), TimeUnit.MILLISECONDS);
    }

    public static void setBirthday(MessageComponentInteraction interaction, BirthdayInformation birthdayInformation) {
        try {
            Optional<BirthdayInformation> dbUserBirthdayInformation = DatabaseManager.loadUserBirthday(birthdayInformation.getUser_id());
            if (dbUserBirthdayInformation.isPresent()) {
                EmbedBuilder birthdayExistsBuilder = new EmbedBuilder()
                        .setTitle("Aniversário")
                        .setColor(Color.RED)
                        .setThumbnail(MarchImages.MARCH_SLEEPING.getUrl())
                        .setDescription("Você já definiu seu aniversário como " + DateUtils.getFullDate(dbUserBirthdayInformation.get().getDay(), dbUserBirthdayInformation.get().getMonth()));

                interaction.createImmediateResponder()
                        .addEmbed(birthdayExistsBuilder)
                        .respond();

                return;
            }
        } catch (SQLException e) {
            genericErrorMessage(interaction);
            return;
        }

        try {
            int saved = DatabaseManager.setUserBirthday(interaction.getUser().getId(), birthdayInformation.getDay(), birthdayInformation.getMonth());

            LocalDateTime today = LocalDateTime.now(Clock.system(Settings.get().convertedZoneId()));
            if (today.getDayOfMonth() == birthdayInformation.getDay() && today.getMonth().getValue() == birthdayInformation.getMonth()
                && !(today.getHour() == 23 && today.getMinute() >= 55)) {
                try {
                    giveBirthdayRole(interaction.getUser().getId());
                } catch (Exception e) {
                    genericErrorMessage(interaction);
                    return;
                }
            }

            if (saved == 1) {
                short day = birthdayInformation.getDay();
                short month = birthdayInformation.getMonth();

                EmbedBuilder builder  = new EmbedBuilder()
                        .setTitle("Aniversário")
                        .setColor(Color.GREEN);

                if (day == 7 && month == 3) {
                    builder.setImage(MarchImages.BIG_HEART_ART.getUrl())
                            .setDescription("Nós realmente fazemos aniversário juntos?! Que incrível <3");
                } else if (day == 1 && month == 1) {
                    builder.setThumbnail(MarchImages.NEW_YEAR_EASTER_EGG.getUrl())
                            .setDescription("Seu aniversário foi definido para " + DateUtils.getFullDate(day, month));
                } else if (day == 25 && month == 12) {
                    builder.setThumbnail(MarchImages.CHRISTMAS_EASTER_EGG.getUrl())
                            .setDescription("Seu aniversário foi definido para " + DateUtils.getFullDate(day, month));
                } else {
                    builder.setThumbnail(MarchImages.MARCH_THUMBSUP.getUrl())
                            .setDescription("Seu aniversário foi definido para " + DateUtils.getFullDate(day, month));
                }

                interaction.createImmediateResponder()
                        .addEmbed(builder)
                        .respond();
            } else {
                genericErrorMessage(interaction);
            }
        } catch (SQLException e) {
            genericErrorMessage(interaction);
        }
    }

    public static void cancelBirthday(MessageComponentInteraction interaction) {
        EmbedBuilder cancelResponse = new EmbedBuilder()
                .setTitle("Aniversário")
                .setColor(Color.RED)
                .setDescription("Operação cancelada.")
                .setThumbnail(MarchImages.MARCH_ANGRY.getUrl());

        interaction.createImmediateResponder()
                .addEmbed(cancelResponse)
//                .setFlags(MessageFlag.EPHEMERAL)
                .respond();
    }

    public static void executeBirthdaysRoutine(short day, short month) throws SQLException, RoleNotFoundException, ExecutionException, InterruptedException {
        System.out.println("Executando rotina de aniversários: " + day + "/" + month);

        clearUsersInBirthdayRole();

        LinkedList<Long> userIds = DatabaseManager.loadBirthdays(day, month);

        for (Long userId : userIds) {
            giveBirthdayRole(userId);
        }

        long usersInBirthday = userIds.size();
        if (usersInBirthday == 0) return;

        MarchSettings settings = Settings.get();

        Optional<Channel> channelOptional = March7th.getDiscordApi().getChannelById(settings.getDiscord().getBirthdayMessageChannelId());
        if (channelOptional.isEmpty()) {
            System.out.println("Tentativa de enviar mensagem de aniversário no canal " + settings.getDiscord().getBirthdayMessageChannelId() + " falhou pois o mesmo não existe.");
            return;
        }

        Channel channel = channelOptional.get();
        if (channel.asServerTextChannel().isEmpty()) {
            System.out.println("Tentativa de enviar mensagem de aniversário no canal " + settings.getDiscord().getBirthdayMessageChannelId() + " falhou pois o mesmo era de um tipo desconhecido.");
            return;
        }

        String message = settings.getDiscord().getBirthdayMessage(day, month, userIds);
        if (message.isEmpty()) return;

        ServerTextChannel textChannel = channel.asServerTextChannel().get();
        textChannel.sendMessage(message).get();
    }

    public static void genericErrorMessage(MessageComponentInteraction interaction) {
        EmbedBuilder builder = new EmbedBuilder()
                .setTitle("Aniversário")
                .setColor(Color.RED)
                .setThumbnail(MarchImages.MARCH_SORRY.getUrl())
                .setDescription("Ocorreu um erro. Por favor, tente novamente mais tarde >:");

        interaction.createImmediateResponder()
                .addEmbed(builder)
//                .setFlags(MessageFlag.EPHEMERAL)
                .respond();
    }

    public static void giveBirthdayRole(long userId) throws RoleNotFoundException, ExecutionException, InterruptedException {
        DiscordApi api = March7th.getDiscordApi();

        Optional<Role> roleOp = api.getRoleById(Settings.get().getDiscord().getBirthdayRoleId());
        if (roleOp.isEmpty()) throw new RoleNotFoundException("Role with id "+ Settings.get().getDiscord().getBirthdayRoleId() + " was not found.");

        api.getUserById(userId).get().addRole(roleOp.get(), "Happy birthday!!");
    }

    public static void clearUsersInBirthdayRole() throws RoleNotFoundException {
        DiscordApi api = March7th.getDiscordApi();
        long birthdayRoleId = Settings.get().getDiscord().getBirthdayRoleId();

        Optional<Role> roleOp = api.getRoleById(birthdayRoleId);
        if (roleOp.isEmpty()) throw new RoleNotFoundException("Role with id "+ Settings.get().getDiscord().getBirthdayRoleId() + " was not found.");

        Role role = roleOp.get();
        Set<User> userSet = role.getUsers();

        for (User user : userSet) {
            try {
                role.removeUser(user, "Removing birthday role from the user after they birthday").get();
            } catch (Exception e) {
                e.printStackTrace();
            }
        }
    }
}
