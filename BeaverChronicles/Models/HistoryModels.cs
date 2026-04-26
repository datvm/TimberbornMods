namespace BeaverChronicles.Models;

public record EventHistoryRecord(string Id, float StartDay, float? EndDay)
{
    public string Serialize() => $"{Id}|{StartDay}|{EndDay ?? -1}";

    public static EventHistoryRecord Deserialize(string s)
    {
        var parts = s.Split('|');
        if (parts.Length != 3)
        {
            throw new FormatException($"Invalid event history record: {s}");
        }

        var endDay = float.Parse(parts[2]);

        return new(
            parts[0],
            float.Parse(parts[1]),
            endDay == -1 ? null : endDay
        );
    }
}
