namespace TimberPipes.Services;

[BindModUpdateNotifier]
public class ModUpdateNotifier : IModUpdateNotifier2
{
    public string ModId => nameof(TimberPipes);
    public string Version => "11.2.0";
    public int VersionNumber => 112000;
    public string MessageLocKey => "LV.TPi.ModUpdate112000";
}
