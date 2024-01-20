package me.luucx7.march7th.settings.model;

import lombok.Getter;
import lombok.Setter;

@Getter
@Setter
public class DatabaseSettings {
    private String username;
    private String password;
    private String host;
    private int port;
    private String database;
}
