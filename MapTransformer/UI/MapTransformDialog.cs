namespace MapTransformer.UI;

class MapTransformDialog : DialogBoxElement
{
    static readonly ImmutableArray<MapOp> Ops =
        [MapOp.ResizeHeight, MapOp.ResizeXy, MapOp.AddHeight, MapOp.Rotate, MapOp.Flip];
    static readonly ImmutableArray<EnlargeFill> Fills =
        [EnlargeFill.Empty, EnlargeFill.CopyEdge, EnlargeFill.Mirror];
    static readonly ImmutableArray<int> RotateSteps = [1, 2, 3];

    readonly ILoc t;
    readonly MapSize mapSize;
    readonly VisualElement resizeXyPanel;
    readonly VisualElement resizeHeightPanel;
    readonly VisualElement addHeightPanel;
    readonly VisualElement rotatePanel;
    readonly VisualElement flipPanel;
    readonly IntegerField txtX, txtY, txtPivotX, txtPivotY, txtTerrainZ, txtTotalZ, txtAddLayers;
    readonly Toggle chkIncreaseTerrainHeight;
    readonly AreaScope rotateArea;
    readonly AreaScope flipArea;

    MapOp op = Ops[0];
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

        resizeXyPanel = Content.AddChild();
        txtX = AddInt(resizeXyPanel, "LV.MTr.SizeX", mapSize.TerrainSize.x);
        txtY = AddInt(resizeXyPanel, "LV.MTr.SizeY", mapSize.TerrainSize.y);
        txtPivotX = AddInt(resizeXyPanel, "LV.MTr.PivotX", 0);
        txtPivotY = AddInt(resizeXyPanel, "LV.MTr.PivotY", 0);
        resizeXyPanel.AddGameLabel("LV.MTr.PivotHint".T(t)).SetMarginBottom();
        AddDropdownRow(resizeXyPanel, "LV.MTr.Fill", dropdownItemsSetter, veInit,
            [.. Fills.Select(f => t.T("LV.MTr.Fill" + f))],
            0,
            i => fill = Fills[i]);

        resizeHeightPanel = Content.AddChild();
        txtTerrainZ = AddInt(resizeHeightPanel, "LV.MTr.SizeZ", mapSize.TerrainSize.z);
        txtTotalZ = AddInt(resizeHeightPanel, "LV.MTr.SizeTotal", mapSize.TotalSize.z);

        addHeightPanel = Content.AddChild();
        txtAddLayers = AddInt(addHeightPanel, "LV.MTr.AddLayers", 1);
        chkIncreaseTerrainHeight = addHeightPanel.AddToggle(t.T("LV.MTr.IncreaseTerrainHeight"))
            .SetMarginBottom();
        chkIncreaseTerrainHeight.SetValueWithoutNotify(true);

        rotatePanel = Content.AddChild();
        AddDropdownRow(rotatePanel, "LV.MTr.Rotate", dropdownItemsSetter, veInit,
            [t.T("LV.MTr.Rotate90"), t.T("LV.MTr.Rotate180"), t.T("LV.MTr.Rotate270")],
            0,
            i => rotateSteps = RotateSteps[i]);
        rotateArea = AddAreaScope(rotatePanel, dropdownItemsSetter, veInit);

        flipPanel = Content.AddChild();
        AddDropdownRow(flipPanel, "LV.MTr.FlipAxis", dropdownItemsSetter, veInit,
            [t.T("LV.MTr.FlipX"), t.T("LV.MTr.FlipY"), t.T("LV.MTr.FlipBoth")],
            0,
            i =>
            {
                flipX = i != 1;
                flipY = i != 0;
            });
        flipArea = AddAreaScope(flipPanel, dropdownItemsSetter, veInit);

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
            MapOp.ResizeXy => MapTransform.ResizeXy(
                oldTerrain,
                oldTotal,
                new Vector2Int(Math.Max(1, txtX.value), Math.Max(1, txtY.value)),
                new Vector2Int(txtPivotX.value, txtPivotY.value),
                fill),
            MapOp.ResizeHeight => MapTransform.ResizeHeight(
                oldTerrain,
                oldTotal,
                txtTerrainZ.value,
                txtTotalZ.value),
            MapOp.AddHeight => MapTransform.AddHeight(
                oldTerrain,
                oldTotal,
                Math.Max(0, txtAddLayers.value),
                chkIncreaseTerrainHeight.value),
            MapOp.Rotate => MapTransform.RotateCw(
                oldTerrain,
                oldTotal,
                rotateSteps,
                rotateArea.UseArea,
                rotateArea.Origin,
                rotateArea.SizeValue),
            MapOp.Flip => MapTransform.Flip(
                oldTerrain,
                oldTotal,
                flipX,
                flipY,
                flipArea.UseArea,
                flipArea.Origin,
                flipArea.SizeValue),
            _ => MapTransform.ResizeXy(oldTerrain, oldTotal, new Vector2Int(oldTerrain.x, oldTerrain.y), Vector2Int.zero, EnlargeFill.Empty),
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

    AreaScope AddAreaScope(
        VisualElement parent,
        DropdownItemsSetter dropdownItemsSetter,
        VisualElementInitializer veInit)
    {
        VisualElement? areaPanel = null;
        var scope = new AreaScope();
        AddDropdownRow(parent, "LV.MTr.Scope", dropdownItemsSetter, veInit,
            [t.T("LV.MTr.ScopeWhole"), t.T("LV.MTr.ScopeArea")],
            0,
            i =>
            {
                scope.UseArea = i == 1;
                areaPanel?.SetDisplay(scope.UseArea);
            });
        areaPanel = parent.AddChild();
        scope.X = AddInt(areaPanel, "LV.MTr.AreaX", 0);
        scope.Y = AddInt(areaPanel, "LV.MTr.AreaY", 0);
        scope.Size = AddInt(areaPanel, "LV.MTr.AreaSize", 1);
        areaPanel.SetDisplay(false);
        return scope;
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
        resizeXyPanel.SetDisplay(op == MapOp.ResizeXy);
        resizeHeightPanel.SetDisplay(op == MapOp.ResizeHeight);
        addHeightPanel.SetDisplay(op == MapOp.AddHeight);
        rotatePanel.SetDisplay(op == MapOp.Rotate);
        flipPanel.SetDisplay(op == MapOp.Flip);
    }

    sealed class AreaScope
    {
        public bool UseArea;
        public IntegerField X = null!;
        public IntegerField Y = null!;
        public IntegerField Size = null!;

        public Vector2Int Origin => new(X.value, Y.value);
        public int SizeValue => Size.value;
    }
}
