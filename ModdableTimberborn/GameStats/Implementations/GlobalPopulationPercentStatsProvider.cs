namespace ModdableTimberborn.GameStats.Implementations;

public class GlobalPopulationPercentStatsProvider(PopulationService populationService) : IPercentGameStatProvider
{
    public IEnumerable<string> AvailableStats => [
        "BeaverPercent",
        "BotPercent",
        "AdultPercent",
        "ChildPercent",
        "HomelessPercent",
        "AdultContaminatedPercent",
        "ChildContaminatedPercent",
        "ContaminatedPercent",
    ];

    public float GetStat(string statId)
    {
        var data  = populationService.GlobalPopulationData;

        return statId switch
        {
            "BeaverPercent" => data.NumberOfBeavers.PercentOf(data.TotalPopulation),
            "BotPercent" => data.NumberOfBots.PercentOf(data.TotalPopulation),
            "AdultPercent" => data.NumberOfAdults.PercentOf(data.NumberOfBeavers),
            "ChildPercent" => data.NumberOfChildren.PercentOf(data.NumberOfBeavers),
            "HomelessPercent" => data.BedData.Homeless.PercentOf(data.NumberOfBeavers),
            "AdultContaminatedPercent" => data.ContaminationData.ContaminatedAdults.PercentOf(data.NumberOfBeavers),
            "ChildContaminatedPercent" => data.ContaminationData.ContaminatedChildren.PercentOf(data.NumberOfBeavers),
            "ContaminatedPercent" => data.ContaminationData.ContaminatedTotal.PercentOf(data.NumberOfBeavers),
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    

}
