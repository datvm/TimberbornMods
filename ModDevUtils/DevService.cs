using Timberborn.AssetSystem;
using UiBuilder.Common;

namespace ModDevUtils;

[Context("MainMenu")]
public class DevServiceConfig : IConfigurator
{
    public void Configure(IContainerDefinition containerDefinition)
    {
        containerDefinition.Bind<DevService>().AsSingleton();
    }
}

public class DevService(DialogBoxShower diag, IAssetLoader assets, VisualElementLoader ve, ILoc loc) : ILoadableSingleton
{

    public async void Load()
    {
        TestDescHeader();
        TestDialog();
        await Task.CompletedTask;
    }

    public void TestDescHeader()
    {
        ve.LoadVisualElement("mainmenu/WelcomeScreenBox").PrintVisualTree();

    }

    public void TestDialog()
    {
        var container = new DialogBoxElement(true);
        var scroll = container.Box;

        var header = scroll.AddChild<VisualElement>()
            .SetMargin(bottom: 20)
            .SetAsRow();

        var image = CreateChild<Image>(header)
            .SetSize(100)
            .SetMargin(right: 20);
        image.image = assets.Load<Texture2D>("Sprites/elder");

        header.AddLabel(loc.T("LV.NE.E1.Title"), "Title", additionalClasses: ["text--header"]);

        scroll.AddLabel(loc.T("LV.NE.E1.Description"), "Desc", additionalClasses: ["text--default"])
            .SetMargin(bottom: 20);

        var btn1 = scroll.AddButton(loc.T("LV.NE.E1.Choice1"), onClick: () => Debug.Log("Hi"));

        diag.Show(new(diag._panelStack, DoNothing, DoNothing, container));
    }

    static void DoNothing() { }

    static T CreateChild<T>(VisualElement parent) where T : VisualElement, new()
    {
        var child = new T();
        parent.Add(child);
        return child;
    }

}
