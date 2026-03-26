namespace TimberLive.Components;

sealed partial class TopBar : IDisposable
{
    static readonly float[] SpeedTemplates = [0f, 1f, 3f, 7f, .25f, 30f, 99f];

    HttpCommonData? data;
    bool forceShowNextWeather;
    float currSpeed = 1f;

    protected override void OnInitialized()
    {
        data = LiveData.Data;
        LiveData.NewDataArrived += OnNewData;
    }

    void OnNewData(object? sender, HttpCommonData e)
    {
        data = e;
        _ = InvokeAsync(StateHasChanged);
    }

    async Task SetTemplateSpeedAsync(float speed)
    {
        currSpeed = speed;
        await SetSpeedAsync();
    }

    async Task SetSpeedAsync()
    {
        if (currSpeed >= 0f)
        {
            await SetSpeedAsync(currSpeed);
        }
    }

    async Task SetSpeedAsync(float speed)
    {
        await Api.GetStringAsync("live-data/set-game-speed?speed=" + speed.ToString("0.##"));
        await Task.Delay(1000); // There is a delay before the game applies the speed
        LiveData.UpdateNow();
    }

    static string FormatHours(float dayProgress)
    {
        var hours = dayProgress * 24f;

        var hour = (int)hours;
        var minute = (int)((hours - hour) * 60);

        return $"{hour:D2}:{minute:D2}";
    }

    static string FormatDays(float days) => $"{days:F1}d";

    public void Dispose() => LiveData.NewDataArrived -= OnNewData;

}
