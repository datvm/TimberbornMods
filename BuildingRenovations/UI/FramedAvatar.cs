namespace BuildingRenovations.UI;

[BindTransient]
public class FramedAvatar : VisualElement
{
    readonly NamedIconProvider namedIconProvider;
    readonly Image icon;
    readonly VisualElement frame;

    const float FramePadding = .12f;

    public FramedAvatar(NamedIconProvider namedIconProvider)
    {
        frame = this.AddChild();
        frame.style.backgroundImage = new(namedIconProvider.GetOrLoad("renovations-bg", "UI/Renovations/renovation-bg"));

        icon = this.AddImage(namedIconProvider.QuestionMark).SetPosition();
        this.namedIconProvider = namedIconProvider;
    }

    public void SetIcon(Sprite? sprite, int? size = null)
    {
        icon.sprite = sprite ?? namedIconProvider.QuestionMark;

        if (size.HasValue)
        {
            SetSize(size.Value);
        }
    }

    public void SetSize(int size)
    {
        this.SetSize(size, size);
        frame.SetSize(size);

        var padding = Mathf.CeilToInt(size * FramePadding);
        icon.SetSize(size - padding * 2);
        icon.style.top = icon.style.left = padding;
    }

}
