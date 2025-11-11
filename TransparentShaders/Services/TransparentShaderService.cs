namespace TransparentShaders.Services;

public class TransparentShaderService(IAssetLoader assets) : ILoadableSingleton, IUnloadableSingleton
{
    public const string EnvironmentShaderName = "Shader Graphs/EnvironmentURP";
    public const string TerrainShaderName = "Shader Graphs/TerrainURP";

    public static int ShaderEnvironmentAlphaId = Shader.PropertyToID("_EnvironmentAlpha");
    public static int ShaderTerrainAlphaId = Shader.PropertyToID("_TerrainAlpha");
    public static int ShaderColorAlphaId = Shader.PropertyToID("_ColorAlpha");
    public static int ShaderColorId = Shader.PropertyToID("_Color");

    public static TransparentShaderService? Instance { get; private set; }

    Shader? transparentEnvironment;
    Shader? terrainTransparent;
    Shader? plainColorTransparent;
    Shader? unlitPlainColorTransparent;

    Shader? originalEnvironment;
    Shader? originalTerrain;

    public Shader TransparentEnvironmentShader => GetOrLoad(ref transparentEnvironment, "Shaders/EnvironmentTransparentURP");
    public Shader TerrainTransparentShader => GetOrLoad(ref terrainTransparent, "Shaders/TerrainTransparentURP");
    public Shader PlainColorTransparentShader => GetOrLoad(ref plainColorTransparent, "Shaders/PlainColorTransparentURP");
    public Shader UnlitPlainColorTransparentShader => GetOrLoad(ref unlitPlainColorTransparent, "Shaders/UnlitPlainColorTransparentURP");

    public void Load()
    {
        Instance = this;
    }

    Shader GetOrLoad(ref Shader? shader, string path)
    {
        if (!shader)
        {
            shader = assets.Load<Shader>(path);

            if (!shader)
            {
                throw new InvalidOperationException($"Failed to load shader at path '{path}'");
            }
        }

        return shader;
    }

    static T[] Materialize<T>(Func<IEnumerable<T>> func) => [.. func()];

    public Material[] ReplaceShaders(IEnumerable<Material> materials, bool replaceEnv = true, bool replaceTerrain = true)
    {
        return Materialize(Internal);

        IEnumerable<Material> Internal()
        {
            var env = replaceEnv ? TransparentEnvironmentShader : null;
            var terrain = replaceTerrain ? TerrainTransparentShader : null;

            foreach (var m in materials)
            {
                if (!m || !m.shader) { continue; }

                var name = m.shader.name;
                switch (name)
                {
                    case EnvironmentShaderName when replaceEnv:
                        originalEnvironment ??= m.shader;
                        m.shader = env;
                        yield return m;
                        break;
                    case TerrainShaderName when replaceTerrain:
                        originalTerrain ??= m.shader;
                        m.shader = terrain;
                        yield return m;
                        break;
                }
            }
        }
    }

    public Material[] ReplaceRenderersMaterials(IEnumerable<Renderer> renderers, bool replaceMaterials = true, bool replaceSharedMaterials = true, bool replaceEnv = true, bool replaceTerrain = true)
    {
        return Materialize(Internal);

        IEnumerable<Material> Internal()
        {
            foreach (var r in renderers)
            {
                if (!r) { continue; }

                if (replaceMaterials)
                {
                    var mats = r.materials;

                    foreach (var m in TryUpdatingMaterials(
                        () => ReplaceShaders(mats, replaceEnv, replaceTerrain),
                        () => r.materials = mats)
                    )
                    {
                        yield return m;
                    }

                }

                if (replaceSharedMaterials)
                {
                    var sharedMats = r.sharedMaterials;

                    foreach (var m in TryUpdatingMaterials(
                        () => ReplaceShaders(sharedMats, replaceEnv, replaceTerrain),
                        () => r.sharedMaterials = sharedMats)
                    )
                    {
                        yield return m;
                    }
                }
            }
        }
    }

    public Material[] RestoreShaders(IEnumerable<Material> materials, bool restoreEnv = true, bool restoreTerrain = true, bool ignoreIfMissing = false)
    {
        return Materialize(Internal);

        IEnumerable<Material> Internal()
        {
            if (restoreEnv && !originalEnvironment && !ignoreIfMissing)
            {
                throw new InvalidOperationException("Original environment shader not stored (it was never replaced).");
            }

            if (restoreTerrain && !originalTerrain && !ignoreIfMissing)
            {
                throw new InvalidOperationException("Original terrain shader not stored (it was never replaced).");
            }

            var doEnv = restoreEnv && originalEnvironment;
            var doTer = restoreTerrain && originalTerrain;

            var transparentEnvironment = doEnv ? TransparentEnvironmentShader : null;
            var terrainTransparent = doTer ? TerrainTransparentShader : null;

            foreach (var m in materials)
            {
                if (!m || !m.shader) { continue; }

                if (doEnv && m.shader == transparentEnvironment)
                {
                    m.shader = originalEnvironment;
                    yield return m;
                }
                else if (doTer && m.shader == terrainTransparent)
                {
                    m.shader = originalTerrain;
                    yield return m;
                }
            }
        }
    }

    public Material[] RestoreRenderersMaterials(IEnumerable<Renderer> renderers, bool restoreMaterials = true, bool restoreSharedMaterials = true, bool restoreEnv = true, bool restoreTerrain = true, bool ignoreIfMissing = false)
    {
        return Materialize(Internal);

        IEnumerable<Material> Internal()
        {
            foreach (var r in renderers)
            {
                if (!r) { continue; }
                if (restoreMaterials)
                {
                    var mats = r.materials;

                    foreach (var m in TryUpdatingMaterials(
                        () => RestoreShaders(mats, restoreEnv, restoreTerrain, ignoreIfMissing),
                        () => r.materials = mats))
                    {
                        yield return m;
                    }
                }
                if (restoreSharedMaterials)
                {
                    var sharedMats = r.sharedMaterials;
                    foreach (var m in TryUpdatingMaterials(
                        () => RestoreShaders(sharedMats, restoreEnv, restoreTerrain, ignoreIfMissing),
                        () => r.sharedMaterials = sharedMats))
                    {
                        yield return m;
                    }
                }
            }
        }
    }

    IEnumerable<Material> TryUpdatingMaterials(Func<IEnumerable<Material>> action, Action set)
    {
        var changed = false;

        foreach (var m in action())
        {
            changed = true;
            yield return m;
        }

        if (changed)
        {
            set();
        }
    }

    internal void OnSceneUnload()
    {
        transparentEnvironment = null;
        terrainTransparent = null;
        originalEnvironment = null;
        originalTerrain = null;
    }

    public void Unload()
    {
        Instance = null;
    }
}
