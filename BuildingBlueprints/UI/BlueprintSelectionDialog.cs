namespace BuildingBlueprints.UI;

[BindTransient]
public class BlueprintSelectionDialog(
    BuildingBlueprintsService blueprintService,
    BuildingBlueprintPersistentService persistentService,
    IGoodService goods,

    ILoc t,
    VisualElementInitializer veInit,
    PanelStack panelStack
) : DialogBoxElement
{

#nullable disable
    VisualElement lstBlueprints;
    Button btnBuild;
#nullable enable

    ParsedBlueprintInfo? SelectingBlueprint => bpEls.FirstOrDefault(el => el.Selected)?.Blueprint;
    readonly List<BuildingBlueprintElement> bpEls = [];

    public async Task<ParsedBlueprintInfo?> PickAsync()
    {
        SetTitle(t.T("LV.BB.SelectTitle"));
        AddCloseButton();
        SetDialogPercentSize(height: .75f);

        var parent = Content;

        var commands = parent.AddRow().AlignItems().SetMarginBottom(5);
        commands.AddLabel(t.T("LV.BB.SelectDesc"));
        commands.AddChild().SetMarginLeftAuto();
        commands.AddGameButtonPadded(t.T("LV.BB.Browse"), onClick: persistentService.ShowFolder).SetMarginRight(5);
        commands.AddGameButtonPadded(t.T("LV.BB.Reload"), onClick: RefreshCache);

        btnBuild = parent.AddMenuButton(t.T("LV.BB.Build"), onClick: OnUIConfirmed, stretched: true);
        btnBuild.enabledSelf = false;

        lstBlueprints = parent.AddChild();
        ReloadContent();

        var confirmed = await ShowAsync(veInit, panelStack);
        return confirmed ? SelectingBlueprint : null;
    }

    void RefreshCache()
    {
        blueprintService.GetParsedBlueprints(true);
        ReloadContent();
    }

    void ReloadContent()
    {
        lstBlueprints.Clear();
        bpEls.Clear();

        foreach (var bp in blueprintService.GetParsedBlueprintsWithValidation())
        {
            BuildingBlueprintElement el = new(bp, t, goods);
            bpEls.Add(el);
            el.BlueprintSelected += () => OnBlueprintSelected(el);

            lstBlueprints.Add(el.SetMarginBottom());
        }

        if (bpEls.Count == 0)
        {
            lstBlueprints.AddLabel(t.T("LV.BB.NoBlueprints"));
        }
        else
        {
            var el = bpEls.FirstOrDefault(e => !e.Invalid);
            if (el is not null)
            {
                el.Selected = true;
                OnBlueprintSelected(el);
            }
        }
    }

    void OnBlueprintSelected(BuildingBlueprintElement el)
    {
        foreach (var other in bpEls)
        {
            if (other != el && other.Selected)
            {
                other.Selected = false;
            }
        }

        btnBuild.enabledSelf = true;
    }

}
