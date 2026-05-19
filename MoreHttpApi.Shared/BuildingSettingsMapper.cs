namespace MoreHttpApi.Shared;

public static class BuildingSettingsMapper
{
    public const string TimedComponentActivator = "Timberborn.ActivatorSystem.TimedComponentActivator";
    public const string Automatable = "Timberborn.Automation.Automatable";
    public const string Chronometer = "Timberborn.AutomationBuildings.Chronometer";
    public const string ContaminationSensor = "Timberborn.AutomationBuildings.ContaminationSensor";
    public const string DepthSensor = "Timberborn.AutomationBuildings.DepthSensor";
    public const string FlowSensor = "Timberborn.AutomationBuildings.FlowSensor";
    public const string Gate = "Timberborn.AutomationBuildings.Gate";
    public const string Indicator = "Timberborn.AutomationBuildings.Indicator";
    public const string Lever = "Timberborn.AutomationBuildings.Lever";
    public const string Memory = "Timberborn.AutomationBuildings.Memory";
    public const string PopulationCounter = "Timberborn.AutomationBuildings.PopulationCounter";
    public const string PowerMeter = "Timberborn.AutomationBuildings.PowerMeter";
    public const string Relay = "Timberborn.AutomationBuildings.Relay";
    public const string ResourceCounter = "Timberborn.AutomationBuildings.ResourceCounter";
    public const string ScienceCounter = "Timberborn.AutomationBuildings.ScienceCounter";
    public const string Speaker = "Timberborn.AutomationBuildings.Speaker";
    public const string Timer = "Timberborn.AutomationBuildings.Timer";
    public const string WeatherStation = "Timberborn.AutomationBuildings.WeatherStation";
    public const string BuilderPrioritizable = "Timberborn.BuilderPrioritySystem.BuilderPrioritizable";
    public const string PausableBuilding = "Timberborn.Buildings.PausableBuilding";
    public const string DecalSupplier = "Timberborn.DecalSystem.DecalSupplier";
    public const string FlippableDecal = "Timberborn.DecalSystem.FlippableDecal";
    public const string Demolishable = "Timberborn.Demolishing.Demolishable";
    public const string UnstableCore = "Timberborn.Explosions.UnstableCore";
    public const string FarmHouse = "Timberborn.Fields.FarmHouse";
    public const string FireworkLauncher = "Timberborn.FireworkSystem.FireworkLauncher";
    public const string Forester = "Timberborn.Forestry.Forester";
    public const string GatherablePrioritizer = "Timberborn.Gathering.GatherablePrioritizer";
    public const string HaulPrioritizable = "Timberborn.Hauling.HaulPrioritizable";
    public const string HttpLever = "Timberborn.HttpApiSystem.HttpLever";
    public const string HttpAdapter = "Timberborn.HttpApiSystem.HttpAdapter";
    public const string CustomizableIlluminator = "Timberborn.Illumination.CustomizableIlluminator";
    public const string SingleGoodAllower = "Timberborn.InventorySystem.SingleGoodAllower";
    public const string FixedStockpileInventorySetter = "Timberborn.MapEditorStockpilesUI.FixedStockpileInventorySetter";
    public const string PlantablePrioritizer = "Timberborn.Planting.PlantablePrioritizer";
    public const string Clutch = "Timberborn.PowerManagement.Clutch";
    public const string StockpilePriority = "Timberborn.StockpilePrioritySystem.StockpilePriority";
    public const string Floodgate = "Timberborn.WaterBuildings.Floodgate";
    public const string Sluice = "Timberborn.WaterBuildings.Sluice";
    public const string Valve = "Timberborn.WaterBuildings.Valve";
    public const string WaterInputCoordinates = "Timberborn.WaterBuildings.WaterInputCoordinates";
    public const string WaterMover = "Timberborn.WaterBuildings.WaterMover";
    public const string WaterSource = "Timberborn.WaterSourceSystem.WaterSource";
    public const string WaterSourceRegulator = "Timberborn.WaterSourceSystem.WaterSourceRegulator";
    public const string Manufactory = "Timberborn.Workshops.Manufactory";
    public const string DistrictDefaultWorkerType = "Timberborn.WorkSystem.DistrictDefaultWorkerType";
    public const string Workplace = "Timberborn.WorkSystem.Workplace";
    public const string WorkplacePriority = "Timberborn.WorkSystem.WorkplacePriority";
    public const string WorkplaceWorkerType = "Timberborn.WorkSystem.WorkplaceWorkerType";

    public static readonly IReadOnlyDictionary<string, Type> BuildingSettingsTypes = new Dictionary<string, Type>
    {
         { TimedComponentActivator, typeof(TimedComponentActivatorSettingsModel) },
         { Automatable, typeof(AutomatableSettingsModel) },
         { Chronometer, typeof(ChronometerSettingsModel) },
         { ContaminationSensor, typeof(ComparisonSettingsModel) },
         { DepthSensor, typeof(ComparisonSettingsModel) },
         { FlowSensor, typeof(ComparisonSettingsModel) },
         { Gate, typeof(ValueSettingModel<HttpGateOpeningMode>) },
         { Indicator, typeof(IndicatorSettingsModel) },
         { Lever, typeof(LeverSettingsModel) },
         { Memory, typeof(MemorySettingsModel) },
         { PopulationCounter, typeof(PopulationCounterSettingsModel) },
         { PowerMeter, typeof(PowerMeterSettingsModel) },
         { Relay, typeof(RelaySettingsModel) },
         { ResourceCounter, typeof(ResourceCounterSettingsModel) },
         { ScienceCounter, typeof(ComparisonSettingsModel) },
         { Speaker, typeof(SpeakerSettingsModel) },
         { Timer, typeof(TimerSettingsModel) },
         { WeatherStation, typeof(WeatherStationSettingsModel) },
         { BuilderPrioritizable, typeof(PrioritySettingModel) },
         { PausableBuilding, typeof(BoolSettingModel) },
         { DecalSupplier, typeof(DecalSupplierSettingsModel) },
         { FlippableDecal, typeof(BoolSettingModel) },
         { Demolishable, typeof(BoolSettingModel) },
         { UnstableCore, typeof(IntSettingModel) },
         { FarmHouse, typeof(BoolSettingModel) },
         { FireworkLauncher, typeof(FireworkLauncherSettingsModel) },
         { Forester, typeof(BoolSettingModel) },
         { GatherablePrioritizer, typeof(StringSettingModel) },
         { HaulPrioritizable, typeof(BoolSettingModel) },
         { HttpLever, typeof(object) },
         { HttpAdapter, typeof(HttpAdapterSettingsModel) },
         { CustomizableIlluminator, typeof(CustomizableIlluminatorSettingsModel) },
         { SingleGoodAllower, typeof(StringSettingModel) },
         { FixedStockpileInventorySetter, typeof(FixedStockpileInventorySetterSettingsModel) },
         { PlantablePrioritizer, typeof(StringSettingModel) },
         { Clutch, typeof(ClutchSettingsModel) },
         { StockpilePriority, typeof(ValueSettingModel<HttpStockpilePriorityState>) },
         { Floodgate, typeof(FloodgateSettingsModel) },
         { Sluice, typeof(SluiceSettingsModel) },
         { Valve, typeof(ValveSettingsModel) },
         { WaterInputCoordinates, typeof(WaterInputCoordinatesSettingsModel) },
         { WaterMover, typeof(WaterMoverSettingsModel) },
         { WaterSource, typeof(FloatSettingModel) },
         { WaterSourceRegulator, typeof(WaterSourceRegulatorSettingsModel) },
         { Manufactory, typeof(StringSettingModel) },
         { DistrictDefaultWorkerType, typeof(StringSettingModel) },
         { Workplace, typeof(IntSettingModel) },
         { WorkplacePriority, typeof(PrioritySettingModel) },
         { WorkplaceWorkerType, typeof(StringSettingModel) },
    };

    public static readonly HashSet<string> AutomationTypes = [.. BuildingSettingsTypes.Keys
        .Where(k => k.StartsWith("Timberborn.AutomationBuildings", StringComparison.InvariantCulture))];
}