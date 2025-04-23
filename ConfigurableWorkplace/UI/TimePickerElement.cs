namespace ConfigurableWorkplace.UI;

public class TimePickerElement : VisualElement
{

    readonly Label label;
    readonly Label lblHour;
    readonly Button add, minus;
    int value = 0;
    string hourFormat = "{0}h";

    public Vector2Int MinMax
    {
        get;
        set
        {
            field = value;
            OnDataChanged();
        }
    } = new(0, WorkplaceShiftFragment.HoursPerDay);

    public int Value
    {
        get => value;
        set
        {
            SetValueWithoutNotifying(value);
            OnValueChanged(this.value);
        }
    }

    public event Action<int> OnValueChanged = delegate { };

    public TimePickerElement()
    {
        var row = this.AddRow();

        label = row.AddGameLabel("");

        lblHour = row.AddGameLabel("00h")
            .SetFlexGrow()
            .SetMarginRight(10);
        lblHour.style.unityTextAlign = TextAnchor.MiddleRight;

        minus = row.AddMinusButton(size: UiBuilder.GameButtonSize.Small);
        minus.clicked += () => ChangeValue(-1);

        add = row.AddPlusButton(size: UiBuilder.GameButtonSize.Small);
        add.clicked += () => ChangeValue(1);
    }
    
    public TimePickerElement SetTexts(string label, string hourFormat)
    {
        this.label.text = label;
        this.hourFormat = hourFormat;
        return this;
    }

    public TimePickerElement SetMinMax(in Vector2Int minMax)
    {
        MinMax = minMax;
        return this;
    }

    public TimePickerElement SetValuesWithoutNotifying(int value, in Vector2Int minMax)
    {
        MinMax = minMax;
        return SetValueWithoutNotifying(value);
    }

    public TimePickerElement SetValueWithoutNotifying(int value)
    {
        this.value = value;
        OnDataChanged();

        return this;
    }

    public TimePickerElement RegisterOnValueChanged(Action<int> action)
    {
        OnValueChanged += action;
        return this;
    }
    
    public void ChangeValue(int delta)
    {
        Value = value + delta;
    }

    void OnDataChanged()
    {
        if (value < MinMax.x || value > MinMax.y)
        {
            value = Math.Clamp(value, MinMax.x, MinMax.y);
        }

        add.enabledSelf = value < MinMax.y;
        minus.enabledSelf = value > MinMax.x;

        lblHour.text = string.Format(hourFormat, value.ToString("00"));
    }

}
