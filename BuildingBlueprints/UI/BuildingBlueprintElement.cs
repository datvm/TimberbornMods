namespace BuildingBlueprints.UI;

public class BuildingBlueprintElement : VisualElement
{

    readonly Toggle chkTitle;

    public bool Selected
    {
        get => chkTitle.value;
        set => chkTitle.SetValueWithoutNotify(value && !Invalid);
    }

    public event Action BlueprintSelected = null!;

    public ParsedBlueprintInfo Blueprint { get; }
    public bool Invalid { get; }

    public BuildingBlueprintElement(BlueprintWithValidation info, ILoc t, IGoodService goods)
    {
        var bp = Blueprint = info.Blueprint;

        var title = bp.Name.Bold();
        var invalid = Invalid = info.Invalid;
        title = invalid ? title.Color(TimberbornTextColor.Red) : title.Highlight();
        chkTitle = this.AddToggle(title, onValueChanged: OnChecked).SetMarginBottom(10);

        if (invalid)
        {
            var errors = "";

            if (info.MissingTemplates.Count > 0)
            {
                errors = t.T("LV.BB.MissingBuildings", string.Join(", ", info.MissingTemplates));
            }

            if (info.LockedTools.Count > 0)
            {
                errors += t.T("LV.BB.LockedBuildings", string.Join(", ", info.LockedTools.Select(b => t.T(b.LabeledEntitySpec!.DisplayNameLocKey))));
            }

            this.AddLabel(errors.Color(TimberbornTextColor.Red)).SetMarginBottom(10);
            chkTitle.enabledSelf = false;
        }

        var buildings = this.AddRow().AlignItems().SetWrap().SetMarginBottom(10);
        foreach (var (b, count) in bp.BuildingsCount)
        {
            if (!b.Missing)
            {
                buildings.AddIconSpan(b.LabeledEntitySpec!.Icon.Asset, prefixText: $"×{count}", size: 32);
            }
        }

        var costs = this.AddRow().AlignItems().SetWrap();
        foreach (var g in bp.Costs)
        {
            costs.AddIconSpan().SetGood(goods, g.GoodId, g.Amount.ToString()).SetMarginRight(10);
        }
    }

    void OnChecked(bool check)
    {
        if (Invalid)
        {
            chkTitle.SetValueWithoutNotify(false);
            return;
        }

        if (check)
        {
            BlueprintSelected();
        }
        else
        {
            // Should not deselect
            chkTitle.SetValueWithoutNotify(true);
        }
    }

}
