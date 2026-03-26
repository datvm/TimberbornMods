namespace MoreHttpApi.Shared;

public record HttpCommonData(
    HttpTopBar TopBar
);

public record HttpTopBar(
    HttpCycle Cycle,
    HttpCurrentWeather Weather,
    HttpGameSpeed Speed
);