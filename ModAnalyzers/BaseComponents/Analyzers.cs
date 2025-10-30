namespace ModAnalyzers.BaseComponents;

[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
public class AwakeInterfaceAnalyzer : MethodRequiresInterfaceAnalyzerBase
{
    protected override string DiagnosticId { get; } = "BASECOMPONENT001";
    protected override string RequiredInterfaceName { get; } = "IAwakableComponent";
    protected override string MethodName { get; } = "Awake";
}

[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
public class StartInterfaceAnalyzer : MethodRequiresInterfaceAnalyzerBase
{
    protected override string DiagnosticId { get; } = "BASECOMPONENT002";
    protected override string RequiredInterfaceName { get; } = "IStartableComponent";
    protected override string MethodName { get; } = "Start";
}

[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
public class UpdateInterfaceAnalyzer : MethodRequiresInterfaceAnalyzerBase
{
    protected override string DiagnosticId { get; } = "BASECOMPONENT003";
    protected override string RequiredInterfaceName { get; } = "IUpdatableComponent";
    protected override string MethodName { get; } = "Update";
}

[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
public class LateUpdateInterfaceAnalyzer : MethodRequiresInterfaceAnalyzerBase
{
    protected override string DiagnosticId { get; } = "BASECOMPONENT004";
    protected override string RequiredInterfaceName { get; } = "ILateUpdatableComponent";
    protected override string MethodName { get; } = "LateUpdate";
}
