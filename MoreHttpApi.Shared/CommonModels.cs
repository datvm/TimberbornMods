namespace MoreHttpApi.Shared;

public record HttpCommonData(
    HttpTopBar TopBar
);

public record HttpTopBar(
    HttpCycle Cycle,
    HttpCurrentWeather Weather,
    HttpGameSpeed Speed
);

public readonly record struct HttpColor(int R, int G, int B, int A = 255)
{

    public HttpColor(float r, float g, float b, float a = 1f)
        : this(
            (int)(r * 255),
            (int)(g * 255),
            (int)(b * 255),
            (int)(a * 255))
    { }

}