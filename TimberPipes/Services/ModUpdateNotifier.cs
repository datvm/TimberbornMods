namespace TimberPipes.Services;

[BindModUpdateNotifier]
public class ModUpdateNotifier : IModUpdateNotifier2
{
    public string ModId => nameof(TimberPipes);
    public string Version => "11.4.0";
    public int VersionNumber => 114000;
    public string MessageLocKey => "LV.TPi.ModUpdate114000";
}
