package me.luucx7.march7th.database.models;

import lombok.AllArgsConstructor;
import lombok.Getter;
import lombok.NoArgsConstructor;
import lombok.Setter;

@Getter
@Setter
@NoArgsConstructor
@AllArgsConstructor
public class BirthdayInformation {
    private long user_id;
    private short day;
    private short month;
}
