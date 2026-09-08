namespace MapTransformer.UI;

class MapTransformDialog : DialogBoxElement
{
    static readonly ImmutableArray<MapOp> Ops =
        [MapOp.Resize, MapOp.AddHeight, MapOp.RemoveHeight, MapOp.Rotate, MapOp.Flip];
    static readonly ImmutableArray<MapPivot> Pivots =
        [MapPivot.MinMin, MapPivot.MaxMin, MapPivot.MinMax, MapPivot.MaxMax, MapPivot.Center];
    static readonly ImmutableArray<EnlargeFill> Fills =
        [EnlargeFill.Empty, EnlargeFill.CopyEdge, EnlargeFill.Mirror];
    static readonly ImmutableArray<int> RotateSteps = [1, 2, 3];

    readonly ILoc t;
    readonly MapSize mapSize;
    readonly VisualElement resizePanel;
    readonly VisualElement addHeightPanel;
    readonly VisualElement removeHeightPanel;
    readonly VisualElement rotatePanel;
    readonly VisualElement flipPanel;
    readonly IntegerField txtX, txtY, txtZ, txtAbove, txtAddLayers, txtAddAbove, txtRemoveZ1, txtRemoveZ2;

    MapOp op = MapOp.Resize;
    MapPivot pivot = MapPivot.MinMin;
    EnlargeFill fill = EnlargeFill.Empty;
    int rotateSteps = 1;
    bool flipX = true;
    bool flipY;

    public MapTransformDialog(
        MapSize mapSize,
        ILoc t,
        DropdownItemsSetter dropdownItemsSetter,
        VisualElementInitializer veInit,
        bool isGame
    )
    {
        this.t = t;
        this.mapSize = mapSize;

        SetTitle(t.T("LV.MTr.TransformMap"));
        AddCloseButton();

        AddDropdownRow(Content, "LV.MTr.Mode", dropdownItemsSetter, veInit,
            [.. Ops.Select(o => t.T("LV.MTr.Mode" + o))],
            0,
            i =>
            {
                op = Ops[i];
                UpdatePanels();
            });

        resizePanel = Content.AddChild();
        txtX = AddInt(resizePanel, "LV.MTr.SizeX", mapSize.TerrainSize.x);
        txtY = AddInt(resizePanel, "LV.MTr.SizeY", mapSize.TerrainSize.y);
        txtZ = AddInt(resizePanel, "LV.MTr.SizeZ", mapSize.TerrainSize.z);
        txtAbove = AddInt(resizePanel, "LV.MTr.SizeAbove", mapSize.TotalSize.z - mapSize.TerrainSize.z);
        AddDropdownRow(resizePanel, "LV.MTr.Pivot", dropdownItemsSetter, veInit,
            [.. Pivots.Select(p => t.T("LV.MTr.Pivot" + p))],
            0,
            i => pivot = Pivots[i]);
        AddDropdownRow(resizePanel, "LV.MTr.Fill", dropdownItemsSetter, veInit,
            [.. Fills.Select(f => t.T("LV.MTr.Fill" + f))],
            0,
            i => fill = Fills[i]);

        addHeightPanel = Content.AddChild();
        txtAddLayers = AddInt(addHeightPanel, "LV.MTr.AddLayers", 1);
        txtAddAbove = AddInt(addHeightPanel, "LV.MTr.SizeAbove", mapSize.TotalSize.z - mapSize.TerrainSize.z);

        removeHeightPanel = Content.AddChild();
        txtRemoveZ1 = AddInt(removeHeightPanel, "LV.MTr.RemoveZ1", 0);
        txtRemoveZ2 = AddInt(removeHeightPanel, "LV.MTr.RemoveZ2", 0);

        rotatePanel = Content.AddChild();
        AddDropdownRow(rotatePanel, "LV.MTr.Rotate", dropdownItemsSetter, veInit,
            [t.T("LV.MTr.Rotate90"), t.T("LV.MTr.Rotate180"), t.T("LV.MTr.Rotate270")],
            0,
            i => rotateSteps = RotateSteps[i]);

        flipPanel = Content.AddChild();
        AddDropdownRow(flipPanel, "LV.MTr.FlipAxis", dropdownItemsSetter, veInit,
            [t.T("LV.MTr.FlipX"), t.T("LV.MTr.FlipY"), t.T("LV.MTr.FlipBoth")],
            0,
            i =>
            {
                flipX = i != 1;
                flipY = i != 0;
            });

        Content.AddGameLabel("LV.MTr.SaveWarning".T(t), centered: true);
        if (isGame)
        {
            Content.AddGameLabel("LV.MTr.GameDisclaimer".T(t), centered: true);
        }

        Content.AddMenuButton(t.T("LV.MTr.Apply"), OnUIConfirmed);
        UpdatePanels();
    }

    public MapTransform BuildTransform()
    {
        var oldTerrain = mapSize.TerrainSize;
        var oldTotal = mapSize.TotalSize;
        return op switch
        {
            MapOp.Resize => MapTransform.Resize(
                oldTerrain,
                oldTotal,
                new Vector3Int(Math.Max(1, txtX.value), Math.Max(1, txtY.value), Math.Max(1, txtZ.value)),
                new Vector3Int(Math.Max(1, txtX.value), Math.Max(1, txtY.value), Math.Max(1, txtZ.value) + Math.Max(0, txtAbove.value)),
                pivot,
                fill),
            MapOp.AddHeight => MapTransform.AddHeight(oldTerrain, oldTotal, Math.Max(1, txtAddLayers.value), Math.Max(0, txtAddAbove.value)),
            MapOp.RemoveHeight => MapTransform.RemoveHeight(oldTerrain, oldTotal, txtRemoveZ1.value, txtRemoveZ2.value),
            MapOp.Rotate => MapTransform.RotateCw(oldTerrain, oldTotal, rotateSteps),
            MapOp.Flip => MapTransform.Flip(oldTerrain, oldTotal, flipX, flipY),
            _ => MapTransform.Resize(oldTerrain, oldTotal, oldTerrain, oldTotal, MapPivot.MinMin, EnlargeFill.Empty),
        };
    }

    IntegerField AddInt(VisualElement parent, string key, int init)
    {
        var row = parent.AddChild().SetAsRow().SetMarginBottom();
        row.AddLabel(t.T(key) + ":").SetMarginRight();
        var txt = row.AddIntField().SetFlexGrow();
        txt.SetValueWithoutNotify(init);
        return txt;
    }

    void AddDropdownRow(
        VisualElement parent,
        string key,
        DropdownItemsSetter dropdownItemsSetter,
        VisualElementInitializer veInit,
        string[] items,
        int selected,
        Action<int> onSelected)
    {
        var row = parent.AddRow().SetMarginBottom();
        row.AddGameLabel(t.T(key)).SetMarginRight();
        var dropdown = row.AddDropdown()
            .AddChangeHandler((_, index) =>
            {
                if (index >= 0)
                {
                    onSelected(index);
                }
            })
            .SetFlexGrow()
            .Initialize(veInit);
        dropdown.SetItems(dropdownItemsSetter, items, items[selected]);
    }

    void UpdatePanels()
    {
        resizePanel.SetDisplay(op == MapOp.Resize);
        addHeightPanel.SetDisplay(op == MapOp.AddHeight);
        removeHeightPanel.SetDisplay(op == MapOp.RemoveHeight);
        rotatePanel.SetDisplay(op == MapOp.Rotate);
        flipPanel.SetDisplay(op == MapOp.Flip);
    }
}
