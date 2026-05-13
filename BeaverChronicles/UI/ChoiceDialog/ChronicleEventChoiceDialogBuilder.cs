namespace BeaverChronicles.UI;



[BindTransient]
public class ChronicleEventChoiceDialogBuilder(
    ILoc t,
    ChronicleEventUIHelper helper,
    IAssetLoader assets
)
{

    public IChronicleEvent Event { get; private set; } = null!;

    readonly ChronicleEventDialogDefinition def = new();

    internal void SetEvent(IChronicleEvent ev)
    {
        Event = def.Event = ev;
        SetLocalizedTitle(ev.NameLoc);
    }

    public ChronicleEventChoiceDialogBuilder SetLocalizedTitle(string loc) => SetTitle(t.T(loc));
    public ChronicleEventChoiceDialogBuilder SetTitle(string title)
    {
        def.Title = title;
        return this;
    }

    public ChronicleEventChoiceDialogBuilder SetTextContent() => SetTextContent(ChronicleEventUIHelper.GetDefaultContentLoc(Event.Id));
    public ChronicleEventChoiceDialogBuilder SetLocalizedTextContent(string loc) => SetTextContent(t.T(loc));

    public ChronicleEventChoiceDialogBuilder SetTextContent(string text)
    {
        def.Content.Clear();
        AddMainContent(helper.CreateFormattedText(text));
        return this;
    }

    public ChronicleEventChoiceDialogBuilder AddGoodsContent(GoodAmountsDefinition goodAmountsDefinition)
    {
        var el = helper.CreateGoodDisplay(goodAmountsDefinition);
        return AddMainContent(el);
    }

    public ChronicleEventChoiceDialogBuilder AddMainContent(VisualElement ve)
    {
        def.Content.Add(ve);
        return this;
    }

    public ChronicleEventChoiceDialogBuilder AddChoices(SimpleChoiceData[] choices)
    {
        foreach (var c in choices)
        {
            AddChoice(c.Text, c.Note, c.Disabled);
        }

        return this;
    }

    public ChronicleEventChoiceDialogBuilder ModifyChoices(Action<List<IEventDialogChoice>> choices)
    {
        choices(def.Choices);
        return this;
    }

    public ChronicleEventChoiceDialogBuilder ModifySimpleChoices(Action<IReadOnlyList<SimpleEventDialogChoice?>> choices)
    {
        choices([..def.Choices.Select(c => c as SimpleEventDialogChoice)]);
        return this;
    }

    public ChronicleEventChoiceDialogBuilder GetChoices(out IReadOnlyList<IEventDialogChoice> choices)
    {
        choices = def.Choices;
        return this;
    }

    public ChronicleEventChoiceDialogBuilder AddDismissChoice() => AddChoice(t.T("LV.BCEv.GenericOk"));

    public ChronicleEventChoiceDialogBuilder AddChoice(string text, string? note = null, bool disabled = false)
    {
        var c = new SimpleEventDialogChoice(text, helper, note)
        {
            Disabled = disabled,
        };

        return AddChoice(c);
    }

    public ChronicleEventChoiceDialogBuilder SetDefaultChoice(int index)
    {
        def.DefaultChoice = index;
        return this;
    }

    public ChronicleEventChoiceDialogBuilder AddCustomChoice(Action<VisualElement, Button> render)
        => AddChoice(new CustomEventDialogChoice(render));

    public ChronicleEventChoiceDialogBuilder EditChoice(int index, Action<IEventDialogChoice> editFn)
    {
        editFn(def.Choices[index]);
        return this;
    }

    public ChronicleEventChoiceDialogBuilder AddChoice(IEventDialogChoice choice)
    {
        def.Choices.Add(choice);
        return this;
    }

    public ChronicleEventChoiceDialogBuilder SetTopImage()
        => SetTopImage(helper.GetTopImage(def.Event));

    public ChronicleEventChoiceDialogBuilder SetTopImage(string path)
        => SetTopImage(assets.Load<Sprite>(path));

    public ChronicleEventChoiceDialogBuilder SetTopImage(Sprite sprite)
    {
        def.TopImage = sprite;
        return this;
    }

    public ChronicleEventChoiceDialogBuilder SetSideImage()
        => SetSideImage(helper.GetSideImage(def.Event));

    public ChronicleEventChoiceDialogBuilder SetSideImage(string path)
        => SetSideImage(assets.Load<Sprite>(path));

    public ChronicleEventChoiceDialogBuilder SetSideImage(Sprite sprite)
    {
        def.SideImage = sprite;
        return this;
    }

    public ChronicleEventDialogDefinition Build() => def;
}
