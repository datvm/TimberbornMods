namespace Timberborn.Population;

public static class PopulationDataExtensions
{

    extension(PopulationCounterMode mode)
    {

        public bool UseCountParameters() => mode
            is PopulationCounterMode.Jobs
            or PopulationCounterMode.Employed
            or PopulationCounterMode.Unemployed
            or PopulationCounterMode.Vacancies
            or PopulationCounterMode.TotalWorkers
            or PopulationCounterMode.HealthyWorkers
            or PopulationCounterMode.UnhealthyWorkers;

    }

    extension(PopulationData data)
    {

        public int GetData(PopulationCounterMode mode) => data.GetData(mode, true, true);

        public int GetData(PopulationCounterMode mode, bool countBeavers, bool countBots) => mode switch
        {
            PopulationCounterMode.TotalPopulation => data.TotalPopulation,
            PopulationCounterMode.TotalBeavers => data.NumberOfBeavers,
            PopulationCounterMode.Adults => data.NumberOfAdults,
            PopulationCounterMode.Children => data.NumberOfChildren,
            PopulationCounterMode.Bots => data.NumberOfBots,
            PopulationCounterMode.OccupiedBeds => data.BedData.OccupiedBeds,
            PopulationCounterMode.FreeBeds => data.BedData.FreeBeds,
            PopulationCounterMode.Homeless => data.BedData.Homeless,
            PopulationCounterMode.Jobs => (countBeavers ? data.BeaverWorkplaceData.TotalWorkslots : 0) + (countBots ? data.BotWorkplaceData.TotalWorkslots : 0),
            PopulationCounterMode.Employed => (countBeavers ? data.BeaverWorkplaceData.OccupiedWorkslots : 0) + (countBots ? data.BotWorkplaceData.OccupiedWorkslots : 0),
            PopulationCounterMode.Unemployed => (countBeavers ? data.BeaverWorkplaceData.Unemployed : 0) + (countBots ? data.BotWorkplaceData.Unemployed : 0),
            PopulationCounterMode.Vacancies => (countBeavers ? data.BeaverWorkplaceData.FreeWorkslots : 0) + (countBots ? data.BotWorkplaceData.FreeWorkslots : 0),
            PopulationCounterMode.TotalWorkers => (countBeavers ? data.BeaverWorkforceData.Total : 0) + (countBots ? data.BotWorkforceData.Total : 0),
            PopulationCounterMode.HealthyWorkers => (countBeavers ? data.BeaverWorkforceData.Employable : 0) + (countBots ? data.BotWorkforceData.Employable : 0),
            PopulationCounterMode.UnhealthyWorkers => (countBeavers ? data.BeaverWorkforceData.Unemployable : 0) + (countBots ? data.BotWorkforceData.Unemployable : 0),
            PopulationCounterMode.ContaminatedTotal => data.ContaminationData.ContaminatedTotal,
            PopulationCounterMode.ContaminatedAdults => data.ContaminationData.ContaminatedAdults,
            PopulationCounterMode.ContaminatedChildren => data.ContaminationData.ContaminatedChildren,
            _ => throw new ArgumentOutOfRangeException(),
        };

    }

}
