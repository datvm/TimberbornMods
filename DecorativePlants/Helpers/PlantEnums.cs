namespace DecorativePlants.Helpers;

public static class PlantEnums
{
    public static string T(this PlantMatureState e, ILoc t) => t.T("LV.DP.PlantMature" + e.ToString());
    public static string T(this PlantWellnessState e, ILoc t) => t.T("LV.DP.PlantWellness" + e.ToString());
    public static string TMature(this int e, ILoc t) => ((PlantMatureState)e).T(t);
    public static string TWellness(this int e, ILoc t) => ((PlantWellnessState)e).T(t);
}

public enum PlantMatureState
{
    Seedling,
    Mature,
}

public enum PlantWellnessState
{
    Alive,
    Dying,
    Dead
}