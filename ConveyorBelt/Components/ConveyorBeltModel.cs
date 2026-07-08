namespace ConveyorBelt.Components;

[AddTemplateModule2(typeof(ConveyorBeltModelSpec))]
public class ConveyorBeltModel(ConveyorBeltRenderRepository renderRepo)
    : BaseComponent, IPreviewSelectionListener, IAwakableComponent, ISelectionListener, IDeletableEntity
{
    const float ArrowHeight = 1.04f;
    const float SideOffset = 0.02f;

    GameObject arrows = null!;
    ConveyorBeltSpec spec = null!;
    bool rendered;

    public void Awake()
    {
        spec = GetComponent<ConveyorBeltSpec>();
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
        var dir = spec.ArrowDirection;
        var unityDir = CoordinateSystem.GridToWorld(dir);

        if (dir.z != 0)
        {
            RenderArrow("FrontArrow", new(SideOffset, 0.5f, 0.5f), Vector3.left, unityDir);
            RenderArrow("BackArrow", new(1f - SideOffset, 0.5f, 0.5f), Vector3.right, unityDir);
            RenderArrow("LeftArrow", new(0.5f, 0.5f, SideOffset), Vector3.back, unityDir);
            RenderArrow("RightArrow", new(0.5f, 0.5f, 1f - SideOffset), Vector3.forward, unityDir);
            return;
        }

        RenderArrow("TopArrow", new(0.5f, ArrowHeight, 0.5f), Vector3.up, unityDir);
        RenderArrow("FrontArrow", new(SideOffset, 0.5f, 0.5f), Vector3.left, unityDir);
        RenderArrow("BackArrow", new(1f - SideOffset, 0.5f, 0.5f), Vector3.right, unityDir);
    }

    void RenderArrow(string name, Vector3 localPosition, Vector3 normal, Vector3 forward)
    {
        var arrow = new GameObject(name);
        arrow.transform.SetParent(arrows.transform, false);
        arrow.transform.SetLocalPositionAndRotation(localPosition, Quaternion.LookRotation(normal, forward));

        var meshFilter = arrow.AddComponent<MeshFilter>();
        meshFilter.sharedMesh = renderRepo.ArrowMesh;

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
