
namespace ModdableTimberbornDemo.Features.MechanicalSystem;

public class DemoMechanicalSystemFragment : BaseEntityPanelFragment<DemoAdditiveMechanicalSystemModifier>
{

    DemoMultiplicativeMechanicalSystemModifier? mulModifier;
    DemoForceMechanicalSystemModifier? forceModifier;

    (Toggle Toggle, FloatField Value, Func<BaseDemoMechanicalModifier?> ModifierFunc)[] controls = [];

    protected override void InitializePanel()
    {
        controls = [
            CreateControl(panel, "Add power I/O:", () => component),
            CreateControl(panel, "Multiply power I/O:", () => mulModifier),
            CreateControl(panel, "Force power I/O:", () => forceModifier),
        ];
        
        (Toggle, FloatField, Func<BaseDemoMechanicalModifier?>) CreateControl(VisualElement parent, string text, Func<BaseDemoMechanicalModifier?> modifier)
        {
            var container = parent.AddChild().SetMarginBottom(10);

            var toggle = container.AddToggle(text, onValueChanged: v => OnModifierToggled(v, modifier()));
            var txt = container.AddFloatField(changeCallback: v => OnValueChanged(v, modifier()));

            return (toggle, txt, modifier);
        }
    }

    public override void ShowFragment(BaseComponent entity)
    {
        base.ShowFragment(entity);

        if (!component) { return; }

        var mechNode = component.GetComponentFast<ModdableMechanicalNode>();
        var (input, output) = mechNode.MechanicalNodeValues.OriginalValue;
        if (input == 0 && output == 0)
        {
            ClearFragment();
            return;
        }

        mulModifier = component.GetComponentFast<DemoMultiplicativeMechanicalSystemModifier>();
        forceModifier = component.GetComponentFast<DemoForceMechanicalSystemModifier>();

        foreach (var c in controls)
        {
            var mod = c.ModifierFunc()!;
            c.Toggle.SetValueWithoutNotify(!mod.Disabled);
            c.Value.SetValueWithoutNotify(mod.Amount);
        }
    }

    public override void ClearFragment()
    {
        base.ClearFragment();
        mulModifier = null;
        forceModifier = null;
    }

    void OnModifierToggled(bool enabled, BaseDemoMechanicalModifier? mod)
    {
        if (!mod) { return; }
        mod.Disabled = !enabled;
    }

    void OnValueChanged(float value, BaseDemoMechanicalModifier? mod)
    {
        if (!mod) { return; }
        mod.Amount = value;
    }

}
