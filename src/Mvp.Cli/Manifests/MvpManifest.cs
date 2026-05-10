namespace Mvp.Cli.Manifests;

public class MvpManifest
{
    public string Name { get; set; } = string.Empty;

    public string? Output { get; set; }

    public List<MvpManifestEntity> Entities { get; set; } = new();

    public List<MvpManifestPage> Pages { get; set; } = new();

    public List<MvpManifestComponent> Components { get; set; } = new();
}

public class MvpManifestEntity
{
    public string Name { get; set; } = string.Empty;

    public List<MvpManifestProperty> Properties { get; set; } = new();
}

public class MvpManifestProperty
{
    public string Name { get; set; } = string.Empty;

    public string Type { get; set; } = "string";
}

public class MvpManifestPage
{
    public string Name { get; set; } = string.Empty;

    public string? Route { get; set; }

    public bool RequiresAuth { get; set; } = true;
}

public class MvpManifestComponent
{
    public string Name { get; set; } = string.Empty;

    public string Library { get; set; } = "components";
}
