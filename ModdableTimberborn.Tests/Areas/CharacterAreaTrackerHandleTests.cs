namespace ModdableTimberborn.Tests.Areas;

public class CharacterAreaTrackerHandleTests
{
    [Fact]
    public void InitializeWithCorrectCharacterType()
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
    public void IgnoreWrongCharacterType()
    {
        var handle = CreateHandle(CharacterType.Beavers);
        var bot = AreaTestFactory.CreateCharacter(CharacterType.Bot, new Vector3Int(12, 12, 0));

        handle.Initialize([bot]);

        Assert.Empty(handle.Entities);
    }

    [Fact]
    public void IgnoreOutsideArea()
    {
        var handle = CreateHandle(CharacterType.All);
        var adult = AreaTestFactory.CreateCharacter(CharacterType.AdultBeaver, new Vector3Int(30, 30, 0));

        handle.Initialize([adult]);

        Assert.Empty(handle.Entities);
    }

    [Fact]
    public void ComputeSegments()
    {
        var handle = CreateHandle(CharacterType.All, new BoundsInt(20, 20, 0, 20, 20, 1));

        Assert.Equal([0, 1, 10, 11], handle.Segments.Order());
    }

    [Fact]
    public void MatchSecondArea()
    {
        var handle = CreateHandle(
            CharacterType.All,
            new BoundsInt(10, 10, 0, 10, 10, 1),
            new BoundsInt(40, 40, 0, 10, 10, 1));
        var adult = AreaTestFactory.CreateCharacter(CharacterType.AdultBeaver, new Vector3Int(45, 45, 0));

        handle.Initialize([adult]);

        Assert.Contains(adult, handle.Entities);
    }

    [Fact]
    public void EnterOnUpdate()
    {
        var handle = CreateHandle(CharacterType.All);
        var adult = AreaTestFactory.CreateCharacter(CharacterType.AdultBeaver, new Vector3Int(12, 12, 0));
        var entered = 0;
        handle.OnEntityEntered += (_, entity) =>
        {
            Assert.Same(adult, entity);
            entered++;
        };

        handle.OnEntityUpdated(adult);

        Assert.Equal(1, entered);
        Assert.Contains(adult, handle.Entities);
    }

    [Fact]
    public void ExitOnUpdate()
    {
        var handle = CreateHandle(CharacterType.All);
        var adult = AreaTestFactory.CreateCharacter(CharacterType.AdultBeaver, new Vector3Int(12, 12, 0));
        handle.Initialize([adult]);
        var exited = 0;
        handle.OnEntityExited += (_, entity) =>
        {
            Assert.Same(adult, entity);
            exited++;
        };

        AreaTestFactory.SetCell(adult, new Vector3Int(30, 30, 0));
        handle.OnEntityUpdated(adult);

        Assert.Equal(1, exited);
        Assert.DoesNotContain(adult, handle.Entities);
    }

    [Fact]
    public void RemoveExistingEntity()
    {
        var handle = CreateHandle(CharacterType.All);
        var adult = AreaTestFactory.CreateCharacter(CharacterType.AdultBeaver, new Vector3Int(12, 12, 0));
        handle.Initialize([adult]);

        handle.OnEntityRemoved(adult);

        Assert.Empty(handle.Entities);
    }

    [Fact]
    public void DoNotDuplicateEntity()
    {
        var handle = CreateHandle(CharacterType.All);
        var adult = AreaTestFactory.CreateCharacter(CharacterType.AdultBeaver, new Vector3Int(12, 12, 0));

        handle.OnEntityUpdated(adult);
        handle.OnEntityUpdated(adult);

        Assert.Single(handle.Entities);
    }

    [Fact]
    public void RejectEmptyAreas()
    {
        var registration = new CharacterAreaTrackerRegistration
        {
            Areas = [],
        };

        Assert.Throws<ArgumentException>(() => new CharacterAreaTrackerHandle(registration, AreaTestFactory.CreateSegmentService()));
    }

    [Fact]
    public void RejectUnknownCharacterType()
    {
        var registration = new CharacterAreaTrackerRegistration
        {
            CharacterTypes = CharacterType.Unknown,
            Areas = [new BoundsInt(10, 10, 0, 10, 10, 1)],
        };

        Assert.Throws<ArgumentException>(() => new CharacterAreaTrackerHandle(registration, AreaTestFactory.CreateSegmentService()));
    }

    static CharacterAreaTrackerHandle CreateHandle(
        CharacterType characterTypes,
        params BoundsInt[] areas)
    {
        var registration = new CharacterAreaTrackerRegistration
        {
            CharacterTypes = characterTypes,
            Areas = areas.Length == 0
                ? [new BoundsInt(10, 10, 0, 10, 10, 1)]
                : areas,
        };

        return new CharacterAreaTrackerHandle(registration, AreaTestFactory.CreateSegmentService());
    }
}
