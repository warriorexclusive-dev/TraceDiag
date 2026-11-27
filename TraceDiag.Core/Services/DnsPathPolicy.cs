namespace TraceDiag.Services;

/// <summary>Default location for the editable alternative DNS list.</summary>
internal static class DnsPathPolicy
{
    internal static string Default()
    {
        if (Environment.OSVersion.Platform is PlatformID.Unix or PlatformID.MacOSX)
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                DnsServerConfig.FileName);
        return Path.Combine(AppFiles.GetAppDataDirectory(), DnsServerConfig.FileName);
    }
}
