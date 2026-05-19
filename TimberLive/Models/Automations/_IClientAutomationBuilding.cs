namespace TimberLive.Models.Automations;

public interface IClientAutomationBuilding
{
    Type SettingsType { get; }
    string SettingsName { get; }
    string DisplayTypeName { get; }
    IReadOnlyList<AutomationSlot> InputSlots { get; }
    string? GetOperatorLabel(object settings);
    string? GetValueLabel(object settings);
}

public sealed record AutomationSlot(int Index, string Name);

public abstract class BaseClientAutomationBuilding<T> : IClientAutomationBuilding
{    
    public abstract string SettingsName { get; }
    public Type SettingsType => typeof(T);

    public string DisplayTypeName { get; protected set; }
    public virtual IReadOnlyList<AutomationSlot> InputSlots => [];

    public string? GetOperatorLabel(object settings) => GetOperatorLabel((T)settings);
    public virtual string? GetOperatorLabel(T settings) => null;
       
    public string? GetValueLabel(object settings) => GetValueLabel((T)settings);
    public virtual string? GetValueLabel(T settings) => null;

    public BaseClientAutomationBuilding()
    {
        DisplayTypeName = SettingsName[(SettingsName.LastIndexOf('.') + 1)..];
    }

    protected static IReadOnlyList<AutomationSlot> CreateSlots(params string[] names)
        => [.. names.Select((n, i) => new AutomationSlot(i, n))];

}