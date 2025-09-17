namespace BuildingDecal.UI;

public class BuildingDecalItemPanel : CollapsiblePanel
{
    public event EventHandler? OnDecalRequested;
    public event EventHandler? OnDecalDeletionRequested;

    readonly ILoc t;
    readonly Dropdown cboTemplate;
    readonly Vector3SliderPanel positionPanel, rotationPanel, scalePanel;

    ImmutableArray<DecalPositionSpec> templatePositions = [];
    Vector3Int buildingSize;

    public BuildingDecalItem Item { get; }
    public int Index { get; }

    public BuildingDecalItemPanel(BuildingDecalItem item, int index, GameSliderAlternativeManualValueDI sliderDI)
    {
        Item = item;
        Index = index;
        var t = this.t = sliderDI.t;

        var parent = Container;

        parent.AddGameButton(t.T("LV.BDl.Change"), onClick: () => OnDecalRequested?.Invoke(this, EventArgs.Empty), stretched: true)
            .SetPadding(5)
            .SetMarginBottom(5);

        cboTemplate = parent.AddDropdown()
            .SetMarginBottom(5)
            .AddChangeHandler(OnTemplateSelected);

        positionPanel = parent.AddChild(() => new Vector3SliderPanel(sliderDI))
            .SetLabels(["X", "Y", "Z"])
            .SetRange(-10, 10)
            .RegisterChange(OnPositionChanged)
            .SetValueWithoutNotify(item.Position)
            .SetTitleFunction(v => TitleFunction("LV.BDl.Position", v));
        rotationPanel = parent.AddChild(() => new Vector3SliderPanel(sliderDI))
            .SetTitle(t.T("LV.BDl.Rotation"))
            .SetLabels(["X", "Y", "Z"])
            .SetRange(-180, 180)
            .RegisterChange(OnRotationChanged)
            .SetValueWithoutNotify(item.Rotation.eulerAngles)
            .SetTitleFunction(v => TitleFunction("LV.BDl.Rotation", v));
        scalePanel = parent.AddChild(() => new Vector3SliderPanel(sliderDI))
            .SetTitle(t.T("LV.BDl.Scale"))
            .SetLabels(["X"])
            .SetRange(0.01f, 10)
            .RegisterChange(OnScaleChanged)
            .SetValueWithoutNotify(item.Scale)
            .SetTitleFunction(v => t.T("LV.BDl.Scale", v.x));
        parent.AddChild(() => new Vector3SliderPanel(sliderDI))
            .SetTitle(t.T("LV.BDl.Color"))
            .SetLabels(["R", "G", "B"])
            .SetRange(0, 1)
            .RegisterChange(OnColorChanged)
            .SetValueWithoutNotify(item.Color.ToVector3())
            .SetTitleFunction(v => TitleFunction("LV.BDl.Color", v));

        parent.AddToggle(t.T("LV.BDl.FlipX"), onValueChanged: SetFlipX)
            .SetValueWithoutNotify(item.FlipX);
        parent.AddToggle(t.T("LV.BDl.FlipY"), onValueChanged: SetFlipY)
            .SetMarginBottom()
            .SetValueWithoutNotify(item.FlipY);

        parent.AddGameButton(t.T("LV.BDl.Remove"), onClick: () => OnDecalDeletionRequested?.Invoke(this, EventArgs.Empty), stretched: true)
            .SetPadding(5);

        SetExpand(false);
        HeaderLabel.SetMaxSizePercent(100, null);
        RefreshTitle();

        string TitleFunction(string loc, in Vector3 v) => t.T(loc, v.x, v.y, v.z);
    }

    void OnPositionChanged(Vector3 pos) => Item.Position = pos;
    void OnRotationChanged(Vector3 rotation) => Item.Rotation = Quaternion.Euler(rotation);
    void OnScaleChanged(Vector3 scale) => Item.Scale = new(scale.x, scale.x, scale.x);
    void OnColorChanged(Vector3 color) => Item.Color = color.ToColor();
    void SetFlipX(bool flip) => Item.FlipX = flip;
    void SetFlipY(bool flip) => Item.FlipY = flip;

    public void SetDecal(in SpriteWithName sprite)
    {
        Item.SetSprite(sprite);
        RefreshTitle();
    }

    public void SetTemplatePositions(in ImmutableArray<DecalPositionSpec> templatePositions, in Vector3Int buildingSize, DropdownItemsSetter dropdownItemsSetter)
    {
        IReadOnlyList<string> labels = [t.T("LV.BDl.PosCustom"), .. templatePositions.Select(q => q.Name.Value)];

        cboTemplate.SetItems(dropdownItemsSetter, labels);
        this.templatePositions = templatePositions;
        this.buildingSize = buildingSize;
    }

    public void ActivateFirstTemplate()
    {
        if (templatePositions.Length > 0)
        {
            OnTemplateSelected(null, 1);
        }
    }

    void OnTemplateSelected(string? _, int index)
    {
        if (index <= 0 || index > templatePositions.Length) { return; }

        var t = templatePositions[index - 1];
        var worldSize = CoordinateSystem.GridToWorld(buildingSize);

        positionPanel.Value = new Vector3(
            t.Position.x * worldSize.x,
            t.Position.y * worldSize.y,
            t.Position.z * worldSize.z);

        rotationPanel.Value = t.Rotation;
        
        if (t.ScaleXTo != SizeEdge.None || t.ScaleYTo != SizeEdge.None)
        {
            var scale = GetScale(t, worldSize);
            scalePanel.Value = new Vector3(scale, 0, 0);
        }

        cboTemplate.SetSelectedItem(0);
    }

    float GetScale(DecalPositionSpec t, in Vector3 buildingWorldSize)
    {
        var scale = float.MaxValue;
        var spriteSize = Item.SpriteSize;

        if (t.ScaleXTo != SizeEdge.None)
        {
            var targetSize = buildingWorldSize[(int)t.ScaleXTo];
            scale = targetSize / spriteSize.x;
        }

        if (t.ScaleYTo != SizeEdge.None)
        {
            var targetSize = buildingWorldSize[(int)t.ScaleYTo];
            scale = Math.Min(scale, targetSize / spriteSize.y);
        }

        return Math.Min(1, scale);
    }

    void RefreshTitle()
    {
        SetTitle(t.T("LV.BDl.DecalName", Index, Item.DecalName));
    }

}
