namespace ConfigurableTopBar.Models;

public class EditableGoodSpec(string id)
{
    public string Id { get; set; } = id;

    [JsonIgnore]
    public GoodSpec? GoodSpec { get; set; }
}