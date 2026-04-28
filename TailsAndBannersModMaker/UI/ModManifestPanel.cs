namespace TailsAndBannersModMaker.UI;

[BindTransient(Contexts = BindAttributeContext.MainMenu)]
public class ModManifestPanel(ILoc t) : CollapsiblePanel
{

    public ModManifestPanel SetMod(ModMakerInfo mod)
    {
        this.SetMarginBottom(10).SetBorder();


        SetTitle(t.T("LV.TBMM.ModInfo"));

        AddPair("LV.TBMM.ModId", mod.Id, t => mod.Id = t);
        AddPair("LV.TBMM.ModName", mod.Name, t => mod.Name = t);
        AddPair("LV.TBMM.ModVersion", mod.Version, t => mod.Version = t);

        return this;

        TextField AddPair(string labelLoc, string defaultValue, Action<string>? onTextChanged) 
            => Container.AddPair(labelLoc, defaultValue, onTextChanged, t);
    }

}
