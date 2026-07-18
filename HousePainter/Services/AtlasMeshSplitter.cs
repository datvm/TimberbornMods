using UnityMesh = UnityEngine.Mesh;

namespace HousePainter.Services;

/// <summary>
/// Splits meshes by FactionAtlas UV tiles so each fragment can use its own material.
/// Split meshes are cached per source mesh asset (shared across building instances).
/// </summary>
[BindSingleton]
public class AtlasMeshSplitter : IUnloadableSingleton
{

    readonly Dictionary<UnityMesh, SplitMesh> bySource = [];
    readonly Dictionary<UnityMesh, SplitMesh> bySplitMesh = [];
    readonly List<UnityMesh> ownedMeshes = [];

    public SplitMesh GetOrCreate(UnityMesh sourceMesh, ImmutableArray<AtlasFragment> fragments)
    {
        // Never re-split an already-split mesh (would scramble submesh layout).
        if (bySplitMesh.TryGetValue(sourceMesh, out var alreadySplit))
        {
            return alreadySplit;
        }

        if (bySource.TryGetValue(sourceMesh, out var cached))
        {
            return cached;
        }

        var split = Split(sourceMesh, fragments);
        bySource[sourceMesh] = split;
        bySplitMesh[split.Mesh] = split;
        ownedMeshes.Add(split.Mesh);
        return split;
    }

    static SplitMesh Split(UnityMesh source, ImmutableArray<AtlasFragment> fragments)
    {
        var uvs = source.uv;
        if (uvs is null || uvs.Length == 0)
        {
            return Passthrough(source, [-1]);
        }

        List<int> allTriangles = [];
        for (var sub = 0; sub < source.subMeshCount; sub++)
        {
            allTriangles.AddRange(source.GetTriangles(sub));
        }

        var buckets = new List<int>[fragments.Length + 1];
        for (var i = 0; i < buckets.Length; i++)
        {
            buckets[i] = [];
        }

        for (var t = 0; t + 2 < allTriangles.Count; t += 3)
        {
            var i0 = allTriangles[t];
            var i1 = allTriangles[t + 1];
            var i2 = allTriangles[t + 2];
            var bucketIndex = fragments.Length;

            if (i0 < uvs.Length && i1 < uvs.Length && i2 < uvs.Length)
            {
                var centroid = (uvs[i0] + uvs[i1] + uvs[i2]) / 3f;
                var fragmentIndex = ResolveFragmentIndex(centroid, fragments);
                if (fragmentIndex >= 0)
                {
                    bucketIndex = fragmentIndex;
                }
            }

            buckets[bucketIndex].Add(i0);
            buckets[bucketIndex].Add(i1);
            buckets[bucketIndex].Add(i2);
        }

        List<int> submeshFragmentIndices = [];
        List<int[]> submeshTriangles = [];

        for (var i = 0; i < fragments.Length; i++)
        {
            if (buckets[i].Count == 0)
            {
                continue;
            }

            submeshFragmentIndices.Add(i);
            submeshTriangles.Add([.. buckets[i]]);
        }

        if (buckets[^1].Count > 0)
        {
            submeshFragmentIndices.Add(-1);
            submeshTriangles.Add([.. buckets[^1]]);
        }

        if (submeshTriangles.Count == 0)
        {
            return Passthrough(source, [-1]);
        }

        var mesh = Object.Instantiate(source);
        mesh.name = source.name + " (HousePainter)";
        mesh.subMeshCount = submeshTriangles.Count;
        for (var s = 0; s < submeshTriangles.Count; s++)
        {
            mesh.SetTriangles(submeshTriangles[s], s, calculateBounds: false);
        }

        mesh.RecalculateBounds();
        return new(mesh, [.. submeshFragmentIndices]);
    }

    static SplitMesh Passthrough(UnityMesh source, ImmutableArray<int> indices)
    {
        var mesh = Object.Instantiate(source);
        mesh.name = source.name + " (HousePainter)";
        return new(mesh, indices);
    }

    static int ResolveFragmentIndex(Vector2 uv, ImmutableArray<AtlasFragment> fragments)
    {
        for (var i = 0; i < fragments.Length; i++)
        {
            var f = fragments[i];
            var min = f.UVOffset;
            var max = f.UVOffset + f.UVScale;
            if (uv.x >= min.x && uv.y >= min.y
                && (uv.x < max.x || max.x >= 1f && uv.x <= 1f)
                && (uv.y < max.y || max.y >= 1f && uv.y <= 1f))
            {
                return i;
            }
        }

        return -1;
    }

    public void Unload()
    {
        foreach (var mesh in ownedMeshes)
        {
            if (mesh)
            {
                Object.Destroy(mesh);
            }
        }

        ownedMeshes.Clear();
        bySource.Clear();
        bySplitMesh.Clear();
    }

}

/// <param name="Mesh">Split mesh asset (shared across instances of the same source mesh).</param>
/// <param name="SubmeshFragmentIndices">Per submesh: index into the fragment list, or -1 for unmatched tris.</param>
public readonly record struct SplitMesh(
    UnityMesh Mesh,
    ImmutableArray<int> SubmeshFragmentIndices
);
