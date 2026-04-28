namespace TailsAndBannersModMaker.UI;

[BindTransient(Contexts = BindAttributeContext.MainMenu)]
public class DecalPickerDialog(
    VisualElementInitializer veInit,
    PanelStack panelStack,
    DecalProvider decalProvider,
    UserDecalTextureRepository repo,
    IExplorerOpener opener,
    ILoc t
) : DialogBoxElement
{
    readonly List<DecalElement> decals = [];

    void Init(string type)
    {
        const int imageSize = 100;

        var typeName = t.TDecal(type);

        AddCloseButton();
        SetTitle(t.T("LV.TBMM.AddDecals", typeName));
        SetDialogSize(1200, 800);

        var parent = Content;

        var pnlOpen = parent.AddRow().SetMarginBottom(10).AlignItems();
        pnlOpen.AddGameButtonPadded(t.T("LV.TBMM.OpenFolder", typeName), () => OpenFolder(type)).SetMarginRight(10);
        pnlOpen.AddLabel(t.T("LV.TBMM.OpenFolderNote"));

        var pnlFilter = parent.AddRow().AlignItems().SetMarginBottom(10);
        pnlFilter.AddLabel(t.T("LV.TBMM.Filter")).SetMarginRight(10);
        pnlFilter.AddTextField(changeCallback: kw => Filter(kw)).SetFlexGrow();

        parent.AddMenuButton(t.T("LV.TBMM.AddButton", typeName), OnUIConfirmed, stretched: true).SetMarginBottom(10);

        var pnlDecals = parent.AddRow().SetWrap();
        var decals = decalProvider.GetDecals(type);
        foreach (var d in decals)
        {
            var el = pnlDecals.AddChild().SetMargin(right: 10, bottom: 10);
            el.SetBorder();

            var img = el.AddImage().SetSize(imageSize).SetMarginBottom(5);
            var s = img.style;
            s.backgroundImage = new(d.Texture.Asset);
            s.backgroundSize = new(new BackgroundSize(BackgroundSizeType.Contain));
            s.backgroundPositionX = s.backgroundPositionY = new(new BackgroundPosition(BackgroundPositionKeyword.Center));

            var chk = el.AddToggle(d.Id).SetWidth(imageSize);
            chk.style.textOverflow = TextOverflow.Ellipsis;

            this.decals.Add(new(el, chk, d));
        }
    }

    public async Task<DecalSpec[]> PickAsync(string type)
    {
        Init(type);
        if (!await ShowAsync(veInit, panelStack)) { return []; }

        return [..decals.Where(d => d.Selected).Select(d => d.DecalSpec)];
    }

    void OpenFolder(string type) => opener.OpenDirectory(repo.GetCustomDecalDirectory(type));

    void Filter(string kw)
    {
        kw = kw.ToUpperInvariant().Trim();
        var hasKw = kw.Length > 0;

        foreach (var d in decals)
        {
            d.Element.SetDisplay(!hasKw || d.Id.ToUpperInvariant().Contains(kw));
        }
    }

    readonly record struct DecalElement(VisualElement Element, Toggle Toggle, DecalSpec DecalSpec)
    {
        public string Id => DecalSpec.Id;
        public bool Selected => Toggle.value;
    }
}
