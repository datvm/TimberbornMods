namespace PowerCopy.UI;

public class DefaultDuplicableEntry<T>(ILoc t) : DefaultDuplicableEntry(typeof(T), t);

public class DefaultDuplicableEntry(Type type, ILoc t) : DuplicableEntry(type)
{
    string entryName = "";
    public override int Order => 1000;
    public override string Name => entryName;

    public override void Initialize()
    {
        entryName = GetDefaultName(t);

        base.Initialize();
    }

}
