namespace ModdableTimberborn.GameStats;

public static class GameStats
{
    public const string TimePartialDay = nameof(TimePartialDay);
    public const string TimePartialCycleDay = nameof(TimePartialCycleDay);
    public const string TimeTodayHours = nameof(TimeTodayHours);
    public const string TimeDayNumber = nameof(TimeDayNumber);
    public const string TimeCycle = nameof(TimeCycle);
    public const string TimeCycleDay = nameof(TimeCycleDay);
    public const string TimeCycleDuration = nameof(TimeCycleDuration);
    public const string TimeDayProgress = nameof(TimeDayProgress);
    public const string TimeCycleProgress = nameof(TimeCycleProgress);

    public const string GoodAmountPrefix = "GoodAmount.";
    public const string GoodCapacityPrefix = "GoodCapacity.";
    public const string GoodFillPrefix = "GoodFill.";

    public const string PopulationNumberOfAdult = nameof(PopulationNumberOfAdult);
    public const string PopulationNumberOfChild = nameof(PopulationNumberOfChild);
    public const string PopulationNumberOfBot = nameof(PopulationNumberOfBot);
    public const string PopulationNumberOfBeaver = nameof(PopulationNumberOfBeaver);
    public const string PopulationNumberOfHealthyAdult = nameof(PopulationNumberOfHealthyAdult);
    public const string PopulationNumberOfHealthyChild = nameof(PopulationNumberOfHealthyChild);
    public const string PopulationTotal = nameof(PopulationTotal);
    public const string PopulationBeaverPercent = nameof(PopulationBeaverPercent);
    public const string PopulationBotPercent = nameof(PopulationBotPercent);
    public const string PopulationAdultPercent = nameof(PopulationAdultPercent);
    public const string PopulationChildPercent = nameof(PopulationChildPercent);

    public const string BedOccupied = nameof(BedOccupied);
    public const string BedFree = nameof(BedFree);
    public const string BedHomeless = nameof(BedHomeless);
    public const string BedTotal = nameof(BedTotal);
    public const string BedHomelessPercent = nameof(BedHomelessPercent);
    public const string BedHousedPercent = nameof(BedHousedPercent);

    public const string WorkforceEmployable = nameof(WorkforceEmployable);
    public const string WorkforceUnemployable = nameof(WorkforceUnemployable);
    public const string WorkforceTotal = nameof(WorkforceTotal);
    public const string WorkforceBeaverEmployable = nameof(WorkforceBeaverEmployable);
    public const string WorkforceBeaverUnemployable = nameof(WorkforceBeaverUnemployable);
    public const string WorkforceBeaverTotal = nameof(WorkforceBeaverTotal);
    public const string WorkforceBotEmployable = nameof(WorkforceBotEmployable);
    public const string WorkforceBotUnemployable = nameof(WorkforceBotUnemployable);
    public const string WorkforceBotTotal = nameof(WorkforceBotTotal);

    public const string ContaminationAdult = nameof(ContaminationAdult);
    public const string ContaminationChild = nameof(ContaminationChild);
    public const string ContaminationTotal = nameof(ContaminationTotal);
    public const string ContaminationAdultPercent = nameof(ContaminationAdultPercent);
    public const string ContaminationChildPercent = nameof(ContaminationChildPercent);
    public const string ContaminationPercent = nameof(ContaminationPercent);

    public static string GoodAmount(string goodId) => GoodAmountPrefix + goodId;
    public static string GoodCapacity(string goodId) => GoodCapacityPrefix + goodId;
    public static string GoodFill(string goodId) => GoodFillPrefix + goodId;
}
