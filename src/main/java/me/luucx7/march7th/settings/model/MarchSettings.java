package me.luucx7.march7th.settings.model;

import lombok.Getter;
import lombok.Setter;

import java.time.ZoneId;

@Getter
@Setter
public class MarchSettings {
    private DatabaseSettings database;
    private DiscordSettings discord;
    private String zoneId;

    // different name to the parser don't cry
    public ZoneId convertedZoneId() {
        return ZoneId.of(this.zoneId);
    }
}
