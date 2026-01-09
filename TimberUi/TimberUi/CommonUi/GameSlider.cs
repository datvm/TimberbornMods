namespace TimberUi.CommonUi;

public readonly record struct SliderValues<TValue>(TValue Low, TValue High, TValue Default) where TValue : IComparable<TValue>;

public class GameSliderAlternativeManualValueDI(InputService inputService, ILoc t, VisualElementInitializer veInit, PanelStack panelStack)
{
    public readonly InputService InputService = inputService;
    public readonly ILoc t = t;
    public readonly VisualElementInitializer VeInit = veInit;
    public readonly PanelStack PanelStack = panelStack;
}

public class GameSlider : GameSlider<GameSlider, Slider, float>
{

    public GameSlider RegisterAlternativeManualValue(InputService inputService, ILoc t, VisualElementInitializer veInit, PanelStack panelStack, string? prompt = default)
    {
        RegisterAlternativeManualValue(inputService, veInit, panelStack,
            () => InputDialogBox.CreateFloat(t),
            prompt);

        return this;
    }

    public GameSlider RegisterAlternativeManualValue(GameSliderAlternativeManualValueDI di, string? prompt = default)
        => RegisterAlternativeManualValue(di.InputService, di.t, di.VeInit, di.PanelStack, prompt);

}

public class GameSliderInt : GameSlider<GameSliderInt, SliderInt, int>
{

    public GameSliderInt RegisterAlternativeManualValue(InputService inputService, ILoc t, VisualElementInitializer veInit, PanelStack panelStack, string? prompt = default)
    {
        RegisterAlternativeManualValue(inputService, veInit, panelStack,
            () => InputDialogBox.CreateInteger(t),
            prompt);

        return this;
    }

    public GameSliderInt RegisterAlternativeManualValue(GameSliderAlternativeManualValueDI di, string? prompt = default)
        => RegisterAlternativeManualValue(di.InputService, di.t, di.VeInit, di.PanelStack, prompt);
}

public class GameSlider<TSelf, TSlider, TValue> : GameSlider<TSlider, TValue>
    where TSelf : GameSlider<TSelf, TSlider, TValue>
    where TSlider : BaseSlider<TValue>, new()
    where TValue : IComparable<TValue>
{

    public new TSelf SetLabel(string label) => (TSelf)base.SetLabel(label);
    public new TSelf SetHorizontalSlider(in SliderValues<TValue> values) => (TSelf)base.SetHorizontalSlider(values);
    public new TSelf AddEndLabel(string text) => (TSelf)base.AddEndLabel(text);
    public new TSelf AddEndLabel(Func<TValue, string> textFunc) => (TSelf)base.AddEndLabel(textFunc);
    public new TSelf RegisterChangeCallback(EventCallback<ChangeEvent<TValue>> ev) => (TSelf)base.RegisterChangeCallback(ev);
    public new TSelf RegisterChange(Action<TValue> action) => (TSelf)base.RegisterChange(action);
    public new TSelf SetValue(TValue value) => (TSelf)base.SetValue(value);
    public new TSelf SetValueWithoutNotify(TValue value) => (TSelf)base.SetValueWithoutNotify(value);
    public new TSelf RegisterAlternativeClickCallback(InputService inputService, Action action) => (TSelf)base.RegisterAlternativeClickCallback(inputService, action);

}

public class GameSlider<TSlider, TValue> : VisualElement
    where TSlider : BaseSlider<TValue>, new()
    where TValue : IComparable<TValue>
{


    const string AlternateClickableActionKey = "AlternateClickableAction";
    EventCallback<ChangeEvent<TValue>>? endLabelCallback;

    public TSlider Slider { get; private set; }
    public Label? EndLabel { get; private set; }

    public TValue Value
    {
        get => Slider.value;
        set => Slider.value = value;
    }

    public GameSlider()
    {
        this.AddClasses(UiCssClasses.SliderContainerClasses);
        this.SetAsRow();

        Slider = this.AddChild<TSlider>(classes: UiCssClasses.SliderClasses);
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
        EndLabel ??= this.AddChild<Label>(classes: UiCssClasses.SliderEndLabelClasses);
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

        if (endLabelCallback is not null)
        {
            Slider.UnregisterCallback(endLabelCallback);
        }

        endLabelCallback = e =>
        {
            EndLabel!.text = textFunc(e.newValue);
        };
        Slider.RegisterValueChangedCallback(endLabelCallback);

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

    public GameSlider<TSlider, TValue> SetValueWithoutNotify(TValue value)
    {
        var oldValue = Slider.value;

        Slider.SetValueWithoutNotify(value);
        endLabelCallback?.Invoke(new ChangeEvent<TValue>()
        {
            previousValue = oldValue,
            newValue = value,
            currentTarget = Slider,
            target = Slider,
            elementTarget = Slider,
        });

        return this;
    }

    public GameSlider<TSlider, TValue> RegisterAlternativeClickCallback(InputService inputService, Action action)
    {
        RegisterCallback<MouseUpEvent>(_ =>
        {
            if (inputService.IsKeyHeld(AlternateClickableActionKey))
            {
                action();
            }
        });

        return this;
    }

    protected void RegisterAlternativeManualValue<TInput>(
        InputService inputService,
        VisualElementInitializer veInit,
        PanelStack panelStack,
        Func<InputDialogBox<TInput, TValue>> diagFunc,
        string? prompt
    )
        where TInput : BaseField<TValue>, new()
    {
        RegisterAlternativeClickCallback(inputService, async () =>
        {
            var diag = diagFunc()
                .AddCloseButton()
                .SetValue(Value);

            if (prompt is not null)
            {
                diag.SetPrompt(prompt);
            }

            var (result, value) = await diag.ShowAsync(veInit, panelStack);
            if (!result) { return; }

            SetValue(value!);
        });
    }

}
