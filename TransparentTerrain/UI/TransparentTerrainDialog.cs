namespace TransparentTerrain.UI;

public class TransparentTerrainDialog(
    PanelStack panelStack,
    TransparentTerrainService transparentTerrainService,
    ILoc t,
    VisualElementInitializer veInit
) : DialogBoxElement
{

    public void Show()
    {
        Init();

        Show(veInit, panelStack);
    }

    void Init()
    {
        SetTitle(t.T("LV.TrT.KeyBindingConfigure"));
        AddCloseButton();

        var panel = Content.AddChild().SetPadding(10);
        panel.AddToggle(t.T("LV.TrT.Enable"), onValueChanged: OnEnabledChanged)
            .SetMarginBottom()
            .SetValueWithoutNotify(transparentTerrainService.Enabled);

        panel.AddGameLabel(t.T("LV.TrT.Percent"));
        panel.AddSliderInt(values: new(0, 100, Mathf.RoundToInt(transparentTerrainService.Alpha * 100)))
            .AddEndLabel(v => v + "%")
            .RegisterChange(OnAlphaChanged);
    }

    void OnEnabledChanged(bool e)
    {
        transparentTerrainService.Toggle(e);
    }

    void OnAlphaChanged(int a)
    {
        transparentTerrainService.SetAlpha(a / 100f);
    }

}
