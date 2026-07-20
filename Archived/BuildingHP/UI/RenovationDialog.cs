namespace BuildingHP.UI;

public class RenovationDialog : DialogBoxElement
{
    readonly VisualElement contentPanel;
    BuildingRenovationComponent? comp;

    IRenovationProvider? currentProvider;
    VisualElement? currentUI;

    readonly RenovationDialogFilter filter = new();

    readonly PanelStack panelStack;
    readonly RenovationListView lstItems;
    readonly RenovationRegistry renovationRegistry;
    readonly ILoc t;

    readonly Label lblError;

    public RenovationDialog(RenovationRegistry renovationRegistry, VisualElementLoader veLoader, PanelStack panelStack, ILoc t)
    {
        this.panelStack = panelStack;
        this.t = t;
        this.renovationRegistry = renovationRegistry;

        SetTitle(t.T("LV.BHP.Renovations"));
        AddCloseButton();

        var el = Content;

        var row = el.AddRow().SetMinSize(null, 0).SetFlexGrow().SetFlexShrink(1).AlignItems(Align.Stretch);

        var leftPanel = row.AddChild()
            .SetWidth(300)
            .SetMarginRight();
        AddFilterPanel(leftPanel);
        var scroll = leftPanel.AddScrollView()
            .SetFlexGrow()
            .SetFlexShrink()
            .SetMinSize(null, 0);

        scroll.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
        lstItems = scroll.AddChild<RenovationListView>();
        lstItems.RenovationSelected += OnRenovationSelected;

        contentPanel = row.AddChild().SetFlexGrow(1);

        lblError = this.AddGameLabel();
        lblError.RemoveFromHierarchy();

        this.Initialize(veLoader);
    }

    void AddFilterPanel(VisualElement parent)
    {
        var panel = parent.AddChild()
            .SetFlexShrink(0)
            .SetMarginBottom();

        panel.AddTextField(changeCallback: OnFilterKeywordChanged)
            .SetMarginBottom(5);

        panel.AddToggle(t.T("LV.BHP.ShowUnavailables"), onValueChanged: OnFilterUnavailablesChanged);
    }

    void OnFilterUnavailablesChanged(bool value)
    {
        filter.ShowUnavailables = value;
        ApplyFilter();
    }

    void OnFilterKeywordChanged(string text)
    {
        filter.Keyword = text;
        ApplyFilter();
    }

    void ApplyFilter()
    {
        lstItems.Filter(filter);
    }

    void OnRenovationSelected(RenovationProviderItemModel model)
    {
        currentProvider?.ClearUI(currentUI);
        contentPanel.Clear();
        currentUI = null;

        if (model.IsAvailable)
        {
            if (!comp) { return; }

            currentUI = model.Provider.CreateUI(comp);
            contentPanel.Add(currentUI);
        }
        else
        {
            lblError.text = model.NotAvailableReason!.Color(TimberbornTextColor.Red);
            contentPanel.Add(lblError);
        }
    }

    public async Task<bool> ShowAsync(BuildingRenovationComponent comp)
    {
        this.comp = comp;
        lstItems.Init(comp, renovationRegistry, t);
        ApplyFilter();

        var result = await ShowAsync(null, panelStack);

        currentProvider?.ClearUI(currentUI);
        currentProvider = null;
        currentUI = null;

        return result;
    }

}

public class RenovationDialogFilter
{
    public string Keyword { get; set; } = "";
    public bool ShowUnavailables { get; set; } = false;
}