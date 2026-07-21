namespace BuildingRenovations.UI;

[BindTransient]
public class RenovationDialog(
    ILoc t,
    IContainer container,
    VisualElementLoader veLoader,
    PanelStack panelStack,
    RenovationDialogController controller
) : DialogBoxElement
{
    readonly RenovationDialogFilter filter = new();

#nullable disable
    VisualElement contentPanel;
    RenovationListView lstItems;
    RenovationDetailPanel detailPanel;
#nullable enable

    BuildingRenovationComponent? building;

    public RenovationDialog Init()
    {
        SetTitle(t.T("LV.BRe.Renovations"));
        AddCloseButton();

        var el = Content;
        var row = el.AddRow().SetMinSize(null, 0).SetFlexGrow().SetFlexShrink(1).AlignItems(Align.Stretch);

        var leftPanel = row.AddChild().SetWidth(300).SetMarginRight();
        AddFilterPanel(leftPanel);

        var scroll = leftPanel.AddScrollView()
            .SetFlexGrow()
            .SetFlexShrink()
            .SetMinSize(null, 0);
        scroll.horizontalScrollerVisibility = ScrollerVisibility.Hidden;

        lstItems = container.GetInstance<RenovationListView>();
        scroll.Add(lstItems);
        lstItems.RenovationSelected += OnRenovationSelected;

        contentPanel = row.AddChild().SetFlexGrow(1);

        detailPanel = container.GetInstance<RenovationDetailPanel>().Init();
        detailPanel.RemoveFromHierarchy();

        this.Initialize(veLoader);
        return this;
    }

    void AddFilterPanel(VisualElement parent)
    {
        var panel = parent.AddChild().SetFlexShrink(0).SetMarginBottom();
        panel.AddTextField(changeCallback: text =>
        {
            filter.Keyword = text;
            ApplyFilter();
        }).SetMarginBottom(5);

        panel.AddToggle(t.T("LV.BRe.ShowUnavailables"), onValueChanged: value =>
        {
            filter.ShowUnavailables = value;
            ApplyFilter();
        });
    }

    void ApplyFilter() => lstItems.Filter(filter);

    void OnRenovationSelected(RenovationListItemModel model)
    {
        contentPanel.Clear();
        detailPanel.Unset();

        if (!building) { return; }

        detailPanel.Set(
            building!,
            model.Renovation,
            unavailableReason: model.IsAvailable ? null : model.NotAvailableReason,
            onStarted: controller.CloseDialog);
        contentPanel.Add(detailPanel);
    }

    public async Task<bool> ShowAsync(BuildingRenovationComponent building)
    {
        this.building = building;
        lstItems.Init(building);
        ApplyFilter();

        var result = await ShowAsync(null, panelStack);

        detailPanel.Unset();
        contentPanel.Clear();
        this.building = null;

        return result;
    }
}

public class RenovationDialogFilter
{
    public string Keyword { get; set; } = "";
    public bool ShowUnavailables { get; set; }
}
