using System.Text.RegularExpressions;
using Mvp.Core.Errors;
using Mvp.Core.Features.Generation;
using Mvp.Core.Manifests;

namespace Mvp.Core.Features.FullStack.Manifest;

public sealed partial class ManifestValidator
{
    private static readonly HashSet<string> SupportedTypes =
        ["string", "int", "long", "decimal", "bool", "DateTime", "Guid"];

    private static readonly HashSet<string> ReservedNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "CON", "PRN", "AUX", "NUL",
        "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
        "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9",
        "System", "Microsoft",
    };

    public ValidatedManifest Validate(
        MvpManifest manifest,
        string? nameOverride,
        string? outputOverride,
        IReadOnlyList<string>? warnings = null)
    {
        ArgumentNullException.ThrowIfNull(manifest);
        var errors = new List<string>();
        var name = string.IsNullOrWhiteSpace(nameOverride) ? manifest.Name?.Trim() ?? string.Empty : nameOverride.Trim();
        ValidateIdentifier(name, "Solution name", errors);

        var entities = (manifest.Entities ?? []).Select((entity, index) =>
        {
            var entityName = entity?.Name?.Trim() ?? string.Empty;
            ValidateIdentifier(entityName, $"Entity {index + 1} name", errors);
            if (entityName is "User" or "RefreshToken")
            {
                errors.Add($"Entity name '{entityName}' is reserved by the authentication model.");
            }
            var properties = (entity?.Properties ?? []).Select((property, propertyIndex) =>
            {
                var propertyName = property?.Name?.Trim() ?? string.Empty;
                var type = string.IsNullOrWhiteSpace(property?.Type) ? "string" : property.Type.Trim();
                ValidateIdentifier(propertyName, $"Property {propertyIndex + 1} on '{entityName}'", errors);
                if (propertyName is "Id" or "CreatedAt")
                {
                    errors.Add($"Property name '{entityName}.{propertyName}' is generated automatically and must not be declared.");
                }
                if (!SupportedTypes.Contains(type))
                {
                    errors.Add($"Property '{entityName}.{propertyName}' has unsupported type '{type}'. Supported types: {string.Join(", ", SupportedTypes)}.");
                }

                return new ValidatedProperty(propertyName, type);
            }).ToArray();
            ValidateDuplicates(properties.Select(property => property.Name), $"properties on entity '{entityName}'", errors);
            return new ValidatedEntity(entityName, properties);
        }).ToArray();
        ValidateDuplicates(entities.Select(entity => entity.Name), "entities", errors);

        var pages = (manifest.Pages ?? []).Select((page, index) =>
        {
            var pageName = page?.Name?.Trim() ?? string.Empty;
            ValidateIdentifier(pageName, $"Page {index + 1} name", errors);
            var route = string.IsNullOrWhiteSpace(page?.Route) ? Naming.ToKebabCase(pageName) : page.Route.Trim();
            if (pageName is "SignIn" or "SignUp" or "Dashboard" || route is "sign-in" or "sign-up" or "dashboard")
            {
                errors.Add($"Page '{pageName}' conflicts with a baseline authentication page or route.");
            }
            if (!RoutePattern().IsMatch(route))
            {
                errors.Add($"Page '{pageName}' route '{route}' must contain only kebab-case path segments.");
            }

            return new ValidatedPage(pageName, route, page?.RequiresAuth ?? true);
        }).ToArray();
        ValidateDuplicates(pages.Select(page => page.Name), "pages", errors);
        ValidateDuplicates(pages.Select(page => page.Route), "page routes", errors);

        var components = (manifest.Components ?? []).Select((component, index) =>
        {
            var componentName = component?.Name?.Trim() ?? string.Empty;
            ValidateIdentifier(componentName, $"Component {index + 1} name", errors);
            if (!Enum.TryParse<FrontendLibrary>(component?.Library ?? "components", ignoreCase: true, out var library))
            {
                errors.Add($"Component '{componentName}' has unsupported library '{component?.Library}'. Supported libraries: api, components, domain.");
                library = FrontendLibrary.Components;
            }

            return new ValidatedComponent(componentName, library);
        }).ToArray();
        ValidateDuplicates(components.Select(component => component.Name), "components", errors);

        var rawOutput = outputOverride ?? manifest.Output ?? Directory.GetCurrentDirectory();
        string outputRoot = string.Empty;
        if (string.IsNullOrWhiteSpace(rawOutput))
        {
            errors.Add("Output directory cannot be empty.");
        }
        else
        {
            try
            {
                outputRoot = Path.GetFullPath(rawOutput);
            }
            catch (Exception exception) when (exception is ArgumentException or NotSupportedException or PathTooLongException)
            {
                errors.Add($"Output path is invalid: {exception.Message}");
            }
        }

        if (errors.Count > 0)
        {
            throw new ManifestValidationException(errors);
        }

        return new ValidatedManifest(
            name,
            outputRoot,
            entities,
            pages,
            components,
            warnings ?? []);
    }

    public static string ValidateIncrementalName(string name)
    {
        var errors = new List<string>();
        var normalized = name?.Trim() ?? string.Empty;
        ValidateIdentifier(normalized, "Name", errors);
        if (errors.Count > 0)
        {
            throw new ManifestValidationException(errors);
        }

        return normalized;
    }

    private static void ValidateIdentifier(string value, string label, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors.Add($"{label} is required. Supply --name or set 'name:' in the manifest.");
        }
        else if (!PascalCaseIdentifierPattern().IsMatch(value))
        {
            errors.Add($"{label} '{value}' must be a PascalCase identifier containing only ASCII letters and digits.");
        }
        else if (ReservedNames.Contains(value))
        {
            errors.Add($"{label} '{value}' is reserved and cannot be used.");
        }
    }

    private static void ValidateDuplicates(IEnumerable<string> values, string label, List<string> errors)
    {
        foreach (var duplicate in values.Where(value => value.Length > 0)
                     .GroupBy(value => value, StringComparer.OrdinalIgnoreCase)
                     .Where(group => group.Count() > 1)
                     .Select(group => group.Key))
        {
            errors.Add($"Duplicate {label} value '{duplicate}'.");
        }
    }

    [GeneratedRegex("^[A-Z][A-Za-z0-9]*$", RegexOptions.CultureInvariant)]
    private static partial Regex PascalCaseIdentifierPattern();

    [GeneratedRegex("^[a-z0-9]+(?:-[a-z0-9]+)*(?:/[a-z0-9]+(?:-[a-z0-9]+)*)*$", RegexOptions.CultureInvariant)]
    private static partial Regex RoutePattern();
}
