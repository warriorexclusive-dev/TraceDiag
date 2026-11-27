namespace TraceDiag;

/// <summary>Canonical command-line tokens shared by every host application.</summary>
public static class CommandLineSettingsArg
{
    public const string Run = "run"; public const string Monitor = "monitor"; public const string Reset = "reset";
    public const string Host = "--host"; public const string PacketSize = "--packet-size";
    public const string Attempts = "--attempts"; public const string LossDelay = "--loss-delay";
    public const string Dynamic = "--dynamic"; public const string DontFragment = "--df";
    public const string NoRipe = "--no-ripe"; public const string NoRoutePrecheck = "--no-route-precheck";
    public const string DeepRoutes = "--deep-routes"; public const string ProbeTimeout = "--probe-timeout";
    public const string Ipv4 = "--ipv4"; public const string Ipv6 = "--ipv6";
    public const string Format = "--format"; public const string Language = "--language";
    public const string Help = "--help";

    public static readonly string[] Names =
        [Host, PacketSize, Attempts, LossDelay, Dynamic, DontFragment, NoRipe, NoRoutePrecheck,
            DeepRoutes, ProbeTimeout, Ipv4, Ipv6, Format, Language, Help];

    public static bool IsCommand(string value) => value is Run or Monitor or Reset;
}
