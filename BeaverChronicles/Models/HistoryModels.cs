namespace BeaverChronicles.Models;

public record EventHistoryRecord(string Id, float StartDay, float? EndDay)
{
    public List<EventHistoryPage> Pages { get; } = [];

    [JsonIgnore]
    public EventHistoryPage CurrentPage => Pages[^1];

    public Dictionary<string, string> CustomParameters { get; } = [];

    public void RecordChoice(int pos, int choiceIndex)
    {
        CustomParameters[$"Choice{pos}"] = choiceIndex.ToString();
    }

    public int[] GetRecordedChoices()
    {
        List<int> choices = [];
        for (int i = 0; ; i++)
        {
            if (CustomParameters.TryGetValue($"Choice{i}", out var s) && int.TryParse(s, out var index))
            {
                choices.Add(index);
            }
            else
            {
                break;
            }
        }
        return [.. choices];
    }

    public bool TryGetChoice(int pos, out int choiceIndex)
    {
        if (CustomParameters.TryGetValue($"Choice{pos}", out var s) && int.TryParse(s, out var index))
        {
            choiceIndex = index;
            return true;
        }

        choiceIndex = default;
        return false;
    }

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
        Pages.Add(page);
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