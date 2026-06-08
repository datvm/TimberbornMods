namespace ModdableTimberborn.GameStats.Implementations;

public class GlobalPopulationStatsProvider(PopulationService populationService) : IIntGameStatProvider
{
    static readonly FrozenSet<string> PopulationStats = [
        nameof(PopulationData.NumberOfAdults),
        nameof(PopulationData.NumberOfChildren),
        nameof(PopulationData.NumberOfBots),
        nameof(PopulationData.NumberOfBeavers),
        nameof(PopulationData.NumberOfHealthyAdults),
        nameof(PopulationData.NumberOfHealthyChildren),
        nameof(PopulationData.TotalPopulation),
    ];

    static readonly FrozenSet<string> BedStats = [
        nameof(BedData.OccupiedBeds),
        nameof(BedData.FreeBeds),
        nameof(BedData.Homeless),
        "TotalBeds",
    ];

    static readonly FrozenSet<string> WorkforceStats = [
        nameof(WorkforceData.Employable),
        nameof(WorkforceData.Unemployable),
        nameof(WorkforceData.Total),
    ];

    static readonly FrozenSet<string> BeaverWorkforceStats = [..WorkforceStats.Select(s => nameof(CharacterType.Beavers) + s)];
    static readonly FrozenSet<string> BotWorkforceStats = [..WorkforceStats.Select(s => nameof(CharacterType.Bot) + s)];

    static readonly FrozenSet<string> ContaminationStats = [
        nameof(ContaminationData.ContaminatedAdults),
        nameof(ContaminationData.ContaminatedChildren),
        nameof(ContaminationData.ContaminatedTotal),
    ];

    public IEnumerable<string> AvailableStats => [
        .. PopulationStats,
        .. BedStats,
        .. BeaverWorkforceStats, .. BotWorkforceStats, ..WorkforceStats,
        .. ContaminationStats,
    ];

    public int GetStat(string statId)
    {
        var data = populationService.GlobalPopulationData;

        if (PopulationStats.Contains(statId))
        {
            return statId switch
            {
                nameof(data.NumberOfAdults) => data.NumberOfAdults,
                nameof(data.NumberOfChildren) => data.NumberOfChildren,
                nameof(data.NumberOfBots) => data.NumberOfBots,
                nameof(data.NumberOfBeavers) => data.NumberOfBeavers,
                nameof(data.NumberOfHealthyAdults) => data.NumberOfHealthyAdults,
                nameof(data.NumberOfHealthyChildren) => data.NumberOfHealthyChildren,
                nameof(data.TotalPopulation) => data.TotalPopulation,
                _ => throw new ArgumentOutOfRangeException(),
            };
        }

        if (BedStats.Contains(statId))
        {
            var bedData = data.BedData;
            return statId switch
            {
                nameof(bedData.OccupiedBeds) => bedData.OccupiedBeds,
                nameof(bedData.FreeBeds) => bedData.FreeBeds,
                nameof(bedData.Homeless) => bedData.Homeless,
                "TotalBeds" => bedData.OccupiedBeds + bedData.FreeBeds,
                _ => throw new ArgumentOutOfRangeException(),
            };
        }

        if (BeaverWorkforceStats.Contains(statId))
        {
            return GetWorkforceData(data.BeaverWorkforceData, statId[nameof(CharacterType.Beavers).Length..]);
        }

        if (BotWorkforceStats.Contains(statId))
        {
            return GetWorkforceData(data.BotWorkforceData, statId[nameof(CharacterType.Bot).Length..]);
        }

        if (WorkforceStats.Contains(statId))
        {
            return GetWorkforceData(data.BeaverWorkforceData, statId) + GetWorkforceData(data.BotWorkforceData, statId);
        }

        if (ContaminationStats.Contains(statId))
        {
            var contaminationData = data.ContaminationData;
            return statId switch
            {
                nameof(contaminationData.ContaminatedAdults) => contaminationData.ContaminatedAdults,
                nameof(contaminationData.ContaminatedChildren) => contaminationData.ContaminatedChildren,
                nameof(contaminationData.ContaminatedTotal) => contaminationData.ContaminatedTotal,
                _ => throw new ArgumentOutOfRangeException(),
            };
        }

        throw new ArgumentOutOfRangeException(nameof(statId), $"Stat {statId} is not supported.");
    }

    static int GetWorkforceData(WorkforceData data, string id) => id switch
    {
        nameof(data.Employable) => data.Employable,
        nameof(data.Unemployable) => data.Unemployable,
        nameof(data.Total) => data.Total,
        _ => throw new ArgumentOutOfRangeException(),
    };
}
