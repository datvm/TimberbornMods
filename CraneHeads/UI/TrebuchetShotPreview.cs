using UnityEngine.Rendering;

namespace CraneHeads.UI;

[BindSingleton]
public class TrebuchetShotPreview(
    MarkerDrawerFactory markers,
    IBlockService blocks,
    Highlighter highlighter
) : ILoadableSingleton, IUnloadableSingleton
{
    const float LineWidth = 0.08f;
    const float TileOffset = 0.02f;
    const int SegmentCount = 24;
    static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

    readonly GameObject container = new("TrebuchetShotPreview");
    readonly List<Vector3Int> blockers = [];
    MeshDrawer tile = null!;
    MeshDrawer block = null!;
    LineRenderer line = null!;
    Material success = null!;
    Material failure = null!;
    Vector3Int dest;
    bool valid;
    bool visible;

    public void Load()
    {
        success = CreateMaterial(TimberUiUtils.SuccessColor);
        failure = CreateMaterial(TimberUiUtils.DangerColor);
        tile = markers.CreateTileDrawer(TimberUiUtils.SuccessColor);
        block = markers.CreateSmallBlockTileDrawer();
        line = CreateLine();
        container.SetActive(false);
    }

    public void Unload()
    {
        Object.Destroy(container);
        Object.Destroy(success);
        Object.Destroy(failure);
    }

    public void Show(Vector3Int dest, bool valid, IReadOnlyList<Vector3> path, IReadOnlyList<Vector3Int> blockers)
    {
        this.dest = dest;
        this.valid = valid;
        this.blockers.Clear();
        this.blockers.AddRange(blockers);
        container.SetActive(true);
        highlighter.UnhighlightAllSecondary();
        foreach (var cell in this.blockers)
        {
            foreach (var obj in blocks.GetObjectsAt(cell))
            {
                highlighter.HighlightSecondary(obj, TimberUiUtils.DangerColor);
            }
        }

        if (path.Count < 2)
        {
            line.gameObject.SetActive(false);
            visible = true;
            Draw();
            return;
        }

        line.sharedMaterial = valid ? success : failure;
        line.positionCount = path.Count;
        for (var i = 0; i < path.Count; i++)
        {
            line.SetPosition(i, path[i]);
        }

        line.gameObject.SetActive(true);
        visible = true;
        Draw();
    }

    public void Draw()
    {
        if (!visible)
        {
            return;
        }

        var color = valid ? TimberUiUtils.SuccessColor : TimberUiUtils.DangerColor;
        tile.DrawAtCoordinates(dest, TileOffset, color);
        foreach (var cell in blockers)
        {
            block.DrawAtCoordinates(cell, 0f, TimberUiUtils.DangerColor);
        }
    }

    public void Hide()
    {
        visible = false;
        line.gameObject.SetActive(false);
        highlighter.UnhighlightAllSecondary();
        container.SetActive(false);
    }

    LineRenderer CreateLine()
    {
        var go = new GameObject("Curve");
        go.transform.SetParent(container.transform, false);
        var lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.sharedMaterial = success;
        lr.startWidth = LineWidth;
        lr.endWidth = LineWidth;
        lr.numCapVertices = 4;
        lr.numCornerVertices = 4;
        lr.shadowCastingMode = ShadowCastingMode.Off;
        lr.receiveShadows = false;
        lr.alignment = LineAlignment.View;
        lr.positionCount = SegmentCount + 1;
        lr.startColor = Color.white;
        lr.endColor = Color.white;
        lr.gameObject.SetActive(false);
        return lr;
    }

    static Material CreateMaterial(Color color)
    {
        var shader = Shader.Find("Universal Render Pipeline/Unlit")
            ?? Shader.Find("Unlit/Color")
            ?? Shader.Find("Sprites/Default");
        var material = new Material(shader)
        {
            name = $"TrebuchetShot-{color}",
        };
        material.color = color;
        material.SetColor(BaseColorId, color);
        return material;
    }
}
