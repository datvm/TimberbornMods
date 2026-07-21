namespace BuildingRenovations;

public static class RenovationHelpers
{
    static readonly Phrase DayPhrase = Phrase.New().FormatDays<float>("F1");

    extension(BaseComponent comp)
    {
        public BuildingRenovationComponent GetRenovationComponent() => comp.GetComponent<BuildingRenovationComponent>();
        public WorkplaceBonusComponent GetWorkplaceBonusComponent() => comp.GetComponent<WorkplaceBonusComponent>();

        public bool TemplateStartsWith(IReadOnlyList<string> prefixes)
            => comp.GetTemplateName().StartsWith(prefixes);
    }

    extension(string str)
    {
        public bool StartsWith(IReadOnlyList<string> prefixes)
            => prefixes.Any(prefix => str.StartsWith(prefix));
    }

    extension(ILoc t)
    {

        public string TBonus(string id) => t.T("Bonus." + id);

        public string TWorkplaceWorkerBonus(IReadOnlyList<BonusSpec> bonuses)
        {
            var bonusText = string.Join(", ", bonuses.Select(b =>
                $"{b.MultiplierDelta:+0%;-0%;0%} {t.TBonus(b.Id)}"));
            return t.T("LV.BRe.Common.WorkerBonus", bonusText);
        }

        public string TDays(float days) => t.T(DayPhrase, days);
    }

}
