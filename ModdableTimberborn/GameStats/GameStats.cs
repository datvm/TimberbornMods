namespace ModdableTimberborn.GameStats;

public static class GameStats
{
    public const string TimePartialDay = "GameDayHours";
    public const string TimePartialCycleDay = "GameCycleDayHours";
    public const string TimeTodayHours = "GameTodayHours24";
    public const string TimeDayNumber = "GameDayNumber";
    public const string TimeCycle = "GameCycle";
    public const string TimeCycleDay = "GameCycleDay";
    public const string TimeCycleDuration = "GameCycleDuration";
    public const string TimeDayProgress = "GameDayProgress";
    public const string TimeCycleProgress = "GameCycleProgress";

    public const string GoodAmountPrefix = "GoodAmount.";
    public const string GoodCapacityPrefix = "GoodCapacity.";
    public const string GoodFillPrefix = "GoodFill.";

    public const string PopulationNumberOfAdult = nameof(PopulationData.NumberOfAdults);
    public const string PopulationNumberOfChild = nameof(PopulationData.NumberOfChildren);
    public const string PopulationNumberOfBot = nameof(PopulationData.NumberOfBots);
    public const string PopulationNumberOfBeaver = nameof(PopulationData.NumberOfBeavers);
    public const string PopulationNumberOfHealthyAdult = nameof(PopulationData.NumberOfHealthyAdults);
    public const string PopulationNumberOfHealthyChild = nameof(PopulationData.NumberOfHealthyChildren);
    public const string PopulationTotal = nameof(PopulationData.TotalPopulation);
    public const string PopulationBeaverPercent = "BeaverPercent";
    public const string PopulationBotPercent = "BotPercent";
    public const string PopulationAdultPercent = "AdultPercent";
    public const string PopulationChildPercent = "ChildPercent";

    public const string BedOccupied = nameof(BedData.OccupiedBeds);
    public const string BedFree = nameof(BedData.FreeBeds);
    public const string BedHomeless = nameof(BedData.Homeless);
    public const string BedTotal = "TotalBeds";
    public const string BedHomelessPercent = "HomelessPercent";

    public const string WorkforceEmployable = nameof(WorkforceData.Employable);
    public const string WorkforceUnemployable = nameof(WorkforceData.Unemployable);
    public const string WorkforceTotal = nameof(WorkforceData.Total);
    public const string WorkforceBeaverPrefix = nameof(CharacterType.Beavers);
    public const string WorkforceBotPrefix = nameof(CharacterType.Bot);
    public const string WorkforceBeaverEmployable = WorkforceBeaverPrefix + WorkforceEmployable;
    public const string WorkforceBeaverUnemployable = WorkforceBeaverPrefix + WorkforceUnemployable;
    public const string WorkforceBeaverTotal = WorkforceBeaverPrefix + WorkforceTotal;
    public const string WorkforceBotEmployable = WorkforceBotPrefix + WorkforceEmployable;
    public const string WorkforceBotUnemployable = WorkforceBotPrefix + WorkforceUnemployable;
    public const string WorkforceBotTotal = WorkforceBotPrefix + WorkforceTotal;

    public const string ContaminationAdult = nameof(ContaminationData.ContaminatedAdults);
    public const string ContaminationChild = nameof(ContaminationData.ContaminatedChildren);
    public const string ContaminationTotal = nameof(ContaminationData.ContaminatedTotal);
    public const string ContaminationAdultPercent = "AdultContaminatedPercent";
    public const string ContaminationChildPercent = "ChildContaminatedPercent";
    public const string ContaminationPercent = "ContaminatedPercent";

    public static string GoodAmount(string goodId) => GoodAmountPrefix + goodId;
    public static string GoodCapacity(string goodId) => GoodCapacityPrefix + goodId;
    public static string GoodFill(string goodId) => GoodFillPrefix + goodId;
}
