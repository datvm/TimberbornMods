namespace ModAnalyzers.BaseComponents;

public abstract class MethodRequiresInterfaceAnalyzerBase : DiagnosticAnalyzer
{
    protected abstract string DiagnosticId { get; }

    // Name of the interface that must be implemented (simple name, e.g., "IAwakableComponent")
    protected abstract string RequiredInterfaceName { get; }
    protected abstract string MethodName { get; }

    // Return true for signatures you consider relevant
    protected virtual bool MatchSignature(IMethodSymbol m) => m.Parameters.Length == 0;

    // Allow opt-out via an attribute on the class, e.g. [SkipAwakeCheck]
    protected virtual string? OptOutAttributeMetadataName => null; // e.g. "global::Your.Analyzers.SkipAwakeCheckAttribute"

    DiagnosticDescriptor? _rule;
    protected DiagnosticDescriptor Rule =>
        _rule ??= new DiagnosticDescriptor(
            id: DiagnosticId,
            title: $"Class with {MethodName}() must implement {RequiredInterfaceName}",
            messageFormat: "Class '{0}' declares {1} but does not implement {2}",
            category: "BaseComponent",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
    }

    void AnalyzeNamedType(SymbolAnalysisContext ctx)
    {
        if (ctx.Symbol is not INamedTypeSymbol type || type.TypeKind != TypeKind.Class || type.IsAbstract || HasOptOut(type))
        {
            return;
        }

        // Find any matching method
        IMethodSymbol? hit = null;
        foreach (var m in type.GetMembers().OfType<IMethodSymbol>())
        {
            if (m.MethodKind == MethodKind.Ordinary && MethodName == m.Name && MatchSignature(m))
            {
                hit = m;
                break;
            }
        }
        if (hit is null)
        {
            return;
        }

        // Check interface by simple name to avoid referencing external assemblies from the analyzer
        if (ImplementsInterfaceByName(type, RequiredInterfaceName))
        {
            return;
        }

        var props = ImmutableDictionary<string, string?>.Empty
            .Add("iface", RequiredInterfaceName);

        var location = type.Locations.FirstOrDefault() ?? Location.None;
        ctx.ReportDiagnostic(Diagnostic.Create(Rule, location, props, type.Name, hit.Name, RequiredInterfaceName));
    }

    bool HasOptOut(INamedTypeSymbol type)
    {
        if (OptOutAttributeMetadataName is null)
        {
            return false;
        }

        foreach (var a in type.GetAttributes())
        {
            if (a.AttributeClass?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == OptOutAttributeMetadataName)
            {
                return true;
            }
        }
        return false;
    }

    static bool ImplementsInterfaceByName(INamedTypeSymbol type, string requiredInterfaceSimpleName)
        => type.AllInterfaces.Any(i => string.Equals(i.Name, requiredInterfaceSimpleName, StringComparison.Ordinal));
}
