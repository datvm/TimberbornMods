namespace BuildingRenovations;

public static class RenovationHelpers
{
    public const string ScienceId = "Science";

    public static void LogVerbose(Func<string> msg) => TimberUiUtils.LogVerbose(() => $"[{nameof(BuildingRenovations)}] {msg()}");

    extension(BaseComponent comp)
    {
        public BuildingRenovationComponent GetRenovationComponent() => comp.GetComponent<BuildingRenovationComponent>();

        public bool TemplateStartsWith(IReadOnlyList<string> prefixes)
            => comp.GetTemplateName().StartsWith(prefixes);
    }

    extension(IEnumerable<string> strs)
    {
        public bool AnyStartsWith(IReadOnlyList<string> prefixes)
            => strs.Any(str => str.StartsWith(prefixes));
    }

    extension(string str)
    {
        public bool StartsWith(IReadOnlyList<string> prefixes)
            => prefixes.Any(prefix => str.StartsWith(prefix));
    }

    extension(ILoc t)
    {

        public string TWorkplaceWorkerBonus(string bonus) => t.T("LV.BRe.Common.WorkerBonus", bonus);

        public string TWorkplaceWorkerBonus(IReadOnlyList<BonusSpec> bonuses)
        {
            var bonusText = string.Join(", ", bonuses.Select(b =>
                $"{b.MultiplierDelta:+0%;-0%;0%} {t.TBonus(b.Id)}"));

            return t.TWorkplaceWorkerBonus(bonusText);
        }
    }

}
