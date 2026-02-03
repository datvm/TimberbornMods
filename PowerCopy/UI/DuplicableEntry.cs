namespace PowerCopy.UI;

public abstract class DuplicableEntry(Type type) : VisualElement
{

#nullable disable
    protected Toggle chk;
#nullable enable

    public event Action<bool> OnSelectedChanged = null!;

    public abstract string Name { get; }
    public abstract int Order { get; }

    public Type Type { get; } = type;
    public string DefaultNameLoc { get; } = "DuplicableName_" + type.Name;

    public bool Selected => Visible && chk.value;

    bool currVisible = true;
    public bool Visible
    {
        get => currVisible;
        set
        {
            if (value != currVisible)
            {
                currVisible = value;
                this.SetDisplay(currVisible);
            }
        }
    }

    public virtual void Initialize()
    {
        Visible = false;
        this.SetAsRow().AlignItems();

        chk = this.AddToggle(Name, onValueChanged: v => OnSelectedChanged(v)).SetMarginRight(5).SetFlexGrow();
        chk.SetValueWithoutNotify(true);
    }

    public virtual void ShowFor(IDuplicable duplicable) { }

    protected string GetDefaultName(ILoc t)
    {
        var key = "DuplicableName_" + Type.Name;
        var name = t.T(key);
        if (name == key)
        {
            name = t.T("LV.PC.UnknownOption", name);
        }
        return name;
    }

}