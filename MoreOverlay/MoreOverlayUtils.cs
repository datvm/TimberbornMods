namespace MoreOverlay;

public static class MoreOverlayUtils
{
    public static readonly Phrase DaysPhrase = Phrase.New().FormatDays<float>("F1");

    public const int IconSize = 20;

    extension(NamedIconProvider provider)
    {
        public Sprite GetPriority(Priority p) => provider.GetOrLoad("Priority-" + p.ToString(), "sprites/priority/buttons/" + p.ToString());
        public Sprite GetOrLoadStatusIcon(string name) => provider.GetOrLoad("Status-" + name, "sprites/statusicons/" + name);
    }

    extension(float f)
    {
        public string ToDays(ILoc t) => t.T(DaysPhrase, f);
    }

}
