namespace Mvp.Core.Manifests;

public class MvpManifestPage
{
    public string Name { get; set; } = string.Empty;

    public string? Route { get; set; }

    public bool RequiresAuth { get; set; } = true;
}
