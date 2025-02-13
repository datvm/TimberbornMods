namespace UiBuilder.Common;

public class DialogBoxElement : VisualElement
{
    static readonly ImmutableArray<string> MainClasses = ["content-row-centered"];
    static readonly ImmutableArray<string> NineSliceClasses = ["content-centered", "sliced-border", "sliced-border--nontransparent"];
    static readonly ImmutableArray<string> BoxClasses = ["box"];

    public VisualElement Box { get; private set; }

    public DialogBoxElement() : this(false) { }

    public DialogBoxElement(bool scrollAsBox)
    {
        classList.AddRange(MainClasses);

        var nineSlice = this.AddChild<NineSliceVisualElement>(classes: NineSliceClasses);

        var type = scrollAsBox ? typeof(ScrollView) : typeof(VisualElement);
        Box = nineSlice.AddChild(type, name: "Box", BoxClasses);
    }

}
