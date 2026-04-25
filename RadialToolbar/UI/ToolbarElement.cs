namespace RadialToolbar.UI;

[BindSingleton(Contexts = BindAttributeContext.NonMenu)]
public class ToolbarElement(
    ToolbarFrame frame,
    ToolbarSegmentItemRenderer itemRenderer,
    QuickSlotElement quickSlotElement,
    ToolbarSegmentProvider segmentProvider,
    KeyBindingDescriber keyBindingDescriber,
    ILoc t
) : VisualElement, ILoadableSingleton
{

    public event Action<int> OnSegmentChosen = null!;
    public event Action OnBackButtonRequested = null!;
    public event Action OnCloseButtonRequested = null!;

    public void Load()
    {
        this.SetFullscreen();

        Add(frame);
        frame.RegisterCallback<PointerMoveEvent>(OnPointerMove);
        frame.RegisterCallback<PointerDownEvent>(OnPointerDown);

        Add(itemRenderer);
        Add(quickSlotElement);

        AddCommandButtons();

        this.SetDisplay(false);
    }

    public void HighlightSegment(int? segment)
    {
        if (segment == frame.HighlightedSegment) { return; }

        frame.HighlightedSegment = segment;
        frame.Repaint();
    }

    void OnPointerMove(PointerMoveEvent evt) => HighlightSegmentAt(evt.localPosition);

    void OnPointerDown(PointerDownEvent evt)
    {
        if (evt.button == 1)
        {
            OnBackButtonRequested();
            return;
        }

        var segment = HighlightSegmentAt(evt.localPosition);
        if (segment is not null)
        {
            OnSegmentChosen(segment.Value);
        }
    }

    int? HighlightSegmentAt(Vector3 v)
    {
        var segment = segmentProvider.GetSegmentAt(v);
        HighlightSegment(segment);

        return segment;
    }

    void AddCommandButtons()
    {
        var commandPanel = this.AddChild();
        var s = commandPanel.style;
        s.position = Position.Absolute;
        s.right = 20;
        s.bottom = 20;

        commandPanel.AddGameButtonPadded(
            t.T("LV.RT.CloseButton", keyBindingDescriber.GetOrDefault(ToolBarTriggerer.ToggleKey)),
            onClick: () => OnCloseButtonRequested())
            .SetMarginBottom(5);
        commandPanel.AddGameButtonPadded(
            t.T("LV.RT.BackButton", keyBindingDescriber.GetOrDefault(ToolbarController.BackKeyId)),
            onClick: () => OnBackButtonRequested());
    }

    public void Show()
    {
        segmentProvider.GetSegments(contentRect); // Ensure cache

        frame.HighlightedSegment = null;
        frame.Repaint();
    }

}
