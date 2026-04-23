using UnityEngine.Rendering;

namespace DistroStorage.UI;

[BindSingleton]
public class DistroLinkRenderer : ILoadableSingleton, IUnloadableSingleton
{
    public static readonly Color OutputColor = new(1f, 106f / 255, 0);
    public static readonly Color InputColor = new(0, .5f, 14 / 255f);

    readonly GameObject lineContainer = new();
    Material lineMaterial = null!;

    const float ArcHeight = 1f;
    const float LineWidth = 0.05f;
    const int SegmentCount = 20;

    // Input parameters are reversed to make the arrow point towards the receiver.
    public void AddInputLine(Vector3 to, Vector3 from) => AddLine(from, to, InputColor);
    public void AddOutputLine(Vector3 from, Vector3 to) => AddLine(from, to, OutputColor);

    public void AddLine(Vector3 from, Vector3 to, Color color)
    {
        if (from == to) { return; }

        to = GetOffsetTarget(from, to);

        var go = new GameObject("Line");
        go.transform.SetParent(lineContainer.transform, false);

        var lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.material = lineMaterial;
        lr.startColor = color;
        lr.endColor = color;
        lr.startWidth = LineWidth;
        lr.endWidth = LineWidth;
        lr.numCapVertices = 4;
        lr.numCornerVertices = 4;
        lr.shadowCastingMode = ShadowCastingMode.Off;
        lr.receiveShadows = false;
        lr.alignment = LineAlignment.View;

        var positions = new Vector3[SegmentCount + 1];

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


        // Draw arrowhead
        var end = positions[^1];
        var prev = positions[^2];
        var dir = end - prev;

        const float headLength = 0.35f;

        // Move the line end slightly back so it meets the arrowhead cleanly.
        positions[^1] = end - dir * headLength;

        CreateArrowHead(
            go.transform,
            end,
            dir,
            color);

        lr.positionCount = positions.Length;
        lr.SetPositions(positions);



    }

    public void Clear()
    {
        var children = lineContainer.GetComponentsInChildren<Transform>();
        foreach (var child in children)
        {
            if (child != lineContainer.transform)
            {
                UnityEngine.Object.Destroy(child.gameObject);
            }
        }
    }

    public void Load()
    {
        var shader = Shader.Find("Sprites/Default");
        if (shader == null)
        {
            Debug.LogError("Could not find shader 'Sprites/Default' for line rendering.");
            return;
        }

        lineMaterial = new Material(shader);
    }

    public void Unload()
    {
        UnityEngine.Object.Destroy(lineContainer);
        UnityEngine.Object.Destroy(lineMaterial);
    }

    GameObject CreateArrowHead(
    Transform parent,
    Vector3 tip,
    Vector3 direction,
    Color color,
    float length = 0.35f,
    float width = 0.2f)
    {
        if (direction.sqrMagnitude < 0.0001f)
        {
            direction = Vector3.forward;
        }

        direction.Normalize();

        var go = new GameObject("ArrowHead");
        go.transform.SetParent(parent, false);

        var meshFilter = go.AddComponent<MeshFilter>();
        var meshRenderer = go.AddComponent<MeshRenderer>();

        meshRenderer.material = lineMaterial;
        meshRenderer.material.color = color;

        var halfWidth = width * 0.5f;
        var mesh = new UnityEngine.Mesh
        {
            vertices =
            [
                new Vector3(0f, 0f, 0f),              // tip at local origin
            new Vector3(-halfWidth, 0f, -length), // left base behind tip
            new Vector3(halfWidth, 0f, -length),  // right base behind tip
        ],
            triangles = [0, 1, 2]
        };
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;

        go.transform.position = tip;
        go.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);

        return go;
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
}
