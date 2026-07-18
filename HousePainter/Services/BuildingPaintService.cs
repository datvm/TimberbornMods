namespace HousePainter.Services;

/// <summary>
/// Runtime paint sessions for buildings:
/// 1) resolve which atlas fragments a template can paint,
/// 2) UV-split meshes so each fragment is a submesh,
/// 3) tint submeshes with MaterialPropertyBlock (stable vs selection/material instancing).
/// </summary>
[BindSingleton]
public class BuildingPaintService(
    AtlasFragmentCatalog atlasCatalog,
    AtlasMeshSplitter meshSplitter
) : IUnloadableSingleton
{

    static readonly int MainTexId = Shader.PropertyToID("_MainTex");

    readonly Dictionary<string, TemplatePaintLayout> layouts = [];
    readonly Dictionary<BuildingModel, PaintSession> sessions = [];
    readonly MaterialPropertyBlock propertyBlock = new();

    public TemplatePaintLayout GetLayout(BaseComponent component) =>
        GetLayout(component.GetTemplateName());

    public TemplatePaintLayout GetLayout(string templateName)
    {
        if (layouts.TryGetValue(templateName, out var layout))
        {
            return layout;
        }

        layout = BuildLayout(templateName);
        layouts[templateName] = layout;
        return layout;
    }

    /// <summary>
    /// Start a paint session: split meshes, install a neutral material, return part list (white).
    /// </summary>
    public ImmutableArray<PaintPart> EnablePainting(BuildingModel buildingModel)
    {
        if (sessions.TryGetValue(buildingModel, out var existing))
        {
            WriteAllBlocks(existing);
            return ToParts(existing);
        }

        var layout = GetLayout(buildingModel);
        if (layout.Fragments.Length == 0)
        {
            return [];
        }

        var renderers = CollectRenderers(buildingModel);
        if (renderers.Count == 0)
        {
            return [];
        }

        var sourceMaterial = FindAtlasMaterial(renderers);
        if (!sourceMaterial)
        {
            Debug.LogWarning($"[HousePainter] {buildingModel.Name}: no atlas material with _MainTex.");
            return [];
        }

        var baseMaterial = new Material(sourceMaterial)
        {
            name = $"{layout.TemplateName} (HousePainter)",
            enableInstancing = false
        };
        PaintTint.ResetMaterialBase(baseMaterial);

        List<RendererBinding> bindings = [];
        foreach (var renderer in renderers)
        {
            var filter = renderer.GetComponent<MeshFilter>();
            if (!filter || !filter.sharedMesh)
            {
                continue;
            }

            var originalMesh = filter.sharedMesh;
            var originalMaterials = renderer.sharedMaterials;
            var split = meshSplitter.GetOrCreate(originalMesh, layout.Fragments);

            filter.sharedMesh = split.Mesh;
            var materials = new Material[split.SubmeshFragmentIndices.Length];
            Array.Fill(materials, baseMaterial);
            renderer.sharedMaterials = materials;
            ClearBlocks(renderer, materials.Length);

            bindings.Add(new(filter, renderer, originalMesh, originalMaterials, split.SubmeshFragmentIndices));
        }

        if (bindings.Count == 0)
        {
            Object.Destroy(baseMaterial);
            return [];
        }

        var session = new PaintSession(layout, baseMaterial, bindings);
        sessions[buildingModel] = session;
        return ToParts(session);
    }

    public bool IsPaintingEnabled(BuildingModel buildingModel) =>
        sessions.ContainsKey(buildingModel);

    public bool TrySetPartColor(BuildingModel buildingModel, string materialName, Color? color)
    {
        if (!sessions.TryGetValue(buildingModel, out var session))
        {
            return false;
        }

        var fragmentIndex = IndexOfFragment(session.Layout, materialName);
        if (fragmentIndex < 0)
        {
            return false;
        }

        if (color is { } c)
        {
            session.Colors[fragmentIndex] = c;
        }
        else
        {
            session.Colors.Remove(fragmentIndex);
        }

        WriteFragmentBlocks(session, fragmentIndex);
        return true;
    }

    public bool TryClearPartColor(BuildingModel buildingModel, string materialName) =>
        TrySetPartColor(buildingModel, materialName, color: null);

    public void DisablePainting(BuildingModel buildingModel)
    {
        if (!sessions.Remove(buildingModel, out var session))
        {
            return;
        }

        foreach (var binding in session.Bindings)
        {
            if (binding.Filter)
            {
                binding.Filter.sharedMesh = binding.OriginalMesh;
            }

            if (binding.Renderer)
            {
                ClearBlocks(binding.Renderer, binding.SubmeshFragmentIndices.Length);
                binding.Renderer.sharedMaterials = binding.OriginalMaterials;
            }
        }

        if (session.BaseMaterial)
        {
            Object.Destroy(session.BaseMaterial);
        }
    }

    public void Unload()
    {
        foreach (var model in sessions.Keys.ToArray())
        {
            DisablePainting(model);
        }

        sessions.Clear();
        layouts.Clear();
        PrefabMaterialTracker.Clear();
    }

    // --- layout ---

    TemplatePaintLayout BuildLayout(string templateName)
    {
        var sourceNames = PrefabMaterialTracker.GetForTemplate(templateName);
        var matched = atlasCatalog.MatchAll(sourceNames);

        if (matched.Length == 0)
        {
            matched = atlasCatalog.ForFactionHint(templateName);
            if (matched.Length > 0)
            {
                Debug.LogWarning(
                    $"[HousePainter] '{templateName}': no tracked materials matched atlas fragments; " +
                    $"using faction fallback ({matched.Length} fragments).");
            }
        }

        return new(templateName, sourceNames, matched);
    }

    // --- property blocks ---

    void WriteAllBlocks(PaintSession session)
    {
        for (var i = 0; i < session.Layout.Fragments.Length; i++)
        {
            WriteFragmentBlocks(session, i);
        }
    }

    void WriteFragmentBlocks(PaintSession session, int fragmentIndex)
    {
        var hasColor = session.Colors.TryGetValue(fragmentIndex, out var color);
        var tint = hasColor ? PaintTint.Compensate(color) : Color.white;

        foreach (var binding in session.Bindings)
        {
            if (!binding.Renderer)
            {
                continue;
            }

            for (var s = 0; s < binding.SubmeshFragmentIndices.Length; s++)
            {
                if (binding.SubmeshFragmentIndices[s] != fragmentIndex)
                {
                    continue;
                }

                if (!hasColor)
                {
                    binding.Renderer.SetPropertyBlock(null, s);
                    continue;
                }

                binding.Renderer.GetPropertyBlock(propertyBlock, s);
                propertyBlock.SetColor(PaintTint.ColorPropertyId, tint);
                binding.Renderer.SetPropertyBlock(propertyBlock, s);
            }
        }
    }

    static void ClearBlocks(MeshRenderer renderer, int submeshCount)
    {
        for (var i = 0; i < submeshCount; i++)
        {
            renderer.SetPropertyBlock(null, i);
        }
    }

    // --- discovery helpers ---

    static List<MeshRenderer> CollectRenderers(BuildingModel buildingModel)
    {
        HashSet<MeshRenderer> unique = [];
        AddRenderers(buildingModel.FinishedModel, unique);
        AddRenderers(buildingModel.FinishedUncoveredModel, unique);
        return [.. unique];
    }

    static void AddRenderers(GameObject? root, HashSet<MeshRenderer> unique)
    {
        if (!root)
        {
            return;
        }

        List<MeshRenderer> buffer = [];
        root.GetComponentsInChildren(includeInactive: true, buffer);
        foreach (var renderer in buffer)
        {
            if (renderer)
            {
                unique.Add(renderer);
            }
        }
    }

    static Material? FindAtlasMaterial(List<MeshRenderer> renderers)
    {
        foreach (var renderer in renderers)
        {
            foreach (var mat in renderer.sharedMaterials)
            {
                if (mat && mat.HasProperty(MainTexId) && mat.GetTexture(MainTexId))
                {
                    return mat;
                }
            }
        }

        return null;
    }

    static int IndexOfFragment(TemplatePaintLayout layout, string materialName)
    {
        for (var i = 0; i < layout.Fragments.Length; i++)
        {
            if (layout.Fragments[i].MaterialName == materialName)
            {
                return i;
            }
        }

        return -1;
    }

    static ImmutableArray<PaintPart> ToParts(PaintSession session)
    {
        var layout = session.Layout;
        var builder = ImmutableArray.CreateBuilder<PaintPart>(layout.Fragments.Length);

        for (var i = 0; i < layout.Fragments.Length; i++)
        {
            var fragment = layout.Fragments[i];
            var color = session.Colors.TryGetValue(i, out var c) ? c : Color.white;
            builder.Add(new(fragment.MaterialName, fragment, color));
        }

        return builder.MoveToImmutable();
    }

    sealed class PaintSession(
        TemplatePaintLayout layout,
        Material baseMaterial,
        List<RendererBinding> bindings
    )
    {
        public TemplatePaintLayout Layout { get; } = layout;
        public Material BaseMaterial { get; } = baseMaterial;
        public List<RendererBinding> Bindings { get; } = bindings;
        public Dictionary<int, Color> Colors { get; } = [];
    }

    readonly record struct RendererBinding(
        MeshFilter Filter,
        MeshRenderer Renderer,
        UnityEngine.Mesh OriginalMesh,
        Material[] OriginalMaterials,
        ImmutableArray<int> SubmeshFragmentIndices
    );

}
