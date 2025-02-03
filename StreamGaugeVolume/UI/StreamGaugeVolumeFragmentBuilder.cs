namespace StreamGaugeVolume.UI;

public class StreamGaugeVolumeFragmentBuilder : StreamGaugeVolumeFragmentBuilder<StreamGaugeVolumeFragmentBuilder>
{
    protected override StreamGaugeVolumeFragmentBuilder BuilderInstance => this;
}

public abstract class StreamGaugeVolumeFragmentBuilder<TBuilder> : BaseBuilder<TBuilder, NineSliceVisualElement> where TBuilder : BaseBuilder<TBuilder, NineSliceVisualElement>
{
    protected override NineSliceVisualElement InitializeRoot()
    {
        var fragmentBuilder = UIBuilder.Create<FragmentBuilder>();

        fragmentBuilder.AddComponent<GameTextLabel>("Volume");
        fragmentBuilder.AddComponent<GameTextLabel>("CellsCount");
        fragmentBuilder.AddComponent<GameButton>("MeasureVolume");

        return fragmentBuilder.BuildAndInitialize();
    }
}
