namespace Timerborn.HazardousWeatherSystemUI;

internal static class ModExtensions
{

    public static bool IsPreWarning(this HazardousWeatherApproachingTimer timer, bool ignoreFirstDay) => 
        (ignoreFirstDay && timer._gameCycleService.CycleDay <= 1) ||
        timer.DaysToHazardousWeather > HazardousWeatherApproachingTimer.ApproachingNotificationDays;

}
