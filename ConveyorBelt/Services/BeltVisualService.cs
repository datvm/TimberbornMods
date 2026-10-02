namespace ConveyorBelt.Services;

[BindSingleton]
public class BeltVisualService(IBlockService blocks, MSettings settings, BeltBoxVisuals boxes, ITickProgressService ticks, BeltRunnerMotion runners)
    : ILoadableSingleton, IUnloadableSingleton
{
    UnityEngine.Mesh? runnerMesh;

    public bool Animation => settings.Animation.Value;

    public bool Ready => boxes.Ready;

    public float Progress => ticks.Progress;

    public Material? RunnerWood => boxes.RunnerWood;

    public UnityEngine.Mesh RunnerMesh => runnerMesh!;

    public void Load() => runnerMesh = WoodCube();

    public void Unload()
    {
        if (!runnerMesh)
        {
            return;
        }

        Object.Destroy(runnerMesh);
        runnerMesh = null;
    }

    public float Along(int speed) => runners.Along(speed);

    public float JunctionAlong => runners.MergerAlong;

    public float GoodScale(float size) => boxes.Size > 0f ? size / boxes.Size : 1f;

    public bool ApplyGood(Material boxMaterial, Material iconMaterial, string id) => boxes.ApplyGood(boxMaterial, iconMaterial, id);

    public GameObject CreateGood(Transform parent, EntityMaterials? entityMaterials)
    {
        var copy = new GameObject("Good");
        copy.layer = parent.gameObject.layer;
        copy.transform.SetParent(parent, false);
        copy.SetActive(false);
        var filter = copy.AddComponent<MeshFilter>();
        filter.sharedMesh = boxes.Box;
        var renderer = copy.AddComponent<MeshRenderer>();
        var material = boxes.CreateMaterial();
        renderer.sharedMaterial = material;
        entityMaterials?.AddMaterial(copy.transform, material);

        var icon = new GameObject("Icon");
        icon.layer = copy.layer;
        icon.transform.SetParent(copy.transform, false);
        var iconFilter = icon.AddComponent<MeshFilter>();
        iconFilter.sharedMesh = boxes.IconMesh;
        var iconRenderer = icon.AddComponent<MeshRenderer>();
        var iconMaterial = boxes.CreateIconMaterial();
        iconRenderer.sharedMaterial = iconMaterial;
        iconRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        iconRenderer.receiveShadows = false;
        entityMaterials?.AddMaterial(icon.transform, iconMaterial);
        icon.SetActive(false);
        return copy;
    }

    public GameObject CreateRunner(Transform parent)
    {
        var copy = new GameObject("Runner");
        copy.layer = parent.gameObject.layer;
        copy.transform.SetParent(parent, false);
        copy.SetActive(false);
        var filter = copy.AddComponent<MeshFilter>();
        filter.sharedMesh = runnerMesh;
        var renderer = copy.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = boxes.RunnerWood;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        return copy;
    }

    public bool FacesBuilding(BlockObject block, Direction3D local)
    {
        var world = block.TransformDirection(local);
        var cell = block.Coordinates + world.ToOffset();
        foreach (var obj in blocks.GetObjectsAt(cell))
        {
            if (!obj || obj == block)
            {
                continue;
            }

            if (obj.GetComponent<BeltCarrier>() is not null || obj.GetComponent<BeltMerger>() is not null || obj.GetComponent<BeltSplitter>() is not null || obj.GetComponent<BeltLift>() is not null)
            {
                continue;
            }

            return true;
        }

        return false;
    }

    static UnityEngine.Mesh WoodCube()
    {
        Vector2[] face =
        [
            new(0.7504f, 0.5001f),
            new(0.8128f, 0.5001f),
            new(0.8128f, 0.8753f),
            new(0.7504f, 0.8753f),
        ];

        var verts = new List<Vector3>();
        var uvs = new List<Vector2>();
        var tris = new List<int>();

        void Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            var start = verts.Count;
            verts.Add(a);
            verts.Add(b);
            verts.Add(c);
            verts.Add(d);
            uvs.Add(face[0]);
            uvs.Add(face[1]);
            uvs.Add(face[2]);
            uvs.Add(face[3]);
            tris.Add(start);
            tris.Add(start + 1);
            tris.Add(start + 2);
            tris.Add(start);
            tris.Add(start + 2);
            tris.Add(start + 3);
        }

        Quad(new(-0.5f, -0.5f, 0.5f), new(0.5f, -0.5f, 0.5f), new(0.5f, 0.5f, 0.5f), new(-0.5f, 0.5f, 0.5f));
        Quad(new(0.5f, -0.5f, -0.5f), new(-0.5f, -0.5f, -0.5f), new(-0.5f, 0.5f, -0.5f), new(0.5f, 0.5f, -0.5f));
        Quad(new(0.5f, -0.5f, 0.5f), new(0.5f, -0.5f, -0.5f), new(0.5f, 0.5f, -0.5f), new(0.5f, 0.5f, 0.5f));
        Quad(new(-0.5f, -0.5f, -0.5f), new(-0.5f, -0.5f, 0.5f), new(-0.5f, 0.5f, 0.5f), new(-0.5f, 0.5f, -0.5f));
        Quad(new(-0.5f, 0.5f, 0.5f), new(0.5f, 0.5f, 0.5f), new(0.5f, 0.5f, -0.5f), new(-0.5f, 0.5f, -0.5f));
        Quad(new(-0.5f, -0.5f, -0.5f), new(0.5f, -0.5f, -0.5f), new(0.5f, -0.5f, 0.5f), new(-0.5f, -0.5f, 0.5f));

        var mesh = new UnityEngine.Mesh { name = "BeltRunner" };
        mesh.SetVertices(verts);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }
}
