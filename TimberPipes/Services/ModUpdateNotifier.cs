namespace TimberPipes.Services;

[BindModUpdateNotifier]
public class ModUpdateNotifier : IModUpdateNotifier2
{
    public string ModId => nameof(TimberPipes);
    public string Version => "11.3.0";
    public int VersionNumber => 113000;
    public string MessageLocKey => "LV.TPi.ModUpdate113000";
}
