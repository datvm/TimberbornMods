namespace TimberLive.Components.Pages.Automations;

sealed partial class Automations
{
    const string AutomationsModId = "MoreHttpApiAutomations";

    bool? automationsModEnabled;
    HttpAutomationMap? automationMap;

    protected override async Task OnInitializedAsync()
    {
        automationsModEnabled = await Api.GetAsync<bool>(
            $"misc/mods/enabled?id={Uri.EscapeDataString(AutomationsModId)}");
    }

    async Task GetSnapshot()
    {
        automationMap = await Api.GetAsync<HttpAutomationMap>("automations");
        StateHasChanged();
    }

    static string GetAutomatorKind(HttpAutomatorKind kind) => kind switch
    {
        HttpAutomatorKind.Terminal => "Terminal",
        HttpAutomatorKind.Transmitter => "Transmitter",
        HttpAutomatorKind.CombinationalTransmitter => "Combinational Transmitter",
        HttpAutomatorKind.SequentialTransmitter => "Sequential Transmitter",
        HttpAutomatorKind.SamplingTransmitter => "Sampling Transmitter",
        _ => kind.ToString(),
    };
}
