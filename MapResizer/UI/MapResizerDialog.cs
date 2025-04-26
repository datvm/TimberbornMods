namespace MapResizer.UI;

public class MapResizerDialog : DialogBoxElement
{
    public static readonly ImmutableArray<EnlargeStrategy> EnlargeStrategies =
        [.. Enum.GetValues(typeof(EnlargeStrategy)).Cast<EnlargeStrategy>()];

    readonly ILoc t;
    readonly IntegerField[] txtSizes = new IntegerField[4];

    public ResizeValues ResizeValues => new(
        txtSizes[0].value,
        txtSizes[1].value,
        txtSizes[2].value,
        txtSizes[3].value,
        EnlargeStrategy
    );

    public EnlargeStrategy EnlargeStrategy { get; private set; } = EnlargeStrategy.Mirror;

    public MapResizerDialog(
        MapSize mapSize,
        ILoc t,
        DropdownItemsSetter dropdownItemsSetter,
        VisualElementInitializer veInit
    )
    {
        this.t = t;

        SetTitle(t.T("LV.MRe.ResizeMap"));
        AddCloseButton();

        var sizeContainer = Content.AddChild().SetMarginBottom();
        for (int i = 0; i < txtSizes.Length; i++)
        {
            txtSizes[i] = AddSize(
                sizeContainer,
                "LV.MRe.Size" + i, i == 3
                    ? mapSize.TotalSize.z - mapSize.TerrainSize.z
                    : mapSize.TerrainSize[i]
            );
        }

        AddEnlargeStrategies(Content, dropdownItemsSetter, veInit);

        Content.AddGameLabel("LV.MRe.SaveWarning".T(t), centered: true);
        Content.AddMenuButton(t.T("LV.MRe.Resize"), OnUIConfirmed);
    }

    IntegerField AddSize(VisualElement parent, string key, int initValue)
    {
        var row = parent.AddChild().SetAsRow().SetMarginBottom();

        row.AddLabel(t.T(key) + ":")
            .SetMarginRight();

        var txt = row.AddChild<NineSliceIntegerField>(classes: ["text-field"])
            .SetFlexGrow();
        txt.SetValueWithoutNotify(initValue);
        return txt;
    }

    void AddEnlargeStrategies(VisualElement parent, DropdownItemsSetter dropdownItemsSetter, VisualElementInitializer veInit)
    {
        var enlargeStratContainer = parent.AddRow().SetMarginBottom();

        enlargeStratContainer.AddGameLabel("LV.MRe.ExandStrat".T(t)).SetMarginRight();

        enlargeStratContainer.AddDropdown()
            .AddChangeHandler(OnStrategySelected)
            .SetFlexGrow()
            .Initialize(veInit)
            .SetItems(
                dropdownItemsSetter, 
                [.. EnlargeStrategies.Select(GetTextFor)],
                GetTextFor(EnlargeStrategy));

    }

    void OnStrategySelected(string? _, int index)
    {
        EnlargeStrategy = EnlargeStrategies[index];
    }

    string GetTextFor(EnlargeStrategy strategy) => t.T("LV.MRe.ExandStrat" + strategy);

}

public readonly record struct ResizeValues(int X, int Y, int Z1, int Z2, EnlargeStrategy EnlargeStrategy)
{

    public Vector3Int TerrainSize => new(X, Y, Z1);
    public Vector3Int TotalSize => new(X, Y, Z1 + Z2);

}