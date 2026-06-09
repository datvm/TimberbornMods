namespace BeaverChronicles.UI;

[BindTransient]
public class ChronicleEventsDialog(
    VisualElementInitializer veInit,
    PanelStack panelStack,
    ILoc t,
    ChronicleEventRecords history,
    ChronicleEventRegistry registry,
    IAssetLoader assets,
    ChronicleEventUIHelper uiHelper
) : DialogBoxElement
{

    ToggleGroup<EventHistoryRecord> chkEvents = null!;
#nullable disable
    Label lblTitle, lblPage;
    VisualElement contentPanel;
    Image sideImg, topImg;
#nullable enable

    int currPage, totalPages;
    EventHistoryRecord? currRecord;

    public void Init()
    {
        SetTitle(t.T("LV.BCEv.BeaverChronicles"));
        AddCloseButton();

        const int Height = 700;
        SetDialogSize(width: 1300);

        var parent = Content;
        var row = parent.AddRow();

        var sideBar = row.AddScrollView().SetSize(280, Height).SetMarginRight().SetFlexShrink(0).SetPadding(10);
        var panelContent = row.AddScrollView().SetFlexGrow().SetHeight(Height);

        InitSidebar(sideBar);
        InitContent(panelContent);

        if (chkEvents.Options.Length > 0)
        {
            var v = chkEvents.Options[0].Value;
            chkEvents.SetValueWithoutNotify(v);
            OnEventSelected(this, v);
        }
    }

    void InitSidebar(VisualElement parent)
    {
        chkEvents = new ToggleGroup<EventHistoryRecord>(GetOptions());
        chkEvents.OnValueChanged += OnEventSelected;

        IEnumerable<ToggleGroupOption<EventHistoryRecord>> GetOptions()
        {
            parent.AddGameLabel(t.T("LV.BCEv.CurrentEvent")).SetMarginBottom(5);

            var active = history.ActiveRecord;
            if (active is null)
            {
                parent.AddGameLabel(t.TNone()).SetMarginBottom();
            }
            else
            {
                yield return CreateToggleFor(active);
            }

            parent.AddGameLabel(t.T("LV.BCEv.PastEvents")).SetMargin(top: 5, bottom: 5);
            var all = history.Records;

            var start = all.Count - 1 - (active is null ? 0 : 1);

            if (start >= 0)
            {
                for (int i = start; i >= 0; i--)
                {
                    yield return CreateToggleFor(all[i]);
                }
            }
            else
            {
                parent.AddGameLabel(t.TNone());
            }
        }

        ToggleGroupOption<EventHistoryRecord> CreateToggleFor(EventHistoryRecord r)
        {
            var id = r.Id;
            var chk = parent.AddToggle().SetMargin(top: 5);

            if (!registry.TryGet(id, out var ev))
            {
                chk.enabledSelf = false;
            }
            else
            {
                chk.text = t.T("LV.BCEv.ListText", r.StartDay, t.T(ev.NameLoc));
            }

            return new(chk, r);
        }
    }

    void OnEventSelected(object sender, EventHistoryRecord e)
    {
        ClearContent();

        currRecord = e;

        totalPages = e.Pages.Count;
        currPage = totalPages - 1;

        topImg.SetDisplay(false);
        sideImg.SetDisplay(false);

        registry.TryGet(e.Id, out var ev);
        var name = t.T(ev?.NameLoc ?? "LV.BCEv.Unknown");

        if (e.EndDay is null)
        {
            lblTitle.text = t.T("LV.BCEv.EntryTitleActive", name, e.StartDay);
        }
        else
        {
            lblTitle.text = t.T("LV.BCEv.EntryTitle", name, e.StartDay, e.EndDay.Value);
        }

        ShowCurrentPage();
    }

    void InitContent(VisualElement parent)
    {
        lblTitle = parent.AddLabelHeader().SetMarginBottom(5);

        var pageRow = parent.AddRow().AlignItems().SetMarginBottom();
        pageRow.AddGameLabel(t.T("LV.BCEv.Pages")).SetMarginRight(10);
        lblPage = pageRow.AddLabel("0 / 0").SetMarginRight(5);
        pageRow.AddMinusButton().AddAction(() => ChangePage(-1)).SetMarginRight(5);
        pageRow.AddPlusButton().AddAction(() => ChangePage(1));

        topImg = parent.AddImage().SetMarginBottom(10).SetMaxHeight(200).SetDisplay(false);
        topImg.style.alignSelf = Align.Center;

        var contentRow = parent.AddRow();
        sideImg = contentRow.AddImage().SetMaxWidth(150).SetMaxHeight(450).SetMarginRight(10).SetDisplay(false).SetFlexShrink(0);
        contentPanel = contentRow.AddChild().SetFlexGrow().SetFlexShrink();
    }

    void ChangePage(int delta)
    {
        var target = currPage + delta;
        if (target < 0 || target >= totalPages) { return; }
        currPage = target;
        ShowCurrentPage();
    }

    void ShowCurrentPage()
    {
        ClearContent();
        lblPage.text = $"{currPage + 1} / {totalPages}";

        if (currRecord is null || currPage >= totalPages || currPage < 0) { return; }

        var r = currRecord.Pages[currPage];
        SetImage(topImg, r.TopImagePath);
        SetImage(sideImg, r.SideImagePath);

        foreach (var text in r.Content)
        {
            contentPanel.Add(uiHelper.CreateFormattedText(text).SetMarginBottom(10));
        }
    }

    void SetImage(Image image, string? path)
    {
        if (path is null) { return; }

        image.image = assets.Load<Texture2D>(path);
        image.SetDisplay(true);
    }

    void ClearContent()
    {
        sideImg.image = null;
        sideImg.SetDisplay(false);
        topImg.image = null;
        topImg.SetDisplay(false);
        contentPanel.Clear();
    }

    public void Show()
    {
        Init();
        Show(veInit, panelStack);
    }

}
