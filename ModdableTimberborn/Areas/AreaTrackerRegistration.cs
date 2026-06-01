namespace ModdableTimberborn.Areas;

public class AreaTrackerRegistration
{
    public BoundsInt[] Areas { get; set; } = [];
}

public class CharacterAreaTrackerRegistration : AreaTrackerRegistration
{
    public CharacterType CharacterTypes { get; set; } = CharacterType.All;
}

public class BlockObjectAreaTrackerRegistration : AreaTrackerRegistration
{
    public IReadOnlyCollection<string> BlockObjectTemplateNames { get; set; } = [];
    public Func<BlockObject, bool>? BlockObjectFilter { get; set; } 
    public BlockObjectAreaType AreaType { get; set; } = BlockObjectAreaType.Intersection;
    public bool FinishedBlockObjectOnly { get; set; } = true;
    
}

public enum BlockObjectAreaType
{
    Intersection,
    Containment,
}
