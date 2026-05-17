namespace MoreHttpApi.Shared.BuildingSettings;

public record HttpAdapterSettingsModel(
    bool SwitchedOnWebhookEnabled,
    bool SwitchedOffWebhookEnabled,
    string SwitchedOnWebhookUrl,
    string SwitchedOffWebhookUrl,
    HHttpWebhookMethod Method
);

public enum HHttpWebhookMethod
{
    Get,
    Post
}
