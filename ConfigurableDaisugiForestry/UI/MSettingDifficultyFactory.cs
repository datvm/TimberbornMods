namespace ConfigurableDaisugiForestry.UI;

public class MSettingDifficultyFactory(IContainer container) : IModSettingElementFactory
{
    public int Priority { get; }

    public bool TryCreateElement(ModSetting modSetting, out IModSettingElement? element)
    {
        if (modSetting is not MSettingDifficultySetting)
        {
            element = null;
            return false;
        }

        element = new ModSettingElement(container.GetInstance<MSettingDifficultyElement>(), modSetting);
        return true;
    }

}

public class MSettingDifficultyElement : VisualElement
{

    public MSettingDifficultyElement(ILoc t, MSettings settings)
    {
        this.AddLabel(t.T("LV.DF.ChangeSettingsNote")).SetMarginBottom(5);

        var row = this.AddRow();
        AddButton("Easy", MSettingTemplate.Easy);
        AddButton("Normal", MSettingTemplate.Normal);
        AddButton("Hard", MSettingTemplate.Hard);

        void AddButton(string locKey, MSettingTemplate template)
        {
            var mnu = row.AddMenuButton(t.T("NewGameConfigurationPanel." + locKey),
                onClick: () => OnDifficultyClicked(template, settings), stretched: false)
                .SetFlexShrink(1)
                .SetFlexGrow(1);
            mnu.style.minWidth = 0;
        }
    }


    void OnDifficultyClicked(MSettingTemplate template, MSettings settings)
    {
        var (birch, oak) = template;
        settings.BirchDays.SetValue(birch.Days);
        settings.BirchLogs.SetValue(birch.Logs);
        settings.BirchHarvest.SetValue(birch.HarvestHours);
        settings.BirchPlank.SetValue(birch.IsPlank);

        settings.OakDays.SetValue(oak.Days);
        settings.OakLogs.SetValue(oak.Logs);
        settings.OakHarvest.SetValue(oak.HarvestHours);
        settings.OakPlank.SetValue(oak.IsPlank);
    }

}
