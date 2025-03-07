namespace BuffDebuff.UI;

public class BuffPanelFragment : EntityPanelFragmentElement
{
    const string BuffNameLabel = "BuffName";

    const string BuffNameFormat = "<color=#00CA00>+ {0}</color>";
    const string DeBuffNameFormat = "<color=#E00000>- {0}</color>";

    public bool IsBuff { get; }
    readonly ListView list;
    readonly ITooltipRegistrar tooltips;

    public List<BuffInstance>? Buffs
    {
        set
        {
            list.itemsSource = value;
        }
    }

    public BuffPanelFragment(bool isBuff, ITooltipRegistrar tooltip)
    {
        IsBuff = isBuff;
        tooltips = tooltip;

        list = this.AddListView(isBuff ? "BuffPanel" : "DebuffPanel");
        list.SetMaxHeight(100);

        list.makeItem = MakeItem;
        list.bindItem = BindItem;

        Background = isBuff ? EntityPanelFragmentBackground.Green : EntityPanelFragmentBackground.PalePurple;
    }

    VisualElement MakeItem()
    {
        var container = new VisualElement();

        container.AddGameLabel("", BuffNameLabel, bold: true);

        return container;
    }

    void BindItem(VisualElement ve, int index)
    {
        var instance = list.itemsSource[index] as BuffInstance
            ?? throw new InvalidOperationException($"Item is not a {nameof(BuffInstance)}");

        var label = ve.Q<Label>(BuffNameLabel);
        label.text = string.Format(IsBuff ? BuffNameFormat : DeBuffNameFormat, instance.OverrideName ?? instance.Buff.Name);

        tooltips.Register(ve, () => CreateTooltip(instance));
    }

    static string CreateTooltip(BuffInstance instance)
    {
        StringBuilder tooltipText = new();
        tooltipText.AppendLine(instance.OverrideDescription ?? instance.Buff.Description);

        var instanceDesc = instance.AdditionalDescription;
        if (instanceDesc is not null)
        {
            tooltipText.AppendLine(instanceDesc);
        }

        foreach (var e in instance.Effects)
        {
            var desc = e.Description;
            if (desc is null) { continue; }

            tooltipText.AppendLine(desc);
        }

        return tooltipText.ToString();
    }

}
