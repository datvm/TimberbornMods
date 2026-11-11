namespace ModdableAlternativeBuildingStyle.Services;

sealed class BuildingStyleState
{
    public bool Captured;
    public Transform Finished = null!;
    public BuildingModel? BuildingModel;
    public GameObject OriginalHolder = null!;
    public Dictionary<string, Transform> Anchors = [];
    public List<Transform> Visuals = [];
    public HashSet<Transform> KeptVisuals = [];
    public List<TransformHome> Homes = [];
    public List<Renderer> AnchorRenderers = [];
    public List<SavedBox> Boxes = [];
    public List<SavedSphere> Spheres = [];
    public List<SavedCapsule> Capsules = [];
    public bool CollidersRemoved;
    public TimbermeshAnimator? Animator;
    public List<AnimationMetadata>? OriginalClips;
    public IAnimationUpdater[]? OriginalUpdaters;
    public bool AnimationRebound;
    public GameObject? StyleInstance;
    public readonly List<GameObject> Grafted = [];
    public readonly List<Component> AddedUpdaters = [];
    public MeshRenderer? StockpileBanner;
    public MeshRenderer? ResourceBanner;
    public MeshRenderer? DecalIcon;
}

readonly record struct TransformHome(
    Transform Target,
    Transform Parent,
    Vector3 LocalPosition,
    Quaternion LocalRotation,
    Vector3 LocalScale);

readonly record struct SavedBox(Vector3 Center, Vector3 Size);

readonly record struct SavedSphere(Vector3 Center, float Radius);

readonly record struct SavedCapsule(Vector3 Center, float Radius, float Height, int Direction);
