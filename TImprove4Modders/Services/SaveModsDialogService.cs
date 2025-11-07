namespace TImprove4Modders.Services;

public class SaveModsDialogService : IUpdatableSingleton, IUnloadableSingleton
{
    DateTime endTime;

    public static SaveModsDialogService? Instance { get; private set; }

    string confirmText = "";
    Button? confirmButton;
    Func<bool>? onUIConfirmed;

    public SaveModsDialogService()
    {
        Instance = this;
    }

    public void ActivateFor(in StackedPanel panel)
    {
        onUIConfirmed = panel.PanelController.OnUIConfirmed;

        var ve = panel.VisualElement;

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
        onUIConfirmed = null;
    }

    public void UpdateSingleton()
    {
        if (confirmButton is null) { return; }

        if (DateTime.Now >= endTime)
        {
            onUIConfirmed?.Invoke();
            Dismiss();
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
