namespace TimberUi.CommonUi;

[UxmlElement]
public class NineSliceFloatField : FloatField
{

    [Serializable]
    [CompilerGenerated]
    internal new class UxmlSerializedData : TextField.UxmlSerializedData
    {
        public override object CreateInstance()
        {
            return new NineSliceFloatField();
        }
    }

    readonly NineSliceBackground _nineSliceBackground = new NineSliceBackground();


    public NineSliceFloatField()
    {
        generateVisualContent = (Action<MeshGenerationContext>)Delegate.Combine(
            generateVisualContent,
            new Action<MeshGenerationContext>(OnGenerateVisualContent));
        RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
    }

    new void OnCustomStyleResolved(CustomStyleResolvedEvent e)
    {
        _nineSliceBackground.GetDataFromStyle(base.customStyle);
        MarkDirtyRepaint();
    }

    void OnGenerateVisualContent(MeshGenerationContext mgc)
    {
        _nineSliceBackground.GenerateVisualContent(mgc, base.paddingRect);
    }
}
