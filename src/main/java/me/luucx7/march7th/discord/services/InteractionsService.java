package me.luucx7.march7th.discord.services;

import lombok.Getter;
import me.luucx7.march7th.discord.models.UserInteraction;

import java.util.LinkedHashMap;
import java.util.LinkedList;
import java.util.Optional;

public class InteractionsService {

    @Getter
    private static final LinkedHashMap<Long, LinkedList<UserInteraction>> userInteractions = new LinkedHashMap<>();

    public static <T extends UserInteraction> Optional<T> getInteraction(long userId, Class<T> classz) {
        if (!getUserInteractions().containsKey(userId)) return Optional.empty();

        LinkedList<UserInteraction> interactions = getUserInteractions().get(userId);

        for (UserInteraction interaction : interactions) {
            if (classz.isAssignableFrom(interaction.getClass())) {
                return Optional.of(classz.cast(interaction));
            }
        }

        return Optional.empty();
    }

    public static void addInteraction(long userId, UserInteraction interaction) {
        if (!getUserInteractions().containsKey(userId)) getUserInteractions().put(userId, new LinkedList<>());

        getUserInteractions().get(userId).add(interaction);
    }

    public static void endInteraction(UserInteraction userInteraction) {
        if (!getUserInteractions().containsKey(userInteraction.getUserId())) return;

        getUserInteractions().get(userInteraction.getUserId()).remove(userInteraction);
    }
}
