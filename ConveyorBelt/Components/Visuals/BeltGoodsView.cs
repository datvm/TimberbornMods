namespace ConveyorBelt.Components.Visuals;

sealed class BeltGoodsView(BeltVisualService visuals)
{
    public const float BoxFill = 0.6f;
    public const float BoxY = 0.11f;

    const float ShelfThick = 0.02f;
    const float HoleX0 = 0.35525f;
    const float HoleX1 = 0.64475f;
    const float HoleZ0 = 0.38f;
    const float HoleZ1 = 0.62f;

    public bool ShelfAlongZ;

    readonly List<GameObject> goods = [];
    readonly List<GameObject> shelves = [];
    readonly List<GameObject> icons = [];
    readonly List<MeshRenderer> boxRenderers = [];
    readonly List<MeshRenderer> iconRenderers = [];
    readonly List<Material> materials = [];
    readonly List<Material> iconMaterials = [];
    readonly List<string> shownIds = [];

    EntityMaterials? entityMaterials;
    GameObject? root;

    public int Count => goods.Count;

    public bool HasRoot => root;

    public void Bind(EntityMaterials? materials) => entityMaterials = materials;

    public void Attach(Transform parent, bool active)
    {
        if (root)
        {
            return;
        }

        root = new GameObject("Goods");
        root.layer = parent.gameObject.layer;
        root.transform.SetParent(parent, false);
        root.SetActive(active);
    }

    public void ShowRoot()
    {
        if (root)
        {
            root.SetActive(true);
        }
    }

    public void Hide()
    {
        if (root)
        {
            root.SetActive(false);
        }
    }

    public void EnsureCount(int count)
    {
        if (!root)
        {
            return;
        }

        while (goods.Count < count)
        {
            var copy = visuals.CreateGood(root.transform, entityMaterials);
            var icon = copy.transform.Find("Icon").gameObject;
            goods.Add(copy);
            shelves.Add(visuals.CreatePanel(root.transform));
            icons.Add(icon);
            boxRenderers.Add(copy.GetComponent<MeshRenderer>());
            iconRenderers.Add(icon.GetComponent<MeshRenderer>());
            materials.Add(boxRenderers[^1].sharedMaterial);
            iconMaterials.Add(iconRenderers[^1].sharedMaterial);
            shownIds.Add("");
        }
    }

    public void ShowTrails(IReadOnlyList<BeltTrail> trails, float progress, Func<float, Vector3> pose)
    {
        var scale = GoodScale();
        EnsureCount(trails.Count);
        for (var i = 0; i < trails.Count; i++)
        {
            var trail = trails[i];
            if (!trail.Shows(progress))
            {
                goods[i].SetActive(false);
                shelves[i].SetActive(false);
                continue;
            }

            ShowOne(i, trail.Id, pose(trail.Along(progress)), scale);
        }

        HideRest(trails.Count);
    }

    public void ShowSettled(IReadOnlyList<BeltGood> items, Func<float, Vector3> pose)
    {
        var scale = GoodScale();
        EnsureCount(items.Count);
        for (var i = 0; i < items.Count; i++)
        {
            var item = items[i];
            ShowOne(i, item.Id, pose(item.Position), scale);
        }

        HideRest(items.Count);
    }

    public void SyncFromRenderers()
    {
        for (var i = 0; i < goods.Count; i++)
        {
            var current = boxRenderers[i].sharedMaterial;
            if (current && current != materials[i])
            {
                materials[i] = current;
            }

            var icon = iconRenderers[i].sharedMaterial;
            if (icon && icon != iconMaterials[i])
            {
                iconMaterials[i] = icon;
            }
        }
    }

    public void DestroyMaterials()
    {
        foreach (var material in materials)
        {
            Object.Destroy(material);
        }

        foreach (var material in iconMaterials)
        {
            Object.Destroy(material);
        }
    }

    void ShowOne(int i, string id, Vector3 position, float scale)
    {
        var good = goods[i];
        var boxRenderer = boxRenderers[i];
        var iconRenderer = iconRenderers[i];
        Adopt(i, boxRenderer, iconRenderer);
        if (shownIds[i] != id)
        {
            Paint(i, id);
        }

        var onDeck = position.y > -0.001f && position.y <= 0f;
        var climbing = !onDeck;
        if (onDeck)
        {
            position.y = BoxY;
        }

        good.SetActive(true);
        good.transform.localScale = Vector3.one * scale;
        good.transform.localPosition = position;
        var shelf = shelves[i];
        shelf.SetActive(climbing);
        if (climbing)
        {
            var depth = BeltRates.Spacing * BoxFill * 1.5f;
            var y = position.y - ShelfThick * 0.5f - 0.001f;
            if (ShelfAlongZ)
            {
                shelf.transform.localScale = new Vector3(depth, ShelfThick, HoleZ1 - HoleZ0);
                shelf.transform.localPosition = new Vector3(position.x, y, (HoleZ0 + HoleZ1) * 0.5f);
            }
            else
            {
                shelf.transform.localScale = new Vector3(HoleX1 - HoleX0, ShelfThick, depth);
                shelf.transform.localPosition = new Vector3((HoleX0 + HoleX1) * 0.5f, y, position.z);
            }
        }
        Adopt(i, boxRenderer, iconRenderer);
        if (shownIds[i] != id)
        {
            Paint(i, id);
        }

        icons[i].SetActive(shownIds[i] == id);
    }

    void Adopt(int i, MeshRenderer boxRenderer, MeshRenderer iconRenderer)
    {
        var boxMaterial = boxRenderer.sharedMaterial;
        var iconMaterial = iconRenderer.sharedMaterial;
        if (boxMaterial && boxMaterial != materials[i])
        {
            materials[i] = boxMaterial;
            shownIds[i] = "";
        }

        if (iconMaterial && iconMaterial != iconMaterials[i])
        {
            iconMaterials[i] = iconMaterial;
            shownIds[i] = "";
        }
    }

    void Paint(int i, string id)
    {
        if (!visuals.ApplyGood(materials[i], iconMaterials[i], id))
        {
            return;
        }

        shownIds[i] = id;
    }

    void HideRest(int used)
    {
        for (var i = used; i < goods.Count; i++)
        {
            goods[i].SetActive(false);
            shelves[i].SetActive(false);
            shownIds[i] = "";
        }
    }

    float GoodScale() => visuals.GoodScale(BeltRates.Spacing * BoxFill);
}
