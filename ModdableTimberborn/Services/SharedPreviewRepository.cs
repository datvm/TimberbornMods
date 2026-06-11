namespace ModdableTimberborn.Services;

[BindSingleton(Contexts = BindAttributeContext.NonMenu)]
public class SharedPreviewRepository(
    PreviewFactory previewFactory,
    PreviewShower previewShower,
    TemplateNameMapper templateNameMapper
)
{
    readonly Dictionary<PlaceableBlockObjectSpec, List<PreviewWithInfo>> previews = [];
    readonly Dictionary<PlaceableBlockObjectSpec, string> templateNameBySpec = [];

    public bool TryGetPlaceableSpec(string templateName, [NotNullWhen(true)] out PlaceableBlockObjectSpec? spec, bool throwOnInvalidFoundTemplate = false)
    {
        if (templateNameMapper.TryGetTemplate(templateName, out var template))
        {
            spec = template.GetSpec<PlaceableBlockObjectSpec>();

            if (spec is null)
            {
                if (throwOnInvalidFoundTemplate)
                {
                    throw new InvalidOperationException($"Template '{templateName}' is not a placeable block object.");
                }

                return false;
            }

            templateNameBySpec[spec] = templateName;
            return true;
        }

        spec = null;
        return false;
    }

    public PreviewLease RequestPreview(PlaceableBlockObjectSpec template)
        => RequestPreviews(template, 1)[0];

    public IReadOnlyList<PreviewLease> RequestPreviews(PlaceableBlockObjectSpec template, int count)
    {
        var list = previews.GetOrAdd(template);

        if (list.Count < count)
        {
            FillInPreviews(template, list, count);
        }

        var returning = list.Take(count).Select(p => new PreviewLease(p, this)).ToArray();
        list.RemoveRange(0, count);
        return returning;
    }

    void FillInPreviews(PlaceableBlockObjectSpec template, List<PreviewWithInfo> list, int totalRequired)
    {
        var templateName = templateNameBySpec.GetOrAdd(template, () => template.GetTemplateName());

        while (list.Count < totalRequired)
        {
            var preview = previewFactory.Create(template);
            list.Add(new(preview, template, templateName, preview.GetComponent<BlockObject>()));
        }
    }

    internal void OnPreviewDisposed(PreviewLease entry)
    {
        previewShower.HidePreviews([entry.Preview]);
        previews[entry.Spec].Add(entry.Info);
    }

}

public sealed class PreviewLease : IDisposable
{
    readonly SharedPreviewRepository repo;

    public Preview Preview => !Disposed ? field : throw new ObjectDisposedException(nameof(PreviewLease));
    public string TemplateName { get; }
    public PlaceableBlockObjectSpec Spec { get; }
    public BlockObject BlockObject { get; }
    public bool Disposed { get; private set; }
    internal PreviewWithInfo Info { get; }

    internal PreviewLease(PreviewWithInfo info, SharedPreviewRepository repo)
    {
        this.repo = repo;
        Preview = info.Preview;
        Spec = info.Spec;
        TemplateName = info.TemplateName;
        BlockObject = info.BlockObject;
        Info = info;
    }

    public void Dispose()
    {
        if (Disposed) { return; }
        Disposed = true;

        repo.OnPreviewDisposed(this);
    }

}

readonly record struct PreviewWithInfo(Preview Preview, PlaceableBlockObjectSpec Spec, string TemplateName, BlockObject BlockObject);
