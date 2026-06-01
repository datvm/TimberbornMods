namespace ModdableTimberborn.Tests.Areas;

public class CharacterAreaTrackerHandleTests
{
    [Fact]
    public void Initialize_WhenCharacterTypeIsIncludedAndCellIsInsideArea_AddsEntity()
    {
        var handle = CreateHandle(CharacterType.Beavers);
        var adult = AreaTestFactory.CreateCharacter(CharacterType.AdultBeaver, new Vector3Int(12, 12, 0));
        var child = AreaTestFactory.CreateCharacter(CharacterType.ChildBeaver, new Vector3Int(14, 14, 0));

        handle.Initialize([adult, child]);

        Assert.Equal(2, handle.Count);
        Assert.Contains(adult, handle.Entities);
        Assert.Contains(child, handle.Entities);
    }

    [Fact]
    public void Initialize_WhenCharacterTypeIsNotIncluded_DoesNotAddEntity()
    {
        var handle = CreateHandle(CharacterType.Beavers);
        var bot = AreaTestFactory.CreateCharacter(CharacterType.Bot, new Vector3Int(12, 12, 0));

        handle.Initialize([bot]);

        Assert.Empty(handle.Entities);
    }

    [Fact]
    public void Initialize_WhenCellIsOutsideArea_DoesNotAddEntity()
    {
        var handle = CreateHandle(CharacterType.All);
        var adult = AreaTestFactory.CreateCharacter(CharacterType.AdultBeaver, new Vector3Int(30, 30, 0));

        handle.Initialize([adult]);

        Assert.Empty(handle.Entities);
    }

    [Fact]
    public void Constructor_ComputesSegmentsFromAreas()
    {
        var handle = CreateHandle(CharacterType.All, new BoundsInt(20, 20, 0, 20, 20, 1));

        Assert.Equal([0, 1, 10, 11], handle.Segments.Order());
    }

    static CharacterAreaTrackerHandle CreateHandle(
        CharacterType characterTypes,
        BoundsInt? area = null)
    {
        var registration = new CharacterAreaTrackerRegistration
        {
            CharacterTypes = characterTypes,
            Areas = [area ?? new BoundsInt(10, 10, 0, 10, 10, 1)],
        };

        return new CharacterAreaTrackerHandle(registration, AreaTestFactory.CreateSegmentService());
    }
}
