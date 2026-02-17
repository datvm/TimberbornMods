namespace Timberborn.BlueprintSystem;

public static class CommonTimberUiExtensions
{

    extension<T>(T spec) where T : ComponentSpec
    {

        public string GetTemplateName() => spec.GetSpec<TemplateSpec>().TemplateName;
        public string GetName(ILoc t) => t.T(spec.GetSpec<LabeledEntitySpec>().DisplayNameLocKey);

    }

}
