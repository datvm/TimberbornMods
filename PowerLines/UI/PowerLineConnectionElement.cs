namespace PowerLines.UI;

[BindTransient]
public class PowerLineConnectionElement : VisualElement
{
    readonly Button button;
    readonly Image icon;
    readonly Label nameLabel;
    readonly Button removeButton;
    readonly string addConnectionText;

    PowerLineComponent? target;
    bool isAddMode;

    public PowerLineComponent? Target => target;

    public event EventHandler<PowerLineConnectionElement>? OnMakeConnectionRequested;
    public event EventHandler<PowerLineComponent>? OnSelectRequested;
    public event EventHandler<PowerLineComponent>? OnRemoveRequested;

    public PowerLineConnectionElement(VisualElementLoader loader, ILoc t, ITooltipRegistrar tooltipRegistrar)
    {
        button = loader.LoadVisualElement("Game/EntityPanel/ZiplineConnectionButton").Q<NineSliceButton>();
        Add(button);

        icon = button.Q<Image>("Icon");
        nameLabel = button.Q<Label>("Name");
        removeButton = button.Q<Button>("RemoveConnection");
        addConnectionText = t.T("LV.PL.AddConnection");

        button.RegisterCallback<ClickEvent>(_ => OnMainClicked());
        removeButton.RegisterCallback<ClickEvent>(OnRemoveClicked);
        tooltipRegistrar.RegisterLocalizable(removeButton, "LV.PL.RemoveConnection");

        ClearConnection();
    }

    public void SetConnection(PowerLineDisplay display)
    {
        target = display.Target;
        isAddMode = false;

        button.SetEnabled(true);
        SetName(display.Name);
        SetIcon(display.Icon, plus: false);
        removeButton.ToggleDisplayStyle(true);
    }

    public void SetEmpty()
    {
        target = null;
        isAddMode = false;

        button.SetEnabled(false);
        SetName(null);
        SetIcon(null, plus: false);
        removeButton.ToggleDisplayStyle(false);
    }

    public PowerLineConnectionElement ClearConnection()
    {
        target = null;
        isAddMode = true;

        button.SetEnabled(true);
        SetName(addConnectionText);
        SetIcon(null, plus: true);
        removeButton.ToggleDisplayStyle(false);

        return this;
    }

    void OnMainClicked()
    {
        if (target)
        {
            OnSelectRequested?.Invoke(this, target!);
        }
        else if (isAddMode)
        {
            OnMakeConnectionRequested?.Invoke(this, this);
        }
    }

    void OnRemoveClicked(ClickEvent e)
    {
        e.StopImmediatePropagation();
        e.StopPropagation();

        if (!target) { return; }
        OnRemoveRequested?.Invoke(this, target!);
    }

    void SetName(string? text)
    {
        nameLabel.text = text;
        nameLabel.ToggleDisplayStyle(text is not null);
    }

    void SetIcon(Sprite? sprite, bool plus)
    {
        icon.sprite = sprite;
        icon.EnableInClassList("icon--plus", plus);
    }
}

public readonly record struct PowerLineDisplay(PowerLineComponent Target, Sprite Icon, string Name);