namespace ModdableTimberborn.Common;

public class BaseModdableComponent<TComponent> : BaseComponent
    where TComponent : BaseComponent
{

#nullable disable
    public TComponent OriginalComponent { get; internal protected set; }
#nullable enable

}

public interface IModdableComponentAwake
{
    void AwakeAfter();
}

public interface IModdableComponentStart
{
    void StartAfter();
}
