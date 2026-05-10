namespace BeaverChronicles.UI;

[BindTransient]
public class MixedTextElement(MixedTextService mixedTextService) : VisualElement
{

    public string Content { get; private set; } = "";
    public bool IsRow { get; private set; }

    public void SetFlow(bool row)
    {
        if (row)
        {
            this.SetAsRow().AlignItems();
        }
        else
        {
            style.flexDirection = FlexDirection.Column;
        }
    }

    public void SetText(string content)
    {
        Content = content;
        Clear();

        if (string.IsNullOrEmpty(content)) { return; }

        foreach (var part in mixedTextService.CreateElements(content))
        {
            Add(part);

            if (IsRow)
            {
                part.SetMarginRight(5);
            }
            else
            {
                part.SetMarginBottom(5);
            }
        }
    }

}
