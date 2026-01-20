namespace TimberUi.Services;

[BindSingleton(Contexts = BindAttributeContext.All)]
public class NamedIconProvider(IAssetLoader assets)
{

    readonly Dictionary<string, Sprite> spritesByPath = [];
    readonly Dictionary<string, Sprite> spritesByName = [];

    public Sprite this[string name] => spritesByName[name];

    public Sprite Food => GetOrLoadTopbar(nameof(Food), nameof(Food));
    public Sprite Logs => GetOrLoadTopbar(nameof(Logs), nameof(Logs));
    public Sprite Materials => GetOrLoadTopbar(nameof(Materials), nameof(Materials));
    public Sprite Science => GetOrLoadTopbar(nameof(Science), nameof(Science));
    public Sprite Water => GetOrLoadTopbar(nameof(Water), nameof(Water));

    public Sprite Clock => GetOrLoadGameIcon("Clock", "production-icon");
    public Sprite Arrow => GetOrLoadGameIcon("Arrow", nameof(Arrow));
    public Sprite ScienceAlt => GetOrLoadGameIcon("ScienceAlt", "science-icon");

    public Sprite GetOrLoad(string name, string path)
    {
        if (spritesByName.TryGetValue(name, out var sprite))
        {
            return sprite;
        }

        if (!spritesByPath.TryGetValue(path, out sprite))
        {
            sprite = assets.Load<Sprite>(path);
            spritesByPath[path] = sprite;
        }

        spritesByName[name] = sprite;
        return sprite;
    }
    public Sprite GetOrLoadTopbar(string name, string iconName) => GetOrLoad(name, $"sprites/topbar/{iconName}");
    public Sprite GetOrLoadGameIcon(string name, string iconName) => GetOrLoad(name, $"ui/images/game/{iconName}");

}
