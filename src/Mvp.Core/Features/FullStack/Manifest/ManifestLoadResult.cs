using Mvp.Core.Manifests;

namespace Mvp.Core.Features.FullStack.Manifest;

public sealed record ManifestLoadResult(MvpManifest Manifest, IReadOnlyList<string> Warnings);

public interface IManifestLoader
{
    ManifestLoadResult Load(string path);
}
