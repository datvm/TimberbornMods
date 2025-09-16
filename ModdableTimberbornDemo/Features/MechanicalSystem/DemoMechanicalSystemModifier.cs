namespace ModdableTimberbornDemo.Features.MechanicalSystem;

public abstract class BaseDemoMechanicalModifier : BaseComponent, IModdableMechanicalNodeModifier
{
    public abstract string Id { get; }
    public abstract int Priority { get; }
    protected bool ShouldShortCircuit { get; set; }

    public bool Disabled
    {
        get;
        set
        {
            if (value != field)
            {
                field = value;
                OnChanged?.Invoke();
            }
        }
    } = true;

    public float Amount
    {
        get;
        set
        {
            if (value != field)
            {
                field = value;
                OnChanged?.Invoke();
            }
        }
    }

    public event Action? OnChanged;

    public bool Modify(ModdableMechanicalNodeValues value)
    {
        var (curr, original) = value;
        
        if (original.NominalInput != 0)
        {
            value.Value = curr with { NominalInput = ModifyValue(curr.NominalInput) };
        }

        if (original.NominalOutput != 0)
        {
            value.Value = curr with { NominalOutput = ModifyValue(curr.NominalOutput) };
        }

        return ShouldShortCircuit;
    }
    protected abstract int ModifyValue(int currValue);
}

public class DemoAdditiveMechanicalSystemModifier : BaseDemoMechanicalModifier
{
    public override string Id { get; } = nameof(DemoAdditiveMechanicalSystemModifier);
    public override int Priority { get; } = (int)ModifierPriority.Additive;

    protected override int ModifyValue(int currValue) => Mathf.RoundToInt(currValue + Amount);
}

public class DemoMultiplicativeMechanicalSystemModifier : BaseDemoMechanicalModifier
{
    public override string Id { get; } = nameof(DemoMultiplicativeMechanicalSystemModifier);
    public override int Priority { get; } = (int)ModifierPriority.Multiplicative;
    protected override int ModifyValue(int currValue) => Mathf.RoundToInt(currValue * Amount);
}

public class DemoForceMechanicalSystemModifier : BaseDemoMechanicalModifier
{
    public override string Id { get; } = nameof(DemoForceMechanicalSystemModifier);
    public override int Priority { get; } = (int)ModifierPriority.Force;
    protected override int ModifyValue(int currValue) => Mathf.RoundToInt(Amount);

    public DemoForceMechanicalSystemModifier()
    {
        ShouldShortCircuit = true;
    }

}
