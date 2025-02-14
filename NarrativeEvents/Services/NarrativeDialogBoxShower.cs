global using Timberborn.CoreUI;
global using UiBuilder.Common;

namespace NarrativeEvents.Services;

public class NarrativeDialogBoxShower(PanelStack panelStack)
{

    public NarrativeDialogBox Create()
    {
        return new(panelStack);
    }

}

public class NarrativeDialogBox : DialogBoxElement
{

    readonly Image image;
    readonly Label title, desc;
    readonly VisualElement buttons;

    int choiceCount = 0;
    readonly TaskCompletionSource<int> choiceTask = new();

    readonly PanelStack panelStack;
    DialogBox? diag;

    public NarrativeDialogBox(PanelStack panelStack) : base(true)
    {
        var box = Box;

        var header = box.AddChild<VisualElement>()
            .SetMargin(bottom: 20);
        header.style.unityTextAlign = TextAnchor.MiddleCenter;

        {
            title = header.AddLabel("TITLE", "Title", style: UiBuilder.GameLabelStyle.Header);

            image = header.AddChild<Image>()
                .SetSize(height: 100)
                .SetMargin(right: 20);
            image.ToggleDisplayStyle(false);
            image.scaleMode = ScaleMode.ScaleToFit;
        }

        desc = box.AddLabel("DESC", "Desc")
            .SetMargin(bottom: 20);
        this.panelStack = panelStack;

        buttons = box.AddChild<VisualElement>();
        buttons.style.unityTextAlign = TextAnchor.MiddleCenter;
    }

    public NarrativeDialogBox SetImage(Texture tex)
    {
        image.image = tex;
        image.ToggleDisplayStyle(true);

        return this;
    }

    public NarrativeDialogBox SetTexts(string title, string desc)
    {
        this.title.text = title;
        this.desc.text = desc;

        return this;
    }

    public NarrativeDialogBox AddChoice(string text, string? consequence = default, bool disabled = default)
    {
        var choiceIndex = choiceCount++;

        var btn = buttons.AddButton(text, onClick: () => OnChoiceSelected(choiceIndex))
            .SetMargin(top: 10);
        btn.SetEnabled(!disabled);

        if (consequence is not null)
        {
            var lblConsequence = buttons.AddLabel(consequence);
        }

        return this;
    }

    void OnChoiceSelected(int index)
    {
        choiceTask.TrySetResult(index);
        diag?.OnUICancelled();
    }

    public DialogBox Show()
    {
        var diag = this.diag = new DialogBox(panelStack, DoNothing, DoNothing, this);
        panelStack.Push(diag);

        return diag;
    }

    public async Task<int> ShowAsync()
    {
        if (choiceTask.Task.IsCompleted)
        {
            throw new InvalidOperationException("Dialog already shown");
        }

        Show();
        return await choiceTask.Task;
    }

    static void DoNothing() { }

}