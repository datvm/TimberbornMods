namespace ConfigurableTopBar.UI;

public class ConfigurableTopBarSetting() : NonPersistentSetting(new("", ""))
{

    public event Action OnReset = null!;

    public override void Reset() => OnReset();

}

public class ConfigurableTopBarSettingFactory(
    IContainer container,
    TopBarConfigProvider topBarConfigProvider,
    ISpecService specService,
    GoodSpriteProvider goodSpriteProvider
) : IModSettingElementFactory, ILoadableSingleton
{
    public int Priority { get; }

    public void Load()
    {
        topBarConfigProvider.Initialize(specService, goodSpriteProvider);
    }

    public bool TryCreateElement(ModSetting modSetting, out IModSettingElement? element)
    {
        if (modSetting is ConfigurableTopBarSetting s)
        {
            var el = container.GetInstance<ConfigurableTopBarPanel>().Init();
            s.OnReset += el.Reset;

            element = new ModSettingElement(el, modSetting);
            return true;
        }

        element = null;
        return false;
    }
}
