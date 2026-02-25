namespace HueAndTurnX.UI;

[BindTransient(Contexts = BindAttributeContext.NonMenu)]
public class ManualValueDialog(ILoc t, VisualElementInitializer veInit, PanelStack panelStack) : DialogBoxElement
{
    readonly FloatField[] txtValues = new FloatField[3];

    public async Task<Vector3?> ShowAsync(Vector3 initialValue, SliderValues<float> range)
    {
        SetTitle(t.T("LV.HTX.SetManually"));
        AddCloseButton();

        var parent = Content;

        parent.AddGameButtonPadded(t.T("LV.HTX.Reset"), onClick: () => ResetTo(range.Default))
            .SetMarginLeftAuto().SetMarginBottom(5);

        parent.AddLabel(t.T("LV.HTX.RecommendedValues", range.Low, range.High));
        parent.AddLabel(t.T("LV.HTX.SetManuallyDesc")).SetMarginBottom();

        Toggle chkSetForAll = null!;
        for (int i = 0; i < txtValues.Length; i++)
        {
            var txt = txtValues[i] = parent.AddFloatField().SetMarginBottom(10);
            txt.SetValueWithoutNotify(initialValue[i]);


            if (i == 0)
            {
                chkSetForAll = parent.AddToggle(t.T("LV.HTX.SetForAll"), onValueChanged: OnSetAllForAllChanged)
                    .SetMarginBottom(10);
            }
        }

        parent.AddMenuButton(t.TOK(), onClick: OnUIConfirmed, stretched: true);

        var confirm = await ShowAsync(veInit, panelStack);
        if (!confirm) { return null; }

        var x = txtValues[0].value;
        return chkSetForAll.value
            ? new Vector3(x, x, x)
            : new Vector3(x, txtValues[1].value, txtValues[2].value);
    }

    void ResetTo(float v)
    {
        for (int i = 0; i < txtValues.Length; i++)
        {
            txtValues[i].SetValueWithoutNotify(v);
        }
    }

    void OnSetAllForAllChanged(bool v)
    {
        for (int i = 1; i < txtValues.Length; i++)
        {
            txtValues[i].enabledSelf = !v;
        }
    }

}
