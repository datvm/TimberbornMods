namespace ConveyorBelt.Components.Visuals;

[AddTemplateModule2(typeof(BeltSystemSpec))]
public class BeltArrows(BeltArrowRepository arrowsRepo)
    : BaseComponent, IAwakableComponent, IPreviewSelectionListener, ISelectionListener, IPostPlacementChangeListener, IDeletableEntity
{
    const float DeckArrowHeight = 0.12f;
    const float SideOffset = -0.01f;
    const float ShaftFace = SideOffset;
    static readonly Vector3 ShaftArrowScale = new(0.7f, 0.38f, 1f);
    static readonly Vector3 JunctionScale = new(0.7f, 0.36f, 1f);
    static readonly Vector3 ArmScale = new(0.45f, 0.2f, 1f);

    readonly List<BeltPort> ports = [];
    readonly List<BeltPort> inputs = [];
    readonly List<BeltPort> outputs = [];
    readonly List<Direction3D> directions = [];
    readonly List<UnityEngine.Mesh> bends = [];

    BlockObject block = null!;
    GameObject arrows = null!;
    bool rendered;
    bool renderedFlipped;
    bool focused;

    public void Awake()
    {
        block = GetComponent<BlockObject>();
        arrows = new GameObject("Arrows");
        arrows.transform.SetParent(Transform, false);
        arrows.SetActive(false);
        arrowsRepo.OnVisibility += OnVisibility;
        OnVisibility(arrowsRepo, arrowsRepo.Visible);
    }

    public void OnPreviewSelect() => SetFocused(true);

    public void OnPreviewUnselect() => SetFocused(false);

    public void OnSelect() => SetFocused(true);

    public void OnUnselect() => SetFocused(false);

    public void OnPostPlacementChanged()
    {
        if (!rendered || block.FlipMode.IsFlipped == renderedFlipped)
        {
            return;
        }

        Rebuild();
    }

    public void Rebuild()
    {
        if (!arrows)
        {
            return;
        }

        ClearArrows();
        rendered = false;
        if (arrows.activeSelf)
        {
            TryRender();
        }
    }

    public void DeleteEntity()
    {
        SetFocused(false);
        arrowsRepo.OnVisibility -= OnVisibility;
    }

    void SetFocused(bool value)
    {
        if (focused == value)
        {
            return;
        }

        focused = value;
        arrowsRepo.Focus(value);
    }

    void OnVisibility(object sender, bool visible)
    {
        if (!arrows)
        {
            return;
        }

        if (visible)
        {
            TryRender();
            arrows.SetActive(true);
            return;
        }

        arrows.SetActive(false);
    }

    void TryRender()
    {
        if (rendered || !arrowsRepo.ArrowMaterial || !arrowsRepo.ArrowMesh)
        {
            return;
        }

        CollectDirections();
        if (GetComponent<BeltMerger>() is not null || GetComponent<BeltSplitter>() is not null)
        {
            RenderArms();
        }
        else if (GetComponent<BeltLift>() is { } lift)
        {
            RenderLift(lift.SendingOut);
        }
        else if (GetComponent<BeltCarrierSpec>() is { Shape: BeltShape.RiserUp or BeltShape.RiserDown } riser)
        {
            if (UsesShaft())
            {
                var flow = riser.Shape == BeltShape.RiserUp ? Vector3.up : Vector3.down;
                RenderShaftSide("LeftOut", 0.26f, 0.5f, 0.5f, Vector3.left, flow);
                RenderShaftSide("LeftIn", 0.37f, 0.5f, 0.5f, Vector3.right, flow);
                RenderShaftSide("RightIn", 0.63f, 0.5f, 0.5f, Vector3.left, flow);
                RenderShaftSide("RightOut", 0.74f, 0.5f, 0.5f, Vector3.right, flow);
            }
            else
            {
                RenderRiser(directions[0]);
            }
        }
        else if (GetComponent<BeltCarrierSpec>() is { Shape: BeltShape.Impermeable })
        {
            RenderImpermeable(directions[0]);
        }
        else if (GetComponent<BeltCarrierSpec>() is { Shape: BeltShape.ElbowInUp or BeltShape.ElbowInDown or BeltShape.ElbowOutUp or BeltShape.ElbowOutDown } elbow)
        {
            RenderElbow(elbow.Shape);
        }
        else
        {
            var verticals = 0;
            foreach (var direction in directions)
            {
                if (direction.ToOffset().z != 0)
                {
                    verticals++;
                }
            }

            if (GetComponent<BeltCarrierSpec>() is { Shape: BeltShape.Corner })
            {
                RenderCorner();
            }
            else
            {
                foreach (var direction in directions)
                {
                    RenderBelt(direction, SideHeight(direction, verticals));
                }
            }
        }

        renderedFlipped = block.FlipMode.IsFlipped;
        rendered = true;
    }

    void CollectDirections()
    {
        directions.Clear();
        if (GetComponent<BeltLift>() is { } lift)
        {
            BeltLayout.FillLift(lift.SendingOut, inputs, outputs);
            AddOutputs();
            return;
        }

        if (GetComponent<BeltSplitter>() is not null)
        {
            BeltLayout.FillSplitter(inputs, outputs);
            AddOutputs();
            return;
        }

        if (GetComponent<BeltMerger>() is not null)
        {
            BeltLayout.FillMerger(inputs, outputs);
            AddOutputs();
            return;
        }

        if (GetComponent<BeltCarrierSpec>() is not { } spec)
        {
            return;
        }

        BeltLayout.FillCarrier((BeltForm)spec.Shape, ports);
        foreach (var port in ports)
        {
            if (!port.Incoming)
            {
                directions.Add(port.Direction.Game());
            }
        }
    }

    void AddOutputs()
    {
        foreach (var port in outputs)
        {
            directions.Add(port.Direction.Game());
        }
    }

    static float SideHeight(Direction3D direction, int verticals)
    {
        if (verticals < 2 || direction.ToOffset().z == 0)
        {
            return 0.5f;
        }

        return direction == Direction3D.Top ? 0.65f : 0.35f;
    }

    void RenderImpermeable(Direction3D direction)
    {
        var flow = CoordinateSystem.GridToWorld(direction.ToOffset());
        RenderArrow("Left", new(SideOffset, 0.5f, 0.5f), Vector3.left, flow, Vector3.one);
        RenderArrow("Right", new(1f - SideOffset, 0.5f, 0.5f), Vector3.right, flow, Vector3.one);
    }

    void RenderBelt(Direction3D direction, float sideHeight)
    {
        var grid = direction.ToOffset();
        var unityDir = CoordinateSystem.GridToWorld(grid);
        if (grid.z != 0)
        {
            RenderSide($"{direction}Front", new(SideOffset, sideHeight, 0.5f), Vector3.left, unityDir);
            RenderSide($"{direction}Back", new(1f - SideOffset, sideHeight, 0.5f), Vector3.right, unityDir);
            RenderSide($"{direction}Left", new(0.5f, sideHeight, SideOffset), Vector3.back, unityDir);
            RenderSide($"{direction}Right", new(0.5f, sideHeight, 1f - SideOffset), Vector3.forward, unityDir);
            return;
        }

        RenderArrow($"{direction}Top", new(0.5f, DeckArrowHeight, 0.5f), Vector3.up, unityDir, Vector3.one);
    }

    void RenderCorner()
    {
        const float leg = 0.82f;
        var scale = new Vector3(0.6f, 0.32f, 1f);
        RenderArrow("CornerIn", new(0.5f, DeckArrowHeight, leg), Vector3.up, Vector3.back, scale);
        RenderArrow("CornerOut", new(leg, DeckArrowHeight, 0.5f), Vector3.up, Vector3.right, scale);
    }

    bool UsesShaft()
    {
        var finished = Transform.Find("#Finished");
        if (!finished)
        {
            return false;
        }

        return finished.Find("Poles") is not null || finished.Find("Boards") is not null;
    }

    void RenderElbow(BeltShape shape)
    {
        var upward = shape is BeltShape.ElbowInUp or BeltShape.ElbowOutUp;
        var flow = upward ? Vector3.up : Vector3.down;
        if (UsesShaft())
        {
            RenderShaftElbow(shape, flow);
            return;
        }

        var shaftZ = upward ? 0.6f : 0.5f;
        var shaftY = upward ? 0.45f : 0.5f;
        RenderShaftSide("Left", upward ? 0.34f : ShaftFace, shaftY, shaftZ, Vector3.left, flow);
        RenderShaftSide("Right", upward ? 0.66f : 1f - ShaftFace, shaftY, shaftZ, Vector3.right, flow);

        const float leg = 0.82f;
        var scale = new Vector3(0.6f, 0.32f, 1f);
        if (shape is BeltShape.ElbowInUp or BeltShape.ElbowInDown)
        {
            RenderArrow("ElbowIn", new(0.5f, DeckArrowHeight, leg), Vector3.up, Vector3.back, scale);
            return;
        }

        if (shape is BeltShape.ElbowOutUp)
        {
            RenderArrow("ElbowOut", new(0.5f, DeckArrowHeight, leg), Vector3.up, Vector3.forward, scale);
            return;
        }

        RenderArrow("ElbowOut", new(0.5f, DeckArrowHeight, 1f - leg), Vector3.up, Vector3.back, scale);
    }

    // Shaft arrows sit on the boarded faces. Channels get one bent arrow on each outer side.
    void RenderShaftElbow(BeltShape shape, Vector3 flow)
    {
        const float leg = 0.82f;
        var scale = new Vector3(0.6f, 0.32f, 1f);
        if (shape is BeltShape.ElbowInUp)
        {
            RenderShaftSide("BackOut", 0.5f, 0.5f, 0.28f, Vector3.back, flow);
            RenderShaftSide("BackIn", 0.5f, 0.5f, 0.40f, Vector3.forward, flow);
            RenderArrow("ElbowIn", new(0.5f, DeckArrowHeight, leg), Vector3.up, Vector3.back, scale);
            return;
        }

        if (shape is BeltShape.ElbowOutDown)
        {
            RenderShaftSide("BackOut", 0.5f, 0.5f, 0.72f, Vector3.forward, flow);
            RenderShaftSide("BackIn", 0.5f, 0.5f, 0.60f, Vector3.back, flow);
            RenderArrow("ElbowOut", new(0.5f, DeckArrowHeight, 1f - leg), Vector3.up, Vector3.back, scale);
            return;
        }

        const float left = 0.25f;
        const float right = 0.75f;
        const float hole = 0.5f;
        const float turnY = 0.15f;
        const float stub = 0.1f;
        const float reach = 0.14f;
        if (shape is BeltShape.ElbowOutUp)
        {
            RenderBend("Left", new(left, turnY - stub, hole), new(left, turnY, hole), new(left, turnY, hole + reach), Vector3.left);
            RenderBend("Right", new(right, turnY - stub, hole), new(right, turnY, hole), new(right, turnY, hole + reach), Vector3.right);
            return;
        }

        RenderBend("Left", new(left, turnY, hole + reach), new(left, turnY, hole), new(left, turnY - stub, hole), Vector3.left);
        RenderBend("Right", new(right, turnY, hole + reach), new(right, turnY, hole), new(right, turnY - stub, hole), Vector3.right);
    }

    void RenderBend(string name, Vector3 start, Vector3 corner, Vector3 end, Vector3 outward)
    {
        const float shaft = 0.03f;
        const float headWidth = 0.08f;
        const float headLength = 0.06f;
        var dir = end - corner;
        var length = dir.magnitude;
        if (length <= headLength)
        {
            return;
        }

        var forward = dir / length;
        var neck = end - forward * headLength;
        var mesh = new UnityEngine.Mesh { name = name };
        var verts = new List<Vector3>();
        var tris = new List<int>();
        AddRibbon(verts, tris, start, corner, outward, shaft);
        AddRibbon(verts, tris, corner, neck, outward, shaft);
        AddHead(verts, tris, neck, end, outward, headWidth);
        mesh.SetVertices(verts);
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateBounds();
        bends.Add(mesh);

        var arrow = new GameObject(name);
        arrow.transform.SetParent(arrows.transform, false);
        arrow.AddComponent<MeshFilter>().sharedMesh = mesh;
        arrow.AddComponent<MeshRenderer>().sharedMaterial = arrowsRepo.ArrowMaterial;
    }

    static void AddRibbon(List<Vector3> verts, List<int> tris, Vector3 a, Vector3 b, Vector3 outward, float width)
    {
        var across = Vector3.Cross((b - a).normalized, outward);
        if (across.sqrMagnitude < 0.0001f)
        {
            return;
        }

        var side = across.normalized * (width * 0.5f);
        var index = verts.Count;
        verts.Add(a - side);
        verts.Add(a + side);
        verts.Add(b + side);
        verts.Add(b - side);
        AddDouble(tris, index);
    }

    static void AddHead(List<Vector3> verts, List<int> tris, Vector3 neck, Vector3 tip, Vector3 outward, float width)
    {
        var across = Vector3.Cross((tip - neck).normalized, outward);
        if (across.sqrMagnitude < 0.0001f)
        {
            return;
        }

        var side = across.normalized * (width * 0.5f);
        var index = verts.Count;
        verts.Add(neck - side);
        verts.Add(neck + side);
        verts.Add(tip);
        tris.Add(index);
        tris.Add(index + 1);
        tris.Add(index + 2);
        tris.Add(index);
        tris.Add(index + 2);
        tris.Add(index + 1);
    }

    static void AddDouble(List<int> tris, int index)
    {
        tris.Add(index);
        tris.Add(index + 1);
        tris.Add(index + 2);
        tris.Add(index);
        tris.Add(index + 2);
        tris.Add(index + 3);
        tris.Add(index);
        tris.Add(index + 2);
        tris.Add(index + 1);
        tris.Add(index);
        tris.Add(index + 3);
        tris.Add(index + 2);
    }

    void RenderArms()
    {
        var smart = GetComponent<SmartSplitter>() is not null;
        foreach (var port in inputs)
        {
            RenderArm(port.Direction, inward: true, ArmScale);
        }

        foreach (var port in outputs)
        {
            RenderArm(port.Direction, inward: false, ArmScale, smart ? SplitterMarks.For(port.Direction) : null);
        }
    }

    void RenderArm(BeltDir port, bool inward, Vector3 scale, Color? color = null)
    {
        var outward = CoordinateSystem.GridToWorld(port.Game().ToOffset());
        var flow = inward ? -outward : outward;
        var position = new Vector3(0.5f, DeckArrowHeight, 0.5f) + outward * 0.28f;
        RenderArrow($"{port}{(inward ? "In" : "Out")}", position, Vector3.up, flow, scale, color);
    }

    void RenderLift(bool sendingOut)
    {
        var port = sendingOut ? Vector3.back : Vector3.forward;
        if (UsesShaft())
        {
            RenderShaftLift("LeftOut", 0.26f, Vector3.left, port, sendingOut);
            RenderShaftLift("LeftIn", 0.37f, Vector3.right, port, sendingOut);
            RenderShaftLift("RightIn", 0.63f, Vector3.left, port, sendingOut);
            RenderShaftLift("RightOut", 0.74f, Vector3.right, port, sendingOut);
            return;
        }

        RenderLiftSide("Left", SideOffset, Vector3.left, port, sendingOut);
        RenderLiftSide("Right", 1f - SideOffset, Vector3.right, port, sendingOut);
    }

    void RenderShaftLift(string side, float x, Vector3 normal, Vector3 port, bool sendingOut)
    {
        if (sendingOut)
        {
            RenderShaftSide($"{side}FromAbove", x, 0.72f, 0.5f, normal, Vector3.down);
            RenderShaftSide($"{side}FromBelow", x, 0.28f, 0.5f, normal, Vector3.up);
            RenderShaftSide($"{side}Out", x, 0.5f, 0.5f, normal, port);
            return;
        }

        RenderShaftSide($"{side}In", x, 0.5f, 0.5f, normal, port);
        RenderShaftSide($"{side}Up", x, 0.72f, 0.5f, normal, Vector3.up);
        RenderShaftSide($"{side}Down", x, 0.28f, 0.5f, normal, Vector3.down);
    }

    void RenderLiftSide(string side, float x, Vector3 normal, Vector3 port, bool sendingOut)
    {
        var center = new Vector3(x, 0.5f, 0.5f);
        if (sendingOut)
        {
            RenderJunction($"{side}FromAbove", center, normal, Vector3.down, inward: true);
            RenderJunction($"{side}FromBelow", center, normal, Vector3.up, inward: true);
            RenderJunction($"{side}Out", center, normal, port, inward: false);
            return;
        }

        RenderJunction($"{side}In", center, normal, port, inward: true);
        RenderJunction($"{side}Up", center, normal, Vector3.up, inward: false);
        RenderJunction($"{side}Down", center, normal, Vector3.down, inward: false);
    }

    void RenderJunction(string name, Vector3 center, Vector3 normal, Vector3 flow, bool inward)
    {
        const float reach = 0.2f;
        var position = center + (inward ? -flow : flow) * reach;
        RenderArrow(name, position, normal, flow, JunctionScale);
    }

    void RenderRiser(Direction3D direction)
    {
        var flow = CoordinateSystem.GridToWorld(direction.ToOffset());
        RenderShaftSide("Left", ShaftFace, 0.5f, 0.5f, Vector3.left, flow);
        RenderShaftSide("Right", 1f - ShaftFace, 0.5f, 0.5f, Vector3.right, flow);
    }

    void RenderShaftSide(string name, float x, float y, float z, Vector3 normal, Vector3 flow)
    {
        RenderArrow(name, new(x, y, z), normal, flow, ShaftArrowScale);
    }

    void RenderSide(string name, Vector3 position, Vector3 normal, Vector3 forward)
    {
        if (Mathf.Abs(Vector3.Dot(normal, forward)) > 0.99f)
        {
            return;
        }

        RenderArrow(name, position, normal, forward, Vector3.one);
    }

    void RenderArrow(string name, Vector3 position, Vector3 normal, Vector3 forward, Vector3 scale, Color? color = null)
    {
        var arrow = new GameObject(name);
        arrow.transform.SetParent(arrows.transform, false);
        arrow.transform.SetLocalPositionAndRotation(position, Quaternion.LookRotation(normal, forward));
        arrow.transform.localScale = scale;

        var meshFilter = arrow.AddComponent<MeshFilter>();
        meshFilter.sharedMesh = arrowsRepo.ArrowMesh;

        var meshRenderer = arrow.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = arrowsRepo.ArrowMaterial;
        if (color is { } tint)
        {
            var block = new MaterialPropertyBlock();
            block.SetColor(SplitterMarks.BaseColorId, tint);
            block.SetColor(SplitterMarks.ColorId, tint);
            meshRenderer.SetPropertyBlock(block);
        }
    }

    void ClearArrows()
    {
        foreach (var bend in bends)
        {
            Object.Destroy(bend);
        }

        bends.Clear();
        for (var i = arrows.transform.childCount - 1; i >= 0; i--)
        {
            Object.Destroy(arrows.transform.GetChild(i).gameObject);
        }
    }
}
