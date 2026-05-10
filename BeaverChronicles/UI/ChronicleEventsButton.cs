namespace BeaverChronicles.UI;

[BindSingleton]
public class ChronicleEventsButton(
    VisualElementLoader veLoader,
    ITooltipRegistrar tooltipRegistrar,
    IAssetLoader assets,
    EventBus eb,
    BindableToggleFactory bindableToggleFactory,
    UILayout uILayout,
    ILoc t,
    ChronicleEventUIHelper uiHelper,
    InputService inputService
) : ILoadableSingleton, IInputProcessor
{
    public const string KeyId = "ShowChronicleEvents";

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
        var icon = assets.Load<Texture2D>("Resources/Chronicles/UI/scroll");
        checkMark.style.backgroundImage = icon;

        eb.Register(this);
        inputService.AddInputProcessor(this);
    }

    [OnEvent]
    public void OnShowPrimaryUI(ShowPrimaryUIEvent _)
    {
        bindableToggleFactory.CreateAndBind(toggle, KeyId, OnToggle, () => false);
        uILayout.AddTopRightButton(root, 4);
    }

    void OnToggle(bool _) => uiHelper.ShowChronicleDialog();

    string GetTooltip() => t.T("LV.BCEv.BeaverChronicles");

    public bool ProcessInput()
    {
        if (inputService.IsKeyDown(KeyId))
        {
            OnToggle(true);
            return true;
        }

        return false;
    }
}
