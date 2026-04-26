namespace BeaverChronicles.Events;

public enum EventTriggerSource
{
    None,
    GameLoad,
    NewHour,
    NewDay,
    NewWeather,
    NewCycle,
    CharacterCreated,
    BeaverGrownUp,
    CharacterDeath,
    ToolUnlocked,
    BuildingUnlocked,
    BuildingPlaced,
    BuildingFinished,
    NewWellbeingHighscore,
}
