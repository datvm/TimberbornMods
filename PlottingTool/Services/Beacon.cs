namespace PlottingTool.Services;

public record BeaconPair(Beacon Beacon1, Beacon Beacon2, Beacon Midline);

public class Beacon : IDisposable
{
    static readonly Color DefaultColor = Color.darkGreen;
    static readonly Shader Shader = Shader.Find("Universal Render Pipeline/Lit");

    public GameObject GameObject { get; }
    readonly GameObject horizontal1, horizontal2;
    readonly Transform t1, t2;

    public Vector2Int Location { get; }
    public bool Disposed { get; private set; }

    public HashSet<BeaconPair> RelatedPairs { get; } = [];

    public Beacon(Vector2Int location, Vector3Int mapSize, Color? color = null)
    {
        Location = location;

        GameObject = CreateCyclinder(color);
        var t = GameObject.transform;
        t.position = new Vector3(location.x + .5f, -1, location.y + .5f);
        t.localScale = new(0.1f, mapSize.z + 2, 0.1f);

        horizontal1 = CreateCyclinder(color);
        horizontal1.SetActive(false);
        t1 = horizontal1.transform;        
        t1.rotation = Quaternion.Euler(90f, 0, 0);
        t1.localScale = new(.1f, mapSize.y * 2, .1f);
        t1.parent = GameObject.transform;
        t1.localPosition = Vector3.zero;

        horizontal2 = CreateCyclinder(color);
        horizontal2.SetActive(false);
        t2 = horizontal2.transform;
        t2.rotation = Quaternion.Euler(0,0, 90f);
        t2.localScale = new(.1f, mapSize.x * 2, .1f);
        t2.parent = GameObject.transform;
        t2.localPosition = Vector3.zero;
    }

    public void ShowHorizontalLines(int z)
    {
        horizontal1.SetActive(true);
        t1.position = t1.position with { y = z };

        horizontal2.SetActive(true);
        t2.position = t2.position with { y = z };
    }

    public void DisableHorizontalLines()
    {
        horizontal1.SetActive(false);
        horizontal2.SetActive(false);
    }

    static GameObject CreateCyclinder(Color? color)
    {
        var obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        UnityEngine.Object.Destroy(obj.GetComponent<Collider>());

        var renderer = obj.GetComponent<Renderer>();
        var m = renderer.material;
        m.shader = Shader;
        m.SetColor("_BaseColor", color ?? DefaultColor);

        return obj;
    }

    public void Dispose()
    {
        if (Disposed) { return; }

        Disposed = true;
        RelatedPairs.Clear();
        UnityEngine.Object.Destroy(GameObject);
    }

}
