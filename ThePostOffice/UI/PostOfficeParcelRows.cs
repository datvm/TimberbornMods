namespace ThePostOffice.UI;

static class PostOfficeParcelRows
{
    public static string Signature(IReadOnlyList<PostOfficeParcel> parcels)
    {
        List<string> parts = [];
        foreach (var parcel in parcels)
        {
            var body = string.Join(",", parcel.Goods.Select(good => $"{good.GoodId}:{good.Amount}"));
            parts.Add(parcel.Auto ? $"A{body}" : body);
        }

        return string.Join(";", parts);
    }

    public static void AddGoods(VisualElement parent, IGoodService goods, PostOfficeParcel parcel)
    {
        foreach (var good in parcel.Goods)
        {
            if (!goods.HasGood(good.GoodId))
            {
                continue;
            }

            var row = parent.AddRow().AlignItems().SetMarginBottom(2);
            row.AddIconSpan()
                .SetGood(goods, good.GoodId, $"{good.Amount}×", showName: true)
                .SetFlexGrow(0)
                .SetFlexShrink(0);
        }
    }
}
