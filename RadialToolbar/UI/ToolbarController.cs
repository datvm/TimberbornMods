namespace RadialToolbar.UI;

[BindSingleton(Contexts = BindAttributeContext.NonMenu)]
public class ToolbarController(
    ToolbarElement toolbarElement,
    ToolbarSegmentItemRenderer itemRenderer,
    InputService inputService,
    ToolbarNavigator navigator,
    MSettings s,
    RadialQuickSlotService quickSlotService,
    QuickSlotElement quickSlotElement,
    UILayout uiLayout // Use UI Layout instead of PanelStack to make sure it's loaded
) : IInputProcessor, ILoadableSingleton
{
    public const string BackKeyId = "RadialNavBack";
    public const string QuickSlotKeyPrefix = "RadialQuickSlot";
    public const string PinQuickSlotKeyPrefix = "RadialPinQuickSlot";

    static readonly ImmutableArray<string> EightNavKeyIds = [
        "RadialNavUp", "RadialNavUpRight", "RadialNavRight", "RadialNavDownRight",
        "RadialNavDown", "RadialNavDownLeft", "RadialNavLeft", "RadialNavUpLeft"
    ];

    static readonly ImmutableArray<string> FourNavKeyIds = ["RadialNavUp", "RadialNavRight", "RadialNavDown", "RadialNavLeft",];
    
    const int QuickSlotCount = RadialQuickSlotService.SlotCount;
    public static readonly ImmutableArray<string> QuickSlotKeyIds = [.. Enumerable.Range(1, QuickSlotCount)
        .Select(i => $"{QuickSlotKeyPrefix}{i}")];
    public static readonly ImmutableArray<string> PinQuickSlotKeyIds = [.. Enumerable.Range(1, QuickSlotCount)
        .Select(i => $"{PinQuickSlotKeyPrefix}{i}")];

    bool isHolding;

    public bool Visible => toolbarElement.IsDisplayed();

    public void Load()
    {
        toolbarElement.OnSegmentChosen += OnSegmentChosen;
        toolbarElement.OnBackButtonRequested += NavigateBack;
        toolbarElement.OnCloseButtonRequested += Dismiss;

        quickSlotElement.OnQuickSlotRequested += OnQuickSlotChosen;

        uiLayout._panelStack._root.Add(toolbarElement);
    }

    void NavigateBack()
    {
        if (!navigator.Back())
        {
            Dismiss();
            return;
        }
        RenderItems();
    }

    void RenderItems() => itemRenderer.Render(navigator.CurrentItem);

    void OnSegmentChosen(int index)
    {
        var child = navigator.CurrentItem.Children?[index];
        if (child is null) { return; }

        if (child.HasChildren)
        {
            navigator.NavigateTo(child);
            RenderItems();
        }
        else
        {
            if (child.ButtonId is not null)
            {
                quickSlotService.Push(child.ButtonId);
            }
            
            child.Action?.Invoke();
            Dismiss();
        }

        toolbarElement.HighlightSegment(null);
    }

    void OnQuickSlotChosen(int index)
    {
        var btn = quickSlotService.GetButtonAtSlot(index);
        if (btn is null) { return; }

        btn.Activate?.Invoke();
        Dismiss();
    }

    public void ToggleDisplay(bool? visible = null)
    {
        if (visible == Visible) { return; }

        visible ??= !Visible;

        if (visible.Value)
        {
            Show();
        }
        else
        {
            Dismiss();
        }
    }

    public void Show()
    {
        navigator.Reset();
        toolbarElement.Show();
        RenderItems();

        inputService.FlushUIInput();
        inputService.AddInputProcessor(this);
        toolbarElement.SetDisplay(true);
    }

    public void Dismiss()
    {
        inputService.FlushUIInput();
        inputService.RemoveInputProcessor(this);
        toolbarElement.SetDisplay(false);
    }

    public bool ProcessInput()
    {
        if (inputService.UIConfirm || inputService.IsKeyDown(ToolBarTriggerer.ToggleKey))
        {
            Dismiss();
            return true;
        }

        if (inputService.UICancel)
        {
            if (isHolding)
            {
                isHolding = false;
                toolbarElement.HighlightSegment(null);
            }
            else
            {
                Dismiss();
            }

            return true;
        }

        if (inputService.IsKeyDown(BackKeyId))
        {
            NavigateBack();
            return true;
        }

        var navKeys = s.EightSegment.Value ? EightNavKeyIds : FourNavKeyIds;
        for (int i = 0; i < navKeys.Length; i++)
        {
            if (inputService.IsKeyDown(navKeys[i]))
            {
                toolbarElement.HighlightSegment(i);
                isHolding = true;
                break;
            }
            else if (inputService.IsKeyUp(navKeys[i]) && isHolding)
            {
                OnSegmentChosen(i);
                break;
            }
        }

        for (int i = 0; i < QuickSlotCount; i++)
        {
            if (inputService.IsKeyDown(QuickSlotKeyIds[i]))
            {
                OnQuickSlotChosen(i);
                break;
            }
            else if (inputService.IsKeyDown(PinQuickSlotKeyIds[i]))
            {
                quickSlotService.TogglePin(i);
            }
        }

        return true;
    }

}
