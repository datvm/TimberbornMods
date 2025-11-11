namespace TransparentTerrain.UI;

public class TransparentTerrainButton(
    VisualElementLoader veLoader,
    ITooltipRegistrar tooltipRegistrar,
    IAssetLoader assets,
    EventBus eb,
    BindableToggleFactory bindableToggleFactory,
    TransparentTerrainService transparentTerrainService,
    UILayout uILayout,
    KeyBindingTooltipFactory keyBindingTooltipFactory,
    ILoc t
) : ILoadableSingleton
{

#nullable disable
    VisualElement root;
    Toggle toggle;
#nullable enable

    public void Load()
    {
        root = veLoader.LoadVisualElement("Common/SquareToggle");
        tooltipRegistrar.Register(root, GetTooltip);

        toggle = root.Q<Toggle>("Toggle");

        var checkMark = root.Q(className: "unity-toggle__checkmark");
        var icon = assets.Load<Texture2D>("Resources/UI/transparent-terrain");
        checkMark.style.backgroundImage = icon;

        eb.Register(this);
        transparentTerrainService.OnToggled += OnServiceToggled;

        root.RegisterCallback<PointerDownEvent>(e =>
        {
            if (e.button == 1)
            {
                transparentTerrainService.ShowConfigureDialog();
            }
        });
    }

    void OnServiceToggled(object sender, bool e)
    {
        if (toggle.value != e)
        {
            toggle.SetValueWithoutNotify(e);
        }
    }

    [OnEvent]
    public void OnShowPrimaryUI(ShowPrimaryUIEvent _)
    {
        bindableToggleFactory.CreateAndBind(toggle, TransparentTerrainService.ToggleKeyId, OnTransparencyToggled,
            () => transparentTerrainService.Enabled);
        uILayout.AddTopRightButton(root, 4);
    }

    void OnTransparencyToggled(bool enabled)
    {
        transparentTerrainService.Toggle(enabled);
    }

    string GetTooltip()
    {
        keyBindingTooltipFactory._tooltip.AppendLine(t.T("LV.TrT.ButtonTooltip"));
        keyBindingTooltipFactory.AddKeyBindingInfo(TransparentTerrainService.ToggleKeyId, KeyBindingTooltipFactory.ToggleLocKey);
        keyBindingTooltipFactory.AddKeyBindingInfo(TransparentTerrainService.HoldKeyId, KeyBindingTooltipFactory.HoldLocKey);
        keyBindingTooltipFactory.AddKeyBindingInfo(TransparentTerrainService.ConfigureKeyId, "LV.TrT.KeyBindingConfigure");

        return keyBindingTooltipFactory._tooltip.ToStringWithoutNewLineEndAndClean();
    }

}
