namespace ConveyorBelt.Services;

public readonly struct ConveyorBeltCluster
{
    public readonly ImmutableArray<ConveyorBeltComponent> Belts;
    public readonly bool IsLift;
    readonly FrozenSet<ConveyorBeltComponent> beltSet;

    public ConveyorBeltCluster(IEnumerable<ConveyorBeltComponent> belts)
    {
        Belts = [..belts];
        if (Belts.Length == 0)
        {
            throw new ArgumentException("ConveyorBeltCluster must contain at least one belt.");
        }

        beltSet = [.. belts];
        
        var isLift = IsLift = Belts[0].IsLift;
        if (Belts.FastAny(b => b.IsLift != isLift))
        {
            throw new ArgumentException("All belts in a ConveyorBeltCluster must be of the same type (either all lifts or not).");
        }
    }

    public bool Contain(ConveyorBeltComponent comp) => beltSet.Contains(comp);
    public ConveyorBeltComponent Start => Belts[0];
    public Vector3Int InputCoordinates => Start.Connection.PreviousCoords;
    public ConveyorBeltComponent End => Belts[^1];
    public Vector3Int OutputCoordinates => End.Connection.NextCoords;
    
    public Stockpile? Source => Start.Connection.Source;
    public Stockpile? Destination => End.Connection.Destination;

    public override bool Equals(object obj) => obj is ConveyorBeltCluster other && Belts[0] == other.Belts[0];
    public override int GetHashCode() => Belts[0].GetHashCode();

}

public class ConveyorBeltConnection
{
    public Vector3Int Coordinates { get; init; }
    public Vector3Int PreviousCoords { get; init; }
    public Vector3Int NextCoords { get; init; }

    public ConveyorBeltComponent? PreviousBelt { get; set; }
    public ConveyorBeltComponent? NextBelt { get; set; }
    public Stockpile? Source { get; set; }
    public Stockpile? Destination { get; set; }
}