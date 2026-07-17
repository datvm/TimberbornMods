namespace PowerLines.UI;

[BindFragment]
public class PowerLineFragment(
    ILoc t,
    PowerLineConnectionService connService,
    IContainer container,
    EntitySelectionService entitySelectionService,
    PowerConnectionTool connectionTool,
    ToolService toolService,
    PowerLineRenderer renderer
) : BaseEntityPanelFragment<PowerLineComponent>
{

    VisualElement connList = null!;
    readonly List<PowerLineConnectionElement> connEls = [with(10)]; // Should be more than enough
    Toggle chkShowConnections = null!;

    protected override void InitializePanel()
    {
        chkShowConnections = panel.AddGamePanelToggle(t.T("LV.PL.ShowConnections"), OnShowConnectionsChanged).SetMarginBottom();

        panel.AddLabel(t.T("LV.PL.Connections")).SetMarginBottom(5);
        connList = panel.AddChild(classes: ["zipline-tower-fragment__buttons"]).SetMarginBottom();
    }

    void OnShowConnectionsChanged(bool value) => renderer.ToggleRendering(value);

    public override void ShowFragment(BaseComponent entity)
    {
        base.ShowFragment(entity);
        if (!component) { return; }

        chkShowConnections.SetValueWithoutNotify(renderer.ShouldShow);
        RefreshConnections();
    }

    void RefreshConnections()
    {
        if (!component) { return; }

        var max = component!.MaxConnections;
        EnsureElementCount(max);

        var connected = connService.GetConnectedPowerLines(component).ToArray();
        var i = 0;
        for (; i < connected.Length && i < max; i++)
        {
            connEls[i].SetDisplay(true);

            var target = connected[i];
            var labelEntity = target.GetComponent<LabeledEntity>();
            
            connEls[i].SetConnection(new PowerLineDisplay(target, labelEntity.Image, target.GetName(t)));
        }

        if (i < max)
        {
            connEls[i].SetDisplay(true);
            connEls[i].ClearConnection();
            i++;
        }

        for (; i < max; i++)
        {
            connEls[i].SetDisplay(true);
            connEls[i].SetEmpty();
        }

        for (; i < connEls.Count; i++)
        {
            connEls[i].SetDisplay(false).ClearConnection();
        }
    }

    void EnsureElementCount(int count)
    {
        while (connEls.Count < count)
        {
            var el = container.GetInstance<PowerLineConnectionElement>();
            el.OnMakeConnectionRequested += OnMakeConnectionRequested;
            el.OnSelectRequested += OnSelectRequested;
            el.OnRemoveRequested += OnRemoveRequested;
            connList.Add(el);
            connEls.Add(el);
        }
    }

    void OnMakeConnectionRequested(object? sender, PowerLineConnectionElement el)
    {
        if (!component) { return; }

        connectionTool.SwitchTo(component!);
        toolService.SwitchTool(connectionTool);
    }

    void OnSelectRequested(object? sender, PowerLineComponent other) => entitySelectionService.SelectAndFocusOn(other);

    void OnRemoveRequested(object? sender, PowerLineComponent other)
    {
        if (!component) { return; }

        var c = component!;
        var conn = connService.GetConnections(c)
            .FirstOrDefault(c => c.A == other || c.B == other);
        connService.DisconnectConnection(conn);

        // Reselect owner so the fragment rebuilds
        entitySelectionService.Unselect();
        entitySelectionService.Select(c);
    }

    public override void ClearFragment()
    {
        base.ClearFragment();

        foreach (var el in connEls)
        {
            el.SetDisplay(false).ClearConnection();
        }
    }

}
