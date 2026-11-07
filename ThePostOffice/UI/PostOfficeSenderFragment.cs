namespace ThePostOffice.UI;

[BindFragment]
public class PostOfficeSenderFragment(
    IContainer container,
    ILoc t,
    IGoodService goods,
    DialogService dialogs,
    InputService input,
    VisualElementInitializer veInit
) : BaseEntityPanelFragment<PostOfficeSender>, IEntityFragmentOrder
{
    const string SkipConfirmKey = "SkipDeleteConfirmation";

    VisualElement activeGoods = null!;
    VisualElement waiting = null!;
    Label activeHeader = null!;
    Label queueHeader = null!;
    Label empty = null!;
    Label status = null!;
    Label autoHint = null!;
    VisualElement cancelButton = null!;
    Toggle auto = null!;
    ProgressBarWithLabel progress;
    string signature = "";
    bool rowsDirty;
    bool refreshing;

    public int Order => -20;
    public VisualElement Fragment => panel;

    protected override void InitializePanel()
    {
        panel.AddGameButtonPadded(t.T("LV.TPO.AddManual"), OpenManual).SetMarginBottom(5);
        var autoRow = panel.AddRow().AlignItems().SetMarginBottom(5);
        auto = autoRow.AddGamePanelToggle(t.T("LV.TPO.AutoOrder"), OnAuto).SetFlexGrow();
        autoRow.AddGameButtonPadded(t.T("LV.TPO.ConfigureAuto"), OpenAuto, paddingY: 2).SetFlexShrink(0);

        activeHeader = panel.AddGameLabel(t.T("LV.TPO.ActiveOrder").Bold()).SetMarginBottom(5).SetDisplay(false);
        activeGoods = panel.AddChild();
        progress = panel.AddProgressBarWithLabel();
        progress.Label.SetDisplay(false);
        progress.ProgressBar.SetMarginBottom(2).SetDisplay(false);
        status = panel.AddGameLabel().SetMarginBottom(5).SetDisplay(false);
        status.style.unityTextAlign = TextAnchor.MiddleCenter;
        cancelButton = panel.AddGameButtonPadded(t.T("LV.TPO.Cancel"), () => Ask(0, true))
            .SetMarginBottom(8)
            .SetDisplay(false);
        autoHint = panel.AddGameLabel(t.T("LV.TPO.UncheckAutoToCancel"))
            .SetMarginBottom(8)
            .SetDisplay(false);

        queueHeader = panel.AddGameLabel(t.T("LV.TPO.CurrentQueue").Bold()).SetMarginBottom(5).SetDisplay(false);
        waiting = panel.AddChild();
        empty = panel.AddGameLabel(t.T("LV.TPO.NoParcels"));
        panel.Initialize(veInit);
    }

    public override void ShowFragment(BaseComponent entity)
    {
        base.ShowFragment(entity);
        rowsDirty = true;
        if (!Ready(out var desk))
        {
            panel.Visible = false;
            return;
        }

        refreshing = true;
        auto.SetValueWithoutNotify(desk.AutoEnabled);
        refreshing = false;
        Refresh(desk);
    }

    public override void UpdateFragment()
    {
        if (!Ready(out var desk))
        {
            panel.Visible = false;
            return;
        }

        panel.Visible = true;
        Refresh(desk);
    }

    public override void ClearFragment()
    {
        rowsDirty = true;
        activeGoods.Clear();
        waiting.Clear();
        base.ClearFragment();
    }

    void Refresh(PostOfficeSender desk)
    {
        SyncAuto(desk.Rules.Count > 0, desk.AutoEnabled, () => desk.SetAuto(false, desk.Rules));

        var hasActive = desk.Active is not null;
        var queued = Math.Max(0, desk.Parcels.Count - 1);
        activeHeader.SetDisplay(hasActive);
        progress.ProgressBar.SetDisplay(hasActive);
        status.SetDisplay(hasActive);
        var autoLocked = desk.Active is PostOfficeParcel { Auto: true } && desk.AutoEnabled;
        cancelButton.SetDisplay(hasActive && desk.CanCancel(0));
        autoHint.SetDisplay(autoLocked);
        queueHeader.SetDisplay(queued > 0);
        empty.SetDisplay(!hasActive);
        if (hasActive)
        {
            var sending = desk.WaitKey() == "LV.TPO.Working";
            status.text = Status(desk, sending);
            var ratio = desk.JobHours <= 0f
                ? 0f
                : Mathf.Clamp01(desk.WorkedHours / desk.JobHours);
            progress.ProgressBar.SetProgress(ratio);
        }

        var next = PostOfficeParcelRows.Signature(desk.Parcels);
        if (!rowsDirty && next == signature)
        {
            return;
        }

        rowsDirty = false;
        signature = next;
        activeGoods.Clear();
        waiting.Clear();
        if (desk.Active is PostOfficeParcel active)
        {
            PostOfficeParcelRows.AddGoods(activeGoods, goods, active);
            if (active.Auto)
            {
                activeGoods.AddGameLabel(t.T("LV.TPO.AutoTag").Bold()).SetMargin(top: 2);
            }
        }

        for (var i = 1; i < desk.Parcels.Count; i++)
        {
            AddQueued(desk, i);
        }
    }

    void AddQueued(PostOfficeSender desk, int index)
    {
        var parcel = desk.Parcels[index];
        var block = waiting.AddChild().SetMarginBottom(8);
        block.AddGameLabel(t.T("LV.TPO.ParcelNumber", index).Bold()).SetMarginBottom(2);
        PostOfficeParcelRows.AddGoods(block, goods, parcel);
        if (parcel.Auto)
        {
            block.AddGameLabel(t.T("LV.TPO.AutoTag").Bold()).SetMargin(top: 2);
            return;
        }

        if (desk.CanCancel(index))
        {
            var queuedIndex = index;
            block.AddGameButtonPadded(t.T("LV.TPO.Remove"), () => Ask(queuedIndex, false), paddingY: 2)
                .SetMargin(top: 2);
        }
    }

    string Status(PostOfficeSender desk, bool sending)
    {
        if (sending)
        {
            return t.T(
                "LV.TPO.SendHours",
                desk.WorkedHours.ToString("0.0"),
                desk.JobHours.ToString("0.0"));
        }

        return desk.WaitKey() switch
        {
            "LV.TPO.WaitingStock" => t.T("LV.TPO.GatheringGoods"),
            "LV.TPO.WaitingWeight" => t.T("LV.TPO.NoSpace"),
            "" => "",
            var key => t.T(key),
        };
    }

    async void Ask(int index, bool active)
    {
        if (!Ready(out var desk) || !desk.CanCancel(index))
        {
            return;
        }

        if (!input.IsKeyHeld(SkipConfirmKey))
        {
            var key = active ? "LV.TPO.CancelActiveConfirm" : "LV.TPO.RemoveParcelConfirm";
            if (!await dialogs.ConfirmAsync(key, localized: true))
            {
                return;
            }
        }

        if (!Ready(out desk) || !desk.CanCancel(index))
        {
            return;
        }

        desk.Cancel(index);
        rowsDirty = true;
    }

    async void OpenManual()
    {
        if (!Ready(out var desk))
        {
            return;
        }

        var chosen = await container.GetInstance<PostOfficeOrderDialog>().ShowAsync();
        if (chosen is null || chosen.Count == 0 || !Ready(out desk))
        {
            return;
        }

        desk.Enqueue(chosen);
        rowsDirty = true;
    }

    async void OpenAuto()
    {
        if (!Ready(out var desk))
        {
            return;
        }

        var rules = await container.GetInstance<PostOfficeSenderAutoDialog>().ShowAsync(desk.Rules);
        if (rules is null || !Ready(out desk))
        {
            return;
        }

        desk.SetAuto(desk.AutoEnabled, rules);
    }

    void OnAuto(bool value)
    {
        if (refreshing || !Ready(out var desk))
        {
            return;
        }

        desk.SetAuto(value, desk.Rules);
        rowsDirty = true;
    }

    void SyncAuto(bool configured, bool enabled, Action turnOff)
    {
        auto.SetEnabled(configured);
        var on = configured && enabled;
        if (auto.value != on)
        {
            refreshing = true;
            auto.SetValueWithoutNotify(on);
            refreshing = false;
        }

        if (!configured && enabled)
        {
            turnOff();
        }
    }

    bool Ready(out PostOfficeSender desk)
    {
        desk = component!;
        if (!desk || !desk)
        {
            return false;
        }

        return desk.IsFinished;
    }
}
