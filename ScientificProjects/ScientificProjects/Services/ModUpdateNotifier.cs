namespace ScientificProjects.Services;

public class ModUpdateNotifier : IModUpdateNotifier
{
    public string ModId { get; } = nameof(ScientificProjects);
    public string Version { get; } = "7.4.0";
    public string MessageLocKey { get; } = "LV.SP.UpdateNote740";
}
