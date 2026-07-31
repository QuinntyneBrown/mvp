using Mvp.Cli.Bootstrap;
using Mvp.Cli.Features.FullStack.Manifest;
using Mvp.Cli.Manifests;

namespace Mvp.Cli.Tests.Unit;

// Acceptance Test
// Traces to: L2-008, L2-009, L2-010, L2-011, L2-013, L2-015
// Description: Manifest values are normalized and rejected before any output is written.
public sealed class ManifestValidatorTests
{
    private readonly ManifestValidator _validator = new();

    [Fact]
    public void Validate_UsesCliOverridesBeforeRequiredNameValidation()
    {
        var manifest = new MvpManifest { Name = string.Empty, Output = "ignored" };

        var result = _validator.Validate(manifest, "OrderDesk", ".", []);

        Assert.Equal("OrderDesk", result.Name);
        Assert.Equal(Path.GetFullPath("."), result.OutputRoot);
    }

    [Theory]
    [InlineData("../Escape")]
    [InlineData("C:\\Escape")]
    [InlineData("option-name")]
    [InlineData("lowerCase")]
    [InlineData("CON")]
    [InlineData("Name With Space")]
    public void Validate_RejectsUnsafeOrNonPascalCaseNames(string name)
    {
        var exception = Assert.Throws<ManifestValidationException>(() =>
            _validator.Validate(new MvpManifest { Name = name }, null, "."));

        Assert.NotEmpty(exception.Errors);
    }

    [Fact]
    public void Validate_ReportsAllEntityErrorsTogether()
    {
        var manifest = new MvpManifest
        {
            Name = "OrderDesk",
            Entities =
            [
                new MvpManifestEntity
                {
                    Name = "Order",
                    Properties =
                    [
                        new MvpManifestProperty { Name = "total-value", Type = "double" },
                        new MvpManifestProperty { Name = "Id", Type = "Guid" },
                        new MvpManifestProperty { Name = "id", Type = "Guid" },
                    ],
                },
                new MvpManifestEntity { Name = "order" },
            ],
        };

        var exception = Assert.Throws<ManifestValidationException>(() => _validator.Validate(manifest, null, "."));

        Assert.True(exception.Errors.Count >= 4);
    }

    [Fact]
    public void Validate_AppliesRouteAndComponentDefaults()
    {
        var manifest = new MvpManifest
        {
            Name = "OrderDesk",
            Pages = [new MvpManifestPage { Name = "OrderHistory" }],
            Components = [new MvpManifestComponent { Name = "OrderCard" }],
        };

        var result = _validator.Validate(manifest, null, ".");

        Assert.Equal("order-history", result.Pages.Single().Route);
        Assert.True(result.Pages.Single().RequiresAuth);
        Assert.Equal(FrontendLibrary.Components, result.Components.Single().Library);
    }
}
