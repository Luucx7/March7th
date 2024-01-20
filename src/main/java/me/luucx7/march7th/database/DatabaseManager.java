package me.luucx7.march7th.database;

import com.zaxxer.hikari.HikariConfig;
import com.zaxxer.hikari.HikariDataSource;
import lombok.Getter;
import me.luucx7.march7th.database.models.BirthdayInformation;
import me.luucx7.march7th.settings.Settings;
import me.luucx7.march7th.settings.model.DatabaseSettings;

import java.sql.Connection;
import java.sql.PreparedStatement;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.time.LocalDateTime;
import java.util.LinkedList;
import java.util.Optional;

public class DatabaseManager {

    @Getter
    private static final HikariDataSource dataSource;

    static {
        DatabaseSettings dbSettings = Settings.get().getDatabase();

        HikariConfig hikariConfig = new HikariConfig();
        hikariConfig.setJdbcUrl("jdbc:postgresql://" +  dbSettings.getHost() +":" + dbSettings.getPort() + "/" + dbSettings.getDatabase() + "?user=" + dbSettings.getUsername() + "&password=" + dbSettings.getPassword());

        dataSource = new HikariDataSource(hikariConfig);
    }

    public static boolean testConnection() {
        try (Connection conn = dataSource.getConnection()) {
            PreparedStatement stmt = conn.prepareStatement("SELECT NOW();");
            ResultSet rs = stmt.executeQuery();

            if (rs.next()) {
                LocalDateTime lct = rs.getTimestamp(1).toLocalDateTime();

                return true;
            } else {
                return false;
            }
        } catch (Exception e) {
            return false;
        }
    }

    public static Optional<BirthdayInformation> loadUserBirthday(long userId) throws SQLException {
        try (Connection connection = dataSource.getConnection()) {
            PreparedStatement stmt = connection.prepareStatement(QUERY_USER_BIRTHDAY);
            stmt.setLong(1, userId);

            ResultSet rs = stmt.executeQuery();
            if (rs.next()) {
                long user_id = rs.getLong("user_id");
                short day = rs.getShort("day");
                short month = rs.getShort("month");

                return Optional.of(new BirthdayInformation(user_id, day, month));
            } else return Optional.empty();
        }
    }

    public static LinkedList<Long> loadBirthdays(short day, short month) throws SQLException {
        LinkedList<Long> userIds = new LinkedList<>();

        try (Connection connection = dataSource.getConnection()) {
            PreparedStatement stmt = connection.prepareStatement(QUERY_BIRTHDAYS);
            stmt.setShort(1, day);
            stmt.setShort(2, month);

            ResultSet rs = stmt.executeQuery();
            while(rs.next()) {
                long user_id = rs.getLong("user_id");
                userIds.add(user_id);
            }
        }

        return userIds;
    }

    public static int setUserBirthday(long userId, short day, short month) throws SQLException {
        try (Connection connection = dataSource.getConnection())  {
            PreparedStatement stmt = connection.prepareStatement(SET_USER_BIRTHDAY);
            stmt.setLong(1, userId);
            stmt.setShort(2, day);
            stmt.setShort(3, month);

            return stmt.executeUpdate();
        }
    }

    private static final String QUERY_USER_BIRTHDAY = "select user_id, day, month from birthdays b where b.user_id = ?;";
    private static final String SET_USER_BIRTHDAY = "insert into birthdays(user_id, day, month) values(?, ?, ?) on conflict (user_id) do update set day = EXCLUDED.day, month = EXCLUDED.month;";
    private static final String QUERY_BIRTHDAYS = "select user_id from birthdays where day = ? and month = ?;";
}
