namespace DynamicTailsBanners.UI.DynamicOptions;

[BindTransient]
public class DecalRow(IDecalService decalService, ILoc t) : VisualElement
{

    public event EventHandler<DecalRow> OnRemoveRequested = null!;

    public Decal Decal { get; private set; }

    public DecalRow SetDecal(Decal decal)
    {
        Decal = decal;

        this.SetAsRow().AlignItems();

        var texture = decalService.GetDecalTexture(decal);
        this.AddIconSpan(texture, postfixText: decal.Id, size: 20).SetFlexGrow().SetFlexShrink().SetMarginRight(5);
        this.AddGameButtonPadded(t.T("LV.DTB.Remove"), onClick: () => OnRemoveRequested(this, this));

        return this;
    }

}
