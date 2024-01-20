package me.luucx7.march7th.utils;

import org.javacord.api.interaction.SlashCommandInteractionOption;

import java.util.Optional;

public class DateUtils {
    public static boolean isValidDayAndMonth(Optional<SlashCommandInteractionOption> dayOptionOp, Optional<SlashCommandInteractionOption> monthOptionOp) {
        if (dayOptionOp.isEmpty() || monthOptionOp.isEmpty()) return false;

        SlashCommandInteractionOption dayOption = dayOptionOp.get();
        SlashCommandInteractionOption monthOption = monthOptionOp.get();

        if (dayOption.getDecimalValue().isEmpty() || monthOption.getDecimalValue().isEmpty()) return false;

        int day = dayOption.getDecimalValue().get().intValue();
        int month = monthOption.getDecimalValue().get().intValue();

        if (month <= 0 || month > 12) return false;
        if (day <= 0 || day > 31) return false;

        int max;
        switch (month) {
            case 1,3,5,7,8,10,12:
                max = 31;
                break;
            case 4,6,9,11:
                max = 30;
                break;
            case 2:
                max = 29;
                break;
            default:
                return false;
        }

        return day <= max;
    }

    /**
     * Returns the date as a beautiful String
     * Example:
     *  day 7 - month 3 returns '7 de Março'
     *
     * @param day
     * @param month
     * @return 7 de março
     */
    public static String getFullDate(short day, short month) {
        String dayStr = String.valueOf(day);
        if (day == 1) dayStr += "º";

        return dayStr + " de " + getMonthName(month);
    }

    public static String padDay(short day) {
        if (day < 10) return "0" + day;

        return String.valueOf(day);
    }

    public static String getMonthName(short month) {
        return switch (month) {
            case 1 -> "Janeiro";
            case 2 -> "Fevereiro";
            case 3 -> "Março";
            case 4 -> "Abril";
            case 5 -> "Maio";
            case 6 -> "Junho";
            case 7 -> "Julho";
            case 8 -> "Agosto";
            case 9 -> "Setembro";
            case 10 -> "Outubro";
            case 11 -> "Novembro";
            case 12 -> "Dezembro";
            default -> "??????????";
        };
    }
}
