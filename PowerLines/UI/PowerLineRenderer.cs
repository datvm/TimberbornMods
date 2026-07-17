using UnityEngine.Rendering;

namespace PowerLines.UI;

[BindSingleton]
public class PowerLineRenderer(
    PowerLineConnectionService connService,
    EventBus eb
) : ILoadableSingleton, IUnloadableSingleton
{
    public static readonly Color PowerColor = new(1f, 216f / 255f, 0f);

    const float LineWidth = 0.06f;
    const int SegmentCount = 8;
    const float MinSag = 0.25f;
    const float MaxSag = 2.5f;
    const float SagFactor = 0.18f;

    public bool ShouldShow { get; private set; } = true;

    readonly Dictionary<PowerLineConnection, LineRenderer> lines = [];
    readonly GameObject container = new("PowerLineRenderer");
    readonly GameObject toolContainer = new("PowerLineToolPreview");
#nullable disable
    Material neutral, successful, failure;
    LineRenderer toolLine;
#nullable enable

    public void Load()
    {
        toolContainer.transform.SetParent(container.transform, false);
        container.SetActive(false);
        toolContainer.SetActive(false);

        neutral = CreateMaterial(PowerColor);
        successful = CreateMaterial(TimberUiUtils.SuccessColor);
        failure = CreateMaterial(TimberUiUtils.DangerColor);
        toolLine = CreateLineObject(toolContainer.transform, "ToolLine", neutral);

        connService.ConnectionAdded += OnConnectionAdded;
        connService.ConnectionRemoved += OnConnectionRemoved;

        eb.Register(this);
    }

    [OnEvent]
    public void OnBuildingSelected(SelectableObjectSelectedEvent e)
    {
        if (ShouldShow && e.SelectableObject.HasComponent<PowerLineComponent>())
        {
            container.SetActive(true);
        }
    }

    [OnEvent]
    public void OnDeselected(SelectableObjectUnselectedEvent _)
    {
        // Keep visible while the connection tool is actively drawing a preview.
        if (!toolContainer.activeSelf)
        {
            container.SetActive(false);
        }
    }

    public void StartRenderingConnectionTool()
    {
        toolContainer.SetActive(true);
        container.SetActive(true);
        toolLine.gameObject.SetActive(false);
    }

    public void StopRenderingConnectionTool()
    {
        toolLine.gameObject.SetActive(false);
        toolContainer.SetActive(false);
        if (!ShouldShow)
        {
            container.SetActive(false);
        }
    }

    public void UpdateToolPreview(PowerLineComponent from, PowerLineComponent? to, bool canConnect)
    {
        if (to is not { } target || target == from)
        {
            toolLine.gameObject.SetActive(false);
            return;
        }

        toolLine.sharedMaterial = canConnect ? successful : failure;
        SetBezier(toolLine, from, target);
        toolLine.gameObject.SetActive(true);
    }

    void OnConnectionAdded(object sender, PowerLineConnection conn)
    {
        if (lines.ContainsKey(conn)) { return; }

        var lr = CreateLineObject(container.transform, $"PowerLine-{conn.GuidA:N}-{conn.GuidB:N}", neutral);
        SetBezier(lr, conn.A, conn.B);
        lines[conn] = lr;
    }

    void OnConnectionRemoved(object sender, PowerLineConnection conn)
    {
        if (!lines.Remove(conn, out var lr)) { return; }
        Object.Destroy(lr.gameObject);
    }

    public void ToggleRendering(bool enabled)
    {
        if (ShouldShow == enabled) { return; }

        ShouldShow = enabled;
        // When turning on without a current selection, stay hidden until a power line is selected
        // (or the connection tool starts). When turning off, hide unless the tool is active.
        if (!enabled && !toolContainer.activeSelf)
        {
            container.SetActive(false);
        }
        else if (enabled)
        {
            container.SetActive(true);
        }
    }

    public void Unload()
    {
        connService.ConnectionAdded -= OnConnectionAdded;
        connService.ConnectionRemoved -= OnConnectionRemoved;
        eb.Unregister(this);

        Object.Destroy(container);
        Object.Destroy(neutral);
        Object.Destroy(successful);
        Object.Destroy(failure);
    }

    static Material CreateMaterial(Color color)
    {
        var shader = Shader.Find("Universal Render Pipeline/Unlit")
            ?? Shader.Find("Unlit/Color")
            ?? Shader.Find("Sprites/Default");

        return new Material(shader)
        {
            color = color,
            name = $"PowerLineMaterial-{color}",
        };
    }

    static LineRenderer CreateLineObject(Transform parent, string name, Material material)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);

        var lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.sharedMaterial = material;
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

        return lr;
    }

    static void SetBezier(LineRenderer lr, PowerLineComponent from, PowerLineComponent to)
    {
        // Draw along the shortest connection-point pair between the two components.
        var (gridStart, gridEnd, _) = from.GetClosestEndpoints(to);
        var start = CoordinateSystem.GridToWorld(gridStart);
        var end = CoordinateSystem.GridToWorld(gridEnd);

        // Quadratic bezier: endpoints + a mid control point that sags the line.
        var control = (start + end) * 0.5f;
        var span = Vector3.Distance(start, end);
        var sag = Mathf.Clamp(span * SagFactor, MinSag, MaxSag);
        control += Vector3.down * sag;

        for (var i = 0; i <= SegmentCount; i++)
        {
            var t = i / (float)SegmentCount;
            lr.SetPosition(i, EvaluateQuadraticBezier(start, control, end, t));
        }
    }

    static Vector3 EvaluateQuadraticBezier(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        var u = 1f - t;
        return (u * u * p0) + (2f * u * t * p1) + (t * t * p2);
    }
}
