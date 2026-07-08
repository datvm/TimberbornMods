namespace ConveyorBelt.UI;

[BindFragment]
public class ConveyorBeltFragment(
    IGoodService goods,
    NamedIconProvider namedIconProvider,
    ILoc t
) : BaseEntityPanelFragment<ConveyorBeltComponent>
{
    public const int IconSize = 30;
    public const int Padding = 5;

    readonly List<ConveyorBeltItemElement> icons = [with(20)]; // should be more than enough

    VisualElement container = null!;
    Button btnEject = null!;

    protected override void InitializePanel()
    {
        container = panel.AddChild().SetMarginBottom(10);
        var s = container.style;
        s.height = IconSize + Padding * 2;
        s.backgroundColor = TimberUiUtils.WarningColor;

        btnEject = panel.AddGameButtonPadded(t.T("LV.CBlt.EjectContent"), EjectContent);
    }

    void EnsureImages(int count)
    {
        if (icons.Count < count)
        {
            var stuckIcon = namedIconProvider.GetOrLoad("error-icon", "UI/Images/Core/error-icon");

            for (int i = icons.Count; i < count; i++)
            {
                var img = new ConveyorBeltItemElement(stuckIcon);
                container.Add(img);
                icons.Add(img);
            }
        }

        for (int i = count; i < icons.Count; i++)
        {
            icons[i].SetDisplay(false);
        }
    }

    public override void UpdateFragment()
    {
        if (!component) { return; }

        var items = component!.Items;
        EnsureImages(items.Count);

        if (items.Count == 0)
        {
            btnEject.enabledSelf = false;
            return;
        }

        btnEject.enabledSelf = true;

        var width = container.resolvedStyle.width - IconSize - Padding * 2;
        for (int i = 0; i < items.Count; i++)
        {
            var item = items[i];
            var img = icons[i];
            img.SetDisplay(true);
            img.Icon = goods.GetGood(item.GoodId).Icon.Asset;

            var progress = Mathf.Lerp(0, width, item.Position);
            img.Progress = Padding + progress;

            img.IsStuck = item.Stuck;
        }
    }

    void EjectContent()
    {
        if (component)
        {
            component!.EjectContent();
        }
    }

}
