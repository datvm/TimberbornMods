namespace TimberUi.Services.Dialogs;

public class PromptDialogElement : DialogBoxElement
{

    readonly TextField txt;
    readonly Label lblPrompt;

    public PromptDialogElement(ILoc t)
    {
        AddCloseButton();

        lblPrompt = Content.AddGameLabel().SetDisplay(false).SetMarginBottom(5);
        txt = Content.AddTextField("PromptInput").SetMarginBottom(5);

        var btns = Content.AddRow();
        btns.AddMenuButton(t.T("Core.OK"), onClick: OnUIConfirmed);
        btns.AddMenuButton(t.T("Core.Cancel"), onClick: OnUICancelled);
    }

    public PromptDialogElement SetPrompt(string prompt)
    {
        lblPrompt.text = prompt;
        lblPrompt.SetDisplay(!string.IsNullOrEmpty(prompt));
        return this;
    }

    public PromptDialogElement SetValue(string content)
    {
        txt.value = content;
        return this;
    }

    public async Task<string?> ShowAsync(VisualElementInitializer veInit, PanelStack panelStack)
    {
        var task = base.ShowAsync(veInit, panelStack);
        txt.Focus();

        var confirmed = await task;
        return confirmed ? txt.value : null;
    }

}
