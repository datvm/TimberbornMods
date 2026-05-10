namespace BeaverChronicles.UI;

public abstract class EventDialogChoice : IEventDialogChoice
{
    public bool Disabled { get; set; }

    public abstract void Render(VisualElement buttonContainer, Button btn);

    public Button Render(VisualElement buttonContainer)
    {
        var btn = buttonContainer.AddMenuButton("", stretched: true);
        Render(buttonContainer, btn);

        return btn;
    }
}

public class CustomEventDialogChoice(Action<VisualElement, Button> render) : EventDialogChoice
{
    public override void Render(VisualElement buttonContainer, Button btn)
        => render(buttonContainer, btn);
}