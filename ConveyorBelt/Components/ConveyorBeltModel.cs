namespace ConveyorBelt.Components;

[AddTemplateModule2(typeof(ConveyorBeltModelSpec))]
public class ConveyorBeltModel(ConveyorBeltRenderRepository renderRepo)
    : BaseComponent, IPreviewSelectionListener, IAwakableComponent, ISelectionListener, IDeletableEntity
{
    const float ArrowHeight = 1.04f;
    const float SideOffset = -0.01f;
    const float JunctionArrowSideA = 0.1f;
    const float JunctionArrowSideB = 0.9f;
    const float JunctionArrowBottom = 0.1f;
    const float JunctionArrowTop = 0.9f;

    GameObject arrows = null!;
    ConveyorBeltModelSpec spec = null!;
    bool isJunction;
    bool rendered;

    public void Awake()
    {
        spec = GetComponent<ConveyorBeltModelSpec>();
        isJunction = HasComponent<ConveyorBeltJunctionSpec>();
        arrows = GameObject.FindChild("#Arrows");

        arrows.SetActive(false);
        TryRenderingArrows();

        renderRepo.OnArrowVisibilityRequested += OnArrowVisibilityRequested;
        OnArrowVisibilityRequested(null!, renderRepo.ArrowVisible);
    }

    void OnArrowVisibilityRequested(object sender, bool e)
    {
        if (e)
        {
            RenderAndShowArrows();
        }
        else
        {
            arrows.SetActive(false);
        }
    }

    public void OnPreviewSelect() => renderRepo.RequestArrowVisibility(true);
    public void OnPreviewUnselect() => renderRepo.RequestArrowVisibility(false);

    public void OnSelect() => renderRepo.RequestArrowVisibility(true);
    public void OnUnselect() => renderRepo.RequestArrowVisibility(false);

    void RenderArrows()
    {
        if (isJunction)
        {
            RenderJunctionArrows();
            return;
        }

        RenderBeltArrows();
    }

    void RenderBeltArrows()
    {
        var dir = spec.ArrowDirection;
        var unityDir = CoordinateSystem.GridToWorld(dir);

        if (dir.z != 0)
        {
            RenderArrow("FrontArrow", renderRepo.ArrowMesh, new(SideOffset, 0.5f, 0.5f), Vector3.left, unityDir);
            RenderArrow("BackArrow", renderRepo.ArrowMesh, new(1f - SideOffset, 0.5f, 0.5f), Vector3.right, unityDir);
            RenderArrow("LeftArrow", renderRepo.ArrowMesh, new(0.5f, 0.5f, SideOffset), Vector3.back, unityDir);
            RenderArrow("RightArrow", renderRepo.ArrowMesh, new(0.5f, 0.5f, 1f - SideOffset), Vector3.forward, unityDir);
            return;
        }

        RenderArrow("TopArrow", renderRepo.ArrowMesh, new(0.5f, ArrowHeight, 0.5f), Vector3.up, unityDir);
        RenderArrow("FrontArrow", renderRepo.ArrowMesh, new(SideOffset, 0.5f, 0.5f), Vector3.left, unityDir);
        RenderArrow("BackArrow", renderRepo.ArrowMesh, new(1f - SideOffset, 0.5f, 0.5f), Vector3.right, unityDir);
    }

    void RenderJunctionArrows()
    {
        var dir = spec.ArrowDirection;
        var unityDir = CoordinateSystem.GridToWorld(dir);

        if (dir.z != 0)
        {
            RenderArrowPair("FrontJunctionArrow", new(SideOffset, JunctionArrowSideA, 0.5f), new(SideOffset, JunctionArrowSideB, 0.5f), Vector3.left, unityDir);
            RenderArrowPair("BackJunctionArrow", new(1f - SideOffset, JunctionArrowSideA, 0.5f), new(1f - SideOffset, JunctionArrowSideB, 0.5f), Vector3.right, unityDir);
            RenderArrowPair("LeftJunctionArrow", new(JunctionArrowSideA, 0.5f, SideOffset), new(JunctionArrowSideB, 0.5f, SideOffset), Vector3.back, unityDir);
            RenderArrowPair("RightJunctionArrow", new(JunctionArrowSideA, 0.5f, 1f - SideOffset), new(JunctionArrowSideB, 0.5f, 1f - SideOffset), Vector3.forward, unityDir);
            return;
        }

        RenderArrowPair("TopJunctionArrow", new(JunctionArrowSideA, ArrowHeight, 0.5f), new(JunctionArrowSideB, ArrowHeight, 0.5f), Vector3.up, unityDir);
        RenderArrowPair("FrontJunctionArrow", new(SideOffset, JunctionArrowBottom, 0.5f), new(SideOffset, JunctionArrowTop, 0.5f), Vector3.left, unityDir);
        RenderArrowPair("BackJunctionArrow", new(1f - SideOffset, JunctionArrowBottom, 0.5f), new(1f - SideOffset, JunctionArrowTop, 0.5f), Vector3.right, unityDir);
    }

    void RenderArrowPair(string name, Vector3 localPositionA, Vector3 localPositionB, Vector3 normal, Vector3 forward)
    {
        RenderArrow($"{name}A", renderRepo.ArrowMesh, localPositionA, normal, forward);
        RenderArrow($"{name}B", renderRepo.ArrowMesh, localPositionB, normal, forward);
    }

    void RenderArrow(string name, UnityEngine.Mesh mesh, Vector3 localPosition, Vector3 normal, Vector3 forward)
    {
        var arrow = new GameObject(name);
        arrow.transform.SetParent(arrows.transform, false);
        arrow.transform.SetLocalPositionAndRotation(localPosition, Quaternion.LookRotation(normal, forward));

        var meshFilter = arrow.AddComponent<MeshFilter>();
        meshFilter.sharedMesh = mesh;

        var meshRenderer = arrow.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = renderRepo.ArrowMaterial;
    }

    void TryRenderingArrows()
    {
        if (!rendered && renderRepo.ArrowMaterial && renderRepo.ArrowMesh)
        {
            RenderArrows();
            rendered = true;
        }
    }

    void RenderAndShowArrows()
    {
        TryRenderingArrows();

        if (rendered)
        {
            arrows.SetActive(true);
        }
    }

    public void DeleteEntity()
    {
        renderRepo.OnArrowVisibilityRequested -= OnArrowVisibilityRequested;
    }
}
