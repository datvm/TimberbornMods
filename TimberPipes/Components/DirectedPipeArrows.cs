namespace TimberPipes.Components;

[AddTemplateModule2(typeof(DirectedPipeSpec))]
public class DirectedPipeArrows(PipeArrowRepository arrowsRepo)
    : BaseComponent, IAwakableComponent, ISelectionListener, IPreviewSelectionListener,
        IPostPlacementChangeListener, IInitializableEntity, IDeletableEntity
{
    const float HorizontalArrowHeight = 0.5f;
    const float SideOffset = -0.01f;

    readonly record struct ArrowFace(Transform Transform, Vector3 Normal);

    GameObject arrows = null!;
    BuildingPipeSpec spec = null!;
    BlockObject blockObject = null!;
    readonly List<ArrowFace> faces = [];
    bool rendered;

    public void Awake()
    {
        spec = GetComponent<BuildingPipeSpec>();
        blockObject = GetComponent<BlockObject>();
        arrows = GameObject.FindChild("#Arrows");
        arrows.SetActive(false);

        TryRenderingArrows();
        arrowsRepo.OnArrowVisibilityRequested += OnArrowVisibilityRequested;
        OnArrowVisibilityRequested(null!, arrowsRepo.ArrowVisible);
    }

    public void OnPreviewSelect() => arrowsRepo.RequestArrowVisibility(true);
    public void OnPreviewUnselect() => arrowsRepo.RequestArrowVisibility(false);
    public void OnSelect() => arrowsRepo.RequestArrowVisibility(true);
    public void OnUnselect() => arrowsRepo.RequestArrowVisibility(false);

    public void OnPostPlacementChanged()
    {
        if (rendered)
        {
            ApplyDirection();
        }
    }

    public void InitializeEntity()
    {
        if (rendered)
        {
            ApplyDirection();
        }
    }

    public void DeleteEntity()
    {
        arrowsRepo.OnArrowVisibilityRequested -= OnArrowVisibilityRequested;
    }

    void OnArrowVisibilityRequested(object sender, bool visible)
    {
        if (visible)
        {
            TryRenderingArrows();
            if (rendered)
            {
                arrows.SetActive(true);
            }

            return;
        }

        arrows.SetActive(false);
    }

    void TryRenderingArrows()
    {
        if (rendered || !arrowsRepo.ArrowMaterial || !arrowsRepo.ArrowMesh)
        {
            return;
        }

        if (IsVertical())
        {
            CreateArrow("FrontArrow", new(SideOffset, 0.5f, 0.5f), Vector3.left);
            CreateArrow("BackArrow", new(1f - SideOffset, 0.5f, 0.5f), Vector3.right);
            CreateArrow("LeftArrow", new(0.5f, 0.5f, SideOffset), Vector3.back);
            CreateArrow("RightArrow", new(0.5f, 0.5f, 1f - SideOffset), Vector3.forward);
        }
        else
        {
            CreateArrow("TopArrow", new(0.5f, HorizontalArrowHeight, 0.5f), Vector3.up);
            CreateArrow("FrontArrow", new(SideOffset, 0.23f, 0.5f), Vector3.left);
            CreateArrow("BackArrow", new(1f - SideOffset, 0.23f, 0.5f), Vector3.right);
        }

        rendered = true;
        ApplyDirection();
    }

    bool IsVertical()
    {
        foreach (var portSpec in spec.Ports)
        {
            if (DirectedPipeOrient.HasVertical(portSpec))
            {
                return true;
            }
        }

        return false;
    }

    void CreateArrow(string name, Vector3 localPosition, Vector3 normal)
    {
        var arrow = new GameObject(name);
        arrow.transform.SetParent(arrows.transform, false);
        arrow.transform.localPosition = localPosition;
        arrow.transform.localScale = Vector3.one;

        var meshFilter = arrow.AddComponent<MeshFilter>();
        meshFilter.sharedMesh = arrowsRepo.ArrowMesh;

        var meshRenderer = arrow.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = arrowsRepo.ArrowMaterial;

        faces.Add(new(arrow.transform, normal));
    }

    void ApplyDirection()
    {
        if (!TryUnityOutflow(out var unityDir))
        {
            return;
        }

        foreach (var face in faces)
        {
            face.Transform.localRotation = Quaternion.LookRotation(face.Normal, unityDir);
        }
    }

    bool TryUnityOutflow(out Vector3 unityDir)
    {
        unityDir = default;
        var flipped = blockObject.FlipMode.IsFlipped;
        foreach (var portSpec in spec.Ports)
        {
            if (!DirectedPipeOrient.TryOutflow(portSpec, flipped, out var direction))
            {
                continue;
            }

            unityDir = CoordinateSystem.GridToWorld(direction.ToOffset());
            return true;
        }

        return false;
    }
}
