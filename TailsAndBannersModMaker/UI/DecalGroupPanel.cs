namespace TailsAndBannersModMaker.UI;

[BindTransient(Contexts = BindAttributeContext.MainMenu)]
public class DecalGroupPanel(
    ILoc t,
    IContainer container,
    DialogService diag
) : CollapsiblePanel
{

    public event EventHandler OnRemoveRequested = null!;

#nullable disable
    public DecalGroupInfo DecalGroupInfo { get; private set; }
    string type;

    Label lblCounter;
    VisualElement decalElementList;
    Button btnDeleteDecals;
#nullable enable

    int selectedCount = 0;

    public DecalGroupPanel Initialize(DecalGroupInfo info, string type)
    {
        DecalGroupInfo = info;
        this.type = type;
        var typeName = t.TDecal(type);

        SetTitle(info.Name);
        this.SetMarginBottom().SetBorder();

        lblCounter = this.AddLabel("0");
        lblCounter.InsertSelfAfter(HeaderLabel);

        var parent = Container;

        var buttons = parent.AddRow().AlignItems();
        buttons.AddGameButtonPadded(t.T("LV.TBMM.AddDecals", typeName), PickDecals);

        buttons.AddChild().SetMarginLeftAuto();
        btnDeleteDecals = buttons.AddGameButtonPadded(t.T("LV.TBMM.DeleteDecals", 0, typeName), DeleteDecals);
        btnDeleteDecals.enabledSelf = false;

        var isDefault = info.IsDefault;
        if (!isDefault)
        {
            buttons.AddGameButtonPadded(t.T("LV.TBMM.DeleteGroup"), () => OnRemoveRequested(this, EventArgs.Empty)).SetMargin(left: 5);

            var pnlInfo = parent.AddChild().SetMarginBottom(10);
            pnlInfo.AddPair("LV.TBMM.GroupId", info.Id, v => info.Id = v, t).SetMarginBottom(5);
            pnlInfo.AddPair("LV.TBMM.GroupName", info.Name, v =>
            {
                info.Name = v;
                SetTitle(v);
            }, t).SetMarginBottom(5);
            pnlInfo.AddPair("LV.TBMM.GroupOrder", info.Order.ToString(), v =>
            {
                if (int.TryParse(v, out var order))
                {
                    info.Order = order;
                }
            }, t).SetMarginBottom(5);
        }

        decalElementList = parent.AddChild();

        foreach (var d in info.Decals) // Should not have any but just in case
        {
            AddDecalUI(d);
        }
        UpdateCounter();

        return this;
    }

    async void PickDecals()
    {
        var diag = container.GetInstance<DecalPickerDialog>();
        var picked = await diag.PickAsync(type);
        if (picked.Length == 0) { return; }

        foreach (var d in picked)
        {
            AddDecal(d);
        }
    }

    void AddDecal(DecalSpec d)
    {
        var info = new DecalInfo(d);
        DecalGroupInfo.Decals.Add(info);
        AddDecalUI(info);

        UpdateCounter();
    }

    void AddDecalUI(DecalInfo info)
    {
        var el = container.GetInstance<DecalInfoPanel>().Initialize(info);
        el.SelectedChanged += OnItemSelectionChanged;

        decalElementList.Add(el);
    }

    void OnItemSelectionChanged(object sender, bool e)
    {
        selectedCount += e ? 1 : -1;

        btnDeleteDecals.text = t.T("LV.TBMM.DeleteDecals", selectedCount, t.TDecal(type));
        btnDeleteDecals.enabledSelf = selectedCount > 0;
    }

    void UpdateCounter() => lblCounter.text = DecalGroupInfo.Decals.Count.ToString();

    async void DeleteDecals()
    {
        if (selectedCount == 0) { return; }

        if (!await diag.ConfirmAsync(t.T("LV.TBMM.DeleteDecalsConfirm", selectedCount, t.TDecal(type), DecalGroupInfo.Name)))
        {
            return;
        }

        var children = decalElementList.Children().OfType<DecalInfoPanel>().ToArray();
        var list = DecalGroupInfo.Decals;

        for (int i = children.Length - 1; i >= 0; i--)
        {
            var el = children[i];
            if (!el.Selected) { continue; }

            list.RemoveAt(i);
            el.RemoveFromHierarchy();
        }

        UpdateCounter();

        selectedCount = -1;
        OnItemSelectionChanged(null!, true);
    }

}
