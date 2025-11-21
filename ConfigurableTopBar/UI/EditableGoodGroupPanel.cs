namespace ConfigurableTopBar.UI;

public class EditableGoodGroupPanel : CollapsiblePanel
{
    readonly ILoc t;
    readonly IContainer container;
    readonly GoodSpriteProvider goodSpriteProvider;
    readonly TopBarConfigProvider provider;
    readonly DialogService diag;
    readonly Image icon;
    readonly TextField txtId, txtName;
    readonly VisualElement lstGoods, moveButtons, editors, groupButtons;
    readonly Button btnMoveGoodsHere;

    public EditableGoodGroupSpec Spec { get; private set; } = null!;

    public EditableGoodGroupPanel(
        ILoc t,
        IContainer container,
        GoodSpriteProvider goodSpriteProvider,
        TopBarConfigProvider provider,
        DialogService diag
    )
    {
        this.t = t;
        this.container = container;
        this.goodSpriteProvider = goodSpriteProvider;
        this.provider = provider;
        this.diag = diag;

        style.borderBottomColor = Color.gray;
        style.borderBottomWidth = 1;

        groupButtons = Container.AddRow();
        groupButtons.AddChild().SetMarginLeftAuto();
        btnMoveGoodsHere = groupButtons.AddGameButtonPadded(t.T("LV.CTB.MoveHere"), onClick: MoveGoodsHere);
        groupButtons.AddGameButtonPadded(t.T("LV.CTB.DeleteGroup"), onClick: OnDeleteRequested);

        editors = Container.AddChild();

        editors.AddLabel(t.T("LV.CTB.Id"));
        txtId = editors.AddTextField(changeCallback: v => Spec.Id = v).SetMarginBottom(10);

        editors.AddLabel(t.T("LV.CTB.Name"));
        txtName = editors.AddTextField(changeCallback: OnNameChanged).SetMarginBottom(10);

        var iconRow = editors.AddRow().AlignItems().SetMarginBottom();
        iconRow.AddLabel(t.T("LV.CTB.Icon")).SetMarginRight();
        icon = iconRow.AddImage().SetSize(30, 30).SetMarginRight();
        iconRow.AddMenuButton(t.T("LV.CTB.SelectIcon"), onClick: SelectIcon);

        Container.AddLabel(t.T("LV.CTB.GoodList"));
        lstGoods = Container.AddChild();

        moveButtons = Container.AddRow().SetFlexShrink(0);
        moveButtons.InsertSelfAfter(HeaderLabel);
        moveButtons.AddMoveButton(true, Move);
        moveButtons.AddMoveButton(false, Move);

        SetSelectingEmpty(provider.IsSelectingGoodsEmpty);
    }

    public void SetSelectingEmpty(bool empty) => btnMoveGoodsHere.SetEnabled(!empty);

    public EditableGoodGroupPanel Init(EditableGoodGroupSpec spec)
    {
        Spec = spec;

        OnNameChanged(spec.Name);
        txtId.text = spec.Id;
        txtName.text = spec.Name;

        if (!goodSpriteProvider.TryGetIcon(spec.Icon, out var sprite))
        {
            spec.Icon = GoodSpriteProvider.QuestionMarkPath;
        }
        icon.sprite = sprite.Asset;

        PopulateGoodList();

        if (spec.Id == EditableGoodGroupSpec.SpecialGroupId)
        {
            groupButtons.enabledSelf = moveButtons.enabledSelf = editors.enabledSelf = false;
        }

        return this;
    }

    void MoveGoodsHere() => provider.MoveGoods(Spec);

    void PopulateGoodList()
    {
        lstGoods.Clear();

        foreach (var g in Spec.Goods)
        {
            var selected = provider.IsGoodSelecting(g.Id);
            var el = lstGoods.AddChild(() => container.GetInstance<EditableGoodPanel>().Init(g, selected));
            el.MoveRequested += OnGoodMoveRequested;
            el.CheckedChanged += OnGoodCheckChanged;
        }
    }

    void OnGoodCheckChanged(object sender, bool e)
    {
        var id = ((EditableGoodPanel)sender).GoodSpec.Id;
        provider.ChangeSelectingGood(id, e);
    }

    void OnGoodMoveRequested(object sender, bool e)
    {
        var el = (EditableGoodPanel)sender;
        var good = el.GoodSpec;

        if (!provider.MoveGood(Spec, good, e)) { return; }
        var index = lstGoods.IndexOf(el);
        lstGoods.Insert(e ? (index - 1) : (index + 1), el);
    }

    void OnNameChanged(string name)
    {
        Spec.Name = name;
        SetTitle(t.T("LV.CTB.GroupHeader", name, Spec.Goods.Count));
    }

    async void SelectIcon()
    {
        var diag = container.GetInstance<SelectIconDialog>();
        var icon = await diag.ShowAsync();
        if (icon is null) { return; }

        Spec.Icon = icon.Path;
        this.icon.sprite = icon.Asset;
    }

    async void OnDeleteRequested()
    {
        if (!await diag.ConfirmAsync(t.T("LV.CTB.DeleteConfirm", Spec.Name))) { return; }

        provider.DeleteGroup(Spec);
    }

    void Move(bool up)
    {
        if (!provider.MoveGroup(Spec, up)) { return; }

        var index = parent.IndexOf(this);
        parent.Insert(up ? (index - 1) : (index + 1), this);
    }

}
