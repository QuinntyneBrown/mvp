using Mvp.Core.Manifests;

namespace Mvp.Core.Features.FullStack.Manifest;

public sealed record ManifestLoadResult(MvpManifest Manifest, IReadOnlyList<string> Warnings)
{
    /// <summary>
    /// Creates a result carrying an empty manifest, for callers that generate without a manifest file.
    /// </summary>
    /// <remarks>
    /// A method rather than a cached static property: <see cref="MvpManifest"/> is mutable, so a
    /// shared instance would hand every caller the same graph.
    /// </remarks>
    public static ManifestLoadResult CreateEmpty() => new(new MvpManifest(), []);
}

public interface IManifestLoader
{
    ManifestLoadResult Load(string path);
}
