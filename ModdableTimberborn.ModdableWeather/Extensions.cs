namespace ModdableTimberborn.ModdableWeather;

public static class Extensions
{

    extension(DetailedWeatherStageReference d)
    {
        public CompatWeatherCycleStage ToCompatWeatherCycleStage()
        {
            return new(d.CycleIndex, d.StageIndex, d.CalculateStartDay(), d.Stage.Days, d.Weather.Id, d.Weather.IsBenign);
        }
    }

    extension(DetailedWeatherCycleStage s)
    {
        public CompatWeatherCycleStage ToCompatWeatherCycleStage(DetailedWeatherCycle cycle) 
            => new DetailedWeatherStageReference(cycle, s).ToCompatWeatherCycleStage();
    }

}
