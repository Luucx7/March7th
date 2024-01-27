package me.luucx7.march7th.settings.model;

import lombok.Getter;
import lombok.Setter;
import me.luucx7.march7th.utils.DateUtils;

import java.util.List;
import java.util.Random;

@Getter
@Setter
public class DiscordSettings {
    private String token;
    private long birthdayRoleId;
    private long birthdayMessageChannelId;
    private String march7th_special;
    private List<String> oneBirthdayMessages;
    private List<String> multipleBirthdayMessages;

    public String getBirthdayMessage(short day, short month, List<Long> userIds) {
        if (userIds.isEmpty()) return "";

        String message;
        Random r = new Random();

        if (userIds.size() == 1) {
            if (oneBirthdayMessages.isEmpty()) return "";

            int randomPos = r.nextInt(oneBirthdayMessages.size());
            message = oneBirthdayMessages.get(randomPos);
        } else {
            if (multipleBirthdayMessages.isEmpty()) return "";

            int randomPos = r.nextInt(multipleBirthdayMessages.size());
            message = multipleBirthdayMessages.get(randomPos);
        }

        return parseMessage(message, day, month, userIds).trim();
    }

    private String parseMessage(String rawMessage, short day, short month, List<Long> userIds) {
        String newString = rawMessage;
        newString = newString.replaceAll("%USER%", "<@" + userIds.get(0) + ">");
        newString = newString.replaceAll("%DAY_NUMBER%", String.valueOf(day));
        newString = newString.replaceAll("%DAY%", String.valueOf(DateUtils.padDay(day)));
        newString = newString.replaceAll("%MONTH_NUMBER%", String.valueOf(month));
        newString = newString.replaceAll("%MONTH%", String.valueOf(DateUtils.padDay(month))); // lol
        newString = newString.replaceAll("%MONTH_NAME%", DateUtils.getMonthName(month));

        if (userIds.size() > 1) {
            StringBuilder mentions = new StringBuilder();
            for (long userId : userIds) {
                mentions.append("<@").append(userId).append("> ");
            }

            newString = newString.replaceAll("%USERS%", mentions.toString().trim());
        }

        if (day == 7 && month == 3 && !march7th_special.isEmpty()) newString += " " + march7th_special;

        return newString;
    }
}
