namespace BeaverChronicles.UI;

public class SimpleEventDialogChoice(string text, ChronicleEventUIHelper uiHelper, string? note = null) : EventDialogChoice
{

    public string Text { get; set; } = text;
    public string? Note { get; set; } = note;

    public override void Render(VisualElement buttonContainer, Button btn)
    {
        btn.text = Text;

        if (Note is not null)
        {
            var noteEl = uiHelper.CreateFormattedText(Note);
            buttonContainer.Add(noteEl);
        }
    }

}
