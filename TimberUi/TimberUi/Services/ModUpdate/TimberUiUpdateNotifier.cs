namespace TimberUi.Services.ModUpdate;

[BindModUpdateNotifier]
public class TimberUiUpdateNotifier : IModUpdateNotifier2
{
    public string ModId => nameof(TimberUi);
    public string Version => "11.0.0";
    public int VersionNumber => 110000;
    public string MessageLocKey => "";
}
