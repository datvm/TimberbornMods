namespace TimberLive.Models.Automations;

public class LeverAutomationBuilding : BaseClientAutomationBuilding<LeverSettingsModel>
{
    public override string SettingsName => BuildingSettingsMapper.Lever;
}

public class Relay : BaseClientAutomationBuilding<RelaySettingsModel>
{
    public override string SettingsName => BuildingSettingsMapper.Relay;
    public override IReadOnlyList<AutomationSlot> InputSlots { get; } = CreateSlots("A", "B");

    public override string? GetOperatorLabel(RelaySettingsModel settings)
        => settings.Mode.ToString().ToUpperInvariant();
}

public class FlowSensor : BaseClientAutomationBuilding<ComparisonSettingsModel>
{
    public override string SettingsName => BuildingSettingsMapper.FlowSensor;
}

public class DepthSensor : BaseClientAutomationBuilding<ComparisonSettingsModel>
{
    public override string SettingsName => BuildingSettingsMapper.DepthSensor;
}

public class ContaminationSensor : BaseClientAutomationBuilding<ComparisonSettingsModel>
{
    public override string SettingsName => BuildingSettingsMapper.ContaminationSensor;
}

public class Chronometer : BaseClientAutomationBuilding<ChronometerSettingsModel>
{
    public override string SettingsName => BuildingSettingsMapper.Chronometer;
}

public class WeatherStation : BaseClientAutomationBuilding<WeatherStationSettingsModel>
{
    public override string SettingsName => BuildingSettingsMapper.WeatherStation;
}

public class PowerMeter : BaseClientAutomationBuilding<PowerMeterSettingsModel>
{
    public override string SettingsName => BuildingSettingsMapper.PowerMeter;
}

public class PopulationCounter : BaseClientAutomationBuilding<PopulationCounterSettingsModel>
{
    public override string SettingsName => BuildingSettingsMapper.PopulationCounter;
}

public class ResourceCounter : BaseClientAutomationBuilding<ResourceCounterSettingsModel>
{
    public override string SettingsName => BuildingSettingsMapper.ResourceCounter;
}

public class ScienceCounter : BaseClientAutomationBuilding<ComparisonSettingsModel>
{
    public override string SettingsName => BuildingSettingsMapper.ScienceCounter;
}

public class Timer : BaseClientAutomationBuilding<TimerSettingsModel>
{
    public override string SettingsName => BuildingSettingsMapper.Timer;
    public override IReadOnlyList<AutomationSlot> InputSlots { get; } = CreateSlots("A", "RST");
}

public class Memory : BaseClientAutomationBuilding<MemorySettingsModel>
{
    public override string SettingsName => BuildingSettingsMapper.Memory;
    public override IReadOnlyList<AutomationSlot> InputSlots { get; } = CreateSlots("A", "B", "RST");
}

public class HttpLever : BaseClientAutomationBuilding<object>
{
    public override string SettingsName => BuildingSettingsMapper.HttpLever;
}

public class HttpAdapter : BaseClientAutomationBuilding<HttpAdapterSettingsModel>
{
    public override string SettingsName => BuildingSettingsMapper.HttpAdapter;
}