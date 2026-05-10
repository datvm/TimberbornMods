namespace BeaverChronicles.UI;

[BindTransient]
public class ChronicleEventChoiceDialog(
    VisualElementInitializer veInit,
    PanelStack panelStack
) : DialogBoxElement
{

    int choice;

    void Initialize(ChronicleEventDialogDefinition def)
    {
        SetTitle(def.Title);
        SetDialogSize(1000, 700);

        var parent = Content.AddChild(name: "ChronicleEvent");

        if (def.TopImage)
        {
            var img = parent.AddImage(def.TopImage, "TopImage").SetMarginBottom(10)
                .SetMaxSizePercent(100, null)
                .SetMaxHeight(200);
            img.style.alignContent = Align.Center;
        }

        var row = parent.AddRow().AlignItems();
        if (def.SideImage)
        {
            var img = row.AddImage(def.SideImage, "SideImage").SetMarginRight().SetFlexShrink(0)
                .SetMaxWidth(150).SetMaxHeight(450);
            img.style.alignContent = Align.Center;
        }

        var mainContent = row.AddChild().SetFlexGrow();

        var mainContentText = mainContent.AddChild().SetMarginBottom();
        AddMainContent(mainContentText, def);

        var choices = mainContent.AddChild();
        AddChoices(choices, def);
    }

    void AddMainContent(VisualElement parent, ChronicleEventDialogDefinition def)
    {
        foreach (var element in def.Content)
        {
            parent.Add(element.SetMarginBottom(10));
        }
    }

    void AddChoices(VisualElement parent, ChronicleEventDialogDefinition def)
    {
        var index = -1;
        foreach (var choice in def.Choices)
        {
            var z = ++index;

            var container = parent.AddChild().SetMarginBottom();

            var btn = choice.Render(container);
            btn.enabledSelf = !choice.Disabled;
            btn.AddAction(() => Choose(z));
        }
    }

    void Choose(int index)
    {
        choice = index;
        OnUIConfirmed();
    }

    public async Task<int> ShowAsync(ChronicleEventDialogDefinition def)
    {
        if (def.Choices.Count == 0)
        {
            throw new ArgumentException("At least one choice must be provided", nameof(def.Choices));
        }

        var firstChoice = GetFirstEnabledChoice(def);

        Initialize(def);
        if (await ShowAsync(veInit, panelStack))
        {
            PrintChoice(choice);
            return choice;
        }
        else
        {
            var index = def.DefaultChoice;
            if (index.HasValue && !def.Choices[index.Value].Disabled)
            {
                PrintChoice(index.Value);
                return index.Value;
            }

            PrintChoice(firstChoice);
            return firstChoice;
        }

        void PrintChoice(int index)
        {
            BeaverChroniclesUtils.Log($"Choice for Event {def.Event.Id}: {index}");
        }
    }

    int GetFirstEnabledChoice(ChronicleEventDialogDefinition def)
    {
        for (int i = 0; i < def.Choices.Count; i++)
        {
            if (!def.Choices[i].Disabled)
            {
                return i;
            }
        }

        throw new InvalidOperationException("There is no valid choice");
    }

}
