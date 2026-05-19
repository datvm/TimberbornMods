namespace TimberLive.Components.Pages.Automations;

sealed partial class Automations
{
    const string AutomationsModId = "MoreHttpApiAutomations";

    bool? automationsModEnabled;
    ClientAutomationMap? automationMap;
    string selectedView = "";
    string fullMermaidText = "";
    string currentMermaidText = "";
    bool hideSingleNodePartitions = true;

    protected override async Task OnInitializedAsync()
    {
        automationsModEnabled = await Api.GetAsync<bool>(
            $"misc/mods/enabled?id={Uri.EscapeDataString(AutomationsModId)}");
    }

    async Task GetSnapshot()
    {
        var httpMap = await Api.GetAsync<HttpAutomationMap>("automations");
        automationMap = AutomationsSvc.ConvertToClientMap(httpMap);
        selectedView = "";
        OnPartitionViewSelected();
    }

    async Task CopyMermaidAsync()
    {
        if (string.IsNullOrEmpty(fullMermaidText))
        {
            return;
        }

        await JS.InvokeVoidAsync("navigator.clipboard.writeText", fullMermaidText);
    }

    void OnPartitionViewSelected()
    {
        if (automationMap is null)
        {
            fullMermaidText = "";
            currentMermaidText = "";
            return;
        }

        var visiblePartitions = GetVisiblePartitions();

        ClientAutomationMap overviewMap = new();
        foreach (var visiblePartition in visiblePartitions)
        {
            overviewMap.Partitions.Add(visiblePartition);
        }

        foreach (var (id, building) in automationMap.Buildings)
        {
            overviewMap.Buildings[id] = building;
        }

        fullMermaidText = MermaidCharts.ConvertToMermaid(overviewMap);

        if (selectedView == "")
        {
            currentMermaidText = fullMermaidText;
            return;
        }

        if (!int.TryParse(selectedView, out var partitionId))
        {
            currentMermaidText = fullMermaidText;
            return;
        }

        var selectedPartition = visiblePartitions.FirstOrDefault(p => p.Id == partitionId);
        currentMermaidText = selectedPartition is null
            ? fullMermaidText
            : MermaidCharts.ConvertToMermaid(selectedPartition);
    }

    IEnumerable<ClientAutomationPartition> GetVisiblePartitions()
    {
        if (automationMap is null)
        {
            return [];
        }

        if (!hideSingleNodePartitions)
        {
            return automationMap.Partitions;
        }

        return automationMap.Partitions.Where(p => p.Automators.Count > 1);
    }

    async Task OnNodeClick(NodeClickEventArgs args)
    {
        if (!Guid.TryParse(args.EntityId, out var entityId))
        {
            return;
        }

        await Api.SelectEntityAsync(entityId, true);
    }
}
