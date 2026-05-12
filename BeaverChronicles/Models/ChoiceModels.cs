namespace BeaverChronicles.Models;

public readonly record struct SimpleChoiceData(string Text, string? Note = null, bool Disabled = false)
{

    public static SimpleChoiceData[] Create(int count, string id, ILoc t)
        => Create(count, id, t, noteTransformFn: null, null);

    public static SimpleChoiceData[] CreateNoNote(int count, string id, ILoc t)
        => Create(count, id, t, noteFn: null, null);

    public static SimpleChoiceData[] Create(int count, string id, ILoc t, Func<int, string, string?>? noteTransformFn)
        => Create(count, id, t, noteTransformFn, null);

    public static SimpleChoiceData[] Create(int count, string id, ILoc t, Func<int, string?>? noteFn)
        => Create(count, id, t, noteFn, null);

    public static SimpleChoiceData[] Create(int count, string id, ILoc t, Func<int, string?>? noteFn, Func<int, bool>? disabledFunc)
    {
        var result = new SimpleChoiceData[count];

        for (int i = 0; i < count; i++)
        {
            result[i] = new(t.TEventChoice(id, i), noteFn?.Invoke(i), disabledFunc is not null && disabledFunc(i));
        }

        return result;
    }

    public static SimpleChoiceData[] Create(int count, string id, ILoc t, Func<int, string, string?>? noteTransformFn, Func<int, bool>? disabledFunc)
        => Create(count, id, t, i =>
        {
            var note = t.TEventChoiceNote(id, i);
            return noteTransformFn?.Invoke(i, note) ?? note;
        }, disabledFunc);

    public void Record(EventHistoryPage page)
    {
        page.AddContent(Text.CenterMixed());
        if (Note is not null)
        {
            page.AddContent(Note);
        }
    }

}
