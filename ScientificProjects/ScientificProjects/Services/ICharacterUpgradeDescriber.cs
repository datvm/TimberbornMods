namespace ScientificProjects.Services;

public interface ICharacterUpgradeDescriber : IEntityUpgradeDescriber<CharacterProjectUpgradeComponent>
{

    public static EntityEffectDescription? DescribeProjects(
        CharacterProjectUpgradeComponent comp, DescribeEffectsParameters parameters,
        IEnumerable<string> ids, int parameterIndex,
        string titleKey, string descKey,
        string? bonusId = null,
        bool percent = false
    )
    {
        if (bonusId is not null && !comp.HasBonus(bonusId)) { return null; }
        var t = parameters.T;

        var total = 0f;
        var sub = new List<string>();
        foreach (var id in ids)
        {
            if (parameters.ActiveProjects.TryGetValue(id, out var info))
            {
                var eff = info.GetEffect(parameterIndex);
                total += eff;
                sub.Add(info.Spec.DescribeEffect(t, eff, percent));
            }
        }

        return total == 0f ? null : new(
            t.T(titleKey, total),
            t.T(descKey, total) + Environment.NewLine + string.Join(Environment.NewLine, sub)
        );
    }

}
