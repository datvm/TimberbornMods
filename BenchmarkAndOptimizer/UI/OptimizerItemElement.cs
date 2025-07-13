namespace BenchmarkAndOptimizer.UI;

public class OptimizerItemElement : VisualElement
{

#nullable disable
    public Type Type { get; private set; }
    public OptimizerItem Value { get; private set; }

    Toggle chkEnabled;
    GameSliderInt txtValue;
#nullable enable

    public event Action<OptimizerItem>? OnValueChanged;

    public OptimizerItemElement Init(Type type, OptimizerItem? value, string? desc = null)
    {
        Value = value = value is null ? new(type) : value with { };

        this.SetMarginBottom(10);
        Type = type;

        var parent = this;

        chkEnabled = parent.AddToggle(type.Name, onValueChanged: OnEnableChanged);
        chkEnabled.SetValueWithoutNotify(value.Enabled);

        txtValue = parent.AddSliderInt(values: new(0, 20, value.Value))
            .AddEndLabel(v => v.ToString())
            .RegisterChange(OnSliderValueChanged)
            .SetWidthPercent(100);

        SetUIEnable();

        if (desc is not null)
        {
            this.AddGameLabel(desc);
        }

        return this;
    }

    void SetUIEnable()
    {
        txtValue.enabledSelf = chkEnabled.value;
    }

    void OnEnableChanged(bool _)
    {
        SetUIEnable();
        RaiseEvent();
    }

    void OnSliderValueChanged(int value)
    {
        RaiseEvent();
    }

    void RaiseEvent()
    {
        Value.Enabled = chkEnabled.value;
        Value.Value = txtValue.Value;

        OnValueChanged?.Invoke(Value);
    }

}
