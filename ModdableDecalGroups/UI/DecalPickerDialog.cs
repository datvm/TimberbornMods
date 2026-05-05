namespace ModdableDecalGroups.UI;

[BindTransient]
public class DecalPickerDialog(
    VisualElementInitializer veInit,
    PanelStack panelStack,
    ILoc t,
    DecalGroupService decalGroupService,
    IDecalService decalService
) : DialogBoxElement
{
    bool multiple;
#nullable disable
    string type, decalName;
#nullable enable

    readonly List<PickerItem> items = [];
    int selectionCount = 0;

    Button? btnConfirmPick;
    DecalSpec[] selectedItems = [];

    void Initialize()
    {
        decalName = t.TDecal(type);

        SetTitle(t.T("LV.MDG.PickerTitle", decalName));
        AddCloseButton();

        var parent = Content;

        var row = parent.AddRow().AlignItems().SetMarginBottom();
        row.AddLabel(t.T("LV.MDG.Filter")).SetMarginRight(5);
        row.AddTextField(changeCallback: v => Filter(v)).SetFlexGrow();

        if (multiple)
        {
            btnConfirmPick = row.AddMenuButton(onClick: ConfirmPick).SetMargin(left: 10);
            UpdateSelectionCount();
        }

        var decalContainer = parent.AddScrollView().SetHeight(600);
        PopulateDecals(decalContainer);
    }

    void PopulateDecals(VisualElement container)
    {
        const int Size = 150;

        var grps = decalGroupService.GetGroups(type).Groups;

        foreach (var grp in grps)
        {
            var grpEl = container.AddCollapsiblePanel(grp.Spec.Title.Value);            
            var content = grpEl.Container.AddRow().SetWrap();

            foreach (var decal in grp.Decals)
            {
                var decalEl = content.AddChild().SetMargin(right: 10, bottom: 10);
                var spec = decalService.GetSpec(decal);

                decalEl.AddImage(spec.Texture.Asset).SetSize(Size).SetMarginBottom(5);
                Toggle? toggle = null;
                if (multiple)
                {
                    toggle = decalEl.AddToggle(decal.Id, onValueChanged: OnSelectedChanged).SetWidth(Size);
                }
                else
                {
                    decalEl.AddGameButtonPadded(decal.Id, onClick: () => ConfirmSinglePick(spec)).SetWidth(Size);                    
                }

                items.Add(new(decalEl, spec, toggle));
            }
        }
    }

    void OnSelectedChanged(bool v)
    {
        selectionCount += v ? 1 : -1;
        UpdateSelectionCount();
    }

    void UpdateSelectionCount()
    {
        btnConfirmPick!.text = t.T("LV.MDG.ConfirmPick", selectionCount, decalName);
        btnConfirmPick.enabledSelf = selectionCount > 0;
    }

    void ConfirmPick()
    {
        selectedItems = [.. items.Where(i => i.Selected).Select(i => i.Decal)];
        OnUIConfirmed();
    }

    void ConfirmSinglePick(DecalSpec decal)
    {
        selectedItems = [decal];
        OnUIConfirmed();
    }

    public async Task<DecalSpec?> PickSingleAsync(DecalTypeEnum type) => await PickSingleAsync(type.ToString());
    public async Task<DecalSpec[]?> PickMultipleAsync(DecalTypeEnum type) => await PickMultipleAsync(type.ToString());

    public async Task<DecalSpec?> PickSingleAsync(string type) => (await PickAsync(type, false))?.FirstOrDefault();
    public async Task<DecalSpec[]?> PickMultipleAsync(string type) => await PickAsync(type, true);

    public async Task<DecalSpec[]?> PickAsync(string type, bool multiple)
    {
        this.multiple = multiple;
        this.type = type;

        Initialize();

        if (!await ShowAsync(veInit, panelStack)) { return null; }

        return selectedItems;
    }

    void Filter(string kw)
    {
        kw = kw.Trim().ToUpperInvariant();
        var hasKw = !string.IsNullOrEmpty(kw);

        foreach (var item in items)
        {
            var show = !hasKw || item.Id.ToUpperInvariant().Contains(kw);
            item.Item.SetDisplay(show);
        }
    }

    readonly record struct PickerItem(VisualElement Item, DecalSpec Decal, Toggle? Toggle)
    {
        public string Id => Decal.Id;
        public bool Selected => Toggle?.value == true;
    }

}
