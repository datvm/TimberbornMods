namespace ModdableTimberborn.GameStats.Implementations;

public class MapStatsProvider(MapNameService mapNameService) : IStringGameStatProvider
{
    public IEnumerable<string> AvailableStats => [GameStats.MapName];

    public string? GetStat(string statId) => statId switch
    {
        GameStats.MapName => mapNameService.HasMapName ? mapNameService.Name : null,
        _ => throw new ArgumentOutOfRangeException(),
    };

}

public class IntMapStatsProvider(MapSize mapSize) : IIntGameStatProvider
{
    public IEnumerable<string> AvailableStats => [
        GameStats.MapSizeX,
        GameStats.MapSizeY,
        GameStats.MapSizeTerrainZ,
        GameStats.MapSizeTotalZ,
    ];

    public int GetStat(string statId) => statId switch
    {
        GameStats.MapSizeX => mapSize.TotalSize.x,
        GameStats.MapSizeY => mapSize.TotalSize.y,
        GameStats.MapSizeTerrainZ => mapSize.TerrainSize.z,
        GameStats.MapSizeTotalZ => mapSize.TotalSize.z,
        _ => throw new ArgumentOutOfRangeException(),
    };

}
