namespace Mvp.Core.Features.Generation;

/// <summary>
/// Resolves the manifest-resource prefix the packaged templates are addressed by.
/// </summary>
/// <remarks>
/// Embedded resource names derive from the project's RootNamespace, which Mvp.Core.csproj pins
/// equal to AssemblyName. Deriving the prefix here rather than repeating a literal in each
/// generator means the two cannot drift apart — a drift that compiles cleanly and fails only at
/// run time. <c>TemplateResourceTests</c> asserts the derived value matches the shipped resources.
/// </remarks>
internal static class TemplateResources
{
    internal static readonly string Root = typeof(TemplateResources).Assembly.GetName().Name + ".Templates.";

    internal static readonly string FullStack = Root + "FullStack.";

    internal static readonly string Incremental = Root + "Incremental.";
}
