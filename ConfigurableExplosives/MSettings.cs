namespace ConfigurableExplosives;

public class MSettings(
    ISettings settings,
    ModSettingsOwnerRegistry modSettingsOwnerRegistry,
    ModRepository modRepository,
    ILoc t
) : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository)
{
    public static readonly ImmutableArray<int> DefaultDepths = [1, 2, 3];
    static readonly ImmutableArray<string> DynamiteNames = ["Building.Dynamite.DisplayName", "Building.DoubleDynamite.DisplayName", "Building.TripleDynamite.DisplayName"];

    public static int[] MaxDepths { get; } = [.. DefaultDepths];
    public static bool NoCostIncrease { get; private set; }

    const string MaxDepthKey = "DynamiteMaxDepth";

    public override string ModId { get; } = nameof(ConfigurableExplosives);

    public override void OnAfterLoad()
    {
        for (int i = 0; i < DefaultDepths.Length; i++)
        {
            var z = i;

            RangeIntModSetting maxDepth = new(DefaultDepths[z], 1, 30,
                ModSettingDescriptor.Create(t.T("LV.CE.MaxDepth", t.T(DynamiteNames[z]))));

            AddCustomModSetting(maxDepth, MaxDepthKey + z);

            maxDepth.ValueChanged += (_, v) => MaxDepths[z] = v;
            MaxDepths[z] = maxDepth.Value;
        }

        ModSetting<bool> noCost = new(false, 
            ModSettingDescriptor.CreateLocalized("LV.CE.NoCostIncrease")
                .SetLocalizedTooltip("LV.CE.NoCostIncreaseDesc"));
        AddCustomModSetting(noCost, "NoCostIncrease");

        noCost.ValueChanged += (_, v) => NoCostIncrease = v;
        NoCostIncrease = noCost.Value;
    }

}
