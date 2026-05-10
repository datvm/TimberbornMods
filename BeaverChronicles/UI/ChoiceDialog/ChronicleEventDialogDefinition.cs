namespace BeaverChronicles.UI;

public class ChronicleEventDialogDefinition(IChronicleEvent ev)
{

    internal ChronicleEventDialogDefinition() : this(null!) { }

    public IChronicleEvent Event { get; internal set; } = ev;

    public string Title { get; set; } = "";
    public Sprite? TopImage { get; set; }
    public Sprite? SideImage { get; set; }

    public List<VisualElement> Content { get; } = [];

    public List<IEventDialogChoice> Choices { get; } = [];
    public int? DefaultChoice { get; set; } = 0;

}
