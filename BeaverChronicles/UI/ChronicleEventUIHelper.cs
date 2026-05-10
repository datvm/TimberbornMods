namespace BeaverChronicles.UI;

[BindSingleton]
public class ChronicleEventUIHelper(
    IContainer container,
    IGoodService goodService,
    IAssetLoader assets,
    MixedTextService mixedTextService
)
{
    public const string RecommendedImagePath = "Resources/Chronicles/";

    public static string GetDefaultContentLoc(string eventId) => $"LV.BCEv.{eventId}.Content";
    public static string GetTopImagePath(string id) => RecommendedImagePath + id + "_Top";
    public static string GetSideImagePath(string id) => RecommendedImagePath + id + "_Side";

    public async Task<int> ShowEventDialogAsync(IChronicleEvent ev, Action<ChronicleEventChoiceDialogBuilder> buildFn)
    {
        var builder = container.GetInstance<ChronicleEventChoiceDialogBuilder>();
        builder.SetEvent(ev);
        buildFn(builder);
        return await ShowEventDialogAsync(builder.Build());
    }

    public async Task<int> ShowEventDialogAsync(ChronicleEventDialogDefinition def)
    {
        var diag = container.GetInstance<ChronicleEventChoiceDialog>();
        return await diag.ShowAsync(def);
    }

    public void ShowChronicleDialog() => container.GetInstance<ChronicleEventsDialog>().Show();

    public Sprite GetTopImage(IChronicleEvent ev) => assets.Load<Sprite>(GetTopImagePath(ev.Id));
    public Sprite GetSideImage(IChronicleEvent ev) => assets.Load<Sprite>(GetSideImagePath(ev.Id));

    public VisualElement CreateGoodDisplay(GoodAmountsDefinition def)
    {
        var ve = new VisualElement().SetAsRow().AlignItems().JustifyContent();

        foreach (var (g, highlight) in def.Goods)
        {
            var amount = g.Amount.ToString();
            switch (highlight)
            {
                case GoodModifier.Positive:
                    amount = "+" + amount;
                    break;
                case GoodModifier.Negative:
                    amount = "-" + amount;
                    break;
            }

            var span = ve.AddIconSpan().SetGood(goodService, g.GoodId, amount, true);
            span.HighlightAll(highlight);
        }

        return ve;
    }

    public MixedTextElement CreateFormattedText(string content)
    {
        MixedTextElement el = new(mixedTextService);
        el.SetText(content);

        return el;
    }

}
