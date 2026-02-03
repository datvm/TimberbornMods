namespace PowerCopy.UI;

public class DefaultUpdatableEntry<T>(Func<T, string[]> displayParameters, ILoc t) : DefaultDuplicableEntry<T>(t), IDuplicableUpdatableEntry
    where T : IDuplicable
{
    readonly ILoc t = t;
    public override int Order { get; } = 1000;

    public void UpdateFor(IDuplicable duplicable)
        => chk.text = DefaultNameLoc.TFormat(t, displayParameters((T)duplicable));
}
