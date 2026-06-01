namespace BeaverChronicles.Events;

public record OnCharacterEnteredAreaEvent(BaseComponent Character, Vector3 Position, IAreaChronicleEvent Event);
public record OnCustomChronicleEvent(string Name, object? Data = null);
