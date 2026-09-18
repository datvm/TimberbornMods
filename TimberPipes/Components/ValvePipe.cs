namespace TimberPipes.Components;

[AddTemplateModule2(typeof(ValvePipeSpec))]
public class ValvePipe : BaseComponent, IFinishedPausable, IAwakableComponent, IDuplicable<ValvePipe>
{
#nullable disable
    BuildingPipe pipe;
#nullable enable

    ExtractionPipe? extraction;

    public BuildingPipe Pipe => pipe;
    public ExtractionPipe? Extraction => extraction;

    public void Awake()
    {
        pipe = GetComponent<BuildingPipe>();
        extraction = this.GetComponentOrNull<ExtractionPipe>();
    }

    public void DuplicateFrom(ValvePipe source)
    {
        if (extraction is not { } dest || source.Extraction is not { } from)
        {
            return;
        }

        dest.CopySettings(from.InletEnabled, from.OutletEnabled, from.OutletGoodId);
    }
}
