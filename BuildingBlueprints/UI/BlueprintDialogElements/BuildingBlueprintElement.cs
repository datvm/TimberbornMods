namespace BuildingBlueprints.UI.BlueprintDialogElements;

[BindTransient]
public class BuildingBlueprintElement(
    ILoc t,
    IGoodService goods,
    NamedIconProvider namedIconProvider
) : VisualElement
{

    public event Action BlueprintSelected = null!;
    public event Action UnlockRequested = null!;
    public event Action EditRequested = null!;

#nullable disable
    public BlueprintWithValidation Blueprint { get; private set; }
#nullable enable

    public bool Invalid { get; private set; }

    public void Init(BlueprintWithValidation info)
    {
        this.SetBorder();

        Blueprint = info;
        var bp = info.Blueprint;

        var title = bp.Name.Bold();
        var invalid = Invalid = info.Invalid;
        title = invalid ? title.Color(TimberbornTextColor.Red) : title.Highlight();

        var isLocal = bp.RawInfo.Source.IsLocal;
        var header = this.AddRow().AlignItems().SetMarginBottom(5);
        header.AddImage(BuildingBlueprintSourceInfo.GetIcon(isLocal, namedIconProvider)).SetSize(20).SetMarginRight(5);
        header.AddLabel(title).SetFlexGrow().SetFlexShrink();

        if (isLocal)
        {
            var btnEdit = header.AddGameButtonPadded(t.T("LV.BB.Edit"), onClick: () => EditRequested()).SetMarginRight(10);
        }
        var btnBuild = header.AddMenuButton(t.T("LV.BB.Build"), onClick: () => BlueprintSelected());

        var hasLockedTools = info.HasLockedTools;
        if (hasLockedTools)
        {
            var btn = this.AddGameButtonPadded(onClick: () => UnlockRequested())
                .SetAsRow().AlignItems()
                .SetMarginBottom(10);

            btn.AddIconSpan().SetScience(namedIconProvider, info.ScienceCost.ToString()).SetMarginRight(10).SetVertical(false);
            btn.AddGameLabel(t.T("LV.BB.UnlockBuildings"));
        }

        var tagsRow = this.AddRow().SetWrap().SetMarginBottom(10);
        foreach (var tag in bp.Tags)
        {
            tagsRow.AddGameLabel(tag.Italic()).SetMarginRight(10);
        }

        if (invalid)
        {
            var errors = "";

            if (info.MissingTemplates.Count > 0)
            {
                errors = t.T("LV.BB.MissingBuildings", string.Join(", ", info.MissingTemplates));
            }

            if (hasLockedTools)
            {
                errors += t.T("LV.BB.LockedBuildings", string.Join(", ", info.LockedTools.Select(b => t.T(b.LabeledEntitySpec!.DisplayNameLocKey))));
            }

            this.AddLabel(errors.Color(TimberbornTextColor.Red)).SetMarginBottom(10);
            btnBuild.enabledSelf = false;
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

    public bool Filter(BlueprintListingSettings settings)
    {
        var match = Match();
        this.SetDisplay(match);
        return match;

        bool Match()
        {
            var bp = Blueprint;
            if (settings.HideInvalids && bp.HasMissingTemplates) { return false; }

            var raw = bp.Blueprint.RawInfo;
            var isLocal = raw.Source.IsLocal;
            if ((isLocal && !settings.ShowLocal) || (!isLocal && !settings.ShowNonLocal)) { return false; }

            var tags = raw.Tags;
            var filteredTags = settings.FilteredTags;
            if (filteredTags.Count > 0 && filteredTags.Any(t => !tags.Contains(t)))
            {
                return false;
            }

            var kw = settings.FilterKeyword;
            if (!string.IsNullOrEmpty(kw))
            {
                if (!raw.Name.Contains(kw, StringComparison.OrdinalIgnoreCase)
                    && !tags.Any(t => t.Contains(kw, StringComparison.OrdinalIgnoreCase)))
                {
                    return false;
                }
            }

            return true;
        }
    }

}
