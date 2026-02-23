namespace BuildingBlueprints.UI;

[BindTransient]
public class HotkeyToolDescriptionSection
{
    public VisualElement Root { get; }
    public VisualElement Content { get; }
    
    readonly KeyBindingShortcutService keyBindingShortcutService;

    public HotkeyToolDescriptionSection(VisualElementLoader veLoader, KeyBindingShortcutService keyBindingShortcutService)
    {
        this.keyBindingShortcutService = keyBindingShortcutService;

        Root = veLoader.LoadVisualElement("Common/ToolPanel/DescriptionPanelSection");
        Root.AddClass(DescriptionPanels.BackgroundClass);
        
        var label = Root.Q(className: "game-text-small");
        Content = Root.AddChild(classes:["game-text-small"]);
        Content.InsertSelfAfter(label);
        label.RemoveFromHierarchy();
    }

    public HotkeyEntry AddEntry(string? keyId)
    {
        var entry = new HotkeyEntry(keyId, keyBindingShortcutService)
            .SetMarginBottom(5);
        Content.Add(entry);

        return entry;
    }

    public MultiHotkeyEntry AddMultiEntry(IReadOnlyList<string> keyIds)
    {
        var entry = new MultiHotkeyEntry(keyIds, keyBindingShortcutService)
            .SetMarginBottom(5);
        Content.Add(entry);
        return entry;
    }
}

public class HotkeyEntry : VisualElement
{

    public string KeyId { get; }
    public Label? HotkeyLabel { get; }

    public Label TextLabel { get; }
    public string Text
    {
        get => TextLabel.text;
        set => TextLabel.text = value;
    }

    public HotkeyEntry(string? keyId, KeyBindingShortcutService keyBindingShortcutService)
    {
        KeyId = keyId ?? string.Empty;

        this.SetWidthPercent(100);

        if (keyId is not null)
        {
            HotkeyLabel = this.AddGameLabel().SetFlexShrink(0);
            keyBindingShortcutService.CreateAny(HotkeyLabel, keyId);
        }

        TextLabel = this.AddGameLabel();
    }

}

public class MultiHotkeyEntry : VisualElement
{

    public IReadOnlyList<string> KeyIds { get; }

    public Label TextLabel { get; }
    public string Text
    {
        get => TextLabel.text;
        set => TextLabel.text = value;
    }

    public Label[] HotkeyLabels { get; }

    public MultiHotkeyEntry(IReadOnlyList<string> keyIds, KeyBindingShortcutService keyBindingShortcutService)
    {
        KeyIds = keyIds;

        this.SetWidthPercent(100).SetAsRow().SetWrap();

        TextLabel = this.AddGameLabel().SetMarginRight(10).SetFlexShrink(0);

        HotkeyLabels = new Label[keyIds.Count];
        for (int i = 0; i < keyIds.Count; i++)
        {
            var hotkeyLabel = this.AddGameLabel().SetFlexShrink(0).SetMarginRight(5);
            keyBindingShortcutService.CreateAny(hotkeyLabel, keyIds[i]);
            HotkeyLabels[i] = hotkeyLabel;
        }
    }

}