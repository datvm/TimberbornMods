namespace ExtendedBuilderReach;

public static class ModUtils
{

    public static int GetMinZ(int z) => MSettings.UnlimitedAbove ? 0 : (z - MSettings.RangeAbove);
    public static int GetMaxZ(int z, int mapSizeZ) => MSettings.UnlimitedBelow ? (mapSizeZ - 1) : (z + MSettings.RangeBelow - 1);

}
