namespace Timberborn.Localization;

public static class ModdableTimberbornLocExtensions
{
    static readonly Phrase DayPhrase = Phrase.New().FormatDays<float>("F1");
    static readonly Phrase HourPhrase = Phrase.New().FormatHours<float>("F1");

    extension(ILoc t)
    {
        public string TDays(float time) => t.T(DayPhrase, time);
        public string THours(float time) => t.T(HourPhrase, time);
        public string TDaysOrHours(float days) => days >= 1f ? t.TDays(days) : t.THours(days * 24f);
        public string TBonus(string id) => t.T("Bonus." + id);
    }

    extension(BonusSpec spec)
    {
        public string T(ILoc t) => t.TBonus(spec.ToString());
    }

}
