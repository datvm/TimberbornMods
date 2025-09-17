namespace BuildingDecal.UI;

public class BuildingDecalImage : VisualElement
{
    public const int ImageSize = 100;

    public SpriteWithName SpriteWithName { get; }
    readonly string lowercaseName;

    public BuildingDecalImage(SpriteWithName spriteWithName)
    {
        SpriteWithName = spriteWithName;
        lowercaseName = spriteWithName.Name.ToLower();

        this.SetMargin(10);

        var icon = this.AddImage(name: "DecalIcon")
            .SetSize(ImageSize, ImageSize)
            .SetMarginBottom(5);
        icon.sprite = spriteWithName.Sprite;

        var lbl = this.AddGameLabel(spriteWithName.Name).SetMaxSizePercent(null, 100);
        lbl.style.unityTextAlign = TextAnchor.MiddleCenter;
    }

    public void Filter(string lowercaseKeyword)
    {
        this.SetDisplay(lowercaseKeyword.Length == 0 || lowercaseName.Contains(lowercaseKeyword));
    }

}
