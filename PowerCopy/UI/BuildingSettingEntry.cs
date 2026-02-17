namespace PowerCopy.UI;

public class BuildingSettingEntry : VisualElement
{

    readonly Toggle chk;

    public bool Selected => chk.value;
    public event Action OnCheckedChanged = null!;

    bool isDisplaying = false;
    public bool Visible
    {
        get => isDisplaying;
        set
        {
            if (value == isDisplaying) { return; }
            isDisplaying = value;
            this.SetDisplay(value);
        }
    }

    public IBuildingSettings BuildingSetting { get; }

    public BuildingSettingEntry(IBuildingSettings settings)
    {
        BuildingSetting = settings;

        chk = this.AddToggle(onValueChanged: _ => OnCheckedChanged());
        chk.SetValueWithoutNotify(true);

        this.SetDisplay(false);        
    }

    public void Set(IDuplicable duplicable)
    {
        var text = BuildingSetting.DescribeDuplicable(duplicable);
        text = BuildingSetting.Name + (string.IsNullOrEmpty(text) ? "" : $": {text}");

        chk.text = text;
    }

}
