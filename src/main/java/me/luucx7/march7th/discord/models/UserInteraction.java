package me.luucx7.march7th.discord.models;

import java.time.LocalDateTime;

/**
 * Defines a user interaction with the bot with commands.
 * Necessary to manage multi commands contexts.
 */
public interface UserInteraction {

    /**
     * Returns the user id that started the interaction
     *
     * @return long Discord user id
     */
    long getUserId();

    /**
     * Returns the date where the user created an command interaction
     *
     * @return LocalDateTime Date when the interaction started
     */
    LocalDateTime getStartDate();

    /**
     * Returns the date of the last user interaction here
     *
     * @return LocalDateTime The date of the last interaction
     */
    LocalDateTime getLastActionDate();

    /**
     * Updates the date of the last interaction of the player
     */
    void setLastActionDate(LocalDateTime lastActionDate);

    /**
     * Ends a successful operation
     */
    void end();

    /**
     * Cancels the operation and removes it from cache.
     */
    void abort(boolean followupMessage);
}
