namespace Mvp.Core.Manifests;

public class MvpManifest
{
    public string Name { get; set; } = string.Empty;

    public string? Output { get; set; }

    public List<MvpManifestEntity> Entities { get; set; } = new();

    public List<MvpManifestPage> Pages { get; set; } = new();

    public List<MvpManifestComponent> Components { get; set; } = new();
}
