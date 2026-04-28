namespace TailsAndBannersModMaker.UI;

[BindTransient(Contexts = BindAttributeContext.MainMenu)]
public class DecalInfoPanel(ILoc t) : VisualElement
{

#nullable disable
    Toggle chk;

    public DecalInfo Decal { get; private set; }
#nullable enable

    static readonly string?[] FactionIds = [null, "Folktails", "IronTeeth"];
    readonly Toggle[] chkFactions = new Toggle[3];


    public bool Selected => chk.value;
    public event EventHandler<bool> SelectedChanged = null!;

    public DecalInfoPanel Initialize(DecalInfo decal)
    {
        Decal = decal;
        this.SetAsRow().AlignItems().SetMarginBottom(5);

        chk = this.AddToggle(onValueChanged: v => SelectedChanged(this, v)).SetMarginRight(5).SetFlexShrink(0);

        const int imageSize = 20;
        this.AddImage(decal.Texture).SetSize(imageSize).SetMarginRight(5).SetFlexShrink(0);

        this.AddLabel(decal.Id).SetFlexGrow().SetFlexShrink();

        this.AddLabel(t.T("LV.TBMM.Faction")).SetMarginRight(5);
        for (int i = 0; i < FactionIds.Length; i++)
        {
            var z = i;
            var id = FactionIds[i];
            chkFactions[z] = this.AddToggle(
                t.T(i == 0 ? "LV.TBMM.FactionAny" : $"Faction.{id}.DisplayName"),
                onValueChanged: v => OnFactionChecked(v, z));
        }
        OnFactionChecked(false, 0);

        return this;
    }

    void OnFactionChecked(bool v, int index)
    {
        if (v)
        {
            Decal.FactionId = FactionIds[index];
        }

        for (int i = 0; i < chkFactions.Length; i++)
        {
            chkFactions[i].SetValueWithoutNotify(i == index);
        }
    }

}
