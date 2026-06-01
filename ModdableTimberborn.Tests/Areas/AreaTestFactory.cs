using System.Reflection;

namespace ModdableTimberborn.Tests.Areas;

static class AreaTestFactory
{
    public static AreaSegmentService CreateSegmentService(int x = 255, int y = 255, int z = 1)
    {
        var size = new Vector3Int(x, y, z);
        var service = new AreaSegmentService(new MapSize(null, null)
        {
            TerrainSize = size,
            TotalSize = size,
        });

        service.Load();
        return service;
    }

    public static CharacterPositionTracker CreateCharacter(
        CharacterType characterType,
        Vector3Int cell)
    {
        var tracker = new CharacterPositionTracker(CreateSegmentService());
        SetProperty(tracker, nameof(CharacterPositionTracker.CharacterType), characterType);
        SetCell(tracker, cell);
        return tracker;
    }

    public static void SetCell(CharacterPositionTracker tracker, Vector3Int cell)
    {
        SetProperty(tracker, nameof(CharacterPositionTracker.Cell), cell);
    }

    public static void SetCharacterType(CharacterPositionTracker tracker, CharacterType characterType)
    {
        SetProperty(tracker, nameof(CharacterPositionTracker.CharacterType), characterType);
    }

    static void SetProperty<T>(object target, string propertyName, T value)
    {
        var property = target.GetType().GetProperty(
            propertyName,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        property?.SetValue(target, value);
    }
}
