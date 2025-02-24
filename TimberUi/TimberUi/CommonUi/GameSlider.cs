namespace TimberUi.CommonUi;

public readonly record struct SliderValues<TValue>(TValue Low, TValue High, TValue Default) where TValue : IComparable<TValue>;

public class GameSlider : GameSlider<Slider, float> { }

public class GameSliderInt : GameSlider<SliderInt, int> { }

public class GameSlider<TSlider, TValue> : VisualElement
    where TSlider : BaseSlider<TValue>, new()
    where TValue : IComparable<TValue>
{
    public static readonly ImmutableArray<string> ContainerClasses = ["settings-slider", "settings-text"];
    public static readonly ImmutableArray<string> SliderClasses = ["settings-slider__slider"];
    public static readonly ImmutableArray<string> EndLabelClasses = ["settings-slider__end-label"];

    public TSlider Slider { get; private set; }
    public Label? EndLabel { get; private set; }

    public GameSlider()
    {
        this.AddClasses(ContainerClasses);
        this.SetAsRow();

        Slider = this.AddChild<TSlider>(classes: SliderClasses);
    }

    public GameSlider<TSlider, TValue> SetLabel(string label)
    {
        Slider.label = label;
        return this;
    }

    public GameSlider<TSlider, TValue> SetHorizontalSlider(in SliderValues<TValue> values)
    {
        Slider.lowValue = values.Low;
        Slider.highValue = values.High;
        Slider.value = values.Default;

        Slider.direction = SliderDirection.Horizontal;
        Slider.style.flexGrow = 1;

        return this;
    }

    void AddEndLabel()
    {
        EndLabel ??= this.AddChild<Label>(classes: EndLabelClasses);
    }

    public GameSlider<TSlider, TValue> AddEndLabel(string text)
    {
        AddEndLabel();
        EndLabel!.text = text;

        return this;
    }

    public GameSlider<TSlider, TValue> AddEndLabel(Func<TValue, string> textFunc)
    {
        AddEndLabel();

        EndLabel!.text = textFunc(Slider.value);
        Slider.RegisterValueChangedCallback((e) =>
        {
            EndLabel.text = textFunc(e.newValue);
        });

        return this;
    }

    public GameSlider<TSlider, TValue> RegisterChangeCallback(EventCallback<ChangeEvent<TValue>> ev)
    {
        Slider.RegisterCallback(ev);
        return this;
    }

    public GameSlider<TSlider, TValue> RegisterChange(Action<TValue> action)
    {
        Slider.RegisterValueChangedCallback((e) => action(e.newValue));
        return this;
    }

    public GameSlider<TSlider, TValue> SetValue(TValue value)
    {
        Slider.value = value;
        return this;
    }

}
