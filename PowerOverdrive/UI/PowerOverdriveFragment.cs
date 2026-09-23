namespace PowerOverdrive.UI;

[BindFragment]
public class PowerOverdriveFragment(
    VisualElementLoader veLoader,
    DialogService diag
) : BaseEntityPanelFragment<PowerOverdriveComponent>
{
    VisualElement buttonsRow = null!;
    Label effect = null!;
    LevelButton[] buttons = [];
    int? hoveredLevel;
    bool refreshing;

    protected override void InitializePanel()
    {
        buttonsRow = panel.AddRow().AlignItems(Align.Stretch).SetMarginBottom();
        effect = panel.AddGameLabel();
    }

    public override void ShowFragment(BaseComponent entity)
    {
        var overdrive = entity.GetComponent<PowerOverdriveComponent>();
        if (overdrive is null || !overdrive.Service.ShouldShowFragment(overdrive))
        {
            ClearFragment();
            return;
        }

        hoveredLevel = null;
        EnsureButtons(overdrive.Service);
        base.ShowFragment(entity);
        Refresh();
    }

    public override void ClearFragment()
    {
        hoveredLevel = null;
        base.ClearFragment();
    }

    public override void UpdateFragment()
    {
        if (component is null)
        {
            return;
        }

        if (!component.Service.ShouldShowFragment(component))
        {
            ClearFragment();
            return;
        }

        Refresh();
    }

    void EnsureButtons(PowerOverdriveService service)
    {
        if (buttons.Length == service.LevelCount)
        {
            return;
        }

        buttonsRow.Clear();
        buttons = new LevelButton[service.LevelCount];
        for (var i = 0; i < service.LevelCount; i++)
        {
            buttons[i] = CreateButton(i);
        }
    }

    LevelButton CreateButton(int slotIndex)
    {
        var level = slotIndex + 1;
        var root = veLoader.LoadVisualElement("Game/EntityPanel/CharacterButton");
        root.style.flexGrow = 1;
        root.style.flexBasis = 0;
        root.style.flexShrink = 1;
        root.style.width = StyleKeyword.Auto;
        root.style.minWidth = 0;
        root.SetHeight(100);
        buttonsRow.Add(root);

        var btn = root.Q<Button>("CharacterButton");
        btn.SetEnabled(true);
        btn.style.width = Length.Percent(100);
        btn.style.height = Length.Percent(100);
        btn.style.minWidth = 0;
        FitBackground(btn);
        btn.AddAction(() => OnClicked(level));

        root.RegisterCallback<MouseEnterEvent>(_ =>
        {
            hoveredLevel = level;
            RefreshEffect();
        });
        root.RegisterCallback<MouseLeaveEvent>(_ =>
        {
            if (hoveredLevel == level)
            {
                hoveredLevel = null;
                RefreshEffect();
            }
        });

        return new(root, btn);
    }

    void Refresh()
    {
        if (component is null)
        {
            return;
        }

        refreshing = true;
        var service = component.Service;
        for (var i = 0; i < buttons.Length; i++)
        {
            var sprite = i < component.ActiveLevel
                ? service.Icon(service.GetLevel(i))
                : service.EmptyButtonBackground;
            buttons[i].Button.style.backgroundImage = sprite is null
                ? StyleKeyword.None
                : new StyleBackground(sprite);
        }

        refreshing = false;
        RefreshEffect();
    }

    void RefreshEffect()
    {
        if (component is null)
        {
            return;
        }

        effect.text = component.Service.FormatEffect(hoveredLevel ?? component.ActiveLevel);
    }

    void OnClicked(int level)
    {
        if (refreshing || component is null)
        {
            return;
        }

        var target = level == component.ActiveLevel ? level - 1 : level;
        if (!component.TrySetActiveLevel(target, out var error) && error is not null)
        {
            diag.Alert(error);
            return;
        }

        Refresh();
    }

    static void FitBackground(VisualElement el)
    {
        var s = el.style;
        s.backgroundSize = new BackgroundSize(BackgroundSizeType.Contain);
        s.backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat);
        s.backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center);
        s.backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center);
    }

    readonly record struct LevelButton(VisualElement Root, Button Button);
}
