using UnityEngine.Rendering;

namespace DistroStorage.UI;

[BindSingleton]
public class DistroLinkRenderer : ILoadableSingleton, IUnloadableSingleton
{
    const string SettingsKey = $"{nameof(DistroStorage)}.Settings.DisableArrows";

    public static readonly Color OutputColor = new(1f, 106f / 255, 0);
    public static readonly Color InputColor = new(0, .5f, 14 / 255f);

    readonly GameObject lineContainer = new();
    readonly List<LinkView> pool = [];
    readonly List<LinkView> shown = [];

    Material lineMaterial = null!;
    Material inputHeadMaterial = null!;
    Material outputHeadMaterial = null!;
    UnityEngine.Mesh headMesh = null!;

    public bool DisableArrows { get; private set; }

    const float ArcHeight = 1f;
    const float LineWidth = 0.05f;
    const float HeadLength = 0.35f;
    const float HeadWidth = 0.2f;
    const int SegmentCount = 20;

    // Input parameters are reversed to make the arrow point towards the receiver.
    public void AddInputLine(Vector3 to, Vector3 from) => AddLine(from, to, InputColor);
    public void AddOutputLine(Vector3 from, Vector3 to) => AddLine(from, to, OutputColor);

    public void AddLine(Vector3 from, Vector3 to, Color color)
    {
        if (DisableArrows || from == to || !headMesh) { return; }

        to = GetOffsetTarget(from, to);
        var headMaterial = color == InputColor ? inputHeadMaterial : outputHeadMaterial;
        Rent().Show(from, to, color, headMaterial);
    }

    public void Clear()
    {
        for (var i = 0; i < shown.Count; i++)
        {
            var view = shown[i];
            view.SetActive(false);
            pool.Add(view);
        }

        shown.Clear();
    }

    public void Load()
    {
        DisableArrows = PlayerPrefs.GetInt(SettingsKey, 0) == 1;
        lineContainer.SetActive(!DisableArrows);
        EnsureMaterials();
    }

    public void Unload()
    {
        shown.Clear();
        pool.Clear();
        UnityEngine.Object.Destroy(lineContainer);
        UnityEngine.Object.Destroy(lineMaterial);
        UnityEngine.Object.Destroy(inputHeadMaterial);
        UnityEngine.Object.Destroy(outputHeadMaterial);
        UnityEngine.Object.Destroy(headMesh);
    }

    public void SetDisableArrows(bool disable)
    {
        DisableArrows = disable;
        lineContainer.SetActive(!disable);
        if (disable)
        {
            Clear();
        }

        PlayerPrefs.SetInt(SettingsKey, disable ? 1 : 0);
    }

    void EnsureMaterials()
    {
        if (lineMaterial) { return; }

        var shader = Shader.Find("Sprites/Default");
        if (shader == null)
        {
            Debug.LogError("Could not find shader 'Sprites/Default' for line rendering.");
            return;
        }

        lineMaterial = new(shader);
        inputHeadMaterial = new(shader) { color = InputColor };
        outputHeadMaterial = new(shader) { color = OutputColor };
        headMesh = CreateHeadMesh();
    }

    LinkView Rent()
    {
        var count = pool.Count;
        LinkView view;
        if (count > 0)
        {
            view = pool[count - 1];
            pool.RemoveAt(count - 1);
        }
        else
        {
            view = new(lineContainer.transform, lineMaterial, headMesh);
        }

        view.SetActive(true);
        shown.Add(view);
        return view;
    }

    static UnityEngine.Mesh CreateHeadMesh()
    {
        var halfWidth = HeadWidth * 0.5f;
        var mesh = new UnityEngine.Mesh
        {
            vertices =
            [
                new Vector3(0f, 0f, 0f),
                new Vector3(-halfWidth, 0f, -HeadLength),
                new Vector3(halfWidth, 0f, -HeadLength),
            ],
            triangles = [0, 1, 2],
        };
        mesh.RecalculateNormals();
        mesh.UploadMeshData(true);
        return mesh;
    }

    static Vector3 GetOffsetTarget(Vector3 from, Vector3 center, float radius = 0.25f)
    {
        var incoming = center - from;

        // Flatten to ground plane so the offset is horizontal, not vertical.
        var planar = Vector3.ProjectOnPlane(incoming, Vector3.up);

        if (planar.sqrMagnitude < 0.0001f)
        {
            return center;
        }

        planar.Normalize();

        // Arrow approaches toward center, so land on the near edge of the circle.
        return center - planar * radius;
    }

    sealed class LinkView
    {
        readonly GameObject root;
        readonly LineRenderer line;
        readonly Transform head;
        readonly MeshRenderer headRenderer;
        readonly Vector3[] positions = new Vector3[SegmentCount + 1];

        public LinkView(Transform parent, Material lineMaterial, UnityEngine.Mesh headMesh)
        {
            root = new("Line");
            root.transform.SetParent(parent, false);

            line = root.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.sharedMaterial = lineMaterial;
            line.startWidth = LineWidth;
            line.endWidth = LineWidth;
            line.positionCount = positions.Length;
            line.numCapVertices = 4;
            line.numCornerVertices = 4;
            line.shadowCastingMode = ShadowCastingMode.Off;
            line.receiveShadows = false;
            line.alignment = LineAlignment.View;

            var headObject = new GameObject("ArrowHead");
            headObject.transform.SetParent(root.transform, false);
            head = headObject.transform;
            headObject.AddComponent<MeshFilter>().sharedMesh = headMesh;

            headRenderer = headObject.AddComponent<MeshRenderer>();
            headRenderer.shadowCastingMode = ShadowCastingMode.Off;
            headRenderer.receiveShadows = false;
        }

        public void SetActive(bool active)
        {
            root.SetActive(active);
        }

        public void Show(Vector3 from, Vector3 to, Color color, Material headMaterial)
        {
            var peakOffset = ArcHeight + Mathf.Abs(to.y - from.y) * 0.5f;

            for (var i = 0; i <= SegmentCount; i++)
            {
                var t = i / (float)SegmentCount;
                var p = Vector3.Lerp(from, to, t);

                // 0 at endpoints, 1 at midpoint
                var arc = 4f * t * (1f - t);
                p.y += arc * peakOffset;
                positions[i] = p;
            }

            var end = positions[^1];
            var prev = positions[^2];
            var dir = end - prev;

            // Move the line end slightly back so it meets the arrowhead cleanly.
            positions[^1] = end - dir * HeadLength;

            line.startColor = color;
            line.endColor = color;
            line.SetPositions(positions);

            if (dir.sqrMagnitude < 0.0001f)
            {
                dir = Vector3.forward;
            }

            head.SetPositionAndRotation(end, Quaternion.LookRotation(dir, Vector3.up));
            headRenderer.sharedMaterial = headMaterial;
        }
    }
}
