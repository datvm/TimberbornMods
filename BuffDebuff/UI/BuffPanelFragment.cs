namespace BuffDebuff.UI;

public class BuffPanelFragment : EntityPanelFragmentElement
{
    const string BuffNameLabel = "BuffName";
    const string BuffDescLabel = "BuffDesc";

    const string BuffNameFormat = "<color=#00CA00>+ {0}</color>";
    const string DeBuffNameFormat = "<color=#E00000>- {0}</color>";

    public bool IsBuff { get; }
    readonly ListView list;

    public List<BuffInstance>? Buffs
    {
        set
        {
            list.itemsSource = value;
        }
    }

    public BuffPanelFragment(bool isBuff)
    {
        IsBuff = isBuff;

        list = this.AddListView(isBuff ? "BuffPanel" : "DebuffPanel");
        list.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
        list.style.maxHeight = 200;

        list.makeItem = MakeItem;
        list.bindItem = BindItem;

        Background = isBuff ? EntityPanelFragmentBackground.Green : EntityPanelFragmentBackground.PalePurple;
    }

    VisualElement MakeItem()
    {
        var container = new VisualElement();

        container.AddLabelGame("", BuffNameLabel, bold: true);
        container.AddLabelGame("", BuffDescLabel, color: UiBuilder.GameLabelColor.Yellow)
            .SetPadding(left: 20);

        return container;
    }

    void BindItem(VisualElement ve, int index)
    {
        var instance = list.itemsSource[index] as BuffInstance
            ?? throw new InvalidOperationException($"Item is not a {nameof(BuffInstance)}");

        var label = ve.Q<Label>(BuffNameLabel);
        label.text = string.Format(IsBuff ? BuffNameFormat : DeBuffNameFormat, instance.Buff.Name);

        var desc = ve.Q<Label>(BuffDescLabel);
        StringBuilder tooltipText = new();
        tooltipText.AppendLine(instance.Buff.Description);

        foreach (var e in instance.Effects)
        {
            tooltipText.AppendLine(e.Description);
        }

        desc.text = tooltipText.ToString();
    }

}
