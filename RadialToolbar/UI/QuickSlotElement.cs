namespace RadialToolbar.UI;

[BindSingleton(Contexts = BindAttributeContext.NonMenu)]
public class QuickSlotElement(
    RadialQuickSlotService service,
    NamedIconProvider namedIconProvider,
    KeyBindingShortcutService keyBindingShortcutService
) : VisualElement, ILoadableSingleton
{
    const int SlotSize = StandaloneToolButton.Size;
    const int HotkeySize = 20;
    const int Padding = 5;
    const int SlotCount = RadialQuickSlotService.SlotCount;
    const int LockSize = 20;

    readonly QuickSlotItemElement[] slots = new QuickSlotItemElement[SlotCount];

    public event Action<int> OnQuickSlotRequested = null!;

    public void Load()
    {
        InitElements();
        style.position = Position.Absolute;
        pickingMode = PickingMode.Ignore;
        style.right = 10;
        style.top = Length.Percent(50);
        style.translate = new(new Translate(0, Length.Percent(-50)));
        style.width = style.height = SlotSize * 3 + Padding * 4;

        service.OnChanged += Render;
        Render();
    }

    void InitElements()
    {
        for (int i = 0; i < SlotCount; i++)
        {
            var btn = this.AddChild(() => new StandaloneToolButton(namedIconProvider));
            var z = i;
            btn.AddAction(() => OnQuickSlotRequested(z));

            var s = btn.style;
            s.position = Position.Absolute;

            var dx = i % 2 == 0 ? 1 
                : i == 1 ? 2 : 0;
            var dy = i % 2 == 1 ? 1
                : i == 0 ? 0 : 2;

            s.left = dx * (SlotSize + Padding) + Padding;
            s.top = dy * (SlotSize + Padding) + Padding;

            var lblHotkey = btn.AddLabel().SetSize(HotkeySize).SetBorderRadius(5f);
            s = lblHotkey.style;
            s.position = Position.Absolute;
            s.left = s.top = 0;
            s.unityTextAlign = TextAnchor.MiddleCenter;
            s.backgroundColor = ToolbarFrame.BorderColor;
            s.color = Color.white;
            keyBindingShortcutService.CreateAny(lblHotkey, ToolbarController.QuickSlotKeyIds[i]);

            var pinIcon = btn.AddIconSpan(namedIconProvider.GetOrLoadGameIcon("LockIconYellow", "lock-icon-yellow"), size: LockSize);
            s = pinIcon.style;
            s.position = Position.Absolute;
            s.right = s.top = -LockSize / 2;
            pinIcon.SetDisplay(false);

            slots[i] = new(btn, pinIcon, lblHotkey);
        }
    }

    void Render()
    {
        var slots = service.Slots;

        for (int i = 0; i < SlotCount; i++)
        {
            var (id, pinned) = slots[i];
            var (btn, pinIcon, _) = this.slots[i];

            if (id is null)
            {
                btn.SetSprite(null);
                continue;
            }

            var originalBtn = service.GetButtonAtSlot(i)!;
            btn.SetSprite(originalBtn.Sprite);
            btn.SetBackground(originalBtn.VisualElement, namedIconProvider);
            pinIcon.SetDisplay(pinned);
        }
    }

    readonly record struct QuickSlotItemElement(StandaloneToolButton Button, VisualElement Pin, VisualElement Hotkey);
}
