namespace Mvp.Cli.Manifests;

public interface IMvpManifestLoader
{
    MvpManifest Load(string path);
}
