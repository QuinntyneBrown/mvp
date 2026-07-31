using Mvp.Core.Manifests;

namespace Mvp.Core.Features.FullStack.Manifest;

public interface IManifestLoader
{
    ManifestLoadResult Load(string path);
}
