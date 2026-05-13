namespace BeaverChronicles.UI;

[BindSingleton]
public class ChronicleEventPanel(
    UILayout uILayout,
    ILoc t,
    ActiveChronicleEventService activeService,
    NamedIconProvider namedIconProvider,
    IGoodService goods,
    ChronicleEventUIHelper uiHelper
) : NineSliceVisualElement, ILoadableSingleton, ITickableSingleton
{
    static readonly Phrase DayPhrase = Phrase.New().FormatDays<float>("F1");
    static readonly Phrase HourPhrase = Phrase.New().FormatHours<float>("F1");

#nullable disable
    IconSpan lblTimer;
    VisualElement lstPayment;
    VisualElement descContainer;
    CollapsiblePanel collapsible;
#nullable enable

    bool showingTimer;

    public void Load()
    {
        this.SetWidth(300)
            .SetMarginBottom(80)
            .AddClass("square-large--green")
            .SetPadding(10)
            .SetDisplay(false);

        collapsible = this.AddCollapsiblePanel();
        collapsible.HeaderLabel.SetFlexShrink();

        var parent = collapsible.Container;
        descContainer = parent.AddChild().SetMarginBottom(5).SetDisplay(false);
        lblTimer = parent.AddIconSpan().SetTime(namedIconProvider, "").SetVertical(false).SetMarginBottom(5).SetDisplay(false);
        lstPayment = parent.AddChild().SetDisplay(false).SetMarginBottom(10);

        parent.AddMenuButton(t.T("LV.BCEv.Details"), uiHelper.ShowChronicleDialog, stretched: true);

        uILayout.AddBottomRight(this, 1);

        activeService.OnEventChanged += OnActiveEventChanged;
        activeService.OnActiveDescriptionChanged += OnDescChanged;
        activeService.OnHasTimeLimitChanged += OnTimeLimitChanged;
        activeService.OnPaymentChanged += OnPaymentChanged;

        OnActiveEventChanged(this, activeService.ActiveEvent);
        OnTimeLimitChanged(this, activeService.HasTimeLimit);
        OnDescChanged(this, activeService.ActiveDescription);
        OnPaymentChanged(this, activeService.Payment);
    }

    void OnPaymentChanged(object sender, ActiveEventPayment? e)
    {
        lstPayment.Clear();

        if (e is null)
        {            
            lstPayment.SetDisplay(false);
            return;
        }

        if (e.HasScience)
        {
            AddItemRow(e.Science!);
        }
        foreach (var g in e.Goods)
        {
            AddItemRow(g);
        }

        lstPayment.SetDisplay(true);

        void AddItemRow(ActiveEventPaymentItem item)
        {
            var row = lstPayment.AddRow().AlignItems();

            var chk = row.AddToggle().SetMarginRight(5).SetFlexShrink(0);
            chk.SetValueWithoutNotify(item.IsPaid);
            chk.enabledSelf = false;

            var ico = row.AddIconSpan().SetFlexShrink(0);
            var amountText = $"{item.PaidAmount}/{item.Amount}";
            if (item.IsScience)
            {
                ico.SetScience(namedIconProvider);
            }
            else
            {
                ico.SetGood(goods, item.Id, showName: true);
            }

            row.AddChild().SetFlexGrow().SetFlexShrink(1);
            row.AddLabel(amountText).SetFlexShrink(0);
        }
    }

    void OnTimeLimitChanged(object sender, bool e)
    {
        if (!e)
        {
            lblTimer.SetDisplay(false);
            showingTimer = false;

        }
        else
        {
            showingTimer = true;
            ShowTimerTime();
            lblTimer.SetDisplay(true);
        }
    }

    void OnDescChanged(object sender, string? e)
    {
        descContainer.Clear();
        if (e is null)
        {   
            descContainer.SetDisplay(false);
        }
        else
        {
            descContainer.AddChild(() => uiHelper.CreateFormattedText(e));
            descContainer.SetDisplay(true);
        }
    }

    void OnActiveEventChanged(object sender, IChronicleEvent? e)
    {
        if (e is null)
        {
            this.SetDisplay(false);
            return;
        }

        collapsible.SetTitle(t.T(e.NameLoc)).SetExpand(true);
        this.SetDisplay(true);
    }

    public void Tick()
    {
        if (!showingTimer) { return; }

        if (!activeService.HasTimeLimit)
        {
            OnTimeLimitChanged(this, false);
            return;
        }

        ShowTimerTime();
    }

    void ShowTimerTime()
    {
        var remaining = activeService.RemainingDays;
        var text = remaining >= 1 ? t.T(DayPhrase, remaining) : t.T(HourPhrase, remaining * 24);
        lblTimer.SetPostfixText(text);
    }
}
