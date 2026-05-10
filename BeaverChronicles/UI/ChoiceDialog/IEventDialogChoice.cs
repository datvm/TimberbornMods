namespace BeaverChronicles.UI;

public interface IEventDialogChoice
{
    bool Disabled { get; set; }
    Button Render(VisualElement buttonContainer);
}
