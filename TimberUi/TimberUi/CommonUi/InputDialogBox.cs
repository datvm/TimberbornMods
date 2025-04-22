namespace TimberUi.CommonUi;

public class InputDialogBox(ILoc t) : InputDialogBox<InputDialogBox, NineSliceTextField, string>(t)
{
    public static IntegerInputDialogBox CreateInteger(ILoc t) => new(t);
    public static FloatInputDialogBox CreateFloat(ILoc t) => new(t);
    public static InputDialogBox CreateString(ILoc t) => new(t);
}

public class IntegerInputDialogBox(ILoc t) : InputDialogBox<IntegerInputDialogBox, NineSliceIntegerField, int>(t) { }
public class FloatInputDialogBox(ILoc t) : InputDialogBox<FloatInputDialogBox, NineSliceFloatField, float>(t) { }

public class InputDialogBox<TSelf, TInput, TValueType>(ILoc t) : InputDialogBox<TInput, TValueType>(t)
    where TSelf : InputDialogBox<TSelf, TInput, TValueType>
    where TInput : BaseField<TValueType>, new()
{

    public new TSelf SetPrompt(string prompt)
    {
        return (TSelf)base.SetPrompt(prompt);
    }

    public new TSelf SetValue(TValueType value)
    {
        return (TSelf)base.SetValue(value);
    }

    public new TSelf AddCloseButton()
    {
        return (TSelf)base.AddCloseButton();
    }

}

public class InputDialogBox<TInput, TValueType> : DialogBoxElement
    where TInput : BaseField<TValueType>, new()
{
    protected ILoc t;

    public Label PromptLabel { get; private set; }
    public TInput InputField { get; private set; }
    public Button ConfirmButton { get; private set; }
    public Button CancelButton { get; private set; }

    public TValueType Value => InputField.value;

    public InputDialogBox(ILoc t)
    {
        this.t = t;

        PromptLabel = Content.AddGameLabel(name: "Prompt")
            .SetDisplay(false)
            .SetMarginBottom();

        InputField = Content.AddValueField<TInput, TValueType>(name: "InputBox")
            .SetMarginBottom();

        var btns = Content.AddRow().SetMarginBottom();

        CancelButton = btns.AddMenuButton(name: "CancelButton", text: "Core.Cancel".T(t), onClick: OnUICancelled, stretched: true)
            .SetFlexGrow(1)
            .SetDisplay(false);
        ConfirmButton = btns.AddMenuButton(name: "ConfirmButton", text: "Core.OK".T(t), onClick: OnUIConfirmed, stretched: true)
            .SetFlexGrow(1);
    }

    public InputDialogBox<TInput, TValueType> SetPrompt(string prompt)
    {
        PromptLabel.text = prompt;
        PromptLabel.SetDisplay(true);

        return this;
    }

    public InputDialogBox<TInput, TValueType> SetValue(TValueType value)
    {
        InputField.value = value;

        return this;
    }

    public InputDialogBox<TInput, TValueType> AddCloseButton()
    {
        base.AddCloseButton();
        CancelButton.SetDisplay(true);

        return this;
    }

    public async Task<(bool Confirmed, TValueType? Value)> ShowAsync(VisualElementInitializer? initializer, PanelStack panelStack)
    {
        var result = await base.ShowAsync(initializer, panelStack);

        return (result, result ? Value : default);
    }

}
