global using Timberborn.AssetSystem;
global using Timberborn.TickSystem;

namespace NarrativeEvents.Services;

public class NarrativeService(NarrativeDialogBoxShower shower, ILoc loc, IAssetLoader assets) : ITickableSingleton
{

    static bool showed = false;

    public async void Tick()
    {
        if (showed) { return; }
        showed = true;

        var img = assets.Load<Texture2D>("Sprites/birthday");

        var choice = await shower.Create()
            .SetTexts(
                "Coming of Age",
                "The first child, {BeaverName}, has grown up today and is ready to contribute to our bustling community. Such a momentous occasion deserves recognition.")
            .SetImage(img)
            .AddChoice("A cause for celebration", "-24 berries, work efficiency +15% for 24 hours.\r\n(You cannot afford this)", disabled: true)
            .AddChoice("I see", "(Nothing happens)")
            .ShowAsync();
        Debug.Log("User chose " + choice);
    }

}
