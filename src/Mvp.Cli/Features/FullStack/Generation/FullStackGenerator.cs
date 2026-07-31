using System.Globalization;
using System.Reflection;
using DotLiquid;
using Mvp.Cli.Bootstrap;
using Mvp.Cli.Features.FullStack.Manifest;
using Mvp.Cli.Features.Generation;
using Mvp.Cli.Infrastructure.Output;

namespace Mvp.Cli.Features.FullStack.Generation;

public sealed class FullStackGenerator(ITransactionalGenerationOutput output) : IFullStackGenerator
{
    private const string ResourcePrefix = "Mvp.Cli.Templates.FullStack.";
    private const string ManifestResource = ResourcePrefix + "manifest.txt";

    public Task<GenerationResult> GenerateAsync(
        ValidatedManifest manifest,
        bool force,
        CancellationToken cancellationToken)
    {
        var target = Path.Combine(manifest.OutputRoot, manifest.Name);
        return output.CommitAsync(
            target,
            force,
            (stage, token) => RenderAsync(stage, manifest, token),
            cancellationToken);
    }

    private static async Task RenderAsync(
        string root,
        ValidatedManifest manifest,
        CancellationToken cancellationToken)
    {
        var assembly = typeof(FullStackGenerator).Assembly;
        var entries = ReadManifest(assembly);
        if (entries.Count == 0)
        {
            throw new GenerationException("The packaged full-stack template manifest is missing or empty.");
        }

        var rootTokens = BuildRootTokens(manifest);
        foreach (var entry in entries)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var content = ReadResource(assembly, entry.ResourceName);
            if (entry.RelativePath.StartsWith("_entity/", StringComparison.Ordinal))
            {
                foreach (var entity in manifest.Entities)
                {
                    var tokens = BuildRootTokens(manifest);
                    tokens["Entity"] = entity.Name;
                    tokens["EntityLower"] = entity.Name.ToLowerInvariant();
                    tokens["EntityKebab"] = Naming.ToKebabCase(entity.Name);
                    tokens["EntityProperties"] = entity.Properties.Select(ToPropertyTokens).Cast<object>().ToList();
                    await RenderEntryAsync(root, entry.RelativePath["_entity/".Length..], content, tokens, cancellationToken)
                        .ConfigureAwait(false);
                }

                continue;
            }

            if (entry.RelativePath.StartsWith("_page/", StringComparison.Ordinal))
            {
                foreach (var page in manifest.Pages)
                {
                    var tokens = BuildRootTokens(manifest);
                    tokens["Page"] = page.Name;
                    tokens["PageKebab"] = Naming.ToKebabCase(page.Name);
                    tokens["PageRoute"] = page.Route;
                    tokens["PageRequiresAuth"] = page.RequiresAuth;
                    await RenderEntryAsync(root, entry.RelativePath["_page/".Length..], content, tokens, cancellationToken)
                        .ConfigureAwait(false);
                }

                continue;
            }

            if (entry.RelativePath.StartsWith("_component/", StringComparison.Ordinal))
            {
                foreach (var component in manifest.Components)
                {
                    var tokens = BuildRootTokens(manifest);
                    tokens["Component"] = component.Name;
                    tokens["ComponentKebab"] = Naming.ToKebabCase(component.Name);
                    tokens["ComponentLibrary"] = component.Library.ToString().ToLowerInvariant();
                    await RenderEntryAsync(root, entry.RelativePath["_component/".Length..], content, tokens, cancellationToken)
                        .ConfigureAwait(false);
                }

                continue;
            }

            await RenderEntryAsync(root, entry.RelativePath, content, rootTokens, cancellationToken)
                .ConfigureAwait(false);
        }
    }

    private static async Task RenderEntryAsync(
        string root,
        string relativePathTemplate,
        string content,
        IDictionary<string, object> tokens,
        CancellationToken cancellationToken)
    {
        var relativePath = Render(relativePathTemplate, tokens);
        await SafeFileWriter.WriteAsync(root, relativePath, Render(content, tokens), cancellationToken)
            .ConfigureAwait(false);
    }

    private static Dictionary<string, object> BuildRootTokens(ValidatedManifest manifest) => new()
    {
        ["Name"] = manifest.Name,
        ["NameLower"] = manifest.Name.ToLowerInvariant(),
        ["NameKebab"] = Naming.ToKebabCase(manifest.Name),
        ["JwtSigningKey"] = "REPLACE_WITH_AT_LEAST_32_CHARACTER_RANDOM_SECRET_VALUE",
        ["Entities"] = manifest.Entities.Select(entity => new Dictionary<string, object>
        {
            ["Name"] = entity.Name,
            ["NameLower"] = entity.Name.ToLowerInvariant(),
            ["NameKebab"] = Naming.ToKebabCase(entity.Name),
            ["Properties"] = entity.Properties.Select(ToPropertyTokens).Cast<object>().ToList(),
        }).Cast<object>().ToList(),
        ["Pages"] = manifest.Pages.Select(page => new Dictionary<string, object>
        {
            ["Name"] = page.Name,
            ["NameKebab"] = Naming.ToKebabCase(page.Name),
            ["Route"] = page.Route,
            ["RequiresAuth"] = page.RequiresAuth,
        }).Cast<object>().ToList(),
        ["Components"] = manifest.Components.Select(component => new Dictionary<string, object>
        {
            ["Name"] = component.Name,
            ["NameKebab"] = Naming.ToKebabCase(component.Name),
            ["Library"] = component.Library.ToString().ToLowerInvariant(),
        }).Cast<object>().ToList(),
    };

    private static Dictionary<string, object> ToPropertyTokens(ValidatedProperty property) => new()
    {
        ["Name"] = property.Name,
        ["NameCamel"] = Naming.ToCamelCase(property.Name),
        ["Type"] = property.Type,
    };

    private static List<ManifestEntry> ReadManifest(Assembly assembly)
    {
        using var stream = assembly.GetManifestResourceStream(ManifestResource)
            ?? throw new GenerationException("The packaged full-stack template manifest was not found.");
        using var reader = new StreamReader(stream);
        var entries = new List<ManifestEntry>();
        while (reader.ReadLine() is { } line)
        {
            var trimmed = line.Trim();
            if (trimmed.Length == 0 || trimmed.StartsWith('#'))
            {
                continue;
            }

            var separator = trimmed.IndexOf('=');
            if (separator <= 0)
            {
                throw new GenerationException($"Invalid packaged template manifest entry: '{trimmed}'.");
            }

            entries.Add(new ManifestEntry(trimmed[..separator].Trim(), trimmed[(separator + 1)..].Trim()));
        }

        return entries;
    }

    private static string ReadResource(Assembly assembly, string resourceName)
    {
        var fullName = resourceName.StartsWith(ResourcePrefix, StringComparison.Ordinal)
            ? resourceName
            : ResourcePrefix + resourceName;
        using var stream = assembly.GetManifestResourceStream(fullName)
            ?? throw new GenerationException($"Packaged template resource '{resourceName}' was not found.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private static string Render(string template, IDictionary<string, object> tokens) =>
        Template.Parse(template).Render(Hash.FromDictionary(tokens), CultureInfo.InvariantCulture);

    private sealed record ManifestEntry(string RelativePath, string ResourceName);
}
