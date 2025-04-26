namespace TheArchitectsToolkit.Services.Copier;

public class CopierDialog : DialogBoxElement
{
    readonly IntegerField[] txtSize = new IntegerField[2];

    public Vector2Int Size => new(txtSize[0].value, txtSize[1].value);

    public CopierDialog(ILoc t, Vector2Int defaultSize)
    {
        SetTitle("LV.TAT.CopyObjects".T(t));
        AddCloseButton();

        var container = Content.AddChild();

        for (int i = 0; i < 2; i++)
        {
            var row = container.AddRow().SetMarginBottom();

            row.AddGameLabel(t.T("LV.TAT.Size" + i)).SetMarginRight();
            var txt = txtSize[i] = row.AddIntField().SetFlexGrow(1);
            txt.SetValueWithoutNotify(defaultSize[i]);
        }

        container.AddMenuButton("LV.TAT.CopyObjects".T(t), onClick: OnUIConfirmed, stretched: true);
    }

}
