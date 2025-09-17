
namespace BuildingDecal.Services;

public class BuildingDecalItem
{
#nullable disable
    GameObject decalObject;
    SpriteRenderer renderer;
#nullable enable

    public bool Attached => decalObject && renderer;
    public string DecalName { get; set; } = DecalPictureService.ErrorIconName;
    public Vector3 Position
    {
        get;
        set
        {
            field = value;
            if (decalObject)
            {
                decalObject.transform.localPosition = value;
            }
        }
    }
    public Quaternion Rotation
    {
        get;
        set
        {
            field = value;
            if (decalObject)
            {
                decalObject.transform.localRotation = value;
            }
        }
    }
    public Vector3 Scale
    {
        get;
        set
        {
            field = value;
            if (decalObject)
            {
                decalObject.transform.localScale = value;
            }
        }
    } = Vector3.one;
    public Color Color
    {
        get;
        set
        {
            field = value;
            if (renderer)
            {
                renderer.color = value;
            }
        }
    } = Color.white;
    public bool FlipX
    {
        get;
        set
        {
            field = value;
            if (renderer)
            {
                renderer.flipX = value;
            }
        }
    }
    public bool FlipY
    {
        get;
        set
        {
            field = value;
            if (renderer)
            {
                renderer.flipY = value;
            }
        }
    }

    public Vector2 SpriteSize => renderer.sprite.textureRect.size / DecalPictureService.PixelPerUnit;

    public void AttachTo(GameObject parent)
    {
        if (Attached) { return; }

        decalObject = new();
        var t = decalObject.transform;
        t.parent = parent.transform;
        t.localPosition = Position;
        t.localRotation = Rotation;
        t.localScale = Scale;

        renderer = decalObject.AddComponent<SpriteRenderer>();
        renderer.color = Color;
        renderer.flipX = FlipX;
        renderer.flipY = FlipY;
    }

    public void Detach()
    {
        if (!Attached) { return; }

        GameObject.Destroy(decalObject);
        decalObject = null;
        renderer = null;
    }

    public void SetSprite(Sprite sprite)
    {
        renderer.sprite = sprite;
    }

    public void SetSprite(SpriteWithName spriteWithName)
    {
        SetSprite(spriteWithName.Sprite);
        DecalName = spriteWithName.Name;
    }

}

