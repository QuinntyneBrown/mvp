namespace Mvp.Core.Manifests;

public class MvpManifestEntity
{
    public string Name { get; set; } = string.Empty;

    public List<MvpManifestProperty> Properties { get; set; } = new();
}
