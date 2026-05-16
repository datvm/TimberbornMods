namespace MoreOverlay.Services.Providers;

[MultiBind(typeof(IMoreOverlayProvider))]
public class GrowableOverlayProvider(MoreOverlayHighlighter highlighter, NamedIconProvider namedIconProvider, ILoc t, IGoodService goodService) : ComponentOverlayProviderBase<Growable, GrowableOverlayInstance>
{
    protected override GrowableOverlayInstance? CreateInstance(MoreOverlayComponent overlayComp, Growable comp)
        => new(overlayComp, comp, highlighter, namedIconProvider, t, goodService);
}

public class GrowableOverlayInstance(MoreOverlayComponent overlayComp, Growable comp, MoreOverlayHighlighter highlighter, NamedIconProvider namedIconProvider, ILoc t, IGoodService goodService) : ComponentOverlayInstanceBase<Growable>(overlayComp, comp), ITickableMoreOverlayInstance
{
    readonly LivingNaturalResource? livingNaturalResource = comp.GetComponent<LivingNaturalResource>();
    readonly List<IDyingProgressProvider> dyingProviders = comp.GetComponentsAllocating<IDyingProgressProvider>();
    IDyingProgressProvider? dying;

    readonly Cuttable? cuttable = comp.GetComponent<Cuttable>();
    readonly GatherableYieldGrower? yield = comp.GetComponent<GatherableYieldGrower>();

#nullable disable
    IconSpan status;
    VisualElement growthPanel;
    Label growthLabel;
#nullable enable

    VisualElement? yeildPanel;
    Label? yieldLabel;
    bool dead;

    bool hasCuttable, hasGatherable;

    public override void Initialize(VisualElement container)
    {
        base.Initialize(container);

        status = el.AddIconSpan().SetImageSize(IconSize);

        growthPanel = container.AddRow().AlignItems().SetMarginBottom(5);
        hasCuttable = cuttable;
        hasGatherable = yield;

        var growthIcon = growthPanel.AddIconSpan().SetMarginRight(5);
        if (hasCuttable)
        {
            var yield = cuttable!.YielderSpec.Yield;
            growthIcon.SetGood(goodService, yield.Id, yield.Amount.ToString(), size: IconSize);
        }
        else
        {
            growthIcon.SetImage(namedIconProvider.GetOrLoadGameIcon("plants-growth", "plants-growth"), IconSize);
        }
        growthLabel = growthPanel.AddLabel();

        if (hasGatherable)
        {
            yeildPanel = container.AddRow().AlignItems().SetMarginBottom(5);
            var gatherIcon = yeildPanel.AddIconSpan().SetMarginRight(5);
            var yield = this.yield!._gatherable.YielderSpec.Yield;
            gatherIcon.SetGood(goodService, yield.Id, yield.Amount.ToString(), size: IconSize);
            yieldLabel = yeildPanel.AddLabel();
        }

        if (livingNaturalResource)
        {
            livingNaturalResource!.Died += (_, _) => OnDead();
            if (livingNaturalResource.IsDead) { OnDead(); }
        }
    }

    public override void OnShow()
    {
        foreach (var p in dyingProviders)
        {
            p.StartedDying += OnDyingStatusChanged;
            p.StoppedDying += OnDyingStatusChanged;
        }

        base.OnShow();
    }

    public override void OnHide()
    {
        base.OnHide();

        foreach (var p in dyingProviders)
        {
            p.StartedDying -= OnDyingStatusChanged;
            p.StoppedDying -= OnDyingStatusChanged;
        }
    }

    void OnDyingStatusChanged(object sender, EventArgs e)
    {
        UpdateStatus();
    }

    void OnDead()
    {
        if (dead) { return; }

        if (yeildPanel is not null)
        {
            yeildPanel.RemoveFromHierarchy();
            yeildPanel = null;
            yieldLabel = null;
        }

        growthLabel.text = "";

        dead = true;
        dying = null;
        UpdateData();
    }

    void UpdateStatus()
    {
        status.SetDisplay(true);

        if (dead)
        {
            status.SetContent(namedIconProvider.GetOrLoadGameIcon("ico-death", "ico-death"), null, "");
            return;
        }

        foreach (var p in dyingProviders)
        {
            if (!p.DyingProgress.IsDying)
            {
                continue;
            }

            dying = p;
            var icon = p switch
            {
                WateredNaturalResource => namedIconProvider.GetOrLoadStatusIcon("DryingNaturalResource"),
                LivingWaterNaturalResource w => namedIconProvider.GetOrLoadStatusIcon(w.DeathByFlooding ? "TooMuchWater" : "NotEnoughWater"),
                ContaminatedNaturalResource => namedIconProvider.GetOrLoadStatusIcon("ContaminatedNaturalResource"),
                _ => null,
            };
            status.SetImage(icon!, (int?)null);

            SetDyingText();
            return;
        }

        status.SetDisplay(false);
    }

    void SetDyingText()
    {
        if (dying is null) { return; }

        status.SetPostfixText(dying.DyingProgress.DaysLeft.ToDays(t));
    }

    public override void UpdateData()
    {
        UpdateStatus();
        OnTickUpdate();
    }

    public void OnTickUpdate()
    {
        if (dead) { return; }

        SetDyingText();

        var growthProgress = 1f;
        if (growthPanel is not null)
        {
            var growthTime = Component._timeTrigger;
            growthProgress = growthTime.Progress;
            if (growthProgress >= 1f)
            {
                growthLabel.text = "";

                if (!hasCuttable)
                {
                    growthPanel.RemoveFromHierarchy();
                    growthPanel = null!;
                    growthLabel = null!;
                }
            }
            else
            {
                var total = Component.GrowthTimeInDays;
                growthLabel.text = $"{growthProgress * total:F1}/{total:F1}d";
            }
        }

        if (hasGatherable)
        {
            var yieldTime = yield!._timeTrigger;
            var yieldProgress = yieldTime.Progress;
            if (yieldProgress >= 1f)
            {
                yieldLabel!.text = "";
            }
            else
            {
                var yieldTotalTime = yield._gatherable.YieldGrowthTimeInDays;
                yieldLabel!.text = $"{yieldProgress * yieldTotalTime:F1}/{yieldTotalTime:F1}d";
            }


            growthProgress = growthProgress / 2 + yieldProgress;
        }

        highlighter.Highlight(OverlayComponent, growthProgress, 100);
    }
}
