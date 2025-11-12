namespace MechanicalFilterPump.Components;

public class MechanicalFilterPumpPower : BaseComponent, IModdableMechanicalNodeModifier
{
    public const float PowerMultiplier = 2f;

    public string Id { get; } = nameof(MechanicalFilterPumpPower);
    public int Priority { get; } = (int)ModifierPriority.Multiplicative;
    public bool Disabled { get; private set; } = true;

    public event Action? OnChanged;

    public bool Modify(ModdableMechanicalNodeValues value)
    {
        value.Value = value.Value with
        {
            NominalInput = Mathf.CeilToInt(value.Value.NominalInput * PowerMultiplier)
        };
        return false;
    }

    public void Toggle(bool enabled)
    {
        if (Disabled != enabled) { return; }

        Disabled = !enabled;
        OnChanged?.Invoke();
    }

}
