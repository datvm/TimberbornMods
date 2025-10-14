namespace BlockObjectDebugger.UI;

public class BlockObjectFragment(
    IBlockService blockService,
    EntityRegistry entities
) : BaseEntityPanelFragment<BlockObjectDebuggerComponent>, IEntityFragmentOrder, ILoadableSingleton
{
    public int Order { get; } = 1;
    public VisualElement Fragment => panel;

#nullable disable
    Label lblInfo;
    Button btnDraw, btnUndraw;
#nullable enable

    public void Load()
    {
        ModUtils.AppendLoc("Name_BlockObjectFragment", "Block Object Debugger");
    }

    protected override void InitializePanel()
    {
        panel.Background = EntityPanelFragmentBackground.PurpleStriped;

        btnDraw = CreateButton("Draw Block Occupations", () => ToggleDrawing(true));
        btnUndraw = CreateButton("Undraw Block Occupations", () => ToggleDrawing(false));
        CreateButton("Draw all objects within boundary", () => ToggleDrawInBoundary(true));
        CreateButton("Undraw all objects within boundary", () => ToggleDrawInBoundary(false));
        CreateButton("Undraw all objects", UndrawAll);

        lblInfo = panel.AddGameLabel().SetMarginBottom();

        Button CreateButton(string text, Action onClick)
        {
            return panel.AddGameButton(text, onClick: onClick, stretched: true)
                .SetPadding(0, 5)
                .SetMarginBottom();
        }
    }

    public override void ShowFragment(BaseComponent entity)
    {
        base.ShowFragment(entity);

        if (!component) { return; }

        lblInfo.text = component.Info;
        ToggleDrawingButtons();
    }

    void ToggleDrawing(bool draw)
    {
        if (!component) { return; }

        component.ToggleDebug(draw);
        ToggleDrawingButtons();
    }

    void ToggleDrawingButtons()
    {
        var drawing = component && component.DrawingDebug;
        btnDraw.SetDisplay(!drawing);
        btnUndraw.SetDisplay(drawing);
    }

    void ToggleDrawInBoundary(bool draw)
    {
        if (!component) { return; }

        HashSet<BlockObject> processed = [];

        foreach (var coord in component.PositionedCoordinates)
        {
            var objs = blockService.GetObjectsAt(coord);

            foreach (var obj in objs)
            {
                if (processed.Contains(obj)) { continue; }
                processed.Add(obj);

                var comp = obj.GetComponentFast<BlockObjectDebuggerComponent>();
                if (comp)
                {
                    comp.ToggleDebug(draw);
                }
            }
        }

        ToggleDrawingButtons();
    }

    void UndrawAll()
    {
        foreach (var e in entities.Entities)
        {
            var comp = e.GetComponentFast<BlockObjectDebuggerComponent>();
            if (comp)
            {
                comp.ToggleDebug(false);
            }
        }

        ToggleDrawingButtons();
    }

}
