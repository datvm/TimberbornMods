namespace MoreOverlay;

public static class MoreOverlayUtils
{

    public const int IconSize = 20;

    extension(NamedIconProvider provider)
    {
        public Sprite GetPriority(Priority p) => provider.GetOrLoad("Priority-" + p.ToString(), "sprites/priority/buttons/" + p.ToString());
    }

}
