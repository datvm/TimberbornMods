namespace WoodYouLookAtTheLog.Services;

[BindSingleton]
public class TimerService(
    IContainer container,
    DialogService diag,
    ILoc t,
    SettlementReferenceService settlementReferenceService,
    GameSaver gameSaver,
    TimerPanel timerPanel
) : IPostLoadableSingleton, IUpdatableSingleton
{
    static readonly ImmutableArray<int> WarningSeconds = [10 * 60, 5 * 60, 60];
    const float StartCooldownTime = 10f;
    bool? prompted;

    float startTime;
    int? minutes;
    float nextWarning;
    float closeTime;

    public void PostLoad()
    {
        startTime = Time.unscaledTime;
    }

    public void UpdateSingleton()
    {
        if (prompted is null)
        {
            if (Time.unscaledTime - startTime > StartCooldownTime)
            {
                prompted = false;
                PromptForTimeAsync();
            }

            return;
        }
        else if (prompted == false || minutes is null)
        {
            return;
        }

        var time = Time.unscaledTime;
        var remaining = closeTime - time;
        var remainingMin = Mathf.CeilToInt(remaining / 60f);
        timerPanel.SetTime(remainingMin);

        if (nextWarning > 0) 
        {
            // Always show a warning before exit
            if (remaining < nextWarning)
            {   
                diag.Alert(t.T("LV.WYLATL.CloseWarning", remainingMin));
                SetNextWarning();
            }

            return;
        }

        if (time >= closeTime)
        {
            minutes = null;
            timerPanel.Hide();

            if (!SaveGame(ExitGame))
            {
                diag.Alert(t.T("LV.WYLATL.SaveError"));
            }
        }
    }

    void ExitGame()
    {
        Application.Quit();
    }

    async void PromptForTimeAsync()
    {
        var diag = container.GetInstance<TimePickerDialog>();

        minutes = await diag.ShowAsync();
        prompted = true;

        if (minutes <= 0) { minutes = null; }
        if (minutes is null) { return; }

        startTime = Time.unscaledTime;
        closeTime = startTime + minutes.Value * 60f;
        timerPanel.SetTime(minutes.Value);
        timerPanel.Show();
        SetNextWarning();
    }

    void SetNextWarning()
    {
        var timeLeft = closeTime - Time.unscaledTime;
        if (timeLeft <= 0)
        {
            nextWarning = 0;
            return;
        }

        foreach (var s in WarningSeconds)
        {
            if (timeLeft > s)
            {
                nextWarning = s;
                return;
            }
        }

        nextWarning = 0;
    }

    bool SaveGame(Action onSaveDone)
    {
        try
        {
            var saveRef = new SaveReference("WoodYouLookAtTheLog_ExitSave", settlementReferenceService.SettlementReference);
            gameSaver.QueueSaveSkippingNameValidation(saveRef, onSaveDone);
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Error occured while saving: {ex}");
            return false;
        }
    }

}
