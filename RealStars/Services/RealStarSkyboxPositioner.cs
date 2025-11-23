namespace RealStars.Services;

public class RealStarSkyboxPositioner(SkyboxPositioner skyboxPositioner) : IUpdatableSingleton
{


    public void UpdateSingleton()
    {
        var transition = skyboxPositioner._dayStageCycle.GetCurrentTransition();

        var isNight = transition.CurrentDayStage == DayStage.Night;
        

        if (transition.CurrentDayStage == DayStage.Night)
        {

        }
    }

}
