namespace DistroStorage.UI;

public class DistroPanel<T> : VisualElement where T : IDistroComponent
{

    public bool IsSender { get; }
    public T? Component { get; private set; }

    readonly VisualElement connectionList;
    readonly Dictionary<IDistroComponent, ConnectedComponentRow> connectedComponents = [];
    
    readonly VisualElement goodsList;
    readonly HashSet<string> showingGoods = [];

    readonly ILoc t;
    readonly IGoodService goods;
    readonly Label lblConnectedCounter;

    public event Action<BaseComponent> OnSelectRequested = null!;

    readonly Toggle chkEnabled;
    readonly string enabledText, disabledText;

    readonly ProgressBarWithLabel pgbTransfer;
    readonly PriorityToggleGroup? optPriority;

    public bool Visible => this.IsDisplayed();

    public DistroPanel(ILoc t, IGoodService goods, BuilderPriorityToggleGroupFactory priorityFac)
    {
        this.t = t;
        this.goods = goods;

        enabledText = t.T("LV.DS.Enabled").Color(TimberbornTextColor.Green);
        disabledText = t.T("LV.DS.Disabled").Color(TimberbornTextColor.Red);

        var isSender = IsSender = typeof(T) == typeof(IDistroSender);
        var distroType = isSender ? "Sender" : "Receiver";

        this.AddLabel(t.T("LV.DS.Distro" + distroType).Highlight()).SetMarginBottom(10);

        var pnlEnabled = this.AddChild().SetMarginBottom(10);
        chkEnabled = pnlEnabled.AddToggle(t.T("LV.DS.Enabled"), onValueChanged: OnEnabledChanged);
        pnlEnabled.AddLabel(t.T("LV.DS.EnabledDesc" + distroType));

        if (isSender)
        {
            pgbTransfer = this.AddProgressBarWithLabel(color: ProgressBarColor.Teal);
            pgbTransfer.ProgressBar.SetMarginBottom(10);
        }
        else
        {
            var priContainer = this.AddChild().SetMarginBottom(10);
            optPriority = priorityFac.Create(priContainer, "LV.DS.Priority");            
        }

        goodsList = this.AddRow().SetWrap().SetMarginBottom();

        var pnlConnections = this.AddCollapsiblePanel(t.T($"LV.DS.InRangeFor{distroType}"), false);
        connectionList = pnlConnections.Container.AddChild();
        lblConnectedCounter = pnlConnections.AddLabel("").SetFlexShrink(0);
        lblConnectedCounter.InsertSelfAfter(pnlConnections.HeaderLabel);
    }

    public void SetComponent(T? comp)
    {
        if (comp is not null && !comp.Active) { comp = default; }

        if (Equals(Component, comp)) { return; }

        ClearInfo();
        Component = comp;

        if (comp is not null)
        {
            if (!IsSender && comp is IDistroReceiver r)
            {
                optPriority!.Enable(r);
            }

            this.SetDisplay(true);
        }
    }

    public void Update()
    {
        var c = Component;
        if (c is null) { return; }

        chkEnabled.SetValueWithoutNotify(c.Enabled);

        if (IsConnectionsChanged(c))
        {
            PopulateConnectionList();
        }

        UpdateStatus();
        UpdateGoodList();

        if (IsSender)
        {
            UpdateSenderInfo();
        }
        else
        {
            optPriority!.UpdateGroup();
        }
    }

    bool IsConnectionsChanged(T comp)
    {
        if (connectedComponents.Count != comp.Connections.Count) { return true; }

        foreach (var c in comp.Connections)
        {
            if (!connectedComponents.ContainsKey(c)) { return true; }
        }

        return false;
    }

    void PopulateConnectionList()
    {
        connectionList.Clear();
        connectedComponents.Clear();

        foreach (var other in Component!.Connections)
        {
            var row = connectionList.AddRow().AlignItems().SetMarginBottom(5);

            var otherComp = (BaseComponent)other;
            row.AddGameButtonPadded(t.T("LV.DS.Select"), onClick: () => OnSelectRequested(otherComp), paddingY: 2)
                .SetFlexShrink(0).SetMarginRight(10);
            row.AddLabel(otherComp.GetName(t)).SetFlexGrow(1);
            var lblStatus = row.AddLabel("").SetFlexShrink(0);

            connectedComponents.Add(other, new(row, lblStatus));
        }

        var activeCount = connectedComponents.Count(c => c.Key.Active);
        lblConnectedCounter.text = activeCount.ToString();
        if (activeCount == 0)
        {
            connectionList.AddLabel(t.T("LV.DS.NoneInRange"));
        }
    }

    void UpdateStatus()
    {
        foreach (var (c, (row, lbl)) in connectedComponents)
        {
            if (c.Active)
            {
                row.SetDisplay(true);
                lbl.text = c.Enabled ? enabledText : disabledText;
            }
            else
            {
                row.SetDisplay(false);
            }
        }
    }

    void UpdateGoodList()
    {
        var goodIds = Component!.GoodIds.ToArray();
        if (showingGoods.SetEquals(goodIds)) { return; }

        goodsList.Clear();
        showingGoods.Clear();

        foreach (var g in goodIds)
        {
            goodsList.AddIconSpan().SetGood(goods, g, showName: true).SetMarginRight();
        }
    }

    void UpdateSenderInfo()
    {
        var s = (IDistroSender)Component!;

        pgbTransfer.SetProgress(
            s.TransferProgress,
            t.T("LV.DS.NextTransfer", s.NextTransferTime));
    }

    public void ClearInfo()
    {
        Component = default;

        connectionList.Clear();
        connectedComponents.Clear();

        goodsList.Clear();
        showingGoods.Clear();

        optPriority?.Disable();

        this.SetDisplay(false);
    }

    void OnEnabledChanged(bool enabled) => Component?.SetEnabled(enabled);

    readonly record struct ConnectedComponentRow(VisualElement Row, Label Status);
}
