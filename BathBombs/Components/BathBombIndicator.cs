namespace BathBombs.Components;

public class BathBombIndicator : BaseComponent, IAwakableComponent
{
    static readonly Vector3 LocalPosition = new(.5f, .01f, .5f);
    static readonly Vector3 LocalScale = new(.7f, .7f, 1);

    public void Awake()
    {
        var icon = GetComponent<LabeledEntitySpec>().Icon.Asset;

        var go = new GameObject("BathBombIndicator");
        var t = go.transform;
        t.parent = Transform;
        t.Rotate(Vector3.right, 90);
        t.localScale = LocalScale;
        t.localPosition = LocalPosition;

        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = icon;
    }

}
