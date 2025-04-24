global using Timberborn.MapStateSystem;

namespace MapResizer.UI;

public class MapResizerDialog : DialogBoxElement
{
    readonly ILoc t;
    readonly IntegerField[] txtSizes = new IntegerField[4];

    public ResizeValues Size => new(txtSizes[0].value, txtSizes[1].value, txtSizes[2].value, txtSizes[3].value);

    public MapResizerDialog(MapSize mapSize, ILoc t)
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

}

public readonly record struct ResizeValues(int X, int Y, int Z1, int Z2);