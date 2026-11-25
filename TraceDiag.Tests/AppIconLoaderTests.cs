using TraceDiag.Ui;

namespace TraceDiag.Tests;

public sealed class AppIconLoaderTests
{
    [Fact]
    public void CableIcon_IsEmbeddedInAssembly()
    {
        using var stream = typeof(AppIconLoader).Assembly
            .GetManifestResourceStream("TraceDiag.Resources.cable_24dp.ico");
        Assert.NotNull(stream);
    }
}
