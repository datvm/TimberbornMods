namespace ScientificProjects.Services;

public class ModUpdateNotifier : IModUpdateNotifier2
{
    public string ModId { get; } = nameof(ScientificProjects);
    public string Version => "11.0.0";
    public string MessageLocKey => "";
    public int VersionNumber => 110000;

}
