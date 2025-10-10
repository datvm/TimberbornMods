namespace ScientificProjects.UI;

public partial class ScientificProjectDialog(
    ILoc t,
    PanelStack panelStack,
    VisualElementInitializer veInit,
    IContainer container,
    DialogService dialogService
) : DialogBoxElement
{
#nullable disable
    SPSciencePanel pnlScience;
    FilterBox filterBox;

    SPListElement list;
#nullable enable

    ScientificProjectFilter filter = ScientificProjectFilter.Default;

    public bool MustPay { get; private set; }

    public void Init()
    {
        SetDialogPercentSize(.6f, .75f);

        SetTitle(t.T("LV.SP.Title"));
        AddCloseButton(OnCloseButtonClicked);

        var panel = Content;

        var devPanel = panel.AddChild(container.GetInstance<SPDevPanel>)
            .SetMarginBottom();
        devPanel.OnActionExecuted += OnDevCommandExecuted;

        pnlScience = panel.AddChild(container.GetInstance<SPSciencePanel>)
            .SetMarginBottom();

        filterBox = panel.AddChild(() => new FilterBox(t)).SetMarginBottom();
        filterBox.OnFilterChanged += OnFilterChanged;

        list = panel.AddChild(container.GetInstance<SPListElement>);
        
        this.Initialize(veInit);

        RefreshContent();
    }

    public void AddNotEnoughPanel()
    {
        MustPay = true;
        pnlScience.AddNotEnoughPanel();

        pnlScience.OnDailyPaymentRequested += OnUIConfirmed;
        pnlScience.OnSkipRequested += OnCloseButtonClicked;
    }

    private void OnDevCommandExecuted()
    {
        RefreshContent();
    }

    void OnFilterChanged(ScientificProjectFilter filter)
    {
        this.filter = filter;
        list.ApplyFilter(filter);
    }

    public void RefreshContent()
    {
        list.Clear();
        pnlScience.ReloadContent();
        
        list.ReloadList();
        list.ApplyFilter(filter);

        foreach (var grp in list.Groups)
        {
            foreach (var proj in grp.ProjectElements)
            {
                proj.OnDailyCostChanged += OnDailyCostChanged;
                proj.OnProjectUnlocked += OnProjectUnlocked;
            }
        }
    }

    private void OnProjectUnlocked()
    {
        RefreshContent();
    }

    private void OnDailyCostChanged()
    {
        pnlScience.ReloadContent();
    }

    async void OnCloseButtonClicked()
    {
        if (MustPay)
        {
            var confirm = await dialogService.ConfirmAsync("LV.SP.SkipConfirm", true);

            if (confirm)
            {
                OnUICancelled();
            }
        }
        else
        {
            OnUICancelled();
        }
    }

    public async Task<bool> ShowAsync()
    {
        return await ShowAsync(null, panelStack);
    }

}
