using System.Reflection;

namespace TraceDiag.Ui;

internal static class AppIconLoader
{
    internal static Icon Load()
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(Strings.App.IconResourceName);
        return stream is null ? SystemIconLoader.TryLoadNetworkIcon() ?? SystemIcons.Application : new Icon(stream);
    }
}
