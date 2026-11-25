using TraceDiag.Models;

namespace TraceDiag.Services;

public sealed class AppConfigStore
{
    public AppConfigStore(string configFilePath)
    {
        ConfigFilePath = configFilePath;
    }

    public string ConfigFilePath { get; }

    public AppConfig Load()
    {
        try { return ConfigFileIO.Load(ConfigFilePath); }
        catch { return new AppConfig(); }
    }

    public void Save(AppConfig config)
    {
        try { ConfigFileIO.Save(ConfigFilePath, config); }
        catch
        {
            // Config persistence errors should not break the app.
        }
    }
}
