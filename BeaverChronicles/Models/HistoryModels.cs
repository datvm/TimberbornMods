namespace BeaverChronicles.Models;

public record EventHistoryRecord(string Id, float StartDay, float? EndDay)
{
    readonly List<EventHistoryPage> pages = [];
    public IReadOnlyList<EventHistoryPage> Pages => pages;

    public EventHistoryPage CurrentPage => pages[^1];

    public EventHistoryPage AddPage() => AddPage(false, false);

    public EventHistoryPage AddPage(bool top = false, bool side = false)
    {
        var topImg = top ? ChronicleEventUIHelper.GetTopImagePath(Id) : null;
        var sideImg = side ? ChronicleEventUIHelper.GetSideImagePath(Id) : null;
        return AddPage(topImg, sideImg);
    }

    public EventHistoryPage AddPage(string? topImagePath = null, string? sideImagePath = null)
    {
        var page = new EventHistoryPage()
        {
            TopImagePath = topImagePath,
            SideImagePath = sideImagePath
        };
        pages.Add(page);
        return page;
    }

    public string Serialize() => JsonConvert.SerializeObject(this);
    public static EventHistoryRecord Deserialize(string s) => JsonConvert.DeserializeObject<EventHistoryRecord>(s) 
        ?? throw new InvalidOperationException("Deserialization failed");
}

public class EventHistoryPage
{
    public string? TopImagePath { get; set; }
    public string? SideImagePath { get; set; }

    readonly List<string> content = [];
    public IReadOnlyList<string> Content => content;
    
    public EventHistoryPage AddContent(string text)
    {
        content.Add(text);
        return this;
    }

}