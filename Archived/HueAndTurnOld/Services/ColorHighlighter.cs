namespace HueAndTurn.Services;

public class ColorHighlighter(IInstantiator instantiator) : ILoadableSingleton
{

    Highlighter localHighlighter = null!;

    public void Load()
    {
        localHighlighter = new(instantiator);
    }

    public void SetColor(BaseComponent comp, in Color color)
    {
        // The actual color needs to be scaled with alpha because alpha is not used
        var c = color * color.a;
        localHighlighter.HighlightPrimary(comp, c);
    }

    public void ResetColor(BaseComponent comp)
    {
        localHighlighter.UnhighlightPrimary(comp);
    }

}
