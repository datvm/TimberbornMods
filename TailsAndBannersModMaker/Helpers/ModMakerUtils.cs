namespace TailsAndBannersModMaker.Helpers;

public static class ModMakerUtils
{

    extension<T>(T el) where T: VisualElement
    {

        public T SetBorder() => el.SetBorder(TextColors.YellowHighlight, 1).SetPadding(10);

        public TextField AddPair(string labelLoc, string defaultValue, Action<string>? onTextChanged, ILoc t)
        {
            var pnl = el.AddChild().SetMarginBottom(10);
            pnl.AddLabel(t.T(labelLoc));

            var txt = pnl.AddTextField(changeCallback: onTextChanged);
            txt.SetValueWithoutNotify(defaultValue);
            return txt;
        }
    }

    extension(ILoc t)
    {
        public string TDecal(string type) => t.T("LV.TBMM.DecalType." + type);
    }

}
