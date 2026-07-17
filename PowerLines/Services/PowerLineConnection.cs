namespace PowerLines.Services;

public readonly record struct PowerLineConnection(PowerLineComponent A, PowerLineComponent B, Guid GuidA, Guid GuidB)
{

    public PowerLineConnection(PowerLineComponent a, PowerLineComponent b)
        : this(a, b, a.EntityId, b.EntityId) { }

    public PowerLineConnection Normalize()
        => GuidA.CompareTo(GuidB) <= 0 ? this : new(B, A, GuidB, GuidA);

}
