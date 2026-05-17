namespace MoreHttpApi.Shared.BuildingSettings;

public record CustomizableIlluminatorSettingsModel(bool IsCustomized, HttpSerializableFloats? CustomColor);