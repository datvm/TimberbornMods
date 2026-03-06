namespace ModdableTimberborn.BuildingSettings.BuiltInSettings;

public record HttpAdapterSettingsModel(
    bool SwitchedOnWebhookEnabled,
    bool SwitchedOffWebhookEnabled,
    string SwitchedOnWebhookUrl,
    string SwitchedOffWebhookUrl,
    HttpWebhookMethod Method
);

public class HttpAdapterSettings(ILoc t) : BuildingSettingsBase<HttpAdapter, HttpAdapterSettingsModel>(t)
{
    public override string DescribeModel(HttpAdapterSettingsModel model) => "";

    protected override bool ApplyModel(HttpAdapterSettingsModel model, HttpAdapter target)
    {
        target.SwitchedOnWebhookEnabled = model.SwitchedOnWebhookEnabled;
        target.SwitchedOffWebhookEnabled = model.SwitchedOffWebhookEnabled;
        target.SwitchedOnWebhookUrl = model.SwitchedOnWebhookUrl;
        target.SwitchedOffWebhookUrl = model.SwitchedOffWebhookUrl;
        target.Method = model.Method;
        return true;
    }

    protected override HttpAdapterSettingsModel GetModel(HttpAdapter target)
        => new(target.SwitchedOnWebhookEnabled, target.SwitchedOffWebhookEnabled,
            target.SwitchedOnWebhookUrl, target.SwitchedOffWebhookUrl,
            target.Method);
}