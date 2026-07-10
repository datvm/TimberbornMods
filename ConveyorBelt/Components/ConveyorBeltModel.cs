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
    const float SplitterArrowHalfLength = 0.18f;
    const float SplitterBranchArrowScale = 0.5f;

    GameObject arrows = null!;
    ConveyorBeltModelSpec spec = null!;
    bool rendered;

    public void Awake()
    {
        spec = GetComponent<ConveyorBeltModelSpec>();
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
        var jcSpec = GetComponent<ConveyorBeltJunctionSpec>();
        if (jcSpec is not null)
        {
            RenderJunctionArrows(jcSpec);
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

    void RenderJunctionArrows(ConveyorBeltJunctionSpec jcSpec)
    {
        if (jcSpec.OutputCoordinates.Length == 1)
        {
            RenderMergerArrows();

        }
        else if (jcSpec.InputCoordinates.Length == 1)
        {
            RenderSplitterArrows(jcSpec);
        }
        else
        {
            throw new InvalidOperationException($"Junction spec has {jcSpec.InputCoordinates.Length} inputs and {jcSpec.OutputCoordinates.Length} outputs, which is not supported.");
        }
    }

    void RenderMergerArrows()
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

    void RenderSplitterArrows(ConveyorBeltJunctionSpec jcSpec)
    {
        var dir = spec.ArrowDirection;
        var unityDir = CoordinateSystem.GridToWorld(dir);

        RenderArrow("TopSplitterMiddleArrow", renderRepo.ArrowMesh, new(0.5f, ArrowHeight, 0.5f), Vector3.up, unityDir);

        foreach (var outputCoordinates in jcSpec.OutputCoordinates)
        {
            if (outputCoordinates == dir) { continue; }

            var branchDir = CoordinateSystem.GridToWorld(outputCoordinates);

            var localPosition = new Vector3(0.5f, ArrowHeight, 0.5f) + branchDir * SplitterArrowHalfLength;
            RenderArrow($"TopSplitterBranchArrow{outputCoordinates}", renderRepo.ArrowMesh, localPosition, Vector3.up, branchDir, new(1f, SplitterBranchArrowScale, 1f));
        }

    }

    void RenderArrowPair(string name, Vector3 localPositionA, Vector3 localPositionB, Vector3 normal, Vector3 forward)
    {
        RenderArrow($"{name}A", renderRepo.ArrowMesh, localPositionA, normal, forward);
        RenderArrow($"{name}B", renderRepo.ArrowMesh, localPositionB, normal, forward);
    }

    void RenderArrow(string name, UnityEngine.Mesh mesh, Vector3 localPosition, Vector3 normal, Vector3 forward)
        => RenderArrow(name, mesh, localPosition, normal, forward, Vector3.one);

    void RenderArrow(string name, UnityEngine.Mesh mesh, Vector3 localPosition, Vector3 normal, Vector3 forward, Vector3 scale)
    {
        var arrow = new GameObject(name);
        arrow.transform.SetParent(arrows.transform, false);
        arrow.transform.SetLocalPositionAndRotation(localPosition, Quaternion.LookRotation(normal, forward));
        arrow.transform.localScale = scale;

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
