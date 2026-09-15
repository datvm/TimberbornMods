namespace TimberPipes.Services;

[BindModUpdateNotifier]
public class ModUpdateNotifier : IModUpdateNotifier2
{
    public string ModId => nameof(TimberPipes);
    public string Version => "11.0.1";
    public int VersionNumber => 110010;
    public string MessageLocKey => "";
}
