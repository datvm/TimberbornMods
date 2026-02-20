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