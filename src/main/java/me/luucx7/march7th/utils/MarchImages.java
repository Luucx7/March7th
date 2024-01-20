package me.luucx7.march7th.utils;

import lombok.AllArgsConstructor;
import lombok.Getter;

@Getter
@AllArgsConstructor
public enum MarchImages {
    BIG_HEART_ART("https://raw.githubusercontent.com/Luucx7/temp-repo/main/march-heart-art.jpg"),
    CHRISTMAS_EASTER_EGG("https://raw.githubusercontent.com/Luucx7/temp-repo/main/easter-egg.jpg"),
    NEW_YEAR_EASTER_EGG("https://assets-au-scc.kc-usercontent.com/330b87ea-148b-3ecf-9857-698f2086fe8d/2d6c856b-da1e-45ba-9e0e-7b0eb46a79b8/NYE%20Fireworks.jpg?w=600&fm=jpg"),
    MARCH_ANGRY("https://raw.githubusercontent.com/Luucx7/temp-repo/main/march-angry.png"),
    MARCH_BIRTHDAY("https://raw.githubusercontent.com/Luucx7/temp-repo/main/march-birthday.png"),
    MARCH_CRYING("https://raw.githubusercontent.com/Luucx7/temp-repo/main/march-crying.png"),
    MARCH_POINT("https://raw.githubusercontent.com/Luucx7/temp-repo/main/march-point.png"),
    MARCH_PROUD("https://raw.githubusercontent.com/Luucx7/temp-repo/main/march-proud.png"),
    MARCH_SLEEPING("https://raw.githubusercontent.com/Luucx7/temp-repo/main/march-sleeping.png"),
    MARCH_SORRY("https://raw.githubusercontent.com/Luucx7/temp-repo/main/march-sorry.png"),
    MARCH_THUMBSUP("https://raw.githubusercontent.com/Luucx7/temp-repo/main/march-thumbsup.png")
    ;

    private final String resourceName;

    @Override
    public String toString() {
        return this.resourceName;
    }

    public String getUrl() {
        return this.resourceName;
    }
}
