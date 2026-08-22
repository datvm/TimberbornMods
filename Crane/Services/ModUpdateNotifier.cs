namespace Crane.Services;

[BindModUpdateNotifier]
public class ModUpdateNotifier : IModUpdateNotifier2
{
    public string ModId => nameof(Crane);
    public string Version => "11.1.1";
    public int VersionNumber => 110101;
    public string MessageLocKey => "";
}
