namespace ModdableTimberborn.GameStats.Implementations;

public class GlobalPopulationPercentStatsProvider(PopulationService populationService) : IPercentGameStatProvider
{
    public IEnumerable<string> AvailableStats => [
        GameStats.PopulationBeaverPercent,
        GameStats.PopulationBotPercent,
        GameStats.PopulationAdultPercent,
        GameStats.PopulationChildPercent,
        GameStats.BedHomelessPercent,
        GameStats.ContaminationAdultPercent,
        GameStats.ContaminationChildPercent,
        GameStats.ContaminationPercent,
    ];

    public float GetStat(string statId)
    {
        var data  = populationService.GlobalPopulationData;

        return statId switch
        {
            GameStats.PopulationBeaverPercent => data.NumberOfBeavers.PercentOf(data.TotalPopulation),
            GameStats.PopulationBotPercent => data.NumberOfBots.PercentOf(data.TotalPopulation),
            GameStats.PopulationAdultPercent => data.NumberOfAdults.PercentOf(data.NumberOfBeavers),
            GameStats.PopulationChildPercent => data.NumberOfChildren.PercentOf(data.NumberOfBeavers),
            GameStats.BedHomelessPercent => data.BedData.Homeless.PercentOf(data.NumberOfBeavers),
            GameStats.ContaminationAdultPercent => data.ContaminationData.ContaminatedAdults.PercentOf(data.NumberOfBeavers),
            GameStats.ContaminationChildPercent => data.ContaminationData.ContaminatedChildren.PercentOf(data.NumberOfBeavers),
            GameStats.ContaminationPercent => data.ContaminationData.ContaminatedTotal.PercentOf(data.NumberOfBeavers),
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    

}
