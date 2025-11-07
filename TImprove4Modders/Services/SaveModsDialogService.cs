namespace TImprove4Modders.Services;

public class SaveModsDialogService : IUpdatableSingleton, IUnloadableSingleton
{
    DateTime endTime;

    public static SaveModsDialogService? Instance { get; private set; }

    string confirmText = "";
    Button? confirmButton;

    public SaveModsDialogService()
    {
        Instance = this;
    }

    public void ActivateFor(VisualElement ve)
    {
        confirmButton = ve.Q<Button>("ConfirmButton");
        confirmText = confirmButton.text;
        confirmButton.clicked += Dismiss;

        var cancelButton = ve.Q<Button>("CancelButton");
        cancelButton.clicked += Dismiss;

        endTime = DateTime.Now.AddSeconds(3);
        UpdateButtonText();
    }

    void UpdateButtonText()
    {
        confirmButton!.text = $"{confirmText} ({Mathf.CeilToInt((float)(endTime - DateTime.Now).TotalSeconds)})";
    }

    void Dismiss()
    {
        confirmButton = null;
    }

    public void UpdateSingleton()
    {
        if (confirmButton is null) { return; }

        if (DateTime.Now >= endTime)
        {
            using var clickEvent = ClickEvent.GetPooled(new Event());
            confirmButton.panel.visualTree.SendEvent(clickEvent);
        }
        else
        {
            UpdateButtonText();
        }
    }

    public void Unload()
    {
        Instance = null;
    }
}
