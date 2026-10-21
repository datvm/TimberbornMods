namespace ThePostOffice.UI;

[BindFragment]
public class PostOfficeFragment(
    ILoc t,
    IGoodService goods,
    GoodDescriber describer,
    PostOfficeGlobalInventoryService globalInventory,
    PostOfficeService offices,
    VisualElementInitializer veInit,
    DropdownItemsSetter dropdowns
) : BaseEntityPanelFragment<PostOfficeComponent>, IEntityFragmentOrder
{
    Label buildingWeight = null!;
    Label direction = null!;
    Label emptyQueue = null!;
    VisualElement queueList = null!;
    Button pause = null!;
    ProgressBarWithLabel progress;
    Label status = null!;
    VisualElement draftHost = null!;
    Label draftWeight = null!;
    Label split = null!;
    Label locked = null!;
    Button send = null!;
    Button retrieve = null!;
    CollapsiblePanel autoPanel = null!;
    Toggle autoSend = null!;
    Toggle autoRetrieve = null!;
    VisualElement sendAutoGroup = null!;
    VisualElement retrieveAutoGroup = null!;
    VisualElement sendKeepHost = null!;
    VisualElement retrieveKeepHost = null!;
    bool sends;

    PostOfficeGoodEditor draft = null!;
    PostOfficeGoodEditor sendKeeps = null!;
    PostOfficeGoodEditor retrieveKeeps = null!;
    PostOfficeQueue? queue;
    PostOfficeActiveParcel? active;
    bool editorsReady;
    bool refreshing;
    string queueSignature = "";

    public int Order => -20;
    public VisualElement Fragment => panel;

    protected override void InitializePanel()
    {
        buildingWeight = panel.AddGameLabel().SetMarginBottom(5);

        var header = panel.AddRow().AlignItems().SetMarginBottom(5);
        header.AddGameLabel(t.T("LV.TPO.Queue"), bold: true).SetFlexGrow();
        pause = header.AddGameButtonPadded(t.T("LV.TPO.Pause"), OnPause, paddingY: 2).SetFlexShrink(0);
        pause.SetDisplay(false);

        direction = panel.AddGameLabel().SetMarginBottom(5).SetDisplay(false);
        emptyQueue = panel.AddGameLabel(t.T("LV.TPO.NoParcels")).SetMarginBottom(5);
        queueList = panel.AddChild();

        progress = panel.AddProgressBarWithLabel();
        progress.ProgressBar.SetMarginBottom(5).SetDisplay(false);
        status = panel.AddGameLabel().SetMarginBottom(5).SetDisplay(false);

        var parcelPanel = panel.AddCollapsiblePanel(t.T("LV.TPO.NewParcel"));
        draftHost = parcelPanel.Container.AddChild();
        draftWeight = parcelPanel.Container.AddGameLabel().SetMarginBottom(5);
        split = parcelPanel.Container.AddGameLabel().SetMarginBottom(5).SetDisplay(false);
        locked = parcelPanel.Container.AddGameLabel(color: UiBuilder.GameLabelColor.Yellow).SetMarginBottom(5).SetDisplay(false);

        var actions = parcelPanel.Container.AddRow().AlignItems();
        send = actions.AddGameButtonPadded(t.T("LV.TPO.Send"), () => Enqueue(true)).SetFlexGrow();
        retrieve = actions.AddGameButtonPadded(t.T("LV.TPO.Retrieve"), () => Enqueue(false))
            .SetFlexGrow()
            .SetMargin(left: 5);

        autoPanel = panel.AddCollapsiblePanel(t.T("LV.TPO.Auto"), expand: false);
        autoPanel.Container.AddGameLabel(t.T("LV.TPO.AutoHint")).SetMarginBottom(5);

        sendAutoGroup = autoPanel.Container.AddChild();
        autoSend = sendAutoGroup.AddGamePanelToggle(t.T("LV.TPO.AutoSend"), OnAutoSend).SetMarginBottom(5);
        sendAutoGroup.AddGameLabel(t.T("LV.TPO.KeepAmounts")).SetMarginBottom(5);
        sendKeepHost = sendAutoGroup.AddChild();

        retrieveAutoGroup = autoPanel.Container.AddChild();
        autoRetrieve = retrieveAutoGroup.AddGamePanelToggle(t.T("LV.TPO.AutoRetrieve"), OnAutoRetrieve).SetMarginBottom(5);
        retrieveAutoGroup.AddGameLabel(t.T("LV.TPO.KeepAmounts")).SetMarginBottom(5);
        retrieveKeepHost = retrieveAutoGroup.AddChild();

        panel.Initialize(veInit);
    }

    public override void ShowFragment(BaseComponent entity)
    {
        base.ShowFragment(entity);
        if (component is not PostOfficeComponent office || !office)
        {
            queue = null;
            active = null;
            return;
        }

        queue = office.GetComponent<PostOfficeQueue>();
        active = office.GetComponent<PostOfficeActiveParcel>();
        if (queue is not PostOfficeQueue current || !current
            || active is not PostOfficeActiveParcel job || !job)
        {
            panel.Visible = false;
            return;
        }

        sends = office.PostOfficeSpec.Sends;
        EnsureEditors();
        refreshing = true;
        draft.SetGoods([]);
        sendKeeps.SetGoods(current.SendKeeps);
        retrieveKeeps.SetGoods(current.RetrieveKeeps);
        autoSend.SetValueWithoutNotify(current.SendAutoEnabled);
        autoRetrieve.SetValueWithoutNotify(current.RetrieveAutoEnabled);
        if (current.SendAutoEnabled || current.RetrieveAutoEnabled)
        {
            autoPanel.SetExpandWithoutNotify(true);
        }

        refreshing = false;
        queueSignature = "";
        Refresh();
    }

    public override void UpdateFragment()
    {
        if (!component || !queue || !active || !editorsReady)
        {
            return;
        }

        Refresh();
    }

    public override void ClearFragment()
    {
        queue = null;
        active = null;
        queueSignature = "";
        queueList.Clear();
        base.ClearFragment();
    }

    void EnsureEditors()
    {
        if (editorsReady)
        {
            return;
        }

        var catalog = MailableGoods();
        draft = CreateEditor(catalog);
        draft.Changed += OnDraftChanged;
        draftHost.Add(draft);

        sendKeeps = CreateEditor(catalog);
        sendKeeps.Changed += OnKeepsChanged;
        sendKeepHost.Add(sendKeeps);

        retrieveKeeps = CreateEditor(catalog);
        retrieveKeeps.Changed += OnKeepsChanged;
        retrieveKeepHost.Add(retrieveKeeps);
        editorsReady = true;
    }

    PostOfficeGoodEditor CreateEditor(List<GoodSpec> catalog)
        => new(catalog, goods, describer, t, veInit, dropdowns);

    void Refresh()
    {
        if (component is not PostOfficeComponent office || !office
            || queue is not PostOfficeQueue current || !current
            || active is not PostOfficeActiveParcel job || !job
            || !editorsReady)
        {
            return;
        }

        send.SetDisplay(sends);
        retrieve.SetDisplay(!sends);
        sendAutoGroup.SetDisplay(sends);
        retrieveAutoGroup.SetDisplay(!sends);

        buildingWeight.text = t.T(
            "LV.TPO.BuildingWeight",
            StoredWeight(office),
            office.PostOfficeSpec.InventoryCapacity);

        var hasParcels = current.HasParcels;
        emptyQueue.SetDisplay(!hasParcels);
        direction.SetDisplay(hasParcels);
        pause.SetDisplay(hasParcels);
        progress.ProgressBar.SetDisplay(hasParcels);
        status.SetDisplay(hasParcels);
        if (hasParcels)
        {
            direction.text = current.IsSend ? t.T("LV.TPO.Sending") : t.T("LV.TPO.Retrieving");
            pause.text = job.PlayerPaused ? t.T("LV.TPO.Resume") : t.T("LV.TPO.Pause");
            var ratio = job.JobHours <= 0f ? 0f : Mathf.Clamp01(job.WorkedHours / job.JobHours);
            progress.SetProgress(ratio, $"{Mathf.RoundToInt(ratio * 100)}%");
            status.text = Describe(office, current, job);
        }

        var signature = QueueSignature(current);
        if (signature != queueSignature)
        {
            queueSignature = signature;
            RebuildQueue(current);
        }

        RefreshDraft(current);
    }

    void RebuildQueue(PostOfficeQueue current)
    {
        queueList.Clear();
        var index = 0;
        foreach (var parcel in current.Parcels)
        {
            AddParcelRow(parcel, index);
            index++;
        }
    }

    void AddParcelRow(PostOfficeParcel parcel, int index)
    {
        var row = queueList.AddRow().AlignItems().SetMarginBottom(4);
        row.style.flexWrap = Wrap.Wrap;
        row.AddGameLabel($"{index + 1}.").SetMarginRight(6).SetFlexShrink(0);

        var icons = row.AddChild().SetAsRow().AlignItems().SetFlexGrow();
        icons.style.flexWrap = Wrap.Wrap;
        var named = parcel.Goods.Count == 1;
        foreach (var good in parcel.Goods)
        {
            if (!goods.HasGood(good.GoodId))
            {
                continue;
            }

            icons.AddIconSpan()
                .SetGood(goods, good.GoodId, good.Amount.ToString(), showName: named)
                .SetMarginRight(4);
        }

        if (parcel.Auto)
        {
            row.AddGameLabel(t.T("LV.TPO.AutoTag")).SetMargin(left: 4, right: 4).SetFlexShrink(0);
        }

        var at = index;
        row.AddGameButtonPadded(t.T("LV.TPO.Cancel"), () => CancelAt(at), paddingY: 2).SetFlexShrink(0);
    }

    void RefreshDraft(PostOfficeQueue current)
    {
        var amounts = draft.Read();
        var count = CountParcels(amounts);
        draftWeight.text = t.T("LV.TPO.DraftWeight", WeightOf(amounts), offices.ParcelWeight);
        split.SetDisplay(count > 1);
        if (count > 1)
        {
            split.text = t.T("LV.TPO.Split", count);
        }

        var lockRetrieve = current.HasManual && current.IsSend;
        var lockSend = current.HasManual && !current.IsSend;
        locked.SetDisplay(current.HasManual);
        if (lockSend)
        {
            locked.text = t.T("LV.TPO.LockedSend");
        }
        else if (lockRetrieve)
        {
            locked.text = t.T("LV.TPO.LockedRetrieve");
        }

        send.enabledSelf = count > 0 && !lockSend;
        retrieve.enabledSelf = count > 0 && !lockRetrieve;
    }

    string Describe(PostOfficeComponent office, PostOfficeQueue current, PostOfficeActiveParcel job)
    {
        if (!office.BlockObject || !office.BlockObject.IsFinished)
        {
            return t.T("LV.TPO.Unfinished");
        }

        var link = office.GetComponent<DistrictBuilding>();
        if (!link || !link.InstantDistrict)
        {
            return t.T("LV.TPO.Disconnected");
        }

        if (job.PlayerPaused)
        {
            return t.T("LV.TPO.Paused");
        }

        if (current.IsSend)
        {
            if (current.AutoSendSurplus())
            {
                return t.T("LV.TPO.KeepMet");
            }

            if (!current.FrontIsInStock() || !current.FrontIsUnreserved())
            {
                return t.T("LV.TPO.WaitingStock");
            }

            if (!job.SendHeld)
            {
                return t.T("LV.TPO.WaitingWeight");
            }
        }
        else if (!current.FrontFits())
        {
            return t.T("LV.TPO.WaitingSpace");
        }
        else if (!current.GlobalHasFront())
        {
            return t.T("LV.TPO.WaitingGlobal");
        }

        return t.T("LV.TPO.Working");
    }

    void Enqueue(bool sending)
    {
        if (queue is not PostOfficeQueue current || !current || !editorsReady)
        {
            return;
        }

        if (!current.TryEnqueue(sending, draft.Read()))
        {
            return;
        }

        refreshing = true;
        draft.SetGoods([]);
        refreshing = false;
        queueSignature = "";
        Refresh();
    }

    void CancelAt(int index)
    {
        if (queue is not PostOfficeQueue current || !current)
        {
            return;
        }

        current.Cancel(index);
        queueSignature = "";
        Refresh();
    }

    void OnPause()
    {
        if (active is not PostOfficeActiveParcel job || !job)
        {
            return;
        }

        if (job.PlayerPaused)
        {
            job.Resume();
        }
        else
        {
            job.Pause();
        }

        Refresh();
    }

    void OnDraftChanged()
    {
        if (refreshing || queue is not PostOfficeQueue current || !current || !editorsReady)
        {
            return;
        }

        RefreshDraft(current);
    }

    void OnAutoSend(bool value)
    {
        if (refreshing || !queue || !editorsReady)
        {
            return;
        }

        if (value)
        {
            refreshing = true;
            autoRetrieve.SetValueWithoutNotify(false);
            refreshing = false;
        }

        OnKeepsChanged();
    }

    void OnAutoRetrieve(bool value)
    {
        if (refreshing || !queue || !editorsReady)
        {
            return;
        }

        if (value)
        {
            refreshing = true;
            autoSend.SetValueWithoutNotify(false);
            refreshing = false;
        }

        OnKeepsChanged();
    }

    void OnKeepsChanged()
    {
        if (refreshing || queue is not PostOfficeQueue current || !current || !editorsReady)
        {
            return;
        }

        current.SetAuto(autoSend.value, autoRetrieve.value, sendKeeps.Read(), retrieveKeeps.Read());
        queueSignature = "";
        Refresh();
    }

    List<GoodSpec> MailableGoods()
    {
        var limit = offices.ParcelWeight;
        List<GoodSpec> catalog = [];
        foreach (var id in goods.Goods)
        {
            if (!goods.HasGood(id))
            {
                continue;
            }

            var spec = goods.GetGood(id);
            if (spec.GoodType == "Liquid" || spec.Weight <= 0 || (limit > 0 && spec.Weight > limit))
            {
                continue;
            }

            catalog.Add(spec);
        }

        catalog.Sort((left, right) => string.Compare(
            left.DisplayName.Value,
            right.DisplayName.Value,
            StringComparison.CurrentCultureIgnoreCase));
        return catalog;
    }

    string QueueSignature(PostOfficeQueue current)
    {
        List<string> parts = [];
        foreach (var parcel in current.Parcels)
        {
            var body = string.Join(",", parcel.Goods.Select(good => $"{good.GoodId}:{good.Amount}"));
            parts.Add(parcel.Auto ? $"A{body}" : body);
        }

        return string.Join(";", parts);
    }

    int CountParcels(IReadOnlyList<GoodAmount> amounts)
    {
        List<GoodAmount> left = [];
        foreach (var good in amounts)
        {
            var unit = Unit(good.GoodId);
            if (good.Amount <= 0 || unit <= 0 || unit > offices.ParcelWeight)
            {
                continue;
            }

            left.Add(good);
        }

        var count = 0;
        while (left.Count > 0)
        {
            List<GoodAmount> next = [];
            var budget = offices.ParcelWeight;
            var took = false;
            foreach (var good in left)
            {
                if (budget <= 0)
                {
                    next.Add(good);
                    continue;
                }

                var unit = Unit(good.GoodId);
                var fit = Math.Min(good.Amount, budget / unit);
                if (fit <= 0)
                {
                    next.Add(good);
                    continue;
                }

                took = true;
                budget -= fit * unit;
                var remain = good.Amount - fit;
                if (remain > 0)
                {
                    next.Add(new(good.GoodId, remain));
                }
            }

            if (!took)
            {
                break;
            }

            left = next;
            count++;
        }

        return count;
    }

    int StoredWeight(PostOfficeComponent office)
    {
        if (office.PostOfficeInventory is not PostOfficeInventory building || !building)
        {
            return 0;
        }

        if (building.Inventory is not Inventory stock || !stock)
        {
            return 0;
        }

        var sum = 0;
        foreach (var good in stock.Stock)
        {
            sum += Unit(good.GoodId) * good.Amount;
        }

        return sum;
    }

    int WeightOf(IReadOnlyList<GoodAmount> amounts)
    {
        var sum = 0;
        foreach (var good in amounts)
        {
            if (Unit(good.GoodId) <= 0)
            {
                continue;
            }

            sum += globalInventory.GetItemWeight(good.GoodId, good.Amount);
        }

        return sum;
    }

    int Unit(string id)
    {
        if (!globalInventory.Inventory.ContainsKey(id))
        {
            return 0;
        }

        return globalInventory.GetItemWeight(id);
    }
}
