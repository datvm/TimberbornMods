namespace ModdableTimberborn.GameStats.Implementations;

public class GlobalPopulationStatsProvider(PopulationService populationService) : IIntGameStatProvider
{
    static readonly FrozenSet<string> PopulationStats = ImmutableHelper.CreateFrozenSet([
        GameStats.PopulationNumberOfAdult,
        GameStats.PopulationNumberOfChild,
        GameStats.PopulationNumberOfBot,
        GameStats.PopulationNumberOfBeaver,
        GameStats.PopulationNumberOfHealthyAdult,
        GameStats.PopulationNumberOfHealthyChild,
        GameStats.PopulationTotal,
    ]);

    static readonly FrozenSet<string> BedStats = ImmutableHelper.CreateFrozenSet([
        GameStats.BedOccupied,
        GameStats.BedFree,
        GameStats.BedHomeless,
        GameStats.BedTotal,
    ]);

    static readonly FrozenSet<string> WorkforceStats = ImmutableHelper.CreateFrozenSet([
        GameStats.WorkforceEmployable,
        GameStats.WorkforceUnemployable,
        GameStats.WorkforceTotal,
    ]);

    static readonly FrozenSet<string> BeaverWorkforceStats = ImmutableHelper.CreateFrozenSet([
        GameStats.WorkforceBeaverEmployable,
        GameStats.WorkforceBeaverUnemployable,
        GameStats.WorkforceBeaverTotal,
    ]);

    static readonly FrozenSet<string> BotWorkforceStats = ImmutableHelper.CreateFrozenSet([
        GameStats.WorkforceBotEmployable,
        GameStats.WorkforceBotUnemployable,
        GameStats.WorkforceBotTotal,
    ]);

    static readonly FrozenSet<string> ContaminationStats = ImmutableHelper.CreateFrozenSet([
        GameStats.ContaminationAdult,
        GameStats.ContaminationChild,
        GameStats.ContaminationTotal,
    ]);

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
                GameStats.PopulationNumberOfAdult => data.NumberOfAdults,
                GameStats.PopulationNumberOfChild => data.NumberOfChildren,
                GameStats.PopulationNumberOfBot => data.NumberOfBots,
                GameStats.PopulationNumberOfBeaver => data.NumberOfBeavers,
                GameStats.PopulationNumberOfHealthyAdult => data.NumberOfHealthyAdults,
                GameStats.PopulationNumberOfHealthyChild => data.NumberOfHealthyChildren,
                GameStats.PopulationTotal => data.TotalPopulation,
                _ => throw new ArgumentOutOfRangeException(),
            };
        }

        if (BedStats.Contains(statId))
        {
            var bedData = data.BedData;
            return statId switch
            {
                GameStats.BedOccupied => bedData.OccupiedBeds,
                GameStats.BedFree => bedData.FreeBeds,
                GameStats.BedHomeless => bedData.Homeless,
                GameStats.BedTotal => bedData.OccupiedBeds + bedData.FreeBeds,
                _ => throw new ArgumentOutOfRangeException(),
            };
        }

        if (BeaverWorkforceStats.Contains(statId))
        {
            return statId switch
            {
                GameStats.WorkforceBeaverEmployable => data.BeaverWorkforceData.Employable,
                GameStats.WorkforceBeaverUnemployable => data.BeaverWorkforceData.Unemployable,
                GameStats.WorkforceBeaverTotal => data.BeaverWorkforceData.Total,
                _ => throw new ArgumentOutOfRangeException(),
            };
        }

        if (BotWorkforceStats.Contains(statId))
        {
            return statId switch
            {
                GameStats.WorkforceBotEmployable => data.BotWorkforceData.Employable,
                GameStats.WorkforceBotUnemployable => data.BotWorkforceData.Unemployable,
                GameStats.WorkforceBotTotal => data.BotWorkforceData.Total,
                _ => throw new ArgumentOutOfRangeException(),
            };
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
                GameStats.ContaminationAdult => contaminationData.ContaminatedAdults,
                GameStats.ContaminationChild => contaminationData.ContaminatedChildren,
                GameStats.ContaminationTotal => contaminationData.ContaminatedTotal,
                _ => throw new ArgumentOutOfRangeException(),
            };
        }

        throw new ArgumentOutOfRangeException(nameof(statId), $"Stat {statId} is not supported.");
    }

    static int GetWorkforceData(WorkforceData data, string id) => id switch
    {
        GameStats.WorkforceEmployable => data.Employable,
        GameStats.WorkforceUnemployable => data.Unemployable,
        GameStats.WorkforceTotal => data.Total,
        _ => throw new ArgumentOutOfRangeException(),
    };
}
